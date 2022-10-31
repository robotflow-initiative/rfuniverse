using UnityEngine;

namespace RFUniverse
{
    public class PlayerMain : RFUniverseMain
    {
        public static PlayerMain Instance = null;
        protected override void Awake()
        {
            base.Awake();
            Instance = this;
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            mpb.SetColor("_IDColor", Color.black);
            foreach (var render in Ground.GetComponentsInChildren<Renderer>())
            {
                render.SetPropertyBlock(mpb);
            }
        }

        void OnValidate()
        {
            Instance = this;
            transform.position = Vector3.zero;
        }
    }
}
