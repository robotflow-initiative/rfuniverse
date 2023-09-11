using UnityEngine;
using UnityEditor;

namespace RFUniverse
{
    public class DeleteMeshCollider : Editor
    {
        [MenuItem("RFUniverse/Delete Mesh Colliders")]
        static void DeleteMeshColliers()
        {
            if (Selection.gameObjects.Length == 0)
            {
                Debug.LogError("No game object selected!");
                return;
            }
            GameObject gameObject = Selection.gameObjects[0];

            MeshCollider[] meshColliders = gameObject.GetComponents<MeshCollider>();
            foreach (var meshCollider in meshColliders)
            {
                DestroyImmediate(meshCollider);
            }
        }
    }
}
