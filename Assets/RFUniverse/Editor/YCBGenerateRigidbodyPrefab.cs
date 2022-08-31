using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class YCBGenerateRigidbodyPrefab : Editor
{
    [MenuItem("RFUniverse/YCBGenerateRigidbodyPrefab")]
    public static void Generate()
    {
        Object[] objs = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
        Debug.Log(objs.Length);
        for (int i = 0; i < objs.Length; i++)
        {
            Object item = objs[i];

            string file = AssetDatabase.GetAssetPath(item);
            string extension = Path.GetExtension(file);
            if (extension != ".obj") continue;
            string path = Path.GetDirectoryName(file);
            string pathName = Path.GetFileName(path);
            if (pathName != "google_16k") continue;
            string name = Path.GetDirectoryName(path);
            name = Path.GetFileName(name);

            Debug.Log(item.name);

            GameObject prefab = GameObject.Instantiate((GameObject)item);
            //prefab.AddComponent<RigidbodyAttr>().AddVHACD();

            PrefabUtility.SaveAsPrefabAsset(prefab, $"{UnityEngine.Application.dataPath}/Prefabs/Addressable/Rigidbody/YCB/{name}.prefab");
            DestroyImmediate(prefab);
        }
    }
}
