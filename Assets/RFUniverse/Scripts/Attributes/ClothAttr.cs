using Obi;
using RFUniverse.Manager;
using Robotflow.RFUniverse.SideChannels;
using System.Collections.Generic;
using UnityEngine;

namespace RFUniverse.Attributes
{
    public class ClothAttrData : BaseAttrData
    {
        public float[] color;

        public ClothAttrData() : base()
        {
            type = "Cloth";
        }
        public ClothAttrData(BaseAttrData b) : base(b)
        {
            type = "Cloth";
        }
    }
    public class ClothAttr : BaseAttr
    {
        ObiCloth obiCloth = null;
        protected override void Init()
        {
            base.Init();
            obiCloth = GetComponentInChildren<ObiCloth>();
        }

        public override void CollectData(OutgoingMessage msg)
        {
            msg.WriteString("Cloth");
            // ID
            msg.WriteInt32(ID);
            // // Name
            // msg.WriteString(Name);
            // // NumberOfParticles
            // int particleCount = obiCloth.particleCount;

            // msg.WriteInt32(particleCount);
            // // Positions
            // List<float> positions = new List<float>();
            // for (int j = 0; j < particleCount; ++j)
            // {
            //     Vector4 particlePosition = obiCloth.solver.positions[obiCloth.solverIndices[j]];
            //     positions.Add(particlePosition.x);
            //     positions.Add(particlePosition.y);
            //     positions.Add(particlePosition.z);
            // }
            // msg.WriteFloatList(positions);
        }

        public override void AnalysisMsg(IncomingMessage msg, string type)
        {
            base.AnalysisMsg(msg, type);
        }
    }
}
