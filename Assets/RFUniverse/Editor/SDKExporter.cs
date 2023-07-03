using UnityEngine;
using UnityEditor;

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
                "Assets/RFUniverse",
                "Assets/Plugins/Editor",
                "Assets/Plugins/Demigiant",
                "Assets/Plugins/HeatMap",
                "Assets/Plugins/URDF-Importer"
            };
            AssetDatabase.ExportPackage(filePaths, $"{System.Environment.CurrentDirectory}/Build/RFUniverse_Core_SDK_v{Application.version}.unitypackage", ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeLibraryAssets);
            //PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
        }
    }
}
