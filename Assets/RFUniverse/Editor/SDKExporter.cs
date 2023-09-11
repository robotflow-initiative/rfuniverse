using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using System.Collections;

namespace RFUniverse
{
    public class SDKExporter : Editor
    {
        [MenuItem("RFUniverse/Export SDK Package")]
        public static void Export()
        {
            //string[] defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Split(';');
            //PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, new string[0]);
            string[] filePaths = new[]
            {
                "Assets/AddressableAssetsData",
                "Assets/RFUniverse/Editor",
                "Assets/RFUniverse/Runtime",
                "Assets/Plugins/Editor",
                "Assets/Plugins/Demigiant",
                "Assets/Plugins/HeatMap",
                "Assets/Plugins/URDF-Importer",
                "Assets/Plugins/BioIK/BioIK.asmref"
            };
            AssetDatabase.ExportPackage(filePaths, $"{System.Environment.CurrentDirectory}/Build/RFUniverse_Core_SDK_v{Application.version}.unitypackage", ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeLibraryAssets);
            //PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
        }
    }
}
