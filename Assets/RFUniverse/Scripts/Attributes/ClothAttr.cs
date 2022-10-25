using Robotflow.RFUniverse.SideChannels;

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
#if OBI_ONI_SUPPORTED
        public override string Type
        {
            get { return "Cloth"; }
        }
        Obi.ObiCloth obiCloth = null;
        public override void Init()
        {
            base.Init();
            obiCloth = GetComponentInChildren<Obi.ObiCloth>();
        }

        public override void CollectData(OutgoingMessage msg)
        {
            base.CollectData(msg);
        }

        public override void AnalysisMsg(IncomingMessage msg, string type)
        {
            base.AnalysisMsg(msg, type);
        }
#endif
    }
}
