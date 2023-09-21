# if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
namespace RFUniverse
{
    public class FixAddressable
    {
        [MenuItem("RFUniverse/Fix Addressable")]
        private static void Fix()
        {
            AddressableAssetSettings setting = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>("Assets/AddressableAssetsData/AddressableAssetSettings.asset");
            setting.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.BuildWithPlayer;
            setting.BuildRemoteCatalog = false;
            EditorUtility.SetDirty(setting);
            AddressableAssetSettingsDefaultObject.Settings = setting;
            AddressableAssetGroup group = AssetDatabase.LoadAssetAtPath<AddressableAssetGroup>("Assets/AddressableAssetsData/AssetGroups/RFUniverseBuiltin.asset");
            group.GetSchema<BundledAssetGroupSchema>().BuildPath.SetVariableByName(group.Settings, "LocalBuildPath");
            group.GetSchema<BundledAssetGroupSchema>().LoadPath.SetVariableByName(group.Settings, "LocalLoadPath");
            EditorUtility.SetDirty(group);
            group = AssetDatabase.LoadAssetAtPath<AddressableAssetGroup>("Assets/AddressableAssetsData/AssetGroups/Bundle.asset");
            group.GetSchema<BundledAssetGroupSchema>().BuildPath.SetVariableByName(group.Settings, "LocalBuildPath");
            group.GetSchema<BundledAssetGroupSchema>().LoadPath.SetVariableByName(group.Settings, "LocalLoadPath");
            EditorUtility.SetDirty(group);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif