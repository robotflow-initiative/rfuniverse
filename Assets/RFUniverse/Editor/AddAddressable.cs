using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using RFUniverse.Attributes;

public class AddAddressable : Editor
{
    [MenuItem("RFUniverse/Add to Addressable (user) %G")]
    static void AddUser()
    {

        AddressableAssetSettings setting = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>("Assets/AddressableAssetsData/AddressableAssetSettings.asset");

        foreach (GameObject o in Selection.gameObjects)
        {

            AddressableAssetEntry entry = setting.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(o)), setting.FindGroup("User"));
            string s = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(o));
            entry.SetAddress(s);

            if (o.TryGetComponent(out BaseAttr attr))
            {
                attr.Name = o.name;
                PrefabUtility.SavePrefabAsset(o, out bool b);
            }
        }
    }
    [MenuItem("RFUniverse/Add to Addressable (dev) %#G")]
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
                PrefabUtility.SavePrefabAsset(o, out bool b);
            }
        }
    }
}
