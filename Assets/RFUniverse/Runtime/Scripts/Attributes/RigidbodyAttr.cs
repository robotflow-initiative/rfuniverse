using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace RFUniverse.Attributes
{
    public class RigidbodyAttrData : ColliderAttrData
    {
        public float mass = 1;
        public bool useGravity = true;
        public bool isKinematic = false;
        public RigidbodyAttrData() : base()
        {
            type = "Rigidbody";
        }
        public RigidbodyAttrData(BaseAttrData b) : base(b)
        {
            if (b is RigidbodyAttrData)
            {
                mass = (b as RigidbodyAttrData).mass;
                useGravity = (b as RigidbodyAttrData).useGravity;
                isKinematic = (b as RigidbodyAttrData).isKinematic;
            }
            type = "Rigidbody";
        }
        public override void SetAttrData(BaseAttr attr)
        {
            base.SetAttrData(attr);
            RigidbodyAttr rigidbodyAttr = attr as RigidbodyAttr;
            rigidbodyAttr.Mass = mass;
            rigidbodyAttr.UseGravity = useGravity;
            rigidbodyAttr.IsKinematic = isKinematic;
        }
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
        [EditAttr("Mass", "RFUniverse.EditMode.FloatAttrUI")]
        public float Mass
        {
            get
            {
                return Rigidbody.mass;
            }
            set
            {
                Rigidbody.mass = value;
            }
        }
        [EditAttr("UseGravity", "RFUniverse.EditMode.BoolAttrUI")]

        public bool UseGravity
        {
            get
            {
                return Rigidbody.useGravity;
            }
            set
            {
                Rigidbody.useGravity = value;
            }
        }
        [EditAttr("IsKinematic", "RFUniverse.EditMode.BoolAttrUI")]
        public bool IsKinematic
        {
            get
            {
                return Rigidbody.isKinematic;
            }
            set
            {
                Rigidbody.isKinematic = value;
            }
        }

        public override void Init()
        {
            base.Init();
            Rigidbody.sleepThreshold = 0;
        }

        public override BaseAttrData GetAttrData()
        {
            RigidbodyAttrData data = new RigidbodyAttrData(base.GetAttrData());
            data.mass = Mass;
            data.useGravity = UseGravity;
            data.isKinematic = IsKinematic;
            return data;
        }

        public override void AddPermanentData(Dictionary<string, object> data)
        {
            base.AddPermanentData(data);
            data["velocity"] = Rigidbody.velocity;
            data["angular_velocity"] = Rigidbody.angularVelocity;
        }

        [RFUAPI]
        private void SetMass(float mass)
        {
            Rigidbody.mass = mass;
        }
        [RFUAPI]
        private void SetDrag(float drag)
        {
            Rigidbody.drag = drag;
        }
        [RFUAPI]
        private void SetAngularDrag(float angularDrag)
        {
            Rigidbody.angularDrag = angularDrag;
        }
        [RFUAPI]
        private void SetUseGravity(bool useGravity)
        {
            Rigidbody.useGravity = useGravity;
        }
        [RFUAPI]
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
            if (!Rigidbody.isKinematic)
            {
                Rigidbody.velocity = Vector3.zero;
                Rigidbody.angularVelocity = Vector3.zero;
            }
        }
        protected override void SetRotation(List<float> rotation, bool worldSpace = true)
        {
            base.SetRotation(rotation, worldSpace);
            if (!Rigidbody.isKinematic)
            {
                Rigidbody.velocity = Vector3.zero;
                Rigidbody.angularVelocity = Vector3.zero;
            }
        }
        [RFUAPI]
        private void AddForce(List<float> forceArray)
        {
            Rigidbody.AddForce(new Vector3(forceArray[0], forceArray[1], forceArray[2]));
        }
        [RFUAPI]
        private void SetVelocity(List<float> velocity)
        {
            Rigidbody.velocity = new Vector3(velocity[0], velocity[1], velocity[2]);
        }
        [RFUAPI]
        private void SetAngularVelocity(List<float> angularVelocity)
        {
            Rigidbody.angularVelocity = new Vector3(angularVelocity[0], angularVelocity[1], angularVelocity[2]);
        }
        [RFUAPI]
        private void SetKinematic(bool kinematic)
        {
            Rigidbody.isKinematic = kinematic;
        }
        [RFUAPI]
        private void Link(int id, int jointIndex = 0, float massScale = 1, float connectedMassScale = 1)
        {
            FixedJoint joint = rigidbody.GetComponent<FixedJoint>();
            if (joint == null)
                joint = rigidbody.gameObject.AddComponent<FixedJoint>();

            if (!Attrs.ContainsKey(id))
            {
                Debug.Log($"ID: {id} not exist, break link");
                joint.connectedArticulationBody = null;
                joint.connectedBody = null;
                return;
            }

            if (Attrs[id] is RigidbodyAttr)
            {
                joint.connectedArticulationBody = null;
                joint.connectedBody = (Attrs[id] as RigidbodyAttr).rigidbody;
            }
            else if (Attrs[id] is ControllerAttr)
            {
                ControllerAttr controllerAttr = Attrs[id] as ControllerAttr;
                if (jointIndex >= controllerAttr.Joints.Count)
                {
                    Debug.LogError($"The index of target joint is {jointIndex}, but the valid number of joints in robot arm is {controllerAttr.Joints.Count}");
                    return;
                }
                joint.connectedBody = null;
                joint.connectedArticulationBody = controllerAttr.Joints[jointIndex];
            }
            joint.massScale = massScale;
            joint.connectedMassScale = connectedMassScale;
        }
    }
}

