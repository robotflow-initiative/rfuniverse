using UnityEngine;
using Robotflow.RFUniverse.SideChannels;
using RFUniverse;

namespace RFUniverse.Attributes
{
    public class GameObjectAttrData : BaseAttrData
    {
        public float[] color = new float[]{1,1,1,1};
        public bool render = true;
        public GameObjectAttrData() : base()
        {
            type = "GameObject";
        }
        public GameObjectAttrData(BaseAttrData b) : base(b)
        {
            type = "GameObject";
            if (b is GameObjectAttrData)
            {
                color = (b as GameObjectAttrData).color;
                render = (b as GameObjectAttrData).render;
            }
        }
    }
    public class GameObjectAttr : BaseAttr
    {
        public override string Type
        {
            get { return "GameObject"; }
        }

        private Material mat = null;
        private Material Material
        {
            get
            {
                if (mat == null)
                    mat = GetComponentInChildren<Renderer>()?.material;
                return mat;
            }
        }

        //public Color color;
        [EditableAttr("Color")]
        public Color Color
        {
            get
            {
                if (Material != null)
                    return Material.GetColor("_Color");
                else
                    return Color.white;
            }
            set
            {
                if (Material != null)
                    Material.SetColor("_Color", value);
            }
        }

        bool render = true;
        //[EditableAttr("Render")]
        public bool Render
        {
            get
            {
                return render;
            }
            set
            {
                EnabledRender(value);
                render = value;
            }
        }
        public Texture2D Texture
        {
            set
            {
                if (Material != null)
                    Material.SetTexture("_MainTex", value);
            }
        }

        public override void Init()
        {
            base.Init();
        }
        public override BaseAttrData GetAttrData()
        {
            GameObjectAttrData data = new GameObjectAttrData(base.GetAttrData());
            data.color = new float[4] { Color.r, Color.g, Color.b, Color.a };
            data.render = Render;
            return data;
        }
        public override void SetAttrData(BaseAttrData setData)
        {
            base.SetAttrData(setData);
            if (setData is GameObjectAttrData)
            {
                GameObjectAttrData data = setData as GameObjectAttrData;
                Color = new Color(data.color[0], data.color[1], data.color[2], data.color[3]);
                Render = data.render;
            }
        }
        public override void CollectData(OutgoingMessage msg)
        {
            base.CollectData(msg);
        }
        public override void AnalysisMsg(IncomingMessage msg, string type)
        {
            switch (type)
            {
                // case "Translate":
                //     Translate(msg);
                //     return;
                // case "Rotate":
                //     Rotate(msg);
                //     return;
                case "SetColor":
                    SetColor(msg);
                    return;
                case "EnabledRender":
                    EnabledRender(msg);
                    return;
                case "SetTexture":
                    SetTexture(msg);
                    return;
            }
            base.AnalysisMsg(msg, type);
        }

        // private void Translate(IncomingMessage msg)
        // {
        //     float x = msg.ReadFloat32();
        //     float y = msg.ReadFloat32();
        //     float z = msg.ReadFloat32();
        //
        //     transform.Translate(new Vector3(x, y, z), Space.World);
        // }
        //
        // private void Rotate(IncomingMessage msg)
        // {
        //     float rx = msg.ReadFloat32();
        //     float ry = msg.ReadFloat32();
        //     float rz = msg.ReadFloat32();
        //
        //     transform.Rotate(new Vector3(0, 0, 1), rz);
        //     transform.Rotate(new Vector3(1, 0, 0), rx);
        //     transform.Rotate(new Vector3(0, 1, 0), ry);
        // }
        private void SetColor(IncomingMessage msg)
        {
            float cr = msg.ReadFloat32();
            float cg = msg.ReadFloat32();
            float cb = msg.ReadFloat32();
            float ca = msg.ReadFloat32();
            Color = new Color(cr, cg, cb, ca);
        }
        
        private void EnabledRender(IncomingMessage msg)
        {
            bool enabled = msg.ReadBoolean();
            EnabledRender(enabled);
            
        }
        private void EnabledRender(bool enabled)
        {
            foreach (var item in this.GetChildComponentFilter<Renderer>())
            {
                item.enabled = enabled;
            }
        }
        
        private void SetTexture(IncomingMessage msg)
        {
            string path = msg.ReadString();
            SetTexture(path);
        }
        private void SetTexture(string path)
        {
            if (!System.IO.File.Exists(path)) return;
            byte[] data = System.IO.File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(data);
            Texture = tex;
        }
    }
}

