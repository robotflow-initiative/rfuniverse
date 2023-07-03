using System.Collections.Generic;
using RFUniverse.Attributes;
using RFUniverse.DebugTool;
using UnityEngine;
using System;
using System.Linq;

namespace RFUniverse.Manager
{
    public class DebugManager
    {

        private static DebugManager instance = null;
        public static DebugManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DebugManager();
                }
                return instance;
            }
        }
        public void ReceiveDebugData(object[] data)
        {
            string type = (string)data[0];
            data = data.Skip(1).ToArray();
            switch (type)
            {
                case "DebugGraspPoint":
                    DebugGraspPoint((bool)data[0]);
                    break;
                case "DebugObjectPose":
                    DebugObjectPose((bool)data[0]);
                    break;
                case "DebugCollisionPair":
                    DebugCollisionPair((bool)data[0]);
                    break;
                case "DebugColliderBound":
                    DebugColliderBound((bool)data[0]);
                    break;
                case "Debug3DBBox":
                    Debug3DBBox((bool)data[0]);
                    break;
                case "Debug2DBBox":
                    Debug2DBBox((bool)data[0]);
                    break;
                case "DebugObjectID":
                    DebugObjectID((bool)data[0]);
                    break;
                case "DebugJointLink":
                    DebugJointLink((bool)data[0]);
                    break;
                case "SendLog":
                    SendLog((string)data[0]);
                    break;
                case "SendVersion":
                    SendVersion((string)data[0]);
                    break;
                default:
                    Debug.Log("Dont have mehond:" + type);
                    break;
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
                if (value)
                {
                    if (graspPointSource == null)
                    {
                        graspPointSource = Resources.Load<GraspPoint>("GraspPoint");
                    }
                    BaseAttr.OnAttrChange += RefreshGraspPoint;
                    RefreshGraspPoint();
                }
                else
                {
                    BaseAttr.OnAttrChange -= RefreshGraspPoint;
                }

                isDebugGraspPoint = value;

                foreach (var item in graspPoints)
                {
                    item.Value.gameObject.SetActive(isDebugGraspPoint);
                }

            }
        }

        GraspPoint graspPointSource = null;
        public void DebugGraspPoint(bool active)
        {
            IsDebugGraspPoint = active;
        }
        Dictionary<int, GraspPoint> graspPoints = new Dictionary<int, GraspPoint>();
        void RefreshGraspPoint()
        {
            if (graspPointSource == null) return;
            List<int> current = BaseAttr.Attrs.Where(s => (s.Value is ControllerAttr) && (s.Value as ControllerAttr).joints.Count > 0).Select(s => s.Key).ToList();
            foreach (var item in current.Except(graspPoints.Keys))
            {
                GraspPoint instance = GameObject.Instantiate<GraspPoint>(graspPointSource);
                graspPoints.Add(item, instance);
                instance.target = (BaseAttr.Attrs[item] as ControllerAttr).graspPoint;
            }
            foreach (var item in graspPoints.Keys.Except(current))
            {
                GameObject.Destroy(graspPoints[item].gameObject);
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
                if (value)
                {
                    if (poseGizmoSource == null)
                    {
                        poseGizmoSource = Resources.Load<PoseGizmo>("PoseGizmo");
                    }
                    BaseAttr.OnAttrChange += RefreshObjectPose;
                    RefreshObjectPose();
                }
                else
                {
                    BaseAttr.OnAttrChange -= RefreshObjectPose;
                }

                isDebugObjectPose = value;

                foreach (var item in poseGizmos)
                {
                    item.Value.gameObject.SetActive(isDebugObjectPose);
                }

            }
        }
        PoseGizmo poseGizmoSource = null;
        public void DebugObjectPose(bool active)
        {
            IsDebugObjectPose = active;
        }
        Dictionary<int, PoseGizmo> poseGizmos = new Dictionary<int, PoseGizmo>();
        void RefreshObjectPose()
        {
            if (poseGizmoSource == null) return;
            List<int> current = BaseAttr.Attrs.Keys.ToList();
            foreach (var item in current.Except(poseGizmos.Keys))
            {
                PoseGizmo instance = GameObject.Instantiate<PoseGizmo>(poseGizmoSource);
                poseGizmos.Add(item, instance);
                instance.target = BaseAttr.Attrs[item].transform;
            }
            foreach (var item in poseGizmos.Keys.Except(current))
            {
                GameObject.Destroy(poseGizmos[item].gameObject);
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
                if (value)
                {
                    if (collisionLineSource == null)
                    {
                        collisionLineSource = Resources.Load<CollisionLine>("CollisionLine");
                    }
                    BaseAttr.OnCollisionPairsChange += RefreshCollisionPair;
                    RefreshCollisionPair();
                }
                else
                {
                    BaseAttr.OnCollisionPairsChange -= RefreshCollisionPair;
                }

                isDebugCollisionPair = value;

                foreach (var item in collisionLines)
                {
                    item.gameObject.SetActive(isDebugCollisionPair);
                }
            }
        }
        CollisionLine collisionLineSource = null;
        public void DebugCollisionPair(bool active)
        {
            IsDebugCollisionPair = active;
        }
        List<CollisionLine> collisionLines = new List<CollisionLine>();
        void RefreshCollisionPair()
        {
            if (collisionLineSource == null) return;
            foreach (var item in collisionLines)
            {
                item.gameObject.SetActive(false);
            }
            for (int i = 0; i < BaseAttr.CollisionPairs.Count; i++)
            {
                Tuple<int, int> pair = BaseAttr.CollisionPairs[i];
                if (collisionLines.Count <= i)
                    collisionLines.Add(GameObject.Instantiate<CollisionLine>(collisionLineSource));
                collisionLines[i].point1 = BaseAttr.Attrs[pair.Item1].transform;
                collisionLines[i].point2 = BaseAttr.Attrs[pair.Item2].transform;
                collisionLines[i].gameObject.SetActive(true);
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
                if (value)
                {
                    if (colliderBoundSource == null)
                    {
                        colliderBoundSource = Resources.Load<ColliderBound>("ColliderBound");
                    }
                    BaseAttr.OnAttrChange += RefreshColliderBound;
                    RefreshColliderBound();
                }
                else
                {
                    BaseAttr.OnAttrChange -= RefreshColliderBound;
                }
                isDebugColliderBound = value;
                foreach (var item in colliderBounds)
                {
                    foreach (var bound in item.Value)
                    {
                        bound.gameObject.SetActive(isDebugColliderBound);
                    }
                }
            }
        }

        ColliderBound colliderBoundSource = null;
        public void DebugColliderBound(bool active)
        {
            IsDebugColliderBound = active;
        }
        Dictionary<int, List<ColliderBound>> colliderBounds = new Dictionary<int, List<ColliderBound>>();
        void RefreshColliderBound()
        {
            if (colliderBoundSource == null) return;
            List<int> current = BaseAttr.Attrs.Keys.ToList();
            foreach (var item in current.Except(colliderBounds.Keys))
            {
                List<ColliderBound> colliders = new List<ColliderBound>();
                foreach (Collider col in BaseAttr.Attrs[item].GetChildComponentFilter<Collider>())
                {
                    ColliderBound instance = GameObject.Instantiate<ColliderBound>(colliderBoundSource);
                    colliders.Add(instance);
                    instance.Collider = col;
                }
                colliderBounds.Add(item, colliders);
            }
            foreach (var item in colliderBounds.Keys.Except(current))
            {
                foreach (var i in colliderBounds[item])
                {
                    GameObject.Destroy(i.gameObject);
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
                if (value)
                {
                    if (objectIDSource == null)
                    {
                        objectIDSource = Resources.Load<ObjectID>("ObjectID");
                    }
                    BaseAttr.OnAttrChange += RefreshObjectID;
                    RefreshObjectID();
                }
                else
                {
                    BaseAttr.OnAttrChange -= RefreshObjectID;
                }
                isDebugObjectID = value;
                foreach (var item in objectIDs)
                {
                    item.Value.gameObject.SetActive(isDebugObjectID);
                }

            }
        }

        ObjectID objectIDSource = null;
        public void DebugObjectID(bool active)
        {
            IsDebugObjectID = active;
        }
        Dictionary<int, ObjectID> objectIDs = new Dictionary<int, ObjectID>();
        void RefreshObjectID()
        {
            if (objectIDSource == null) return;
            List<int> current = BaseAttr.Attrs.Keys.ToList();
            foreach (var item in current.Except(objectIDs.Keys))
            {
                ObjectID instance = GameObject.Instantiate<ObjectID>(objectIDSource);
                objectIDs.Add(item, instance);
                instance.target = BaseAttr.Attrs[item];
            }
            foreach (var item in objectIDs.Keys.Except(current))
            {
                GameObject.Destroy(objectIDs[item].gameObject);
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
                if (value)
                {
                    if (jointLinkSource == null)
                    {
                        jointLinkSource = Resources.Load<JointLink>("JointLink");
                    }
                    BaseAttr.OnAttrChange += RefreshJointLink;
                    RefreshJointLink();
                }
                else
                {
                    BaseAttr.OnAttrChange -= RefreshJointLink;
                }
                isDebugJointLink = value;
                foreach (var item in jointLinks)
                {
                    item.Value.gameObject.SetActive(isDebugJointLink);
                }

            }
        }

        JointLink jointLinkSource = null;
        public void DebugJointLink(bool active)
        {
            IsDebugJointLink = active;
        }
        Dictionary<int, JointLink> jointLinks = new Dictionary<int, JointLink>();
        void RefreshJointLink()
        {
            if (jointLinkSource == null) return;
            List<int> current = BaseAttr.Attrs.Keys.ToList();
            foreach (var item in current.Except(jointLinks.Keys))
            {
                if (BaseAttr.Attrs[item] is ControllerAttr)
                {
                    JointLink instance = GameObject.Instantiate<JointLink>(jointLinkSource);
                    jointLinks.Add(item, instance);
                    instance.Target = BaseAttr.Attrs[item] as ControllerAttr;
                }
            }
            foreach (var item in jointLinks.Keys.Except(current))
            {
                GameObject.Destroy(jointLinks[item].gameObject);
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
                if (value)
                {
                    if (dddBBoxSource == null)
                    {
                        dddBBoxSource = Resources.Load<DDDBBox>("3DBBox");
                    }
                    BaseAttr.OnAttrChange += Refresh3DBBox;
                    Refresh3DBBox();
                }
                else
                {
                    BaseAttr.OnAttrChange -= Refresh3DBBox;
                }
                isDebug3DBBox = value;
                foreach (var item in dddBBoxs)
                {
                    item.Value.gameObject.SetActive(isDebug3DBBox);
                }

            }
        }

        DDDBBox dddBBoxSource = null;
        public void Debug3DBBox(bool active)
        {
            IsDebug3DBBox = active;
        }
        Dictionary<int, DDDBBox> dddBBoxs = new Dictionary<int, DDDBBox>();
        void Refresh3DBBox()
        {
            if (dddBBoxSource == null) return;
            List<int> current = BaseAttr.Attrs.Keys.ToList();
            foreach (var item in current.Except(dddBBoxs.Keys))
            {
                if (BaseAttr.Attrs[item] is GameObjectAttr)
                {
                    DDDBBox instance = GameObject.Instantiate<DDDBBox>(dddBBoxSource);
                    dddBBoxs.Add(item, instance);
                    instance.target = BaseAttr.Attrs[item] as GameObjectAttr;
                }
            }
            foreach (var item in dddBBoxs.Keys.Except(current))
            {
                GameObject.Destroy(dddBBoxs[item].gameObject);
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
                if (value)
                {
                    if (ddBBoxSource == null)
                    {
                        ddBBoxSource = Resources.Load<DDBBox>("2DBBox");
                    }
                    BaseAttr.OnAttrChange += Refresh2DBBox;
                    Refresh2DBBox();
                }
                else
                {
                    BaseAttr.OnAttrChange -= Refresh2DBBox;
                }
                isDebug2DBBox = value;
                foreach (var item in ddBBoxs)
                {
                    item.Value.gameObject.SetActive(isDebug2DBBox);
                }

            }
        }

        DDBBox ddBBoxSource = null;
        public void Debug2DBBox(bool active)
        {
            IsDebug2DBBox = active;
        }
        Dictionary<int, DDBBox> ddBBoxs = new Dictionary<int, DDBBox>();
        void Refresh2DBBox()
        {
            if (ddBBoxSource == null) return;
            List<int> current = BaseAttr.Attrs.Keys.ToList();
            foreach (var item in current.Except(ddBBoxs.Keys))
            {
                if (BaseAttr.Attrs[item] is GameObjectAttr)
                {
                    DDBBox instance = GameObject.Instantiate<DDBBox>(ddBBoxSource);
                    ddBBoxs.Add(item, instance);
                    instance.target = BaseAttr.Attrs[item] as GameObjectAttr;
                }
            }
            foreach (var item in ddBBoxs.Keys.Except(current))
            {
                GameObject.Destroy(ddBBoxs[item].gameObject);
                dddBBoxs.Remove(item);
            }
        }

        public void SendLog(string log)
        {
            PlayerMain.Instance.AddLog(log);
        }

        public void SendVersion(string version)
        {
            PlayerMain.Instance.pythonVersion = new Version(version);
        }
    }
}
