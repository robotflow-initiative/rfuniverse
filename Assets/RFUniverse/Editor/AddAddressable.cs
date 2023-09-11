using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using RFUniverse.Attributes;

namespace RFUniverse
{
    public class AddAddressable : Editor
    {
        [MenuItem("RFUniverse/Add to Addressable %G")]
        static void AddDefault()
        {
            AddressableAssetSettings setting = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>("Assets/AddressableAssetsData/AddressableAssetSettings.asset");

            foreach (GameObject o in Selection.gameObjects)
            {

                AddressableAssetEntry entry = setting.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(o)), setting.DefaultGroup);
                string s = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(o));
                entry.SetAddress(s);

                if (o.TryGetComponent(out BaseAttr attr))
                {
                    attr.Name = o.name;
                    PrefabUtility.SavePrefabAsset(o);
                }
            }
        }
    }
}
