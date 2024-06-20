using UnityEngine;
using RFUniverse.Attributes;
using System.Collections.Generic;
using Unity.Robotics.UrdfImporter.Control;
using Unity.Robotics.UrdfImporter;
using System.Linq;
using Newtonsoft.Json;
using System.Collections;
using System.Runtime.CompilerServices;
using System;
using System.Reflection;

namespace RFUniverse
{
    public static class RFUniverseUtility
    {
        static JsonSerializerSettings jsonSerializerSettings = null;
        public static JsonSerializerSettings JsonSerializerSettings
        {
            get
            {
                if (jsonSerializerSettings == null)
                {
                    jsonSerializerSettings = new JsonSerializerSettings();
                    jsonSerializerSettings.Converters = new[] { new UnityJsonConverter() };
                    jsonSerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
                    jsonSerializerSettings.Formatting = Formatting.Indented;
                }
                return jsonSerializerSettings;
            }
        }
        static WaitForFixedUpdate waitForFixedUpdate = new();
        static WaitForEndOfFrame waitForEndOfFrame = new();

        public static IEnumerator WaitFixedUpdateFrame(int count = 1)
        {
            count = Mathf.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                yield return waitForFixedUpdate;
            }
        }
        public static IEnumerator WaitForEndOfFrame(int count = 1)
        {
            count = Mathf.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                yield return waitForEndOfFrame;
            }
        }
        public static Color EncodeIDAsColor(int instanceId)
        {
            long r = (instanceId * (long)16807 + 187) % 256;
            long g = (instanceId * (long)48271 + 79) % 256;
            long b = (instanceId * (long)95849 + 233) % 256;
            return new Color32((byte)r, (byte)g, (byte)b, 255);
        }
        public static List<TResult> GetChildComponentFilter<TResult>(this BaseAttr parent) where TResult : Component
        {
            return GetChildComponentFilter<BaseAttr, TResult>(parent);
        }

        public static List<TResult> GetChildComponentFilter<TParent, TResult>(TParent parent) where TParent : Component where TResult : Component
        {
            List<TResult> components = new List<TResult>();
            foreach (var item in parent.GetComponentsInChildren<TResult>())
            {
                if (item.GetComponentInParent<TParent>() == parent)
                    components.Add(item);
            }
            return components;
        }

        public static Transform FindChlid(this Transform parent, string targetName, bool includeSelf = true)
        {
            if (includeSelf && parent.name == targetName)
                return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = FindChlid(parent.GetChild(i), targetName, true);
                if (child == null)
                    continue;
                else
                    return child;
            }
            return null;
        }

        public static ArticulationUnit GetUnit(this ArticulationBody body)
        {
            if (body.TryGetComponent(out ArticulationUnit unit))
                return unit;
            else
                return body.gameObject.AddComponent<ArticulationUnit>();
        }
        public static ControllerAttr NormalizeRFUniverseArticulation(GameObject root)
        {
            ControllerAttr attr = root.GetComponent<ControllerAttr>() ?? root.AddComponent<ControllerAttr>();

            // Remove URDFImporter Scripts
            UrdfPlugins urdfPlugins = root.GetComponentInChildren<UrdfPlugins>();
            if (urdfPlugins != null)
                GameObject.DestroyImmediate(urdfPlugins.gameObject);
            Controller controller = root.GetComponentInChildren<Controller>();
            if (controller != null)
                Destroy(controller);
            UrdfRobot urdfRobot = root.GetComponentInChildren<UrdfRobot>();
            if (urdfRobot != null)
                Destroy(urdfRobot);
            UrdfLink[] urdfLinks = root.GetComponentsInChildren<UrdfLink>();
            foreach (var urdfLink in urdfLinks)
            {
                Destroy(urdfLink);
            }
            UrdfInertial[] urdfInertial = root.GetComponentsInChildren<UrdfInertial>();
            foreach (var item in urdfInertial)
            {
                Destroy(item);
            }

            UrdfVisual[] urdfVisual = root.GetComponentsInChildren<UrdfVisual>();
            foreach (var item in urdfVisual)
            {
                Destroy(item);
            }

            UrdfCollision[] urdfCollision = root.GetComponentsInChildren<UrdfCollision>();
            foreach (var item in urdfCollision)
            {
                Destroy(item);
            }

            // Add basic script for root node
            IgnoreSelfCollision ign = root.GetComponent<IgnoreSelfCollision>() ?? root.AddComponent<IgnoreSelfCollision>();
            if (root.transform.GetChild(0).GetComponent<ArticulationBody>() == null)
                root.transform.GetChild(0).gameObject.AddComponent<ArticulationBody>();

            // Add RFUniverse scripts
            ArticulationBody[] articulationBodies = root.GetComponentsInChildren<ArticulationBody>();
            foreach (var body in articulationBodies)
            {
                ArticulationUnit unit = body.GetUnit();
                UrdfJoint joint = body.GetComponent<UrdfJoint>();
                unit.jointName = body.name;
                if (joint)
                {
                    unit.jointName = joint.jointName;
                    if (!string.IsNullOrEmpty(joint.minicJointParentName))
                    {
                        ArticulationBody mimicParent = articulationBodies.FirstOrDefault(s => s.GetComponent<UrdfJoint>()?.jointName == joint.minicJointParentName);
                        unit.mimicParent = mimicParent;
                        unit.mimicMultiplier = joint.multiplier;
                        unit.mimicOffset = joint.offset;
                    }
                }
                if (body.isRoot)
                    body.immovable = true;
                body.useGravity = false;

                body.linearDamping = 0.05f;
                body.angularDamping = 0.05f;
                body.jointFriction = 0.05f;

                var xDrive = body.xDrive;
                xDrive.stiffness = 100000;
                xDrive.damping = 9000;
                xDrive.forceLimit = float.MaxValue;
                body.xDrive = xDrive;

                var yDrive = body.yDrive;
                yDrive.stiffness = 100000;
                yDrive.damping = 9000;
                yDrive.forceLimit = float.MaxValue;
                body.yDrive = yDrive;

                var zDrive = body.zDrive;
                zDrive.stiffness = 100000;
                zDrive.damping = 9000;
                zDrive.forceLimit = float.MaxValue;
                body.zDrive = zDrive;

                List<Transform> renders = GetChildComponentFilter<ArticulationBody, Renderer>(body).Select((s) => s.transform).ToList();
                if (renders.Count == 0)
                    renders.Add(new GameObject("None").transform);
                foreach (var item in renders)
                {
#if UNITY_EDITOR
                    GameObject prefab = UnityEditor.PrefabUtility.GetNearestPrefabInstanceRoot(item.gameObject);
                    if (prefab != null)
                        UnityEditor.PrefabUtility.UnpackPrefabInstance(prefab, UnityEditor.PrefabUnpackMode.OutermostRoot, UnityEditor.InteractionMode.AutomatedAction);
#endif
                    item.SetParent(body.transform);
                    item.SetSiblingIndex(0);
                }
                List<Collider> colliders = GetChildComponentFilter<ArticulationBody, Collider>(body);
                for (int i = 0; i < colliders.Count; i++)
                {
                    Collider collider = colliders[i];
#if UNITY_EDITOR
                    GameObject prefab = UnityEditor.PrefabUtility.GetNearestPrefabInstanceRoot(collider.gameObject);
                    if (prefab != null)
                        UnityEditor.PrefabUtility.UnpackPrefabInstance(prefab, UnityEditor.PrefabUnpackMode.OutermostRoot, UnityEditor.InteractionMode.AutomatedAction);
#endif
                    collider.name = "Collider";
                    int index = Mathf.Min(renders.Count - 1, i);
                    if (index >= 0 && index < renders.Count)
                        collider.transform.parent = renders[index];
                    else
                        collider.transform.parent = body.transform;
                }
            }

            UrdfVisuals[] urdfVisuals = root.GetComponentsInChildren<UrdfVisuals>();
            foreach (var item in urdfVisuals)
            {
                Destroy(item.gameObject);
            }
            UrdfCollisions[] urdfCollisions = root.GetComponentsInChildren<UrdfCollisions>();
            foreach (var item in urdfCollisions)
            {
                Destroy(item.gameObject);
            }
            UrdfJoint[] urdfJoints = root.GetComponentsInChildren<UrdfJoint>();
            foreach (var item in urdfJoints)
            {
                Destroy(item);
            }

            attr.GetJointParameters();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(root);
#endif
            return attr;
        }

        public static void Destroy(UnityEngine.Object obj)
        {
            if (Application.isEditor)
                GameObject.DestroyImmediate(obj);
            else
                GameObject.Destroy(obj);
        }

        public static List<BaseAttrData> SortByParent(List<BaseAttrData> datas)
        {
            List<int> headID = new List<int>();
            int i = 0;
            while (i < datas.Count)
            {
                if (datas[i].parentID > 0 && !headID.Contains(datas[i].parentID))
                {
                    BaseAttrData t = datas[i];
                    datas.Remove(t);
                    datas.Add(t);
                }
                else
                {
                    headID.Add(datas[i].id);
                    i++;
                }
            }
            return datas;
        }
        public static Vector3 ListFloatToVector3(List<float> floats)
        {
            return new Vector3(floats[0], floats[1], floats[2]);
        }
        public static List<Vector3> ListFloatToListVector3(List<float> floats)
        {
            List<Vector3> v3s = new List<Vector3>();
            int i = 0;
            while (i + 2 < floats.Count)
                v3s.Add(new Vector3(floats[i++], floats[i++], floats[i++]));
            return v3s;
        }
        public static List<Vector3> FloatArrayToListVector3(float[,] floats)
        {
            List<Vector3> v3s = new List<Vector3>();
            for (int i = 0; i < floats.Length; i++)
            {
                v3s.Add(new Vector3(floats[i, 0], floats[i, 1], floats[i, 2]));
            }
            return v3s;
        }
        public static List<Color> FloatArrayToListColor(float[,] colors)
        {
            List<Color> v3s = new List<Color>();
            for (int i = 0; i < colors.Length; i++)
            {
                v3s.Add(new Color(colors[i, 0], colors[i, 1], colors[i, 2]));
            }
            return v3s;
        }

        public static Color ListFloatToColor(List<float> floats)
        {
            if (floats == null)
                return Color.black;
            return new Color(floats.Count > 0 ? floats[0] : 0, floats.Count > 1 ? floats[1] : 0, floats.Count > 2 ? floats[2] : 0, floats.Count > 3 ? floats[3] : 0);
        }

        public static Color32 ListFloatToColor32(List<int> ints)
        {
            if (ints == null)
                return Color.black;
            return new Color32((byte)(ints.Count > 0 ? ints[0] : 0), (byte)(ints.Count > 1 ? ints[1] : 0), (byte)(ints.Count > 2 ? ints[2] : 0), (byte)(ints.Count > 3 ? ints[3] : 0));
        }
        public static List<Color> ListFloatToListColor(List<float> floats)
        {
            List<Color> v3s = new List<Color>();
            int i = 0;
            while (i + 2 < floats.Count)
                v3s.Add(new Color(floats[i++], floats[i++], floats[i++]));
            return v3s;
        }
        public static List<List<T>> ListSlicer<T>(List<T> sources, int count)
        {
            if (sources.Count % count != 0) return null;
            Queue<T> que = new Queue<T>(sources);
            List<List<T>> result = new();
            while (que.Count >= count)
            {
                List<T> one = new();
                for (int i = 0; i < count; i++)
                {
                    one.Add(que.Dequeue());
                }
                result.Add(one);
            }
            return result;
        }
        public static T[,] ArraySlicer<T>(T[] sources, int count)
        {
            if (sources.Length % count != 0) return null;
            Queue<T> que = new Queue<T>(sources);
            T[,] result = new T[sources.Length / count, count];
            for (int i = 0; i < result.GetLength(0); i++)
            {
                for (int j = 0; j < result.GetLength(1); j++)
                {
                    result[i, j] = que.Dequeue();
                }
            }
            return result;
        }
        public static T[,] ListSlicerArray<T>(List<T> sources, int count)
        {
            if (sources.Count % count != 0) return null;
            Queue<T> que = new Queue<T>(sources);
            T[,] result = new T[sources.Count / count, count];
            for (int i = 0; i < result.GetLength(0); i++)
            {
                for (int j = 0; j < result.GetLength(1); j++)
                {
                    result[i, j] = que.Dequeue();
                }
            }
            return result;
        }

        public static Matrix4x4 ListFloatToMatrix(List<float> floats)
        {
            Matrix4x4 matrix = new();
            for (int i = 0; i < 16; i++)
            {
                matrix[i] = floats[i];
            }
            return matrix;
        }
        public static List<float> MatrixToListFloat(Matrix4x4 matrix)
        {
            List<float> floats = new();
            for (int i = 0; i < 16; i++)
            {
                floats.Add(matrix[i]);
            }
            return floats;
        }
        public static float[,] MatrixToFloatArray(Matrix4x4 matrix)
        {
            float[,] floats = new float[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    floats[i, j] = matrix[i, j];
                }
            }
            return floats;
        }
        public static List<float[,]> ListMatrixToListFloatArray(List<Matrix4x4> ms)
        {
            List<float[,]> f = new();
            foreach (var item in ms)
            {
                f.Add(MatrixToFloatArray(item));
            }
            return f;
        }
        public static List<Matrix4x4> ListMatrixTRS(List<Vector3> positioins, List<Quaternion> rotatiobs, List<Vector3> scales = null)
        {
            if (scales == null)
            {
                scales = new();
                for (int i = 0; i < positioins.Count; i++)
                {
                    scales.Add(Vector3.one);
                }
            }
            List<Matrix4x4> ms = new();
            for (int i = 0; i < positioins.Count; i++)
            {
                ms.Add(Matrix4x4.TRS(positioins[i], rotatiobs[i], scales[i]));
            }
            return ms;
        }
        public static Matrix4x4 FloatArrayToMatrix(float[,] floats)
        {
            Matrix4x4 matrix = Matrix4x4.identity;
            for (int i = 0; i < floats.GetLength(0); i++)
            {
                for (int j = 0; j < floats.GetLength(1); j++)
                {
                    matrix[i, j] = floats[i, j];
                }
            }
            return matrix;
        }
        public static Matrix4x4 DoubleArrayToMatrix(double[,] floats)
        {
            Matrix4x4 matrix = new();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    matrix[i, j] = (float)floats[i, j];
                }
            }
            return matrix;
        }
        public static List<float> QuaternionToListFloat(Quaternion qua)
        {
            List<float> fs = new()
            {
                qua.x,
                qua.y,
                qua.z,
                qua.w
            };
            return fs;
        }
        public static List<float> Vector3ToListFloat(Vector3 v3)
        {
            List<float> fs = new()
            {
                v3.x,
                v3.y,
                v3.z
            };
            return fs;
        }
        public static List<float> ListVector3ToListFloat(List<Vector3> v3s)
        {
            List<float> fs = new();
            foreach (var item in v3s)
            {
                fs.AddRange(Vector3ToListFloat(item));
            }
            return fs;
        }

        public static List<List<float>> ListVector3ToListFloat3(List<Vector3> v3s)
        {
            List<List<float>> f = new();
            foreach (var item in v3s)
            {
                f.Add(Vector3ToListFloat(item));
            }
            return f;
        }
        public static List<float> ListMatrixToListFloat(List<Matrix4x4> ms)
        {
            List<float> f = new();
            foreach (var item in ms)
            {
                for (int i = 0; i < 16; i++)
                {
                    f.Add(item[i]);
                }
            }
            return f;
        }
        public static List<Vector3> ListVector3LocalToWorld(List<Vector3> v3s, Transform trans)
        {
            List<Vector3> world = new();
            foreach (var item in v3s)
            {
                world.Add(trans.TransformPoint(item));
            }
            return world;
        }
        public static List<Quaternion> ListQuaternionLocalToWorld(List<Quaternion> qs, Transform trans)
        {
            List<Quaternion> world = new();
            foreach (var item in qs)
            {
                world.Add(trans.rotation * item);
            }
            return world;
        }
        public static List<Quaternion> ListFloatToListQuaternion(List<float> floats)
        {
            List<Quaternion> qs = new();
            int i = 0;
            while (i + 3 < floats.Count)
                qs.Add(new Quaternion(floats[i++], floats[i++], floats[i++], floats[i++]));
            return qs;
        }
        public static Quaternion ListFloatToQuaternion(List<float> floats)
        {
            Quaternion qs = new();
            qs.x = floats[0];
            qs.y = floats[1];
            qs.z = floats[2];
            qs.w = floats[3];
            return qs;
        }
        public static List<float> ListQuaternionToListFloat(List<Quaternion> qs)
        {
            List<float> fs = new List<float>();
            foreach (var item in qs)
            {
                fs.Add(item.x);
                fs.Add(item.y);
                fs.Add(item.z);
                fs.Add(item.w);
            }
            return fs;
        }

        public static object ConvertObjectType(this object obj, Type type)
        {
            if (obj == null)
                return obj;

            if (obj.GetType() == type)
                return obj;

            if (type.IsArray && obj is Array)
                return obj;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var listType = type.GetGenericArguments()[0];
                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(listType));

                if (obj is IList objList)
                {
                    foreach (var item in objList)
                    {
                        MethodInfo convertToMethod = typeof(RFUniverseUtility).GetMethod("ConvertType").MakeGenericMethod(listType);
                        object convertedItem = convertToMethod.Invoke(null, new object[] { item });
                        list.Add(convertedItem);
                    }
                }
                return list;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var keyType = type.GetGenericArguments()[0];
                var valueType = type.GetGenericArguments()[1];
                var dictionary = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
                if (obj is IDictionary objDict)
                {
                    foreach (DictionaryEntry item in objDict)
                    {
                        MethodInfo convertKeyMethod = typeof(RFUniverseUtility).GetMethod("ConvertType").MakeGenericMethod(keyType);
                        MethodInfo convertValueMethod = typeof(RFUniverseUtility).GetMethod("ConvertType").MakeGenericMethod(valueType);
                        object convertedKey = convertKeyMethod.Invoke(null, new object[] { item.Key });
                        object convertedValue = convertValueMethod.Invoke(null, new object[] { item.Value });
                        dictionary.Add(convertedKey, convertedValue);
                    }
                }
                return dictionary;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition().Name.StartsWith("Tuple") && type.GetGenericArguments().Length > 0)
            {
                var tupleTypes = type.GetGenericArguments();
                var tuple = Activator.CreateInstance(type.GetGenericTypeDefinition().MakeGenericType(tupleTypes), new object[tupleTypes.Length]);
                if (obj is ITuple objTuple && objTuple.Length == tupleTypes.Length)
                {
                    for (int i = 0; i < tupleTypes.Length; i++)
                    {
                        MethodInfo convertToMethod = typeof(RFUniverseUtility).GetMethod("ConvertType").MakeGenericMethod(tupleTypes[i]);
                        object convertedItem = convertToMethod.Invoke(null, new object[] { objTuple[i] });
                        tuple.GetType().GetField($"m_Item{i + 1}", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(tuple, convertedItem);
                    }
                }
                return tuple;
            }

            Debug.LogError($"Cannot convert {obj.GetType()} to {type}");
            return default;
        }

        public static T ConvertType<T>(this object obj)
        {
            if (obj == null)
                return default;

            if (obj is Unboxed<int>)
                return (T)obj;

            if (obj is Unboxed<float>)
                return (T)obj;

            if (obj is Unboxed<bool>)
                return (T)obj;

            if (obj is T)
                return (T)obj;

            if (obj is IConvertible)
                return (T)Convert.ChangeType(obj, typeof(T));

            if (typeof(T).IsArray && obj is Array)
            {
                return (T)obj;
                //var elementType = typeof(T).GetElementType();
                //int[] rankLength = new int[objArr.Rank];
                //for (int i = 0; i < objArr.Rank; i++)
                //{
                //    rankLength[i] = objArr.GetLength(i);
                //}
                //var convertedArray = Array.CreateInstance(elementType, rankLength);
                //int[] rankIndex = new int[convertedArray.Rank];
                //for (int i = 0; i < convertedArray.Rank; i++)
                //{
                //    for (int j = 0; j < convertedArray.GetLength(i); j++)
                //    {
                //        rankIndex[i] = j;
                //        MethodInfo convertToMethod = typeof(RFUniverseUtility).GetMethod("ConvertType").MakeGenericMethod(elementType);
                //        object convertedItem = convertToMethod.Invoke(null, new object[] { objArr.GetValue(rankIndex) });
                //        convertedArray.SetValue(convertedItem, rankIndex);
                //    }
                //}
                //return (T)(object)convertedArray;
            }

            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
            {
                var listType = typeof(T).GetGenericArguments()[0];
                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(listType));

                if (obj is IList objList)
                {
                    foreach (var item in objList)
                    {
                        MethodInfo convertToMethod = typeof(RFUniverseUtility).GetMethod("ConvertType").MakeGenericMethod(listType);
                        object convertedItem = convertToMethod.Invoke(null, new object[] { item });
                        list.Add(convertedItem);
                    }
                }
                return (T)list;
            }

            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var keyType = typeof(T).GetGenericArguments()[0];
                var valueType = typeof(T).GetGenericArguments()[1];
                var dictionary = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
                if (obj is IDictionary objDict)
                {
                    foreach (DictionaryEntry item in objDict)
                    {
                        MethodInfo convertKeyMethod = typeof(RFUniverseUtility).GetMethod("ConvertType").MakeGenericMethod(keyType);
                        MethodInfo convertValueMethod = typeof(RFUniverseUtility).GetMethod("ConvertType").MakeGenericMethod(valueType);
                        object convertedKey = convertKeyMethod.Invoke(null, new object[] { item.Key });
                        object convertedValue = convertValueMethod.Invoke(null, new object[] { item.Value });
                        dictionary.Add(convertedKey, convertedValue);
                    }
                }
                return (T)dictionary;
            }

            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition().Name.StartsWith("Tuple") && typeof(T).GetGenericArguments().Length > 0)
            {
                var tupleTypes = typeof(T).GetGenericArguments();
                var tuple = Activator.CreateInstance(typeof(T).GetGenericTypeDefinition().MakeGenericType(tupleTypes), new object[tupleTypes.Length]);
                if (obj is ITuple objTuple && objTuple.Length == tupleTypes.Length)
                {
                    for (int i = 0; i < tupleTypes.Length; i++)
                    {
                        MethodInfo convertToMethod = typeof(RFUniverseUtility).GetMethod("ConvertType").MakeGenericMethod(tupleTypes[i]);
                        object convertedItem = convertToMethod.Invoke(null, new object[] { objTuple[i] });
                        tuple.GetType().GetField($"m_Item{i + 1}", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(tuple, convertedItem);
                    }
                }
                return (T)tuple;
            }

            Debug.LogError($"Cannot convert {obj.GetType()} to {typeof(T)}");
            return default;
        }

        public static List<int> GetChildIndexQueue(this Transform transform, Transform child)
        {
            if (!child.GetComponentsInParent<Transform>().Contains(transform)) return null;
            List<int> indexQueue = new List<int>();
            Transform current = child;
            do
            {
                indexQueue.Add(current.GetSiblingIndex());
                current = current.parent;
            }
            while (current != transform);
            indexQueue.Reverse();
            return indexQueue;
        }
        public static Transform FindChildIndexQueue(this Transform transform, List<int> indexQueue)
        {
            if (indexQueue.Count == 0) return null;
            foreach (var item in indexQueue)
            {
                if (transform.childCount <= item) return null;
                transform = transform.GetChild(item);
                if (transform == null) return null;
            }
            return transform;
        }

        public static void FlipMesh(Mesh mesh)
        {
            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = -normals[i];
            }
            mesh.normals = normals;

            int[] triangles = mesh.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int temp = triangles[i];
                triangles[i] = triangles[i + 1];
                triangles[i + 1] = temp;
            }
            mesh.triangles = triangles;
        }
        public static void SmoothNormals(Mesh mesh)
        {
            Vector3[] normals = mesh.normals;
            Vector3[] vertices = mesh.vertices;
            Dictionary<Vector3, List<int>> vertexToNormalIndex = new Dictionary<Vector3, List<int>>();

            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertexToNormalIndex.ContainsKey(vertices[i]))
                    vertexToNormalIndex[vertices[i]].Add(i);
                else
                    vertexToNormalIndex.Add(vertices[i], new List<int> { i });
            }

            foreach (var group in vertexToNormalIndex)
            {
                if (group.Value.Count == 1) continue;
                Vector3 avg = Vector3.zero;

                foreach (int index in group.Value)
                {
                    avg += normals[index];
                }

                avg /= group.Value.Count;

                foreach (int index in group.Value)
                {
                    normals[index] = avg;
                }
            }

            mesh.normals = normals;
        }
    }
}
