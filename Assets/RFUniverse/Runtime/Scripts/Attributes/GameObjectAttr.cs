using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

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
            gameObjectAttr.Color = new Color(color[0], color[1], color[2], color[3]);
            gameObjectAttr.Render = render;
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
                        materials.AddRange(item.materials);
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

        [EditAttr("Render", "RFUniverse.EditMode.BoolAttrUI")]
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

        public override BaseAttrData GetAttrData()
        {
            GameObjectAttrData data = new GameObjectAttrData(base.GetAttrData());
            data.color = new float[4] { Color.r, Color.g, Color.b, Color.a };
            data.render = Render;
            return data;
        }

        [RFUAPI]
        private void SetColor(List<float> color)
        {
            Color = new Color(color[0], color[1], color[2], color[3]);
        }
        [RFUAPI]
        private void EnabledRender(bool enabled)
        {
            Render = enabled;
        }
        [RFUAPI]
        private void SetTexture(string path)
        {
            if (!System.IO.File.Exists(path)) return;
            byte[] data = System.IO.File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(data);
            Texture = tex;
        }

        [RFUAPI]
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
            Vector3 min = allVertices[0];
            Vector3 max = allVertices[0];
            Parallel.For(1, allVertices.Count, i =>
            {
                min.x = Mathf.Min(min.x, allVertices[i].x);
                min.y = Mathf.Min(min.y, allVertices[i].y);
                min.z = Mathf.Min(min.z, allVertices[i].z);

                max.x = Mathf.Max(max.x, allVertices[i].x);
                max.y = Mathf.Max(max.y, allVertices[i].y);
                max.z = Mathf.Max(max.z, allVertices[i].z);
            });

            Vector3 position = transform.TransformPoint((min + max) / 2);
            Vector3 rotation = transform.eulerAngles;
            Vector3 size = Vector3.Scale(max - min, transform.lossyScale);
            Tuple<Vector3, Vector3, Vector3> dddBBox = new Tuple<Vector3, Vector3, Vector3>(position, rotation, size);
            if (send)
                CollectData.AddDataNextStep("3d_bounding_box", dddBBox);
            return dddBBox;
        }

        public Rect Get2DBBox(Camera camera)
        {
            Matrix4x4 worldToCameraMatrix = camera.worldToCameraMatrix;
            Matrix4x4 projectionMatrix = camera.projectionMatrix;
            int width = camera.pixelWidth;
            int height = camera.pixelHeight;

            List<Vector3> allVertices = new List<Vector3>();
            foreach (var render in this.GetChildComponentFilter<MeshFilter>())
            {
                Span<Vector3> spanVertices = new Span<Vector3>(render.sharedMesh.vertices);
                render.transform.TransformPoints(render.sharedMesh.vertices, spanVertices);
                var array = spanVertices.ToArray();
                Parallel.For(0, array.Length, i =>
                {
                    array[i] = WorldToScreenPoint(array[i], worldToCameraMatrix, projectionMatrix, width, height);
                });
                allVertices.AddRange(array);
            }
            Vector3 min = allVertices[0];
            Vector3 max = allVertices[0];

            Parallel.For(1, allVertices.Count, i =>
            {
                min.x = Mathf.Min(min.x, allVertices[i].x);
                min.y = Mathf.Min(min.y, allVertices[i].y);

                max.x = Mathf.Max(max.x, allVertices[i].x);
                max.y = Mathf.Max(max.y, allVertices[i].y);
            });
            return new Rect(min, max - min);
        }
        Vector3 WorldToScreenPoint(Vector3 worldPos, Matrix4x4 worldToCameraMatrix, Matrix4x4 projectionMatrix, int width, int height)
        {
            Vector4 clipSpacePos = projectionMatrix * worldToCameraMatrix * new Vector4(worldPos.x, worldPos.y, worldPos.z, 1);

            Vector3 ndcPos = new Vector3(clipSpacePos.x / clipSpacePos.w, clipSpacePos.y / clipSpacePos.w, clipSpacePos.z / clipSpacePos.w);

            Vector3 screenPos = new Vector3(
                (ndcPos.x + 1) * 0.5f * width,
                (ndcPos.y + 1) * 0.5f * height,
                ndcPos.z
            );

            return screenPos;
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

