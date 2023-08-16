using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace RFUniverse.Attributes
{
    [Serializable]
    public class ColliderAttrData : GameObjectAttrData
    {
        public List<ColliderData> colliderDatas = new List<ColliderData>();
        public ColliderAttrData()
        {
            type = "Collider";
        }
        public ColliderAttrData(BaseAttrData b) : base(b)
        {
            if (b is ColliderAttrData)
                colliderDatas = (b as ColliderAttrData).colliderDatas;
            type = "Collider";
        }
        public override void SetAttrData(BaseAttr attr)
        {
            base.SetAttrData(attr);
            ColliderAttr colliderAttr = attr as ColliderAttr;
            colliderAttr.SetColliderDatas(colliderDatas);
        }
    }
    [Serializable]
    public class ColliderData
    {
        public List<int> renderIndexQueue = new List<int>();
        public ColliderType type = ColliderType.Original;
        public float[] position = { 0, 0, 0 };
        public float[] rotation = { 0, 0, 0 };
        public float[] scale = { 1, 1, 1 };
        public float radius = 1;
        public float height = 1;
        public int direction = 0;
        public PhysicMaterialData physicMateria = new PhysicMaterialData(null);
    }
    [Serializable]
    public class PhysicMaterialData
    {
        public float bounciness = 0;
        public float dynamicFriction = 0.6f;
        public float staticFriction = 0.6f;
        public PhysicMaterialCombine frictionCombine = PhysicMaterialCombine.Average;
        public PhysicMaterialCombine bounceCombine = PhysicMaterialCombine.Average;
        public PhysicMaterialData(PhysicMaterial physicMaterial)
        {
            if (physicMaterial == null) return;
            bounciness = physicMaterial.bounciness;
            dynamicFriction = physicMaterial.dynamicFriction;
            staticFriction = physicMaterial.staticFriction;
            frictionCombine = physicMaterial.frictionCombine;
            bounceCombine = physicMaterial.bounceCombine;
        }
        public PhysicMaterial ToPhysicMaterial()
        {
            return new PhysicMaterial
            {
                bounciness = bounciness,
                dynamicFriction = dynamicFriction,
                staticFriction = staticFriction,
                frictionCombine = frictionCombine,
                bounceCombine = bounceCombine
            };
        }
    }
    public enum ColliderType
    {
        None,
        Box,
        Sphere,
        Capsule,
        Mesh,
        Original
    }
    public class ColliderAttr : GameObjectAttr
    {
        private bool isRFMoveCollider = true;
        public bool IsRFMoveCollider
        {
            get
            {
                return isRFMoveCollider;
            }
            set
            {
                isRFMoveCollider = value;
            }
        }
        public override BaseAttrData GetAttrData()
        {
            ColliderAttrData data = new ColliderAttrData(base.GetAttrData());
            data.colliderDatas = GetColliderDatas();
            return data;
        }

        private List<ColliderData> colliderDatas;
        [EditableAttr("Colliders")]
        public List<ColliderData> ColliderDatas
        {
            get
            {
                if (colliderDatas == null)
                    colliderDatas = GetColliderDatas();
                return colliderDatas;
            }
            set
            {
                colliderDatas = value;
            }
        }
        public List<ColliderData> GetColliderDatas()
        {
            List<ColliderData> datas = new List<ColliderData>();
            foreach (var item in this.GetChildComponentFilter<MeshRenderer>())
            {
                ColliderData data = new ColliderData();
                datas.Add(data);
                data.renderIndexQueue = transform.GetChildIndexQueue(item.transform);
                Transform child = item.transform.Find("Collider");
                if (child == null)
                    data.type = ColliderType.None;
                else
                {
                    Collider[] colliders = child.GetComponents<Collider>();
                    if (colliders.Length == 0)
                    {
                        data.type = ColliderType.None;
                    }
                    else if (colliders.Length > 1)
                    {
                        data.type = ColliderType.Original;
                    }
                    else
                    {
                        Collider collider = colliders[0];

                        data.position = new float[3] { collider.transform.localPosition.x, collider.transform.localPosition.y, collider.transform.localPosition.z };
                        data.rotation = new float[3] { collider.transform.localEulerAngles.x, collider.transform.localEulerAngles.y, collider.transform.localEulerAngles.z };
                        data.scale = new float[3] { collider.transform.localScale.x, collider.transform.localScale.y, collider.transform.localScale.z };

                        if (collider is BoxCollider)
                        {
                            data.type = ColliderType.Box;
                        }
                        else if (collider is SphereCollider)
                        {
                            data.type = ColliderType.Sphere;
                            data.radius = (collider as SphereCollider).radius;
                        }
                        else if (collider is CapsuleCollider)
                        {
                            data.type = ColliderType.Capsule;
                            data.radius = (collider as CapsuleCollider).radius;
                            data.height = (collider as CapsuleCollider).height;
                            data.direction = (collider as CapsuleCollider).direction;
                        }
                        else if (collider is MeshCollider)
                        {
                            if ((collider as MeshCollider).sharedMesh == item.GetComponent<MeshFilter>().sharedMesh)
                            {
                                data.type = ColliderType.Mesh;
                            }
                            else
                            {
                                data.type = ColliderType.Original;
                            }
                        }
                        data.physicMateria = new PhysicMaterialData(collider.material);
                    }
                }
            }
            return datas;
        }
        public void SetColliderDatas(List<ColliderData> datas)
        {
            foreach (var data in datas)
            {
                Transform render = transform.FindChildIndexQueue(data.renderIndexQueue);
                if (render == null) continue;
                Transform collider = render.Find("Collider");
                if (collider != null && data.type != ColliderType.Original)
                {
                    if (Application.isPlaying)
                        Destroy(collider.gameObject);
                    else
                        DestroyImmediate(collider.gameObject);
                    collider = null;
                }
                if (collider == null && (data.type != ColliderType.None || data.type != ColliderType.Original))
                {
                    collider = new GameObject("Collider").transform;
                    collider.parent = render.transform;
                }
                switch (data.type)
                {
                    case ColliderType.None:
                        break;
                    case ColliderType.Box:
                        BoxCollider box = collider.gameObject.AddComponent<BoxCollider>();
                        box.material = data.physicMateria.ToPhysicMaterial();
                        break;
                    case ColliderType.Sphere:
                        SphereCollider sphere = collider.gameObject.AddComponent<SphereCollider>();
                        sphere.radius = data.radius;
                        sphere.material = data.physicMateria.ToPhysicMaterial();
                        break;
                    case ColliderType.Capsule:
                        CapsuleCollider capsule = collider.gameObject.AddComponent<CapsuleCollider>();
                        capsule.radius = data.radius;
                        capsule.height = data.height;
                        capsule.direction = data.direction;
                        capsule.material = data.physicMateria.ToPhysicMaterial();
                        break;
                    case ColliderType.Mesh:
                        MeshCollider mesh = collider.gameObject.AddComponent<MeshCollider>();
                        mesh.sharedMesh = render.GetComponent<MeshFilter>().sharedMesh;
                        mesh.convex = true;
                        mesh.material = data.physicMateria.ToPhysicMaterial();
                        break;
                    case ColliderType.Original:
                        break;
                }
                collider.localPosition = new Vector3(data.position[0], data.position[1], data.position[2]);
                collider.localEulerAngles = new Vector3(data.rotation[0], data.rotation[1], data.rotation[2]);
                collider.localScale = new Vector3(data.scale[0], data.scale[1], data.scale[2]);
            }
        }
        public override void AnalysisData(string type, object[] data)
        {
            switch (type)
            {
                case "EnabledAllCollider":
                    EnabledAllCollider((bool)data[0]);
                    return;
                case "SetPhysicMaterial":
                    SetPhysicMaterial((float)data[0], (float)data[1], (float)data[2], (int)data[3], (int)data[4]);
                    return;
                case "SetRFMoveColliderActive":
                    SetRFMoveColliderActive((bool)data[0]);
                    return;
                case "GenerateVHACDColider":
                    GenerateVHACDCollider();
                    return;
            }
            base.AnalysisData(type, data);
        }

        public List<Mesh> GenerateVHACDCollider()
        {
            List<Mesh> meshAssets = new List<Mesh>();
            foreach (var item in this.GetChildComponentFilter<MeshRenderer>())
            {
                foreach (var des in item.GetComponents<Collider>())
                {
                    DestroyImmediate(des, true);
                }
                Mesh sourceMesh = item.GetComponent<MeshFilter>().sharedMesh;
                List<Mesh> meshs = VHACD.GenerateConvexMeshes(sourceMesh);
                meshAssets = meshAssets.Union(meshs).ToList();
                Transform child = item.transform.Find("Collider");
                if (child != null)
                {
                    DestroyImmediate(child.gameObject);
                }
                child = new GameObject("Collider").transform;
                child.parent = item.transform;
                child.localPosition = Vector3.zero;
                child.localEulerAngles = Vector3.zero;
                child.localScale = Vector3.one;
                foreach (var i in meshs)
                {
                    MeshCollider col = child.gameObject.AddComponent<MeshCollider>();
                    col.sharedMesh = i;
                    col.convex = true;
                }
            }
            return meshAssets;
        }

        public void EnabledAllCollider(bool enabled)
        {
            foreach (var item in this.GetChildComponentFilter<Collider>())
            {
                if (!item.isTrigger)
                    item.enabled = enabled;
            }
        }
        public void SetPhysicMaterial(float bounciness, float dynamicFriction, float staticFriction, int frictionCombine, int bounceCombine)
        {
            PhysicMaterial material = new PhysicMaterial
            {
                bounciness = bounciness,
                dynamicFriction = dynamicFriction,
                staticFriction = staticFriction,
                frictionCombine = (PhysicMaterialCombine)frictionCombine,
                bounceCombine = (PhysicMaterialCombine)bounceCombine
            };
            foreach (var item in this.GetChildComponentFilter<Collider>())
            {
                if (!item.isTrigger)
                    item.material = material;
            }
        }

        protected void SetRFMoveColliderActive(bool active)
        {
            IsRFMoveCollider = active;
        }

#if UNITY_EDITOR
        public static void SaveMeshs(string path, List<Mesh> meshs)
        {
            if (!path.EndsWith(".asset"))
                path += ".asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(new Mesh(), path);
            foreach (var i in meshs)
            {
                AssetDatabase.AddObjectToAsset(i, path);
            }
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ColliderAttr), true)]
    public class ColliderAttrEditor : Editor
    {
        string text = "";
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ColliderAttr script = target as ColliderAttr;
            GUILayout.Space(10);
            GUILayout.Label("Editor Tool:");
            GUILayout.BeginHorizontal();
            text = GUILayout.TextField(text);
            if (GUILayout.Button("Generate VHACD Collider"))
            {
                if (string.IsNullOrWhiteSpace(text)) return;
                string path = $"BuiltinAssets/Model/VHACD_Mesh/{text}_VHACD.asset";
                AssetDatabase.DeleteAsset($"Assets/{path}");
                List<Mesh> meshs = script.GenerateVHACDCollider();
                foreach (var i in meshs)
                {
                    if (!System.IO.File.Exists($"{Application.dataPath}/{path}"))
                    {
                        AssetDatabase.CreateAsset(i, $"Assets/{path}");
                    }
                    else
                    {
                        AssetDatabase.AddObjectToAsset(i, $"Assets/{path}");
                    }
                }
                AssetDatabase.Refresh();
                EditorUtility.SetDirty(script);
            }
            GUILayout.EndHorizontal();
        }
    }
#endif
}

