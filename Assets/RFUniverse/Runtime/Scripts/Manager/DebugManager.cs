using System.Collections.Generic;
using RFUniverse.Attributes;
using RFUniverse.DebugTool;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;

namespace RFUniverse.Manager
{
    public class DebugManager : IDisposable
    {
        static DebugManager instance = null;

        public static DebugManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new DebugManager();
                return instance;
            }
        }

        GraspPoint graspPointSource;
        PoseGizmo poseGizmoSource;
        CollisionLine collisionLineSource;
        ObjectID objectIDSource;
        ColliderBound colliderBoundSource;
        DDDBBox dddBBoxSource;
        DDBBox ddBBoxSource;
        JointLink jointLinkSource;

        ObjectPool<GraspPoint> graspPointPool;
        ObjectPool<PoseGizmo> poseGizmoPool;
        ObjectPool<CollisionLine> collisionLinePool;
        ObjectPool<ObjectID> objectIDPool;
        ObjectPool<ColliderBound> colliderBoundPool;
        ObjectPool<DDDBBox> dddBBoxPool;
        ObjectPool<DDBBox> ddBBoxPool;
        ObjectPool<JointLink> jointLinkPool;
        private DebugManager()
        {
            graspPointSource = Addressables.LoadAssetAsync<GameObject>("Debug/GraspPoint").WaitForCompletion().GetComponent<GraspPoint>();
            poseGizmoSource = Addressables.LoadAssetAsync<GameObject>("Debug/PoseGizmo").WaitForCompletion().GetComponent<PoseGizmo>();
            collisionLineSource = Addressables.LoadAssetAsync<GameObject>("Debug/CollisionLine").WaitForCompletion().GetComponent<CollisionLine>();
            objectIDSource = Addressables.LoadAssetAsync<GameObject>("Debug/ObjectID").WaitForCompletion().GetComponent<ObjectID>();
            colliderBoundSource = Addressables.LoadAssetAsync<GameObject>("Debug/ColliderBound").WaitForCompletion().GetComponent<ColliderBound>();
            dddBBoxSource = Addressables.LoadAssetAsync<GameObject>("Debug/3DBBox").WaitForCompletion().GetComponent<DDDBBox>();
            ddBBoxSource = Addressables.LoadAssetAsync<GameObject>("Debug/2DBBox").WaitForCompletion().GetComponent<DDBBox>();
            jointLinkSource = Addressables.LoadAssetAsync<GameObject>("Debug/JointLink").WaitForCompletion().GetComponent<JointLink>();

            graspPointPool = new ObjectPool<GraspPoint>(() => GameObject.Instantiate(graspPointSource), s => s.gameObject.SetActive(true), s => s.gameObject.SetActive(false), s => GameObject.Destroy(s.gameObject));
            poseGizmoPool = new ObjectPool<PoseGizmo>(() => GameObject.Instantiate(poseGizmoSource), s => s.gameObject.SetActive(true), s => s.gameObject.SetActive(false), s => GameObject.Destroy(s.gameObject));
            collisionLinePool = new ObjectPool<CollisionLine>(() => GameObject.Instantiate(collisionLineSource), s => s.gameObject.SetActive(true), s => s.gameObject.SetActive(false), s => GameObject.Destroy(s.gameObject));
            objectIDPool = new ObjectPool<ObjectID>(() => GameObject.Instantiate(objectIDSource), s => s.gameObject.SetActive(true), s => s.gameObject.SetActive(false), s => GameObject.Destroy(s.gameObject));
            colliderBoundPool = new ObjectPool<ColliderBound>(() => GameObject.Instantiate(colliderBoundSource), s => s.gameObject.SetActive(true), s => s.gameObject.SetActive(false), s => GameObject.Destroy(s.gameObject));
            dddBBoxPool = new ObjectPool<DDDBBox>(() => GameObject.Instantiate(dddBBoxSource), s => s.gameObject.SetActive(true), s => s.gameObject.SetActive(false), s => GameObject.Destroy(s.gameObject));
            ddBBoxPool = new ObjectPool<DDBBox>(() => GameObject.Instantiate(ddBBoxSource), s => s.gameObject.SetActive(true), s => s.gameObject.SetActive(false), s => GameObject.Destroy(s.gameObject));
            jointLinkPool = new ObjectPool<JointLink>(() => GameObject.Instantiate(jointLinkSource), s => s.gameObject.SetActive(true), s => s.gameObject.SetActive(false), s => GameObject.Destroy(s.gameObject));

            Application.logMessageReceived += OnLogMessageReceived;
        }
        void IDisposable.Dispose()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }
        void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (!Application.isPlaying) return;
            SendLog(type.ToString(), condition, stackTrace);
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    AddLog($"{condition}\n{stackTrace}", type);
                    break;
                case LogType.Warning:
                case LogType.Log:
                    AddLog($"{condition}", type);
                    break;
            }
        }

        private void SendLog(params string[] strings)
        {
            object[] data = new[] { "Debug", "Log" };
            PlayerMain.Communicator?.SendObject(data.Concat(strings).ToArray());
        }

        //Queue<string> logList = new Queue<string>();
        public void AddLog<T>(T log, LogType type = LogType.Log)
        {
            string richColor = "#FFFFFF";
            switch (type)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    richColor = "red";
                    break;
                case LogType.Warning:
                    richColor = "#FF9300";
                    break;
                case LogType.Log:
                default:
                    break;
            }
            //logList.Enqueue($"{System.DateTime.Now.ToString("<color=#BBBBBB><b>[HH:mm:ss]</b></color>")} <color={richColor}>{log}</color>");
            //if (logList.Count > 50)
            //    logList.Dequeue();
            //PlayerMain.Instance.playerMainUI.RefreshLogList(logList.ToArray());
            PlayerMain.Instance.playerMainUI.AddLog($"{System.DateTime.Now.ToString("<color=#BBBBBB><b>[HH:mm:ss]</b></color>")} <color={richColor}>{log}</color>");
        }

        public void ReceiveDebugData(object[] data)
        {
            string type = (string)data[0];
            data = data.Skip(1).ToArray();
            switch (type)
            {
                case "DebugGraspPoint":
                    IsDebugGraspPoint = (bool)data[0];
                    break;
                case "DebugObjectPose":
                    IsDebugObjectPose = (bool)data[0];
                    break;
                case "DebugCollisionPair":
                    IsDebugCollisionPair = (bool)data[0];
                    break;
                case "DebugColliderBound":
                    IsDebugColliderBound = (bool)data[0];
                    break;
                case "Debug3DBBox":
                    IsDebug3DBBox = (bool)data[0];
                    break;
                case "Debug2DBBox":
                    IsDebug2DBBox = (bool)data[0];
                    break;
                case "DebugObjectID":
                    IsDebugObjectID = (bool)data[0];
                    break;
                case "DebugJointLink":
                    IsDebugJointLink = (bool)data[0];
                    break;
                case "SendLog":
                    AddLog((string)data[0]);
                    break;
                case "SetPythonVersion":
                    SetPythonVersion((string)data[0]);
                    break;
                case "ShowArticulationParameter":
                    ShowArticulationParameter((int)data[0]);
                    return;
                default:
                    Debug.LogWarning("Dont have mehond:" + type);
                    break;
            }
        }

        public void ShowArticulationParameter(int controllerID)
        {
            Debug.Log($"ShowArticulationParameter ID: {controllerID}");
            if (BaseAttr.Attrs.TryGetValue(controllerID, out BaseAttr target) && target is ControllerAttr)
            {
                PlayerMain.Instance.playerMainUI.ShowArticulationParameter((target as ControllerAttr).moveableJoints);
            }
            else
            {
                Debug.LogWarning($"ID {controllerID} is Not Exists or Not ControllerAttr");
            }
        }

        bool isDebugGraspPoint = false;
        public bool IsDebugGraspPoint
        {
            get
            {
                return isDebugGraspPoint;
            }
            set
            {
                if (value == isDebugGraspPoint) return;
                isDebugGraspPoint = value;
                if (isDebugGraspPoint)
                {
                    BaseAttr.OnAttrChange += RefreshGraspPoint;
                    RefreshGraspPoint();
                }
                else
                {
                    BaseAttr.OnAttrChange -= RefreshGraspPoint;
                }
            }
        }


        Dictionary<ControllerAttr, GraspPoint> graspPoints = new Dictionary<ControllerAttr, GraspPoint>();
        void RefreshGraspPoint()
        {
            if (graspPointSource == null) return;
            List<ControllerAttr> current = BaseAttr.ActiveAttrs.Where(s => (s.Value is ControllerAttr) && (s.Value as ControllerAttr).joints.Count > 0).Select(s => s.Value as ControllerAttr).ToList();
            foreach (var item in current.Except(graspPoints.Keys))
            {
                GraspPoint instance = graspPointPool.Get();
                graspPoints.Add(item, instance);
                instance.target = item.graspPoint;
            }
            foreach (var item in graspPoints.Keys.Except(current))
            {
                graspPointPool.Release(graspPoints[item]);
                graspPoints.Remove(item);
            }
        }

        bool isDebugObjectPose = false;
        public bool IsDebugObjectPose
        {
            get
            {
                return isDebugObjectPose;
            }
            set
            {
                if (value == isDebugObjectPose) return;
                isDebugObjectPose = value;
                if (isDebugObjectPose)
                {
                    BaseAttr.OnAttrChange += RefreshObjectPose;
                    RefreshObjectPose();
                }
                else
                {
                    BaseAttr.OnAttrChange -= RefreshObjectPose;
                }
            }
        }

        Dictionary<BaseAttr, PoseGizmo> poseGizmos = new Dictionary<BaseAttr, PoseGizmo>();
        void RefreshObjectPose()
        {
            if (poseGizmoSource == null) return;
            List<BaseAttr> current = BaseAttr.ActiveAttrs.Values.ToList();
            foreach (var item in current.Except(poseGizmos.Keys))
            {
                PoseGizmo instance = poseGizmoPool.Get();
                poseGizmos.Add(item, instance);
                instance.target = item.transform;
            }
            foreach (var item in poseGizmos.Keys.Except(current))
            {
                poseGizmoPool.Release(poseGizmos[item]);
                poseGizmos.Remove(item);
            }
        }


        bool isDebugCollisionPair = false;
        public bool IsDebugCollisionPair
        {
            get
            {
                return isDebugCollisionPair;
            }
            set
            {
                if (value == isDebugCollisionPair) return;
                isDebugCollisionPair = value;
                if (isDebugCollisionPair)
                {
                    BaseAttr.OnCollisionPairsChange += RefreshCollisionPair;
                    RefreshCollisionPair();
                }
                else
                {
                    BaseAttr.OnCollisionPairsChange -= RefreshCollisionPair;
                }
            }
        }

        List<CollisionLine> collisionLines = new List<CollisionLine>();
        void RefreshCollisionPair()
        {
            if (collisionLineSource == null) return;
            foreach (var item in collisionLines)
            {
                collisionLinePool.Release(item);
            }
            collisionLines.Clear();
            foreach (var item in BaseAttr.CollisionPairs)
            {
                CollisionLine instance = collisionLinePool.Get();
                collisionLines.Add(instance);
                instance.point1 = BaseAttr.Attrs[item.Item1].transform;
                instance.point2 = BaseAttr.Attrs[item.Item2].transform;
            }
        }



        bool isDebugColliderBound = false;
        public bool IsDebugColliderBound
        {
            get
            {
                return isDebugColliderBound;
            }
            set
            {
                if (value == isDebugColliderBound) return;
                isDebugColliderBound = value;
                if (isDebugColliderBound)
                {
                    BaseAttr.OnAttrChange += RefreshColliderBound;
                    RefreshColliderBound();
                }
                else
                {
                    BaseAttr.OnAttrChange -= RefreshColliderBound;
                }
            }
        }

        Dictionary<BaseAttr, List<ColliderBound>> colliderBounds = new Dictionary<BaseAttr, List<ColliderBound>>();
        void RefreshColliderBound()
        {
            if (colliderBoundSource == null) return;
            List<BaseAttr> current = BaseAttr.ActiveAttrs.Values.ToList();
            foreach (var item in current.Except(colliderBounds.Keys))
            {
                List<ColliderBound> colliders = new List<ColliderBound>();
                foreach (Collider col in item.GetChildComponentFilter<Collider>())
                {
                    ColliderBound instance = colliderBoundPool.Get();
                    colliders.Add(instance);
                    instance.Collider = col;
                }
                colliderBounds.Add(item, colliders);
            }
            foreach (var item in colliderBounds.Keys.Except(current))
            {
                foreach (var i in colliderBounds[item])
                {
                    colliderBoundPool.Release(i);
                }
                colliderBounds.Remove(item);
            }
        }


        bool isDebugObjectID = false;
        public bool IsDebugObjectID
        {
            get
            {
                return isDebugObjectID;
            }
            set
            {
                if (value == isDebugObjectID) return;
                isDebugObjectID = value;
                if (isDebugObjectID)
                {
                    BaseAttr.OnAttrChange += RefreshObjectID;
                    RefreshObjectID();
                }
                else
                {
                    BaseAttr.OnAttrChange -= RefreshObjectID;
                }
            }
        }


        Dictionary<BaseAttr, ObjectID> objectIDs = new Dictionary<BaseAttr, ObjectID>();
        void RefreshObjectID()
        {
            if (objectIDSource == null) return;
            List<BaseAttr> current = BaseAttr.Attrs.Values.ToList();
            foreach (var item in current.Except(objectIDs.Keys))
            {
                ObjectID instance = objectIDPool.Get();
                objectIDs.Add(item, instance);
                instance.target = item;
            }
            foreach (var item in objectIDs.Keys.Except(current))
            {
                objectIDPool.Release(objectIDs[item]);
                objectIDs.Remove(item);
            }
        }


        bool isDebugJointLink = false;
        public bool IsDebugJointLink
        {
            get
            {
                return isDebugJointLink;
            }
            set
            {
                if (value == isDebugJointLink) return;
                isDebugJointLink = value;
                if (isDebugJointLink)
                {
                    BaseAttr.OnAttrChange += RefreshJointLink;
                    RefreshJointLink();
                }
                else
                {
                    BaseAttr.OnAttrChange -= RefreshJointLink;
                }
            }
        }

        Dictionary<ControllerAttr, JointLink> jointLinks = new Dictionary<ControllerAttr, JointLink>();
        void RefreshJointLink()
        {
            if (jointLinkSource == null) return;
            List<ControllerAttr> current = BaseAttr.ActiveAttrs.Where(s => (s.Value is ControllerAttr) && (s.Value as ControllerAttr).joints.Count > 0).Select(s => s.Value as ControllerAttr).ToList();
            foreach (var item in current.Except(jointLinks.Keys))
            {
                JointLink instance = jointLinkPool.Get();
                jointLinks.Add(item, instance);
                instance.Target = item;
            }
            foreach (var item in jointLinks.Keys.Except(current))
            {
                jointLinkPool.Release(jointLinks[item]);
                jointLinks.Remove(item);
            }
        }


        bool isDebug3DBBox = false;
        public bool IsDebug3DBBox
        {
            get
            {
                return isDebug3DBBox;
            }
            set
            {
                if (value == isDebug3DBBox) return;
                isDebug3DBBox = value;
                if (isDebug3DBBox)
                {
                    BaseAttr.OnAttrChange += Refresh3DBBox;
                    Refresh3DBBox();
                }
                else
                {
                    BaseAttr.OnAttrChange -= Refresh3DBBox;
                }
            }
        }

        Dictionary<GameObjectAttr, DDDBBox> dddBBoxs = new Dictionary<GameObjectAttr, DDDBBox>();
        void Refresh3DBBox()
        {
            if (dddBBoxSource == null) return;
            List<GameObjectAttr> current = BaseAttr.ActiveAttrs.Where(s => s.Value is GameObjectAttr).Select(s => s.Value as GameObjectAttr).ToList();
            foreach (var item in current.Except(dddBBoxs.Keys))
            {
                DDDBBox instance = dddBBoxPool.Get();
                dddBBoxs.Add(item, instance);
                instance.target = item;
            }
            foreach (var item in dddBBoxs.Keys.Except(current))
            {
                dddBBoxPool.Release(dddBBoxs[item]);
                dddBBoxs.Remove(item);
            }
        }


        bool isDebug2DBBox = false;
        public bool IsDebug2DBBox
        {
            get
            {
                return isDebug2DBBox;
            }
            set
            {
                if (value == isDebug2DBBox) return;
                isDebug2DBBox = value;
                if (isDebug2DBBox)
                {
                    BaseAttr.OnAttrChange += Refresh2DBBox;
                    Refresh2DBBox();
                }
                else
                {
                    BaseAttr.OnAttrChange -= Refresh2DBBox;
                }
            }
        }

        Dictionary<GameObjectAttr, DDBBox> ddBBoxs = new Dictionary<GameObjectAttr, DDBBox>();
        void Refresh2DBBox()
        {
            if (ddBBoxSource == null) return;
            List<GameObjectAttr> current = BaseAttr.ActiveAttrs.Where(s => s.Value is GameObjectAttr).Select(s => s.Value as GameObjectAttr).ToList();
            foreach (var item in current.Except(ddBBoxs.Keys))
            {
                DDBBox instance = ddBBoxPool.Get();
                ddBBoxs.Add(item, instance);
                instance.target = item;
            }
            foreach (var item in ddBBoxs.Keys.Except(current))
            {
                ddBBoxPool.Release(ddBBoxs[item]);
                ddBBoxs.Remove(item);
            }
        }

        public void SetPythonVersion(string version)
        {
            PlayerMain.Instance.playerMainUI.SetPythonVersion(new Version(version));
        }
    }
}
