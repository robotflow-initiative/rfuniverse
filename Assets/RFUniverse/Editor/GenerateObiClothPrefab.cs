#if OBI
using Obi;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GenerateObiClothPrefab : MonoBehaviour
{
    [MenuItem("RFUniverse/Obi/Generare ObiCloth Prefab")]
    public static void Generate()
    {
        string stencilPath = EditorUtility.OpenFilePanel("Select a Stencil", Application.dataPath, "prefab");
        Debug.Log("StencilPath: " + stencilPath);
        GameObject stencilObject = AssetDatabase.LoadAssetAtPath<GameObject>(stencilPath.Replace(Application.dataPath, "Assets"));
        if (stencilObject == null) return;
        if (stencilObject.GetComponentInChildren<ObiCloth>() == null) return;
        List<GameObject> objs = Selection.GetFiltered<GameObject>(SelectionMode.Assets).ToList();
        Debug.Log(objs.Count);
        GameObject stencil = Instantiate(stencilObject);
        foreach (var item in objs)
        {
            string file = AssetDatabase.GetAssetPath(item);
            string path = Path.GetDirectoryName(file);
            string name = Path.GetFileNameWithoutExtension(file);
            string blueprintPath = $"{path}/{name}_Blueprint.asset";
            string prefabPath = $"{path}/{name}_ObiCloth.prefab";
            if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(blueprintPath)) && !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(prefabPath)))
                continue;
            Debug.Log(path);
            Debug.Log(name);

            GameObject prefab = Instantiate(item);
            ObiCloth cloth = stencil.GetComponentInChildren<ObiCloth>();

            MeshFilter filter = prefab.GetComponentInChildren<MeshFilter>();
            if (filter != null && filter.sharedMesh != null)
            {
                Debug.LogWarning($"No Mesh: {name}");
                cloth.clothBlueprint = ObiUtility.GenerateClothBlueprints(filter.sharedMesh);
                AssetDatabase.CreateAsset(cloth.clothBlueprint, blueprintPath);
                PrefabUtility.SaveAsPrefabAsset(stencil, prefabPath);
            }
            DestroyImmediate(prefab);
        }
        AssetDatabase.Refresh();
        DestroyImmediate(stencil);
    }
}
#endif