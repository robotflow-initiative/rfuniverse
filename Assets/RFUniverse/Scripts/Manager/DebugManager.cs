using System.Collections.Generic;
using RFUniverse.Attributes;
using RFUniverse.DebugTool;
using Robotflow.RFUniverse.SideChannels;
using UnityEngine;
using System;
using System.Linq;
using Google.Protobuf.WellKnownTypes;

namespace RFUniverse.Manager
{
    public class DebugManager : BaseManager
    {
        const string UUID = "02ac5776-6a7c-54e4-011d-b4c4723831c9";

        private static DebugManager instance = null;
        public static DebugManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DebugManager(UUID);
                }
                return instance;
            }
        }

        public DebugManager(string channel_id) : base(channel_id)
        {
        }
        public override void ReceiveData(IncomingMessage msg)
        {
            string type = msg.ReadString();
            switch (type)
            {
                case "DebugGraspPoint":
                    DebugGraspPoint(msg);
                    break;
                case "DebugObjectPose":
                    DebugObjectPose(msg);
                    break;
                case "DebugCollisionPair":
                    DebugCollisionPair(msg);
                    break;
                case "DebugColliderBound":
                    DebugColliderBound(msg);
                    break;
                case "Debug3DBBox":
                    Debug3DBBox(msg);
                    break;
                case "DebugObjectID":
                    DebugObjectID(msg);
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
                foreach (var item in graspPoints)
                {
                    item.Value.gameObject.SetActive(false);
                }
                isDebugGraspPoint = value;
            }
        }

        GraspPoint graspPointSource = null;
        public void DebugGraspPoint(IncomingMessage msg)
        {
            IsDebugGraspPoint = msg.ReadBoolean();
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
                instance.target = (BaseAttr.Attrs[item] as ControllerAttr).joints.Last().transform;
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
                foreach (var item in poseGizmos)
                {
                    item.Value.gameObject.SetActive(false);
                }
                isDebugObjectPose = value;
            }
        }
        PoseGizmo poseGizmoSource = null;
        public void DebugObjectPose(IncomingMessage msg)
        {
            IsDebugObjectPose = msg.ReadBoolean();
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
                    BaseAttr.OnAttrChange += RefreshCollisionPair;
                    RefreshCollisionPair();
                }
                else
                {
                    BaseAttr.OnAttrChange -= RefreshCollisionPair;
                }
                foreach (var item in collisionLines)
                {
                    item.gameObject.SetActive(false);
                }
                isDebugCollisionPair = value;
            }
        }
        CollisionLine collisionLineSource = null;
        public void DebugCollisionPair(IncomingMessage msg)
        {
            IsDebugCollisionPair = msg.ReadBoolean();
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
                foreach (var item in collisionLines)
                {
                    item.gameObject.SetActive(false);
                }
                isDebugColliderBound = value;
            }
        }

        ColliderBound colliderBoundSource = null;
        public void DebugColliderBound(IncomingMessage msg)
        {
            IsDebugColliderBound = msg.ReadBoolean();
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
                foreach (var item in collisionLines)
                {
                    item.gameObject.SetActive(false);
                }
                isDebugColliderBound = value;
            }
        }

        ObjectID objectIDSource = null;
        public void DebugObjectID(IncomingMessage msg)
        {
            IsDebugObjectID = msg.ReadBoolean();
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
                foreach (var item in collisionLines)
                {
                    item.gameObject.SetActive(false);
                }
                isDebugColliderBound = value;
            }
        }

        DDDBBox dddBBoxSource = null;
        public void Debug3DBBox(IncomingMessage msg)
        {
            IsDebug3DBBox = msg.ReadBoolean();
        }
        Dictionary<int, DDDBBox> dddBBoxs = new Dictionary<int, DDDBBox>();
        void Refresh3DBBox()
        {
            if (dddBBoxSource == null) return;
            List<int> current = BaseAttr.Attrs.Keys.ToList();
            foreach (var item in current.Except(dddBBoxs.Keys))
            {
                DDDBBox instance = GameObject.Instantiate<DDDBBox>(dddBBoxSource);
                dddBBoxs.Add(item, instance);
                instance.target = BaseAttr.Attrs[item];
            }
            foreach (var item in dddBBoxs.Keys.Except(current))
            {
                GameObject.Destroy(dddBBoxs[item].gameObject);
                dddBBoxs.Remove(item);
            }
        }
    }
}
