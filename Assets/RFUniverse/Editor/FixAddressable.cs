# if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace RFUniverse
{
    public class FixAddressable
    {
        [MenuItem("RFUniverse/Fix Addressable")]
        private static void Fix()
        {
            AddressableAssetSettings setting = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>("Assets/AddressableAssetsData/AddressableAssetSettings.asset");
            if (setting == null) return;
            AddressableAssetSettingsDefaultObject.Settings = setting;
            Debug.Log("AddressableAssetSettingsDefaultObject.Settings already setup");
            setting.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.BuildWithPlayer;
            Debug.Log("AddressableAssetSettings.PlayerBuildOption has been set to BuildWithPlayer");
            setting.BuildRemoteCatalog = false;
            Debug.Log("AddressableAssetSettings.BuildRemoteCatalog has been set to false");
            EditorUtility.SetDirty(setting);
            AssetDatabase.SaveAssetIfDirty(setting);

            AddressableAssetGroup group = AssetDatabase.LoadAssetAtPath<AddressableAssetGroup>("Assets/AddressableAssetsData/AssetGroups/RFUniverseBuiltin.asset");
            if (group == null) return;
            group.GetSchema<BundledAssetGroupSchema>().BuildPath.SetVariableByName(group.Settings, "LocalBuildPath");
            Debug.Log("Addressable RFUniverseBuiltin Group BuildPath has been set to LocalBuildPath");
            group.GetSchema<BundledAssetGroupSchema>().LoadPath.SetVariableByName(group.Settings, "LocalLoadPath");
            Debug.Log("Addressable RFUniverseBuiltin Group LoadPath has been set to LocalLoadPath");
            EditorUtility.SetDirty(group);
            AssetDatabase.SaveAssetIfDirty(group);

            group = AssetDatabase.LoadAssetAtPath<AddressableAssetGroup>("Assets/AddressableAssetsData/AssetGroups/Bundle.asset");
            if (group == null) return;
            group.GetSchema<BundledAssetGroupSchema>().BuildPath.SetVariableByName(group.Settings, "LocalBuildPath");
            Debug.Log("Addressable Bundle Group BuildPath has been set to LocalBuildPath");
            group.GetSchema<BundledAssetGroupSchema>().LoadPath.SetVariableByName(group.Settings, "LocalLoadPath");
            Debug.Log("Addressable Bundle Group LoadPath has been set to LocalLoadPath");
            EditorUtility.SetDirty(group);
            AssetDatabase.SaveAssetIfDirty(group);
        }
    }
}
#endif