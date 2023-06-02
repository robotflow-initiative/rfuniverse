using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace RFUniverse
{
    public class SDKExporter : Editor
    {
        [MenuItem("RFUniverse/Export SDK Package")]
        public static void Export()
        {
            //string[] defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Split(';');
            //PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, new string[0]);
            List<string> filePaths = new List<string>();
            filePaths.Add("Assets/RFUniverse");
            filePaths.Add("Assets/Plugins/Editor");
            filePaths.Add("Assets/Plugins/Demigiant");
            filePaths.Add("Assets/Plugins/HeatMap");
            filePaths.Add("Assets/Plugins/URDF-Importer");
            filePaths.Add("Assets/Plugins/RFUniverse-Base");
            AssetDatabase.ExportPackage(filePaths.ToArray(), $"{System.Environment.CurrentDirectory}/Build/RFUniverse_Core_SDK_v{Application.version}.unitypackage", ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeLibraryAssets);
            //PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
        }
    }
}
