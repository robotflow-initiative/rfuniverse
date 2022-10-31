using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RFUniverse.EditMode;
using System.Linq;

public class AddToAssetsData : EditorWindow
{

    [MenuItem("RFUniverse/Edit/AddToAssetsData")]
    static void ShowWindow()
    {
        GetWindow<AddToAssetsData>("AddToAssetsData");
    }
    string group;
    private void OnGUI()
    {
        group = GUILayout.TextField(group);
        if (GUILayout.Button("Add"))
        {
            Go();
        }
    }
    void Go()
    {
        EditAssetsData data = AssetDatabase.LoadAssetAtPath<EditAssetsData>("Assets/EditMode/AssetsData.asset");
        EditTypeData type = data.typeData.SingleOrDefault(s => s.name == group);
        if (type == null)
        {
            type = new EditTypeData
            {
                name = group,
            };
            data.typeData.Add(type);
        }
        foreach (var item in Selection.GetFiltered<Sprite>(SelectionMode.Deep))
        {
            Debug.Log("1");
            type.attrs.Add(new EditAttrData
            {
                name = item.name,
                image = item,
                displayName = item.name
            });
        }
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssetIfDirty(data);
        AssetDatabase.Refresh();
    }
}
