using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

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
                        materials.AddRange(item.sharedMaterials);
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
                if (Materials.Count > 0)
                {
                    Materials[0].SetColor("_Color", value);
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
                if (Materials.Count > 0)
                {
                    Materials[0].SetTexture("_MainTex", value);
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
        public override Dictionary<string, object> CollectData()
        {
            Dictionary<string, object> data = base.CollectData();
            if (dddbbox != null)
            {
                data.Add("3d_bounding_box", dddbbox);
                dddbbox = null;
            }
            return data;
        }
        public override void AnalysisData(string type, object[] data)
        {
            switch (type)
            {
                case "SetColor":
                    SetColor(data[0].ConvertType<List<float>>());
                    return;
                case "EnabledRender":
                    EnabledRender((bool)data[0]);
                    return;
                case "SetTexture":
                    SetTexture((string)data[0]);
                    return;
                case "Get3DBBox":
                    Get3DBBox();
                    return;
            }
            base.AnalysisData(type, data);
        }

        private void SetColor(List<float> color)
        {
            Color = new Color(color[0], color[1], color[2], color[3]);
        }

        private void EnabledRender(bool enabled)
        {
            Render = enabled;
        }
        private void SetTexture(string path)
        {
            if (!System.IO.File.Exists(path)) return;
            byte[] data = System.IO.File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(data);
            Texture = tex;
        }

        Tuple<Vector3, Vector3, Vector3> dddbbox = null;
        public Tuple<Vector3, Vector3, Vector3> Get3DBBox(bool send = true)
        {
            List<Vector3> allVertices = new List<Vector3>();
            foreach (var render in this.GetChildComponentFilter<MeshFilter>())
            {
                Span<Vector3> spanVertices = new Span<Vector3>(render.sharedMesh.vertices);
                render.transform.TransformPoints(render.sharedMesh.vertices, spanVertices);
                transform.InverseTransformPoints(spanVertices, spanVertices);
                allVertices.AddRange(spanVertices.ToArray());
            }
            var x = allVertices.Select(s => s.x);
            var y = allVertices.Select(s => s.y);
            var z = allVertices.Select(s => s.z);
            float maxX = x.Max();
            float minX = x.Min();
            float maxY = y.Max();
            float minY = y.Min();
            float maxZ = z.Max();
            float minZ = z.Min();

            Vector3 position = transform.TransformPoint(new Vector3((maxX + minX) / 2, (maxY + minY) / 2, (maxZ + minZ) / 2));
            Vector3 rotation = transform.eulerAngles;
            Vector3 size = new Vector3((maxX - minX) * transform.lossyScale.x, (maxY - minY) * transform.lossyScale.y, (maxZ - minZ) * transform.lossyScale.z);
            if (send)
                dddbbox = new Tuple<Vector3, Vector3, Vector3>(position, rotation, size);
            return new Tuple<Vector3, Vector3, Vector3>(position, rotation, size);
        }

        public Rect Get2DBBox(Camera cam)
        {
            List<Vector3> allVertices = new List<Vector3>();
            foreach (var render in this.GetChildComponentFilter<MeshFilter>())
            {
                Span<Vector3> spanVertices = new Span<Vector3>(render.sharedMesh.vertices);
                render.transform.TransformPoints(render.sharedMesh.vertices, spanVertices);
                for (int i = 0; i < spanVertices.Length; i++)
                {
                    spanVertices[i] = cam.WorldToScreenPoint(spanVertices[i]);
                }
                allVertices.AddRange(spanVertices.ToArray());
            }
            var x = allVertices.Select(s => s.x);
            var y = allVertices.Select(s => s.y);
            float maxX = x.Max();
            float minX = x.Min();
            float maxY = y.Max();
            float minY = y.Min();
            return new Rect(minX, minY, maxX - minX, maxY - minY);
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

