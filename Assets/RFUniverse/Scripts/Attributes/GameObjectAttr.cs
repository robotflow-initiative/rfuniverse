using UnityEngine;
using Robotflow.RFUniverse.SideChannels;
using RFUniverse;
using System.Collections.Generic;
using System.Linq;

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
            msg.WriteBoolean(dddbbox != null);
            if (dddbbox != null)
            {
                msg.WriteFloat32(dddbbox.Item1.x);
                msg.WriteFloat32(dddbbox.Item1.y);
                msg.WriteFloat32(dddbbox.Item1.z);
                msg.WriteFloat32(dddbbox.Item2.x);
                msg.WriteFloat32(dddbbox.Item2.y);
                msg.WriteFloat32(dddbbox.Item2.z);
                msg.WriteFloat32(dddbbox.Item3.x);
                msg.WriteFloat32(dddbbox.Item3.y);
                msg.WriteFloat32(dddbbox.Item3.z);
                dddbbox = null;
            }
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
                case "Get3DBBox":
                    Get3DBBox(msg);
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

        Dictionary<Mesh, List<Vector3>> vertices = new Dictionary<Mesh, List<Vector3>>();

        System.Tuple<Vector3, Vector3, Vector3> dddbbox = null;
        void Get3DBBox(IncomingMessage msg)
        {
            dddbbox = Get3DBBox();
        }
        public System.Tuple<Vector3, Vector3, Vector3> Get3DBBox()
        {
            float maxX = float.NegativeInfinity;
            float minX = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;
            float minY = float.PositiveInfinity;
            float maxZ = float.NegativeInfinity;
            float minZ = float.PositiveInfinity;
            foreach (var render in this.GetChildComponentFilter<MeshFilter>())
            {
                if (!vertices.TryGetValue(render.mesh, out List<Vector3> thisVertices))
                {
                    thisVertices = render.mesh.vertices.ToList();
                    vertices.Add(render.mesh, thisVertices);
                }
                foreach (var item in thisVertices)
                {
                    Vector3 point = render.transform.TransformPoint(item);
                    point = transform.InverseTransformPoint(point);
                    maxX = Mathf.Max(maxX, point.x);
                    minX = Mathf.Min(minX, point.x);
                    maxY = Mathf.Max(maxY, point.y);
                    minY = Mathf.Min(minY, point.y);
                    maxZ = Mathf.Max(maxZ, point.z);
                    minZ = Mathf.Min(minZ, point.z);
                }
            }
            Vector3 position = transform.TransformPoint(new Vector3((maxX + minX) / 2, (maxY + minY) / 2, (maxZ + minZ) / 2));
            Vector3 rotation = transform.eulerAngles;
            Vector3 size = new Vector3((maxX - minX) * transform.lossyScale.x, (maxY - minY) * transform.lossyScale.y, (maxZ - minZ) * transform.lossyScale.z);
            return new System.Tuple<Vector3, Vector3, Vector3>(position, rotation, size);
        }

        public Rect Get2DBBox(Camera cam)
        {
            float maxX = float.MinValue;
            float minX = float.MaxValue;
            float maxY = float.MinValue;
            float minY = float.MaxValue;
            foreach (var render in this.GetChildComponentFilter<MeshFilter>())
            {
                Vector3[] vertices = render.mesh.vertices;

                foreach (var item in vertices)
                {
                    Vector3 point = render.transform.TransformPoint(item);
                    point = cam.WorldToScreenPoint(point);
                    if (point.x > maxX) maxX = point.x;
                    if (point.x < minX) minX = point.x;
                    if (point.y > maxY) maxY = point.y;
                    if (point.y < minY) minY = point.y;
                }
            }
            return new Rect((maxX + minX) / 2, (maxY + minY) / 2, maxX - minX, maxY - minY);
        }

        public Bounds GetAppendBounds()
        {
            Bounds bounds = new Bounds();
            List<Renderer> renders = this.GetChildComponentFilter<Renderer>();
            if (renders.Count > 0)
            {
                bounds = renders[0].bounds;
                foreach (var item in renders)
                {
                    bounds.Encapsulate(item.bounds);
                }
            }
            return bounds;
        }
    }
}

