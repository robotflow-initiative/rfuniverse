using UnityEngine;
using RFUniverse.Manager;
using Robotflow.RFUniverse.SideChannels;

namespace RFUniverse.Attributes
{
    public class GameObjectAttrData : ColliderAttrData
    {
        public float[] color;

        public GameObjectAttrData() : base()
        {
            type = "GameObject";
        }
        public GameObjectAttrData(BaseAttrData b) : base(b)
        {
            if (b is GameObjectAttrData)
                color = (b as GameObjectAttrData).color;
            type = "GameObject";
        }
    }
    public class GameObjectAttr : ColliderAttr
    {
        public MeshRenderer render = null;

        private MeshRenderer Render
        {
            get
            {
                if (render == null)
                    render = GetComponentInChildren<MeshRenderer>();
                return render;
            }
        }

        [Attr("Color")]
        public Color Color
        {
            get
            {
                if (Render != null && Render.sharedMaterial != null)
                    return Render.sharedMaterial.GetColor("_Color");
                else
                    return Color.white;
            }
            set
            {
                if (Render != null && Render.sharedMaterial != null)
                    Render.material.SetColor("_Color", value);
            }
        }

        protected override void Init()
        {
            base.Init();
        }
        public override BaseAttrData GetAttrData()
        {
            GameObjectAttrData data = new GameObjectAttrData(base.GetAttrData());
            data.color = new float[4] { Color.r, Color.g, Color.b, Color.a };
            return data;
        }
        public override void SetAttrData(BaseAttrData setData)
        {
            base.SetAttrData(setData);
            if (setData is GameObjectAttrData)
            {
                GameObjectAttrData data = setData as GameObjectAttrData;
                Color = new Color(data.color[0], data.color[1], data.color[2], data.color[3]);
            }
        }
        public override void CollectData(OutgoingMessage msg)
        {
            msg.WriteString("GameObject");

            msg.WriteInt32(ID);
            // Name
            msg.WriteString(Name);
            // Position
            msg.WriteFloat32(transform.position.x);
            msg.WriteFloat32(transform.position.y);
            msg.WriteFloat32(transform.position.z);
            // Rotation
            msg.WriteFloat32(transform.eulerAngles.x);
            msg.WriteFloat32(transform.eulerAngles.y);
            msg.WriteFloat32(transform.eulerAngles.z);
            // Quaternion
            msg.WriteFloat32(transform.rotation.x);
            msg.WriteFloat32(transform.rotation.y);
            msg.WriteFloat32(transform.rotation.z);
            msg.WriteFloat32(transform.rotation.w);
        }
        public override void AnalysisMsg(IncomingMessage msg, string type)
        {
            switch (type)
            {
                case "Translate":
                    Translate(msg);
                    return;
                case "Rotate":
                    Rotate(msg);
                    return;
                case "SetColor":
                    SetColor(msg);
                    return;
            }
            base.AnalysisMsg(msg, type);
        }

        private void Translate(IncomingMessage msg)
        {
            float x = msg.ReadFloat32();
            float y = msg.ReadFloat32();
            float z = msg.ReadFloat32();

            transform.Translate(new Vector3(x, y, z), Space.World);
        }

        private void Rotate(IncomingMessage msg)
        {
            float rx = msg.ReadFloat32();
            float ry = msg.ReadFloat32();
            float rz = msg.ReadFloat32();

            transform.Rotate(new Vector3(0, 0, 1), rz);
            transform.Rotate(new Vector3(1, 0, 0), rx);
            transform.Rotate(new Vector3(0, 1, 0), ry);
        }
        private void SetColor(IncomingMessage msg)
        {
            float cr = msg.ReadFloat32();
            float cg = msg.ReadFloat32();
            float cb = msg.ReadFloat32();
            float ca = msg.ReadFloat32();
            Color = new Color(cr, cg, cb, ca);
        }
    }
}

