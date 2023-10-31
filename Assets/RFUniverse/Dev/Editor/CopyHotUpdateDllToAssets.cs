#if UNITY_EDITOR && HYBRID_CLR
using HybridCLR.Editor.Settings;
using RFUniverse;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CopyHotUpdateDllToAssets : Editor
{
    [MenuItem("RFUniverse/Copy Hot Update Dll To Assets")]
    public static void Copy()
    {
        string platform = EditorUserBuildSettings.activeBuildTarget.ToString();
        string sourcePath = $"{Application.dataPath}/../HybridCLRData/HotUpdateDlls/{platform}";
        string destPath = $"{Application.dataPath}/RFUniverse/Dev/HotUpdateDlls";
        Debug.Log(sourcePath);
        foreach (var item in HybridCLRSettings.Instance.hotUpdateAssemblyDefinitions)
        {
            string source = $"{sourcePath}/{item.name}.dll";
            string dest = $"{destPath}/{item.name}.dll.bytes";
            if (File.Exists(source))
            {
                if (!Directory.Exists(destPath))
                    Directory.CreateDirectory(destPath);
                File.Copy(source, dest, true);
                Debug.Log(dest);
            }
        }
        sourcePath = $"{Application.dataPath}/../HybridCLRData/AssembliesPostIl2CppStrip/{platform}";
        destPath = $"{Application.dataPath}/RFUniverse/Dev/HotUpdateDlls";
        foreach (var item in AOTGenericReferences.PatchedAOTAssemblyList)
        {
            string source = $"{sourcePath}/{item}";
            string dest = $"{destPath}/{item}.bytes";
            if (File.Exists(source))
            {
                if (!Directory.Exists(destPath))
                    Directory.CreateDirectory(destPath);
                File.Copy(source, dest, true);
                Debug.Log(dest);
            }
        }

        HotUpdateAsset hotUpdateAsset = AssetDatabase.LoadAssetAtPath<HotUpdateAsset>("Assets/RFUniverse/Dev/HotUpdateAsset.asset");
        hotUpdateAsset.patchNumber++;
        EditorUtility.IsDirty(hotUpdateAsset);
        AssetDatabase.SaveAssetIfDirty(hotUpdateAsset);
        AssetDatabase.Refresh();
    }
}
#endif
