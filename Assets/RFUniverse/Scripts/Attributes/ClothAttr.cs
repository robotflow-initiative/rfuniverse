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
        public override string Type
        {
            get { return "Cloth"; }
        }
        ObiCloth obiCloth = null;
        protected override void Init()
        {
            base.Init();
            obiCloth = GetComponentInChildren<ObiCloth>();
        }

        public override void CollectData(OutgoingMessage msg)
        {
            base.CollectData(msg);
        }

        public override void AnalysisMsg(IncomingMessage msg, string type)
        {
            base.AnalysisMsg(msg, type);
        }
    }
}
