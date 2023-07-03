using UnityEngine;
using Obi;
using System.Collections.Generic;

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

        public override Dictionary<string, object> CollectData()
        {
            Dictionary<string, object> data = base.CollectData();

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
            data.Add("num_particles", numParticles);
            // Average position
            data.Add("avg_position", avgPosition);
            // Average velocity
            data.Add("avg_velocity", avgVelocity);
            // Force zone parameters
            data.Add("force_zone_orientation", forceZone.transform.eulerAngles.y);
            data.Add("force_zone_intensity", forceZone.intensity);
            data.Add("force_zone_turbulence", forceZone.turbulence);
            data.Add("force_zone_turbulence_frequency", forceZone.turbulenceFrequency);
            return data;
        }

        public override void AnalysisData(string type, object[] data)
        {
            switch (type)
            {
                case "SetForceZoneParameters":
                    SetForceZoneParameters((float)data[0], (float)data[1], (float)data[2], (float)data[3]);
                    return;
                case "SetSolverParameters":
                    SetSolverParameters(RFUniverseUtility.ConvertType<List<float>>(data[0]));
                    return;
            }
            base.AnalysisData(type, data);
        }


        void SetForceZoneParameters(float orientation, float intensity, float turbulence, float turbulence_frequency)
        {
            // Force zone orientation
            forceZone.transform.eulerAngles = new Vector3(0, orientation, 0);
            // Force zone intensity
            forceZone.intensity = intensity;
            // Force zone turbulence
            forceZone.turbulence = turbulence;
            // Force zone turbulence frequency
            forceZone.turbulenceFrequency = turbulence_frequency;
        }

        void SetSolverParameters(List<float> gravity)
        {
            // Solver gravity
            solver.gravity = RFUniverseUtility.ListFloatToVector3(gravity);
        }
    }

}