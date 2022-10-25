using Robotflow.RFUniverse.SideChannels;
using UnityEngine;

namespace RFUniverse.Attributes
{
    public class SoftbodyAttr : BaseAttr
    {
#if OBI_ONI_SUPPORTED
        public override string Type
        {
            get { return "Softbody"; }
        }
        Obi.ObiSoftbody obiSoftbody;

        public override void Init()
        {
            base.Init();
            obiSoftbody = GetComponent<Obi.ObiSoftbody>();
        }
        public override BaseAttrData GetAttrData()
        {
            BaseAttrData data = new BaseAttrData();

            data.name = Name;

            data.id = ID;

            data.type = "Softbody";

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
            // NumberOfParticles
            int particleCount = obiSoftbody.particleCount;
            msg.WriteInt32(particleCount);
            // Dynamics parameters
            Vector4 averagePosition = new Vector4(0, 0, 0, 0);
            Vector4 averageVelocity = new Vector4(0, 0, 0, 0);
            Vector4 averageAngularVelocity = new Vector4(0, 0, 0, 0);
            for (int j = 0; j < particleCount; ++j)
            {
                int solverIndex = obiSoftbody.solverIndices[j];
                averagePosition += obiSoftbody.solver.positions[solverIndex];
                averageVelocity += obiSoftbody.solver.velocities[solverIndex];
                averageAngularVelocity += obiSoftbody.solver.angularVelocities[solverIndex];
            }
            // Average Potision
            averagePosition = averagePosition / particleCount;
            msg.WriteFloat32(averagePosition.x + obiSoftbody.solver.transform.position.x);
            msg.WriteFloat32(averagePosition.y + obiSoftbody.solver.transform.position.y);
            msg.WriteFloat32(averagePosition.z + obiSoftbody.solver.transform.position.z);
            // Average Orientation
            Quaternion orientation = obiSoftbody.solver.orientations[obiSoftbody.solverIndices[particleCount / 2]];
            msg.WriteFloat32(orientation.x);
            msg.WriteFloat32(orientation.y);
            msg.WriteFloat32(orientation.z);
            msg.WriteFloat32(orientation.w);
            // Average velocity
            averageVelocity = averageVelocity / particleCount;
            msg.WriteFloat32(averageVelocity.x);
            msg.WriteFloat32(averageVelocity.y);
            msg.WriteFloat32(averageVelocity.z);
            // Average angular velocity
            averageAngularVelocity = averageAngularVelocity / particleCount;
            msg.WriteFloat32(averageAngularVelocity.x);
            msg.WriteFloat32(averageAngularVelocity.y);
            msg.WriteFloat32(averageAngularVelocity.z);
        }

        public override void AnalysisMsg(IncomingMessage msg, string type)
        {
            base.AnalysisMsg(msg, type);
        }
#endif
    }
}
