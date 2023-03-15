using Robotflow.RFUniverse.SideChannels;

namespace RFUniverse.Attributes
{
    public class ClothAttr : BaseAttr
    {
#if OBI
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
