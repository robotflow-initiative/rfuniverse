using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RFUniverse.Attributes
{
    [Serializable]
    public class ColliderAttrData : BaseAttrData
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
    }
    [Serializable]
    public class ColliderData
    {
        public string meshName = "";
        public ColliderType type = ColliderType.Original;
        public float[] position = { 0, 0, 0 };
        public float[] rotation = { 0, 0, 0 };
        public float[] scale = { 1, 1, 1 };
        public float radius = 1;
        public float height = 1;
        public int direction = 0;
        public PhysicMaterialData physicMateria;
    }
    [Serializable]
    public class PhysicMaterialData
    {
        public float bounciness;
        public float dynamicFriction;
        public float staticFriction;
        public PhysicMaterialCombine frictionCombine;
        public PhysicMaterialCombine bounceCombine;
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
    public abstract class ColliderAttr : BaseAttr
    {
        [SerializeField]
        private List<ColliderData> colliderDatas;

        [Attr("Colliders")]
        public List<ColliderData> ColliderDatas
        {
            get
            {
                return colliderDatas;
            }
            set
            {
                colliderDatas = value;
            }
        }

        public override BaseAttrData GetAttrData()
        {
            ColliderAttrData data = new ColliderAttrData(base.GetAttrData());
            data.colliderDatas = ColliderDatas;
            return data;
        }
        public override void SetAttrData(BaseAttrData setData)
        {
            base.SetAttrData(setData);
            if (setData is ColliderAttrData)
            {
                ColliderAttrData data = setData as ColliderAttrData;
                ColliderDatas = data.colliderDatas;
                SetColliderDatas(data.colliderDatas);
            }
        }
        public List<ColliderData> GetColliderDatas()
        {
            List<ColliderData> datas = new List<ColliderData>();
            List<MeshRenderer> renderer = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());
            foreach (var item in renderer)
            {
                ColliderData data = new ColliderData();
                datas.Add(data);
                data.meshName = item.gameObject.name;
                Transform child = item.transform.Find("Collider");
                if (child == null)
                    data.type = ColliderType.None;
                else
                {
                    Component[] com = child.GetComponents<Component>();
                    if (com.Length == 1)
                    {
                        data.type = ColliderType.None;
                    }
                    else if (com.Length > 2 || !(com[1] is Collider))
                    {
                        data.type = ColliderType.Original;
                    }
                    else
                    {
                        Collider collider = com[1] as Collider;

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
        private void SetColliderDatas(List<ColliderData> datas)
        {
            foreach (var data in datas)
            {
                Transform render = FindChlid(transform, data.meshName, false);
                if (render == null) continue;
                Transform collider = render.Find("Collider");
                if (collider != null && data.type != ColliderType.Original)
                {
                    Destroy(collider.gameObject);
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
            GUILayout.BeginHorizontal();
            text = GUILayout.TextField(text);
            if (GUILayout.Button("Generate VHACD Collider"))
            {
                if (string.IsNullOrEmpty(text)) return;
                string path = $"Assets/Model/VHACD_Mesh/{text}_VHACD.asset";
                AssetDatabase.DeleteAsset(path);
                List<MeshRenderer> renderer = new List<MeshRenderer>(script.GetComponentsInChildren<MeshRenderer>());
                foreach (var item in renderer)
                {
                    foreach (var des in item.GetComponents<Collider>())
                    {
                        DestroyImmediate(des, true);
                    }
                    Mesh sourceMesh = item.GetComponent<MeshFilter>().sharedMesh;
                    List<Mesh> meshs = VHACD.GenerateConvexMeshes(sourceMesh);
                    Transform child = item.transform.Find("Collider");
                    if (child == null)
                    {
                        child = new GameObject("Collider").transform;
                        child.parent = item.transform;
                    }
                    else
                    {
                        foreach (var collider in child.GetComponents<Collider>())
                        {
                            DestroyImmediate(collider);
                        }
                    }
                    child.localPosition = Vector3.zero;
                    child.localEulerAngles = Vector3.zero;
                    child.localScale = Vector3.one;
                    for (int i = 0; i < child.childCount; i++)
                    {
                        Destroy(child.GetChild(i).gameObject);
                    }
                    foreach (var i in meshs)
                    {
                        if (!System.IO.File.Exists($"{Application.dataPath}/Model/VHACD_Mash/{text}_VHACD.asset"))
                        {
                            AssetDatabase.CreateAsset(i, path);
                        }
                        else
                        {
                            AssetDatabase.AddObjectToAsset(i, path);
                        }
                        MeshCollider col = child.gameObject.AddComponent<MeshCollider>();
                        col.sharedMesh = i;
                        col.convex = true;
                    }
                    AssetDatabase.ImportAsset(path);
                    EditorUtility.SetDirty(script);
                }
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Get Collider Datas"))
            {
                script.ColliderDatas = script.GetColliderDatas();
                EditorUtility.SetDirty(script);
            }
        }
    }
#endif
}

