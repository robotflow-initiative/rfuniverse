
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
#endif
    }
}
