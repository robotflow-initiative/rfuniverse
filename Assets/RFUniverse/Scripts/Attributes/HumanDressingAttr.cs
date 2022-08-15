using UnityEngine;
using RFUniverse.Manager;
using Robotflow.RFUniverse.SideChannels;

namespace RFUniverse.Attributes
{
    public class HumanDressingAttr : BaseAttr
    {
        public override string Type
        {
            get { return "HumanDressing"; }
        }
        public GameObject spine;
        public GameObject graspPoint;
        public GameObject target;
        protected override void Init()
        {
            base.Init();
            GameObjectAttr[] gameObjectAttrs = GetComponentsInChildren<GameObjectAttr>();
            spine = gameObjectAttrs[1].gameObject;
            target = gameObjectAttrs[2].gameObject;
            graspPoint = GetComponentInChildren<RigidbodyAttr>().gameObject;
        }
        public override BaseAttrData GetAttrData()
        {
            BaseAttrData data = new BaseAttrData();

            data.name = Name;

            data.id = ID;

            data.type = "HumanDressing";

            BaseAttr parentAttr = null;
            if (GetComponentsInParent<BaseAttr>().Length > 1)
                parentAttr = GetComponentsInParent<BaseAttr>()[1];
            data.parentID = parentAttr == null ? -1 : parentAttr.ID;

            Transform parent = transform.parent;
            data.parentName = parent == null ? "" : parent.name;

            data.position = new float[3] { transform.localPosition.x, transform.localPosition.y, transform.localPosition.z };
            data.rotation = new float[3] { transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z };
            data.scale = new float[3] { transform.localScale.x, transform.localScale.y, transform.localScale.z };

            return data;
        }

        public override void CollectData(OutgoingMessage msg)
        {
            base.CollectData(msg);

            Rigidbody graspPointRigidbody = graspPoint.GetComponent<Rigidbody>();
            // Grasp point position
            msg.WriteFloat32(graspPointRigidbody.transform.position.x);
            msg.WriteFloat32(graspPointRigidbody.transform.position.y);
            msg.WriteFloat32(graspPointRigidbody.transform.position.z);
            // Grasp point rotation
            msg.WriteFloat32(graspPointRigidbody.transform.eulerAngles.x);
            msg.WriteFloat32(graspPointRigidbody.transform.eulerAngles.y);
            msg.WriteFloat32(graspPointRigidbody.transform.eulerAngles.z);
            // Grasp point velocity
            msg.WriteFloat32(graspPointRigidbody.velocity.x);
            msg.WriteFloat32(graspPointRigidbody.velocity.y);
            msg.WriteFloat32(graspPointRigidbody.velocity.z);
            // Grasp point angular velocity
            msg.WriteFloat32(graspPointRigidbody.angularVelocity.x);
            msg.WriteFloat32(graspPointRigidbody.angularVelocity.y);
            msg.WriteFloat32(graspPointRigidbody.angularVelocity.z);

            // Target position
            msg.WriteFloat32(target.transform.position.x);
            msg.WriteFloat32(target.transform.position.y);
            msg.WriteFloat32(target.transform.position.z);
            // Target rotation
            msg.WriteFloat32(target.transform.eulerAngles.x);
            msg.WriteFloat32(target.transform.eulerAngles.y);
            msg.WriteFloat32(target.transform.eulerAngles.z);
        }
        public override void AnalysisMsg(IncomingMessage msg, string type)
        {
            switch (type)
            {
                case "SetTargetX":
                    Destroy();
                    return;
            }
            base.AnalysisMsg(msg, type);
        }
        private void SetTargetX(IncomingMessage msg)
        {
            float targetX = msg.ReadFloat32();
            Vector3 localPosition = target.transform.localPosition;
            localPosition.x = targetX;
            target.transform.localPosition = localPosition;

        }
    }
}

