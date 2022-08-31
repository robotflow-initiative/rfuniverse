using UnityEngine;
using System;
using Robotflow.RFUniverse.SideChannels;

namespace RFUniverse.Attributes
{
    public class RigidbodyAttrData : ColliderAttrData
    {
        public RigidbodyData rigidbodyData;
        public RigidbodyAttrData() : base()
        {
            type = "Rigidbody";
        }
        public RigidbodyAttrData(BaseAttrData b) : base(b)
        {
            if (b is RigidbodyAttrData)
                rigidbodyData = (b as RigidbodyAttrData).rigidbodyData;
            type = "Rigidbody";
        }
    }
    [Serializable]
    public class RigidbodyData
    {
        public float mass = 1;
        public bool useGravity = true;
    }
    public class RigidbodyAttr : ColliderAttr
    {
        public override string Type
        {
            get { return "Rigidbody"; }
        }
        private Rigidbody rigidbody = null;
        public Rigidbody Rigidbody
        {
            get
            {
                if (rigidbody == null)
                    rigidbody = GetComponent<Rigidbody>();
                return rigidbody;
            }
        }

        [SerializeField]
        private RigidbodyData rigidbodyData = new RigidbodyData();
        [Attr("Rigidbody")]
        public RigidbodyData RigidbodyData
        {
            get
            {
                return rigidbodyData;
            }
            set
            {
                rigidbodyData = value;
            }
        }
        protected override void Init()
        {
            base.Init();
        }

        public override BaseAttrData GetAttrData()
        {
            RigidbodyAttrData data = new RigidbodyAttrData(base.GetAttrData());
            data.rigidbodyData = RigidbodyData;
            return data;
        }
        public override void SetAttrData(BaseAttrData setData)
        {
            base.SetAttrData(setData);
            if (setData is RigidbodyAttrData)
            {
                RigidbodyAttrData data = setData as RigidbodyAttrData;

                RigidbodyData = data.rigidbodyData;

                Rigidbody.mass = RigidbodyData.mass;
                Rigidbody.useGravity = RigidbodyData.useGravity;
            }
        }
        private Vector3 force = Vector3.zero;
        void FixedUpdate()
        {
            if (force.magnitude > 0)
            {
                Rigidbody.AddForce(force);
                force = Vector3.zero;
            }
        }

        public override void CollectData(OutgoingMessage msg)
        {
            base.CollectData(msg);
            // Velocity
            msg.WriteFloat32(Rigidbody.velocity.x);
            msg.WriteFloat32(Rigidbody.velocity.y);
            msg.WriteFloat32(Rigidbody.velocity.z);
            // Angular velocity
            msg.WriteFloat32(Rigidbody.angularVelocity.x);
            msg.WriteFloat32(Rigidbody.angularVelocity.y);
            msg.WriteFloat32(Rigidbody.angularVelocity.z);
        }

        public override void AnalysisMsg(IncomingMessage msg, string type)
        {
            switch (type)
            {
                case "AddForce":
                    AddForce(msg);
                    return;
                case "SetVelocity":
                    SetVelocity(msg);
                    return;
            }
            base.AnalysisMsg(msg, type);
        }

        protected override void SetTransform(IncomingMessage msg)
        {
            bool set_position = msg.ReadBoolean();
            bool set_rotation = msg.ReadBoolean();
            bool set_scale = msg.ReadBoolean();

            if (set_position)
            {
                float x = msg.ReadFloat32();
                float y = msg.ReadFloat32();
                float z = msg.ReadFloat32();
                Vector3 position = new Vector3(x, y, z);
                transform.localPosition = position;
            }

            if (set_rotation)
            {
                float rx = msg.ReadFloat32();
                float ry = msg.ReadFloat32();
                float rz = msg.ReadFloat32();
                Vector3 rotation = new Vector3(rx, ry, rz);
                transform.localEulerAngles = rotation;
            }

            if (set_scale)
            {
                float sx = msg.ReadFloat32();
                float sy = msg.ReadFloat32();
                float sz = msg.ReadFloat32();
                Vector3 scale = new Vector3(sx, sy, sz);
                transform.localScale = scale;
            }

            Rigidbody.velocity = Vector3.zero;
        }
        private void AddForce(IncomingMessage msg)
        {
            float x = msg.ReadFloat32();
            float y = msg.ReadFloat32();
            float z = msg.ReadFloat32();

            force = new Vector3(x, y, z);
        }

        private void SetVelocity(IncomingMessage msg)
        {
            float x = msg.ReadFloat32();
            float y = msg.ReadFloat32();
            float z = msg.ReadFloat32();

            Vector3 velocity = new Vector3(x, y, z);

            Rigidbody.velocity = velocity;
        }
    }
}

