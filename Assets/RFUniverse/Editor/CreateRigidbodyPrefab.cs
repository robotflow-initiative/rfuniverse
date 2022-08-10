using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using RFUniverse.Attributes;

public class CreateRigidbodyPrefab : MonoBehaviour
{
    static private string prefabFolder = "Assets/Prefabs";
    static private string prefabSuffix = ".prefab";
    static private List<string> legalExtensions = new List<string> { ".obj", ".dae", ".fbx" };
    static private bool CreatePrefabFromPath(string meshPath, string prefabName = "")
    {
        // Get file extension and prefab name
        string meshExtension = Path.GetExtension(meshPath);
        if (prefabName == "")
        {
            prefabName = Path.GetFileNameWithoutExtension(meshPath);
        }

        // Load mesh as prefab and instantiate it.
        GameObject go = AssetDatabase.LoadAssetAtPath(meshPath, typeof(UnityEngine.Object)) as GameObject;
        GameObject gameObject = Instantiate(go);

        MeshFilter meshFilter = null;
        if (meshExtension == ".obj")
        {
            // .obj mesh will automatically generate a parent-child relation,
            // we do not want it, so we only extract the child.
            meshFilter = gameObject.GetComponentInChildren<MeshFilter>();
            if (meshFilter == null)
            {
                return false;
            }
        }
        else if (meshExtension == ".dae" || meshExtension == ".fbx")
        {
            meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                return false;
            }
        }
        else
        {
            Debug.LogError("The file extension is illegal!");
            return false;
        }

        // Rigidbody Attributes
        meshFilter.gameObject.AddComponent<RigidbodyAttr>();
        meshFilter.gameObject.AddComponent<Rigidbody>();

        // VHACD 
        //meshFilter.gameObject.AddComponent<VHACD>();

        // AssetBundle.

        string prefabPath = Path.Combine(prefabFolder, prefabName + prefabSuffix);
        PrefabUtility.SaveAsPrefabAsset(meshFilter.gameObject, prefabPath);
        DestroyImmediate(gameObject);
        AssetDatabase.Refresh();
        Debug.Log(string.Format("Save prefab at {0}.", prefabPath));

        return true;
    }

    [MenuItem("RFUniverse/Create Rigidbody Prefabs/Create Prefabs From Selected Meshes")]
    // You can select one or more meshes together.
    static void CreatePrefabsFromSelectedMeshes()
    {
        if (Selection.assetGUIDs.Length == 0)
        {
            Debug.Log("Didn't create any prefabs; Nothing was selected!");
            return;
        }
        foreach (string guid in Selection.assetGUIDs)
        {
            string meshPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!CreateRigidbodyPrefab.CreatePrefabFromPath(meshPath))
            {
                Debug.LogError(string.Format("{0} is not a valid mesh, please check!", meshPath));
            }
        }
    }

    [MenuItem("RFUniverse/Create Rigidbody Prefabs/Create Prefabs From Select Folder")]
    // The file system should be like this:
    // ROOT (this is what you select)
    // |-- MeshName1
    // |   |-- xxx.obj (or xxx.dae, xxx.fbx, ...)
    // |   |-- xxx.mtl (if needed)
    // |   `-- ...
    // |-- MeshName2
    // |   |-- xxx.obj (or xxx.dae, xxx.fbx, ...)
    // |   `-- ...
    // `-- ...
    // By default, there's only one mesh under each 'MeshNameX' folder.
    static void CreatePrefabsFromSelectedFolder()
    {
        if (Selection.assetGUIDs.Length == 0)
        {
            Debug.Log("Didn't create any prefabs; Nothing was selected!");
            return;
        }
        else if (Selection.assetGUIDs.Length > 1)
        {
            Debug.Log("Please select one folder!");
            return;
        }

        // Check validation of ROOT
        string rootFolderPath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
        DirectoryInfo rootInfo = new DirectoryInfo(rootFolderPath);
        if (!rootInfo.Exists)
        {
            Debug.Log("You are not selecting a folder!");
            return;
        }

        // Get all MeshNames and deal with them.
        DirectoryInfo[] meshNames = rootInfo.GetDirectories();
        foreach (DirectoryInfo meshNameInfo in meshNames)
        {
            // Debug.Log(meshNameInfo.Name);
            FileInfo[] files = meshNameInfo.GetFiles();
            foreach (FileInfo fileInfo in files)
            {
                if (!legalExtensions.Contains(fileInfo.Extension))
                {
                    continue;
                }
                string meshPath = Path.Combine(rootFolderPath, meshNameInfo.Name, fileInfo.Name);
                if (CreateRigidbodyPrefab.CreatePrefabFromPath(meshPath, meshNameInfo.Name))
                {
                    break;
                }
            }
        }
    }
}
