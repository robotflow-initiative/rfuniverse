using UnityEngine;
using System;
using System.Collections.Generic;
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
        public override void SetAttrData(BaseAttr attr)
        {
            base.SetAttrData(attr);
            RigidbodyAttr rigidbodyAttr = attr as RigidbodyAttr;
            rigidbodyAttr.SetRigidbodyData(rigidbodyData);
        }
    }
    [Serializable]
    public class RigidbodyData
    {
        public float mass = 1;
        public bool useGravity = true;
        public bool isKinematic = false;
    }
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyAttr : ColliderAttr
    {
        new private Rigidbody rigidbody = null;
        public Rigidbody Rigidbody
        {
            get
            {
                if (rigidbody == null)
                    rigidbody = GetComponent<Rigidbody>();
                return rigidbody;
            }
        }

        public override void Init()
        {
            base.Init();
        }

        public override BaseAttrData GetAttrData()
        {
            RigidbodyAttrData data = new RigidbodyAttrData(base.GetAttrData());
            data.rigidbodyData = GetRigidbodyData();
            return data;
        }

        private RigidbodyData rigidbodyData = new RigidbodyData();

        [EditableAttr("Rigidbody")]
        [EditAttr("Rigidbody", "RFUniverse.EditMode.RigidbodyAttrUI")]
        public RigidbodyData RigidbodyData
        {
            get
            {
                if (rigidbodyData == null)
                    rigidbodyData = GetRigidbodyData();
                return rigidbodyData;
            }
            set
            {
                rigidbodyData = value;
            }
        }
        public RigidbodyData GetRigidbodyData()
        {
            RigidbodyData data = new RigidbodyData();
            data.mass = Rigidbody.mass;
            data.useGravity = Rigidbody.useGravity;
            data.isKinematic = Rigidbody.isKinematic;
            return data;
        }
        public void SetRigidbodyData(RigidbodyData data)
        {
            Rigidbody.mass = data.mass;
            Rigidbody.useGravity = data.useGravity;
            Rigidbody.isKinematic = data.isKinematic;
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

        public override Dictionary<string, object> CollectData()
        {
            Dictionary<string, object> data = base.CollectData();
            data.Add("velocity", Rigidbody.velocity);
            data.Add("angular_vel", Rigidbody.angularVelocity);
            return data;
        }

        public override void AnalysisData(string type, object[] data)
        {
            switch (type)
            {
                case "SetMass":
                    SetMass((float)data[0]);
                    return;
                case "AddForce":
                    AddForce((List<float>)data[0]);
                    return;
                case "SetVelocity":
                    SetVelocity((List<float>)data[0]);
                    return;
                case "SetKinematic":
                    SetKinematic((bool)data[0]);
                    return;
            }
            base.AnalysisData(type, data);
        }

        private void SetMass(float mass)
        {
            Rigidbody.mass = mass;
        }
        protected override void SetPosition(List<float> position, bool worldSpace = true)
        {
            base.SetPosition(position, worldSpace);
            Rigidbody.velocity = Vector3.zero;
        }
        protected override void SetRotation(List<float> rotation, bool worldSpace = true)
        {
            base.SetRotation(rotation, worldSpace);
            Rigidbody.angularVelocity = Vector3.zero;
        }
        private void AddForce(List<float> forceArray)
        {
            force = new Vector3(force[0], force[1], force[2]);
        }

        private void SetVelocity(List<float> velocity)
        {
            Rigidbody.velocity = new Vector3(velocity[0], velocity[1], velocity[2]);
        }

        private void SetKinematic(bool kinematic)
        {
            Rigidbody.isKinematic = kinematic;
        }
    }
}

