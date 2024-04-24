using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using DG.Tweening;
using RFUniverse.Manager;

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
    public class BaseAttr : MonoBehaviour, IReceiveData, IHaveAPI, ICollectData
    {
        public static Dictionary<int, BaseAttr> Attrs => InstanceManager.Instance.Attrs;
        public static Dictionary<int, BaseAttr> ActiveAttrs => InstanceManager.Instance.ActiveAttrs;

        CreateReference createReference;
        public CreateReference CreateReference
        {
            get { return createReference; }
            set
            {
                if (value == createReference)
                    return;
                if (value == null)
                    createReference.Remove(this);
                createReference = value;
                if (createReference != null)
                    createReference.Add(this);
            }
        }
        public ICollectData CollectData => this;

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
            (this as IHaveAPI).RegisterAPI();

            if (Attrs.ContainsKey(ID))
                Debug.LogError($"ID:{ID} Name:{Name} exist");
            else
            {
                Debug.Log($"Rigister ID:{ID} Name:{Name}");
                InstanceManager.Instance.AddAttr(this, (this as IReceiveData).ReceiveData);
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
            if (parents.Count > 0)
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

        void OnDestroy()
        {
            InstanceManager.Instance.RemoveAttr(this);
        }

        public virtual void AddPermanentData(Dictionary<string, object> data)
        {
            data["name"] = Name;
            data["position"] = transform.position;
            data["rotation"] = transform.eulerAngles;
            data["quaternion"] = transform.rotation;
            data["local_position"] = transform.localPosition;
            data["local_rotation"] = transform.localEulerAngles;
            data["local_quaternion"] = transform.localRotation;
            data["local_to_world_matrix"] = transform.localToWorldMatrix;
            data["move_done"] = moveDone;
            data["rotate_done"] = rotateDone;
        }
        Dictionary<string, object> ICollectData.TemporaryData { get; set; }

        void IReceiveData.ReceiveData(object[] data)
        {
            string hand = (string)data[0];
            data = data.Skip(1).ToArray();
            (this as IHaveAPI).CallAPI(hand, data);
        }

        [RFUAPI]
        public void SetTransform(List<float> position, List<float> rotation, List<float> scale, bool worldSpace = true)
        {
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

        [RFUAPI]
        protected virtual void SetPosition(List<float> position, bool worldSpace = true)
        {
            SetPosition(new Vector3(position[0], position[1], position[2]), worldSpace);
        }
        public virtual void SetPosition(Vector3 position, bool worldSpace = true)
        {
            if (worldSpace)
                transform.position = position;
            else
                transform.localPosition = position;
        }
        [RFUAPI]
        protected virtual void SetRotation(List<float> rotation, bool worldSpace = true)
        {
            SetRotation(new Vector3(rotation[0], rotation[1], rotation[2]), worldSpace);
        }
        public virtual void SetRotation(Vector3 rotation, bool worldSpace = true)
        {
            if (worldSpace)
                transform.eulerAngles = rotation;
            else
                transform.localEulerAngles = rotation;
        }
        [RFUAPI]
        public virtual void SetRotationQuaternion(List<float> quaternion, bool worldSpace = true)
        {
            SetRotation(new Quaternion(quaternion[0], quaternion[1], quaternion[2], quaternion[3]).eulerAngles, worldSpace);
        }
        [RFUAPI]
        protected virtual void SetScale(List<float> scale)
        {
            SetScale(new Vector3(scale[0], scale[1], scale[2]));
        }
        public virtual void SetScale(Vector3 scale)
        {
            transform.localScale = scale;
        }
        [RFUAPI]
        public virtual void Translate(List<float> translate, bool worldSpace)
        {
            Space space = worldSpace ? Space.World : Space.Self;
            transform.Translate(new Vector3(translate[0], translate[1], translate[2]), space);
        }
        [RFUAPI]
        public virtual void Rotate(List<float> translate, bool worldSpace)
        {
            Space space = worldSpace ? Space.World : Space.Self;
            transform.Rotate(new Vector3(0, 0, 1), translate[2], space);
            transform.Rotate(new Vector3(1, 0, 0), translate[0], space);
            transform.Rotate(new Vector3(0, 1, 0), translate[1], space);
        }
        [RFUAPI]
        public virtual void LookAt(List<float> target, List<float> worldUp)
        {
            transform.LookAt(RFUniverseUtility.ListFloatToVector3(target), RFUniverseUtility.ListFloatToVector3(worldUp));
        }
        [RFUAPI]
        public virtual void SetParent(int parentID, string parentName)
        {
            Transform parent = null;
            if (Attrs.TryGetValue(parentID, out BaseAttr attr))
            {
                parent = attr.transform.FindChlid(parentName, true);
                if (parent == null)
                    parent = attr.transform;
            }
            transform.SetParent(parent);
        }
        [RFUAPI]
        protected virtual void SetActive(bool active)
        {
            InstanceManager.Instance.OnAttrChange?.Invoke();
            gameObject.SetActive(active);
        }
        [RFUAPI]
        public void SetLayer(int layer)
        {
            foreach (var item in this.GetChildComponentFilter<Transform>())
            {
                item.gameObject.layer = layer;
            }
        }
        [RFUAPI]
        public BaseAttr Copy(int newID)
        {
            GameObject copy = GameObject.Instantiate(gameObject);
            BaseAttr attr = copy.GetComponent<BaseAttr>();
            attr.CreateReference = CreateReference;
            attr.ID = newID;
            attr.Instance();
            return attr;
        }
        [RFUAPI]
        public virtual void Destroy()
        {
            CreateReference = null;
            DestroyImmediate(gameObject);
        }

        [RFUAPI]
        void GetLocalPointFromWorld(List<float> world)
        {
            CollectData.AddDataNextStep("result_local_point", transform.InverseTransformPoint(RFUniverseUtility.ListFloatToVector3(world)));
        }

        [RFUAPI]
        void GetWorldPointFromLocal(List<float> local)
        {
            CollectData.AddDataNextStep("result_world_point", transform.TransformPoint(RFUniverseUtility.ListFloatToVector3(local)));
        }

        protected bool moveDone = true;
        protected bool rotateDone = true;

        [RFUAPI]
        protected virtual void DoMove(List<float> position, float duration, bool isSpeedBased, bool isRelative)
        {
            moveDone = false;
            Vector3 pos = new Vector3(position[0], position[1], position[2]);
            transform.DOMove(pos, duration).SetSpeedBased(isSpeedBased).SetEase(Ease.Linear).SetRelative(isRelative).onComplete += () =>
            {
                moveDone = true;
            };
        }
        [RFUAPI]
        protected virtual void DoRotate(List<float> eulerAngle, float duration, bool isSpeedBased, bool isRelative)
        {
            Quaternion target = Quaternion.Euler(eulerAngle[0], eulerAngle[1], eulerAngle[2]);
            DoRotateQuaternion(target, duration, isSpeedBased, isRelative);
        }
        [RFUAPI]
        protected virtual void DoRotateQuaternion(List<float> quaternion, float duration, bool isSpeedBased, bool isRelative)
        {
            Quaternion target = new Quaternion(quaternion[0], quaternion[1], quaternion[2], quaternion[3]);
            DoRotateQuaternion(target, duration, isSpeedBased, isRelative);
        }
        protected virtual void DoRotateQuaternion(Quaternion target, float duration, bool isSpeedBased = true, bool isRelative = false)
        {
            rotateDone = false;
            transform.DORotateQuaternion(target, duration).SetSpeedBased(isSpeedBased).SetEase(Ease.Linear).SetRelative(isRelative).onComplete += () =>
            {
                rotateDone = true;
            };
        }
        [RFUAPI]
        protected virtual void DoComplete()
        {
            transform.DOComplete();
            moveDone = true;
            rotateDone = true;
        }
        [RFUAPI]
        protected virtual void DoKill()
        {
            transform.DOKill();
            moveDone = true;
            rotateDone = true;
        }

    }


}
