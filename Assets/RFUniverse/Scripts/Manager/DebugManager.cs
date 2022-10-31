using System.Collections.Generic;
using RFUniverse.Attributes;
using RFUniverse.DebugTool;
using Robotflow.RFUniverse.SideChannels;
using UnityEngine;
using System;
using System.Linq;

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
                    DebugGraspPoint();
                    break;
                case "DebugObjectPose":
                    DebugObjectPose();
                    break;
                case "DebugCollisionPair":
                    DebugCollisionPair();
                    break;
                case "DebugColliderBound":
                    DebugColliderBound();
                    break;
                case "DebugObjectID":
                    DebugObjectID();
                    break;
                default:
                    Debug.Log("Dont have mehond:" + type);
                    break;
            }
        }

        GraspPoint graspPointSource = null;
        public void DebugGraspPoint()
        {
            UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("GraspPoint").Completed += (handle) =>
            {
                graspPointSource = handle.Result.GetComponent<GraspPoint>();
                BaseAttr.OnAttrChange += RefreshGraspPoint;
                RefreshGraspPoint();
            };
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

        PoseGizmo poseGizmoSource = null;
        public void DebugObjectPose()
        {
            UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("PoseGizmo").Completed += (handle) =>
            {
                poseGizmoSource = handle.Result.GetComponent<PoseGizmo>();
                BaseAttr.OnAttrChange += RefreshObjectPose;
                RefreshObjectPose();
            };
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
        CollisionLine collisionLineSource = null;
        public void DebugCollisionPair()
        {
            UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("CollisionLine").Completed += (handle) =>
            {
                collisionLineSource = handle.Result.GetComponent<CollisionLine>();
                BaseAttr.OnCollisionPairsChange += RefreshCollisionPair;
                RefreshCollisionPair();
            };
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
        ColliderBound colliderBoundSource = null;
        public void DebugColliderBound()
        {
            UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("ColliderBound").Completed += (handle) =>
            {
                colliderBoundSource = handle.Result.GetComponent<ColliderBound>();
                BaseAttr.OnAttrChange += RefreshColliderBound;
                RefreshColliderBound();
            };
        }
        Dictionary<int, List<ColliderBound>> colliderBounds = new Dictionary<int, List<ColliderBound>>();
        void RefreshColliderBound()
        {
            if (colliderBoundSource == null) return;
            List<int> current = BaseAttr.Attrs.Keys.ToList();
            foreach (var item in current.Except(colliderBounds.Keys))
            {
                if (!BaseAttr.Attrs[item].IsRFMoveCollider) continue;
                List<ColliderBound> colliders = new List<ColliderBound>();
                foreach (Collider col in BaseAttr.Attrs[item].GetComponentsInChildren<Collider>())
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
        ObjectID objectIDSource = null;
        public void DebugObjectID()
        {
            UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("ObjectID").Completed += (handle) =>
            {
                objectIDSource = handle.Result.GetComponent<ObjectID>();
                BaseAttr.OnAttrChange += RefreshObjectID;
                RefreshObjectID();
            };
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
    }
}
