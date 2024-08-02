#if HYBRID_CLR
using HybridCLR.Editor.Commands;
#endif
using RFUniverse;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager;
using UnityEngine;


public class BuildRelease
{
    public const string BUILD_PATH = "D:/Build";

    [MenuItem("RFUniverse/Build Release/All", false, 0)]
    static void Build()
    {
        BuildWindows();
        BuildLinux();
        ExportSDK();
    }
    [MenuItem("RFUniverse/Build Release/Windows", false, 1)]
    static void BuildWindows()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        Client.Resolve();
        UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
#if HYBRID_CLR
        PrebuildCommand.GenerateAll();
        CopyHotUpdateDllToAssets.Copy();
#endif
        AddressableAssetSettings.CleanPlayerContent();
        AddressableAssetSettings.BuildPlayerContent();

        string windowsPath = $"{BUILD_PATH}/RFUniverse_For_Windows_v{PlayerMain.VERSION}";
        if (System.IO.Directory.Exists(windowsPath))
            System.IO.Directory.Delete(windowsPath, true);
        System.IO.Directory.CreateDirectory(windowsPath);
        BuildReport buildReport = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, $"{windowsPath}/RFUniverse.exe", BuildTarget.StandaloneWindows64, BuildOptions.CompressWithLz4);
        if (buildReport.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Windows发布完成！");
            if (System.IO.Directory.Exists($"{windowsPath}/RFUniverse_BackUpThisFolder_ButDontShipItWithYourGame"))
                System.IO.Directory.Delete($"{windowsPath}/RFUniverse_BackUpThisFolder_ButDontShipItWithYourGame", true);
            if (System.IO.Directory.Exists($"{windowsPath}/RFUniverse_BurstDebugInformation_DoNotShip"))
                System.IO.Directory.Delete($"{windowsPath}/RFUniverse_BurstDebugInformation_DoNotShip", true);
            if (System.IO.File.Exists($"{windowsPath}.zip"))
                System.IO.File.Delete($"{windowsPath}.zip");
            ZipFile.CreateFromDirectory($"{windowsPath}", $"{windowsPath}.zip", System.IO.Compression.CompressionLevel.Optimal, true);
            EditorUtility.RevealInFinder(windowsPath);
        }
        else
            Debug.Log("Windows发布失败！");
    }
    [MenuItem("RFUniverse/Build Release/Linux", false, 2)]
    static void BuildLinux()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);
        Client.Resolve();
        UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
#if HYBRID_CLR
        PrebuildCommand.GenerateAll();
        CopyHotUpdateDllToAssets.Copy();
#endif
        AddressableAssetSettings.CleanPlayerContent();
        AddressableAssetSettings.BuildPlayerContent();

        string linuxPath = $"{BUILD_PATH}/RFUniverse_For_Linux_v{PlayerMain.VERSION}";
        if (System.IO.Directory.Exists(linuxPath))
            System.IO.Directory.Delete(linuxPath, true);
        System.IO.Directory.CreateDirectory(linuxPath);
        BuildReport buildReport = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, $"{linuxPath}/RFUniverse.x86_64", BuildTarget.StandaloneLinux64, BuildOptions.CompressWithLz4);
        if (buildReport.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Linux发布完成！");
            if (System.IO.Directory.Exists($"{linuxPath}/RFUniverse_BackUpThisFolder_ButDontShipItWithYourGame"))
                System.IO.Directory.Delete($"{linuxPath}/RFUniverse_BackUpThisFolder_ButDontShipItWithYourGame", true);
            if (System.IO.Directory.Exists($"{linuxPath}/RFUniverse_BurstDebugInformation_DoNotShip"))
                System.IO.Directory.Delete($"{linuxPath}/RFUniverse_BurstDebugInformation_DoNotShip", true);
            if (System.IO.File.Exists($"{linuxPath}.zip"))
                System.IO.File.Delete($"{linuxPath}.zip");
            ZipFile.CreateFromDirectory($"{linuxPath}", $"{linuxPath}.zip", System.IO.Compression.CompressionLevel.Optimal, true);
            EditorUtility.RevealInFinder(linuxPath);
        }
        else
            Debug.Log("Linux发布失败！");
    }

    [MenuItem("RFUniverse/Build Release/Unity Package SDK", false, 3)]
    static void ExportSDK()
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
                "Assets/Plugins/CoACD",
                "Assets/Plugins/gRPC",
                "Assets/Plugins/URDF-Importer",
                "Assets/Plugins/BioIK/BioIK.asmref",
                "Assets/TextMesh Pro"
            };
        AssetDatabase.ExportPackage(filePaths, $"{BuildRelease.BUILD_PATH}/RFUniverse_Core_SDK_v{PlayerMain.VERSION}.unitypackage", ExportPackageOptions.Interactive | ExportPackageOptions.Recurse);
        //PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
    }
}
