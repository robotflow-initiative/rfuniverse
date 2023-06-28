
namespace RFUniverse.Attributes
{
    public class SoftbodyAttr : BaseAttr
    {
#if OBI
        Obi.ObiSoftbody obiSoftbody;

        public override void Init()
        {
            base.Init();
            obiSoftbody = GetComponent<Obi.ObiSoftbody>();
        }
#endif
    }
}
