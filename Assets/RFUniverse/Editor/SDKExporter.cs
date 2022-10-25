using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace RFUniverse
{
    public class SDKExporter : Editor
    {
        [MenuItem("RFUniverse/Export SDK Package")]
        public static void Export()
        {
            Object[] files = Selection.GetFiltered<Object>(SelectionMode.Assets);
            List<string> filePaths = new List<string>();
            filePaths.Add("Assets/RFUniverse");
            filePaths.Add("Assets/Plugins/Editor");
            AssetDatabase.ExportPackage(filePaths.ToArray(), System.Environment.CurrentDirectory + "/RFUniverse_Core_SDK.unitypackage", ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeLibraryAssets);
        }
    }
}
