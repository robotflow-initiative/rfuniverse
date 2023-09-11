using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace RFUniverse.Attributes
{
    [Serializable]
    public class SceneData
    {
        public bool ground = false;
        public float[] cameraPosition = { 0, 1, -3 };
        public float[] cameraRotation = { 0, 0, 0 };
        public float[] groundPosition = { 0, 0, 0 };

        public List<BaseAttrData> assetsData = new List<BaseAttrData>();
    }
    [Serializable]
    public class BaseAttrData
    {
        public string name;
        public int id = -1;
        public string type;
        public int parentID = -1;
        public string parentName;
        public float[] position;
        public float[] rotation;
        public float[] scale;
        public BaseAttrData()
        {
            type = "Base";
        }
        public BaseAttrData(BaseAttrData b)
        {
            name = b.name;
            id = b.id;
            type = "Base";
            parentID = b.parentID;
            parentName = b.parentName;
            position = b.position;
            rotation = b.rotation;
            scale = b.scale;
        }

        public virtual void SetAttrData(BaseAttr attr)
        {
            attr.Name = name;
            attr.ID = id;
            attr.SetParent(parentID, parentName);

            Vector3 position = new Vector3(this.position[0], this.position[1], this.position[2]);
            Vector3 rotation = new Vector3(this.rotation[0], this.rotation[1], this.rotation[2]);
            Vector3 scale = new Vector3(this.scale[0], this.scale[1], this.scale[2]);

            attr.SetTransform(true, true, true, position, rotation, scale);
        }
    }
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class BaseAttr : MonoBehaviour
    {
        private static Dictionary<int, BaseAttr> attrs = new Dictionary<int, BaseAttr>();
        public static Dictionary<int, BaseAttr> Attrs => new Dictionary<int, BaseAttr>(attrs);
        public static Dictionary<int, BaseAttr> ActiveAttrs => attrs.Where((s) => s.Value.gameObject.activeInHierarchy).ToDictionary((s) => s.Key, (s) => s.Value);

        public static Action OnAttrChange;

        private static void AddAttr(BaseAttr attr)
        {
            if (!attrs.ContainsKey(attr.ID))
            {
                attrs.Add(attr.ID, attr);
                OnAttrChange?.Invoke();
            }
        }
        private static void RemoveAttr(BaseAttr attr)
        {
            if (attrs.ContainsKey(attr.ID))
            {
                attrs.Remove(attr.ID);
                OnAttrChange?.Invoke();
            }
        }

        [SerializeField]
        private int id = -1;
        public int ID
        {
            get
            {
                if (id < 0)
                    id = UnityEngine.Random.Range(100000, 1000000);
                return id;
            }
            set
            {
                id = value;
            }
        }
        [HideInInspector]
        [SerializeField]
        private string attrName = string.Empty;
        public virtual string Name
        {
            get
            {
                if (string.IsNullOrEmpty(attrName))
                    attrName = gameObject.name;
                return attrName;
            }
            set
            {
                attrName = value;
            }
        }

        public List<BaseAttr> childs = new List<BaseAttr>();

        public void Instance()
        {
            Init();
            Rigister();
        }
        public virtual void Init()
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            mpb.SetColor("_IDColor", RFUniverseUtility.EncodeIDAsColor(ID));
            foreach (var item in this.GetChildComponentFilter<Renderer>())
            {
                item.SetPropertyBlock(mpb);
            }
            for (int i = 0; i < childs.Count; i++)
            {
                childs[i].ID = ID * 10 + i;
                childs[i].Instance();
            }
        }

        protected virtual void Rigister()
        {
            if (Attrs.ContainsKey(ID))
                Debug.LogError($"ID:{ID} Name:{Name} exist");
            else
            {
                Debug.Log($"Rigister ID:{ID} Name:{Name}");
                AddAttr(this);
            }
        }
        public virtual BaseAttrData GetAttrData()
        {
            BaseAttrData data = new BaseAttrData();

            data.name = Name;
            data.id = ID;
            BaseAttr parentAttr = null;
            List<BaseAttr> parents = GetComponentsInParent<BaseAttr>().ToList();
            parents.RemoveAt(0);
            if (parents.Count > 1)
            {
                List<BaseAttr> parentsTmp = new List<BaseAttr>(parents);
                foreach (var item in parentsTmp)
                {
                    foreach (var child in item.childs)
                    {
                        parents.Remove(child);
                    }
                }
                parentAttr = parents[0];
            }

            data.parentID = parentAttr == null ? -1 : parentAttr.ID;

            Transform parent = transform.parent;
            data.parentName = parent == null ? "" : parent.name;

            data.position = new float[3] { transform.position.x, transform.position.y, transform.position.z };
            data.rotation = new float[3] { transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z };
            data.scale = new float[3] { transform.localScale.x, transform.localScale.y, transform.localScale.z };

            return data;
        }

        private void OnDestroy()
        {
            RemoveAttr(this);
        }
        public virtual Dictionary<string, object> CollectData()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("name", Name);
            data.Add("position", transform.position);
            data.Add("rotation", transform.eulerAngles);
            data.Add("quaternion", transform.rotation);
            data.Add("local_position", transform.localPosition);
            data.Add("local_rotation", transform.localEulerAngles);
            data.Add("local_quaternion", transform.localRotation);
            data.Add("local_to_world_matrix", transform.localToWorldMatrix);
            if (resultLocalPoint != null)
            {
                data.Add("result_local_point", resultLocalPoint.Value);
                resultLocalPoint = null;
            }
            if (resultWorldPoint != null)
            {
                data.Add("result_world_point", resultWorldPoint.Value);
                resultWorldPoint = null;
            }
            return data;
        }

        public void ReceiveData(object[] data)
        {
            string type = (string)data[0];
            data = data.Skip(1).ToArray();
            AnalysisData(type, data);
        }

        public virtual void AnalysisData(string type, object[] data)
        {
            switch (type)
            {
                case "SetTransform":
                    SetTransform(data[0].ConvertType<List<float>>(), data[1].ConvertType<List<float>>(), data[2].ConvertType<List<float>>(), (bool)data[3]);
                    return;
                case "SetPosition":
                    SetPosition(data[0].ConvertType<List<float>>(), (bool)data[1]);
                    return;
                case "SetRotation":
                    SetRotation(data[0].ConvertType<List<float>>(), (bool)data[1]);
                    return;
                case "SetRotationQuaternion":
                    SetRotationQuaternion(data[0].ConvertType<List<float>>(), (bool)data[1]);
                    return;
                case "SetScale":
                    SetScale(data[0].ConvertType<List<float>>());
                    return;
                case "Translate":
                    Translate(data[0].ConvertType<List<float>>(), (bool)data[1]);
                    return;
                case "Rotate":
                    Rotate(data[0].ConvertType<List<float>>(), (bool)data[1]);
                    return;
                case "LookAt":
                    LookAt(data[0].ConvertType<List<float>>(), data[1].ConvertType<List<float>>());
                    return;
                case "SetParent":
                    SetParent((int)data[0], (string)data[1]);
                    return;
                case "SetActive":
                    SetActive((bool)data[0]);
                    return;
                case "SetLayer":
                    SetLayer((int)data[0]);
                    return;
                case "Copy":
                    Copy((int)data[0]);
                    return;
                case "Destroy":
                    Destroy();
                    return;
                case "GetLocalPointFromWorld":
                    GetLocalPointFromWorld(data[0].ConvertType<List<float>>());
                    return;
                case "GetWorldPointFromLocal":
                    GetWorldPointFromLocal(data[0].ConvertType<List<float>>());
                    return;
                default:
                    Debug.LogWarning($"ID: {ID} Type: {GetType().Name}Dont have mehond: {type}");
                    return;
            }
        }

        public void SetTransform(List<float> position, List<float> rotation, List<float> scale, bool worldSpace = true)
        {
            Debug.Log("SetTransform");
            if (position != null)
            {
                SetPosition(position, worldSpace);
            }
            if (rotation != null)
            {
                SetRotation(rotation, worldSpace);
            }
            if (scale != null)
            {
                SetScale(scale);
            }
        }
        public void SetTransform(bool set_position, bool set_rotation, bool set_scale, Vector3 position, Vector3 rotation, Vector3 scale, bool worldSpace = true)
        {
            if (set_position)
            {
                SetPosition(position, worldSpace);
            }
            if (set_rotation)
            {
                SetRotation(rotation, worldSpace);
            }
            if (set_scale)
            {
                SetScale(scale);
            }
        }
        protected virtual void SetPosition(List<float> position, bool worldSpace = true)
        {
            Debug.Log("SetPosition");
            SetPosition(new Vector3(position[0], position[1], position[2]), worldSpace);
        }
        public virtual void SetPosition(Vector3 position, bool worldSpace = true)
        {
            if (worldSpace)
                transform.position = position;
            else
                transform.localPosition = position;
        }
        protected virtual void SetRotation(List<float> rotation, bool worldSpace = true)
        {
            Debug.Log("SetRotation");
            SetRotation(new Vector3(rotation[0], rotation[1], rotation[2]), worldSpace);
        }
        public virtual void SetRotation(Vector3 rotation, bool worldSpace = true)
        {
            if (worldSpace)
                transform.eulerAngles = rotation;
            else
                transform.localEulerAngles = rotation;
        }
        public virtual void SetRotationQuaternion(List<float> quaternion, bool worldSpace = true)
        {
            Debug.Log("SetRotationQuaternion");
            SetRotation(new Quaternion(quaternion[0], quaternion[1], quaternion[2], quaternion[3]).eulerAngles, worldSpace);
        }
        protected virtual void SetScale(List<float> scale)
        {
            Debug.Log("SetScale");
            SetScale(new Vector3(scale[0], scale[1], scale[2]));
        }
        public virtual void SetScale(Vector3 scale)
        {
            transform.localScale = scale;
        }

        public virtual void Translate(List<float> translate, bool worldSpace)
        {
            Space space = worldSpace ? Space.World : Space.Self;
            transform.Translate(new Vector3(translate[0], translate[1], translate[2]), space);
        }

        public virtual void Rotate(List<float> translate, bool worldSpace)
        {
            Space space = worldSpace ? Space.World : Space.Self;
            transform.Rotate(new Vector3(0, 0, 1), translate[2], space);
            transform.Rotate(new Vector3(1, 0, 0), translate[0], space);
            transform.Rotate(new Vector3(0, 1, 0), translate[1], space);
        }

        public virtual void LookAt(List<float> target, List<float> worldUp)
        {
            transform.LookAt(RFUniverseUtility.ListFloatToVector3(target), RFUniverseUtility.ListFloatToVector3(worldUp));
        }

        public virtual void SetParent(int parentID, string parentName)
        {
            Debug.Log("SetParent");
            Transform parent = null;
            if (Attrs.TryGetValue(parentID, out BaseAttr attr))
            {
                parent = attr.transform.FindChlid(parentName, true);
                if (parent == null)
                    parent = attr.transform;
            }
            transform.SetParent(parent);
        }

        protected virtual void SetActive(bool active)
        {
            Debug.Log("SetActive");
            OnAttrChange?.Invoke();
            gameObject.SetActive(active);
        }
        public void SetLayer(int layer)
        {
            Debug.Log("SetLayer");
            foreach (var item in this.GetChildComponentFilter<Transform>())
            {
                item.gameObject.layer = layer;
            }
        }
        protected void Copy(int newID)
        {
            Debug.Log("Copy");
            GameObject copy = GameObject.Instantiate(gameObject);
            BaseAttr attr = copy.GetComponent<BaseAttr>();
            attr.ID = newID;
            attr.Instance();
        }

        public virtual void Destroy()
        {
            //if (Application.isEditor)
            DestroyImmediate(gameObject);
            //else
            //    Destroy(gameObject);
        }

        Vector3? resultLocalPoint = null;
        void GetLocalPointFromWorld(List<float> world)
        {
            Debug.Log("GetLocalPointFromWorld");
            resultLocalPoint = transform.InverseTransformPoint(new Vector3(world[0], world[1], world[2]));
        }

        Vector3? resultWorldPoint = null;
        void GetWorldPointFromLocal(List<float> local)
        {
            Debug.Log("GetWorldPointFromLocal");
            resultWorldPoint = transform.TransformPoint(new Vector3(local[0], local[1], local[2]));
        }

        private static List<Tuple<int, int>> collisionPairs = new List<Tuple<int, int>>();
        public static List<Tuple<int, int>> CollisionPairs => new List<Tuple<int, int>>(collisionPairs);

        public static Action OnCollisionPairsChange;
        private void OnCollisionEnter(Collision other)
        {
            BaseAttr otherAttr = other.gameObject.GetComponentInParent<BaseAttr>();
            if (otherAttr == null) return;
            if (collisionPairs.Exists(s => (s.Item1 == this.ID && s.Item2 == otherAttr.ID) || (s.Item1 == otherAttr.ID && s.Item2 == this.ID))) return;
            collisionPairs.Add(new Tuple<int, int>(this.ID, otherAttr.ID));
            OnCollisionPairsChange?.Invoke();
        }
        private void OnCollisionExit(Collision other)
        {
            BaseAttr otherAttr = other.gameObject.GetComponentInParent<BaseAttr>();
            if (otherAttr == null) return;
            Tuple<int, int> pair = collisionPairs.Find(s => (s.Item1 == this.ID && s.Item2 == otherAttr.ID) || (s.Item1 == otherAttr.ID && s.Item2 == this.ID));
            collisionPairs.Remove(pair);
            OnCollisionPairsChange?.Invoke();
        }
    }


}
