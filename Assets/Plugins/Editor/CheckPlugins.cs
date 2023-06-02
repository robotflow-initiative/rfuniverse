using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using Unity.EditorCoroutines.Editor;
using System.Collections;

public class CheckPlugins : Editor
{
    [MenuItem("RFUniverse/Check Plugins")]
    private static void Check()
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(Main());
    }
    private static IEnumerator Main()
    {
        string[] packageNames =
{
            "com.unity.textmeshpro",
            "com.unity.addressables",
            "com.unity.nuget.newtonsoft-json",
            "com.unity.barracuda",
            };
        string[] packageAddNames =
        {
            "com.unity.textmeshpro",
            "com.unity.addressables",
            "com.unity.nuget.newtonsoft-json",
            "com.unity.barracuda",
            };

        ListRequest listRequest = Client.List();
        yield return new WaitUntil(() => listRequest.IsCompleted);
        string[] packageList = listRequest.Result.Select((s) => s.name).ToArray();
        for (int i = 0; i < packageNames.Length; i++)
        {
            if (!packageList.Contains(packageNames[i]))
            {
                Debug.Log(packageNames[i]);
                AddRequest addRequest = Client.Add(packageAddNames[i]);
                yield return new WaitUntil(() =>
                {
                    bool userCancel = EditorUtility.DisplayCancelableProgressBar("CheckPlugins", $"Getting Plugin: {packageNames[i]}", 0);
                    return userCancel || addRequest.IsCompleted;
                });
                EditorUtility.ClearProgressBar();
                yield return null;
            }
        }

        List<string> defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Split(';').ToList();

        bool exist = System.IO.Directory.Exists($"{Application.dataPath}/Plugins/BioIK");
        if (exist && !defines.Contains("BIOIK"))
        {
            Debug.Log("BIOIK plugin detected,Add BIOIK DefineSymbols");
            defines.Add("BIOIK");
            string tmpPath = $"{Application.dataPath}/Plugins/Editor/BioIK.cs.backup";
            string bioikPath = $"{Application.dataPath}/Plugins/BioIK/BioIK.cs";
            if (File.Exists(tmpPath))
            {
                File.Copy(tmpPath, bioikPath, true);
            }
        }
        else if (!exist && defines.Contains("BIOIK"))
        {
            UnityEngine.Debug.Log("BIOIK plugin undetected,Remove BIOIK DefineSymbols");
            defines.Remove("BIOIK");
        }

        exist = System.IO.Directory.Exists($"{Application.dataPath}/Plugins/Obi");
        if (exist && !defines.Contains("OBI"))
        {
            UnityEngine.Debug.Log("OBI plugin detected,Add OBI DefineSymbols");
            defines.Add("OBI");
        }
        else if (!exist && defines.Contains("OBI"))
        {
            UnityEngine.Debug.Log("OBI plugin undetected,Remove OBI DefineSymbols");
            defines.Remove("OBI");
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines.ToArray());
        yield break;
    }
}