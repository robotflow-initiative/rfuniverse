using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using System.IO;

namespace RFUniverse
{
    public class SDKExporter : Editor
    {
        [MenuItem("RFUniverse/Build Release/Unity Package SDK", false, 3)]
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
                "Assets/Plugins/BioIK/BioIK.asmref",
                "Assets/RFUniverse/Version.txt"
            };
            File.WriteAllText($"{Application.dataPath}/RFUniverse/Version.txt", Application.version);
            AssetDatabase.ExportPackage(filePaths, $"{Application.dataPath}/../Build/RFUniverse_Core_SDK_v{Application.version}.unitypackage", ExportPackageOptions.Interactive | ExportPackageOptions.Recurse);
            //PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
        }
    }
}
