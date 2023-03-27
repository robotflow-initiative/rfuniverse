using UnityEngine;
using Robotflow.RFUniverse.SideChannels;
using RFUniverse;
using System.Collections.Generic;

namespace RFUniverse.Attributes
{
    public class GameObjectAttrData : BaseAttrData
    {
        public float[] color = new float[] { 1, 1, 1, 1 };
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

        public override void SetAttrData(BaseAttr attr)
        {
            base.SetAttrData(attr);
            GameObjectAttr gameObjectAttr = attr as GameObjectAttr;
            gameObjectAttr.Color = new Color(this.color[0], this.color[1], this.color[2], this.color[3]);
            gameObjectAttr.Render = this.render;
        }
    }
    public class GameObjectAttr : BaseAttr
    {
        private List<Renderer> renderers = null;
        private List<Renderer> Renderers
        {
            get
            {
                if (renderers == null)
                {
                    renderers = this.GetChildComponentFilter<Renderer>();
                }
                return renderers;
            }
        }

        private List<Material> materials = null;
        private List<Material> Materials
        {
            get
            {
                if (materials == null)
                {
                    materials = new List<Material>();
                    foreach (var item in Renderers)
                    {
                        materials.AddRange(Application.isPlaying ? item.materials : item.sharedMaterials);
                    }
                }
                return materials;
            }
        }

        [EditAttr("Color", "RFUniverse.EditMode.ColorAttrUI")]
        public Color Color
        {
            get
            {
                return Materials.Count > 0 ? Materials[0].GetColor("_Color") : Color.white;
            }
            set
            {
                foreach (var item in Materials)
                {
                    item.SetColor("_Color", value);
                }

            }
        }

        [EditAttr("Render", "RFUniverse.EditMode.RenderAttrUI")]
        public bool Render
        {
            get
            {
                return Renderers.Count > 0 ? Renderers[0].enabled : false;
            }
            set
            {
                foreach (var item in Renderers)
                {
                    item.enabled = value;
                }
            }
        }
        public Texture2D Texture
        {
            set
            {
                foreach (var item in Materials)
                {
                    item.SetTexture("_MainTex", value);
                }
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
        public override void CollectData(OutgoingMessage msg)
        {
            base.CollectData(msg);
        }
        public override void AnalysisMsg(IncomingMessage msg, string type)
        {
            switch (type)
            {
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
            Render = enabled;
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

