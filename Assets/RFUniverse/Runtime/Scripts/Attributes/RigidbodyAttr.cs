using UnityEngine;
using System;
using System.Collections.Generic;

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

        public override Dictionary<string, object> CollectData()
        {
            Dictionary<string, object> data = base.CollectData();
            data["velocity"] = Rigidbody.velocity;
            data["angular_vel"] = Rigidbody.angularVelocity;
            return data;
        }

        public override void AnalysisData(string type, object[] data)
        {
            switch (type)
            {
                case "SetMass":
                    SetMass((float)data[0]);
                    return;
                case "SetDrag":
                    SetDrag((float)data[0]);
                    return;
                case "SetAngularDrag":
                    SetAngularDrag((float)data[0]);
                    return;
                case "SetUseGravity":
                    SetUseGravity((bool)data[0]);
                    return;
                case "EnabledMouseDrag":
                    EnabledMouseDrag((bool)data[0]);
                    return;
                case "AddForce":
                    AddForce(data[0].ConvertType<List<float>>());
                    return;
                case "SetVelocity":
                    SetVelocity(data[0].ConvertType<List<float>>());
                    return;
                case "SetAngularVelocity":
                    SetVelocity(data[0].ConvertType<List<float>>());
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
        private void SetDrag(float drag)
        {
            Rigidbody.drag = drag;
        }
        private void SetAngularDrag(float angularDrag)
        {
            Rigidbody.angularDrag = angularDrag;
        }
        private void SetUseGravity(bool useGravity)
        {
            Rigidbody.useGravity = useGravity;
        }
        private void EnabledMouseDrag(bool enabled)
        {
            if (enabled)
                _ = GetComponent<MouseDrag>() ?? gameObject.AddComponent<MouseDrag>();
            else
                Destroy(GetComponent<MouseDrag>());
        }
        protected override void SetPosition(List<float> position, bool worldSpace = true)
        {
            base.SetPosition(position, worldSpace);
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;
        }
        protected override void SetRotation(List<float> rotation, bool worldSpace = true)
        {
            base.SetRotation(rotation, worldSpace);
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;
        }
        private void AddForce(List<float> forceArray)
        {
            Rigidbody.AddForce(new Vector3(forceArray[0], forceArray[1], forceArray[2]));
        }

        private void SetVelocity(List<float> velocity)
        {
            Rigidbody.velocity = new Vector3(velocity[0], velocity[1], velocity[2]);
        }

        private void SetAngularVelocity(List<float> angularVelocity)
        {
            Rigidbody.angularVelocity = new Vector3(angularVelocity[0], angularVelocity[1], angularVelocity[2]);
        }

        private void SetKinematic(bool kinematic)
        {
            Rigidbody.isKinematic = kinematic;
        }
    }
}

