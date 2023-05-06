using Robotflow.RFUniverse.SideChannels;
using RFUniverse.Manager;
using UnityEngine;
using System.Linq;
using Obi;

namespace RFUniverse.Attributes
{
    public class FallingClothAttr : BaseAttr
    {
        ObiSolver solver;
        ObiCloth cloth;
        ObiAmbientForceZone forceZone;
        Vector3 lastStepClothPosition;

        public override void Init()
        {
            base.Init();
            solver = GetComponent<ObiSolver>();
            cloth = GetComponentInChildren<ObiCloth>();
            forceZone = GetComponentInChildren<ObiAmbientForceZone>();
            lastStepClothPosition = new Vector3(-99, -99, -99);
        }

        public override void CollectData(OutgoingMessage msg)
        {
            base.CollectData(msg);

            int numParticles = cloth.solverIndices.Length;
            Vector3 avgPosition = Vector3.zero;
            for (int i = 0; i < numParticles; ++i)
            {
                avgPosition += cloth.GetParticlePosition(i);
            }
            avgPosition /= numParticles;

            Vector3 avgVelocity;
            if (lastStepClothPosition.y < -50)
            {
                avgVelocity = Vector3.zero;
            }
            else
            {
                avgVelocity = (avgPosition - lastStepClothPosition) / Time.fixedDeltaTime;
            }
            lastStepClothPosition = avgPosition;

            // Number of particles
            msg.WriteInt32(numParticles);
            // Average position
            msg.WriteFloat32(avgPosition.x);
            msg.WriteFloat32(avgPosition.y);
            msg.WriteFloat32(avgPosition.z);
            // Average velocity
            msg.WriteFloat32(avgVelocity.x);
            msg.WriteFloat32(avgVelocity.y);
            msg.WriteFloat32(avgVelocity.z);
            // Force zone parameters
            msg.WriteFloat32(forceZone.transform.eulerAngles.y);
            msg.WriteFloat32(forceZone.intensity);
            msg.WriteFloat32(forceZone.turbulence);
            msg.WriteFloat32(forceZone.turbulenceFrequency);
        }

        public override void AnalysisMsg(IncomingMessage msg, string type)
        {
            switch (type)
            {
                case "SetForceZoneParameters":
                    SetForceZoneParameters(msg);
                    return;
                case "SetSolverParameters":
                    SetSolverParameters(msg);
                    return;
            }
            base.AnalysisMsg(msg, type);
        }


        void SetForceZoneParameters(IncomingMessage msg)
        {
            // Force zone orientation
            if (msg.ReadBoolean())
            {
                forceZone.transform.eulerAngles = new Vector3(0, msg.ReadFloat32(), 0);
            }

            // Force zone intensity
            if (msg.ReadBoolean())
            {
                forceZone.intensity = msg.ReadFloat32();
            }

            // Force zone turbulence
            if (msg.ReadBoolean())
            {
                forceZone.turbulence = msg.ReadFloat32();
            }

            // Force zone turbulence frequency
            if (msg.ReadBoolean())
            {
                forceZone.turbulenceFrequency = msg.ReadFloat32();
            }
        }

        void SetSolverParameters(IncomingMessage msg)
        {
            // Solver gravity
            if (msg.ReadBoolean())
            {
                Vector3 gravity = new Vector3(msg.ReadFloat32(), msg.ReadFloat32(), msg.ReadFloat32());
                solver.gravity = gravity;
            }
        }
    }

}