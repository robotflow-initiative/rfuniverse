using UnityEngine;
using Robotflow.RFUniverse.SideChannels;
using System.Linq;
using System.Collections.Generic;

namespace RFUniverse.Attributes
{
    public class PointCloudAttr : BaseAttr
    {
        MeshFilter filter  = null;
        MeshRenderer render = null;
        public override void AnalysisMsg(IncomingMessage msg, string type)
        {
            switch (type)
            {
                case "ShowPointCloud":
                    ShowPointCloud(msg);
                    return;
                case "SetRadius":
                    SetRadius(msg);
                    return;
            }
            base.AnalysisMsg(msg, type);
        }
        private void ShowPointCloud(IncomingMessage msg)
        {
            PlayerMain.Instance.GroundActive = false;
            List<Vector3> positions = RFUniverseUtility.ListFloatToListVector3(msg.ReadFloatList().ToList());
            List<Color> colors = RFUniverseUtility.ListFloatToListColor(msg.ReadFloatList().ToList());

            Mesh mesh = CreatePointCloudMesh(positions, colors);
            if (render == null)
            {
                render = gameObject.AddComponent<MeshRenderer>();
                render.material = new Material(Shader.Find("RFUniverse/PointCloud"));
            }
            if (filter == null)
            {
                filter = gameObject.AddComponent<MeshFilter>();
            }
            filter.mesh = mesh;
        }

        private void SetRadius(IncomingMessage msg)
        {
            float radius = msg.ReadFloat32();
            render.material.SetFloat("_Radius",radius);
        }

        public override void SetTransform(bool set_position, bool set_rotation, bool set_scale, Vector3 position, Vector3 rotation, Vector3 scale, bool worldSpace = true)
        {
            return;
        }

        Mesh CreatePointCloudMesh(List<Vector3> positions, List<Color> colors)
        {
            print(positions.Count);
            print(colors.Count);
            List<Vector3> meshVertices = new ();
            List<Vector3> meshNormals = new();
            List<Color> meshColors = new();
            List<int> meshTriangles = new();

            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 position = positions[i];
                Color color = colors[i];

                meshVertices.Add(position);
                meshNormals.Add(Vector3.up);
                meshColors.Add(color);
                meshVertices.Add(position);
                meshNormals.Add(Vector3.left);
                meshColors.Add(color);
                meshVertices.Add(position);
                meshNormals.Add(Vector3.forward);
                meshColors.Add(color);
                meshVertices.Add(position);
                meshNormals.Add(Vector3.right);
                meshColors.Add(color);
                meshVertices.Add(position);
                meshNormals.Add(Vector3.back);
                meshColors.Add(color);
                meshVertices.Add(position);
                meshNormals.Add(Vector3.down);
                meshColors.Add(color);

                meshTriangles.Add(i * 6 + 0);
                meshTriangles.Add(i * 6 + 1);
                meshTriangles.Add(i * 6 + 2);
                meshTriangles.Add(i * 6 + 0);
                meshTriangles.Add(i * 6 + 2);
                meshTriangles.Add(i * 6 + 3);
                meshTriangles.Add(i * 6 + 0);
                meshTriangles.Add(i * 6 + 3);
                meshTriangles.Add(i * 6 + 4);
                meshTriangles.Add(i * 6 + 0);
                meshTriangles.Add(i * 6 + 4);
                meshTriangles.Add(i * 6 + 1);
                meshTriangles.Add(i * 6 + 5);
                meshTriangles.Add(i * 6 + 1);
                meshTriangles.Add(i * 6 + 4);
                meshTriangles.Add(i * 6 + 5);
                meshTriangles.Add(i * 6 + 4);
                meshTriangles.Add(i * 6 + 3);
                meshTriangles.Add(i * 6 + 5);
                meshTriangles.Add(i * 6 + 3);
                meshTriangles.Add(i * 6 + 2);
                meshTriangles.Add(i * 6 + 5);
                meshTriangles.Add(i * 6 + 2);
                meshTriangles.Add(i * 6 + 1);
            }

            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(meshVertices);
            mesh.SetNormals(meshNormals);
            mesh.SetColors(meshColors);
            mesh.SetTriangles(meshTriangles, 0);
            return mesh;
        }
    }
}

