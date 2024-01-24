using UnityEditor;
using UnityEngine;

public class MeshModify
{
    [MenuItem("RFUniverse/FlipMesh")]
    static void FlipMesh()
    {
        if (Selection.activeGameObject?.GetComponentInChildren<MeshFilter>()?.mesh != null)
            RFUniverse.RFUniverseUtility.FlipMesh(Selection.activeGameObject.GetComponentInChildren<MeshFilter>().mesh);
    }

    [MenuItem("RFUniverse/SmoothMesh")]
    static void SmoothMesh()
    {
        if (Selection.activeGameObject?.GetComponentInChildren<MeshFilter>()?.mesh != null)
            RFUniverse.RFUniverseUtility.SmoothNormals(Selection.activeGameObject.GetComponentInChildren<MeshFilter>().mesh);
    }
}
