using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Unity.Robotics.UrdfImporter.Urdf.Editor
{
    [UnityEditor.AssetImporters.ScriptedImporter(1, "stl")]
    class StlImporter : UnityEditor.AssetImporters.ScriptedImporter
    {
        public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext context)
        {
            // Mesh container
            // Create a prefab with MeshFilter/MeshRenderer.
            var gameObject = new GameObject();
            gameObject.name = "default";
            var mesh = UrdfImporter.StlImporter.ImportMesh(context.assetPath);

            var meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            if (mesh != null) context.AddObjectToAsset("mesh", mesh);

            var meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = MaterialExtensions.CreateBasicMaterial();
            context.AddObjectToAsset("material", meshRenderer.sharedMaterial);

            context.AddObjectToAsset("prefab", gameObject);
            context.SetMainObject(gameObject);
        }

        static Material GetDefaultMaterial()
        {
            Material material = new Material(Shader.Find("Standard"));
            material.name = "defaultMat";
            return material;
        }
    }
}