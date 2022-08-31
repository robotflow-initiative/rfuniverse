using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace RFUniverse
{
    public class SDKExporter : Editor
    {
        string[] sdkFiles;
        [MenuItem("RFUniverse/ExportRFUniverseSDK")]
        public static void Export()
        {
            Object[] files = Selection.GetFiltered<Object>(SelectionMode.Assets);
            List<string> filePaths = new List<string>();
            foreach (var item in files)
            {
                Debug.Log(1);
                filePaths.Add(AssetDatabase.GetAssetPath(item));
            }
            AssetDatabase.ExportPackage(filePaths.ToArray(), System.Environment.CurrentDirectory + "/SDK.unitypackage");
        }
    }
}
