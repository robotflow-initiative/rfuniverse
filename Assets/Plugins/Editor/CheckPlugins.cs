# if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.Threading.Tasks;

public class CheckPlugins
{
    [MenuItem("RFUniverse/Check Plugins (Fix Error)")]
    private static async void FixError()
    {
        string[] packageNames =
        {
            "com.unity.editorcoroutines",
            "com.unity.textmeshpro",
            "com.unity.addressables",
            "com.unity.nuget.newtonsoft-json",
            "com.unity.barracuda",
            };
        string[] packageAddNames =
        {
            "com.unity.editorcoroutines",
            "com.unity.textmeshpro",
            "com.unity.addressables",
            "com.unity.nuget.newtonsoft-json",
            "com.unity.barracuda",
            };

        ListRequest listRequest = Client.List();
        while (!listRequest.IsCompleted)
        {
            await Task.Delay(100);
        }
        string[] packageList = listRequest.Result.Select((s) => s.name).ToArray();
        for (int i = 0; i < packageNames.Length; i++)
        {
            if (!packageList.Contains(packageNames[i]))
            {
                Debug.Log("Geting " + packageNames[i]);
                AddRequest addRequest = Client.Add(packageAddNames[i]);
                while (!addRequest.IsCompleted)
                {
                    await Task.Delay(100);
                }
            }
        }

        List<string> defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Split(';').ToList();

        bool exist = Directory.Exists($"{Application.dataPath}/Plugins/BioIK/Setup");
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
            Debug.Log("BIOIK plugin undetected,Remove BIOIK DefineSymbols");
            defines.Remove("BIOIK");
        }

        exist = Directory.Exists($"{Application.dataPath}/Plugins/Obi");
        if (exist && !defines.Contains("OBI"))
        {
            Debug.Log("OBI plugin detected,Add OBI DefineSymbols");
            defines.Add("OBI");
        }
        else if (!exist && defines.Contains("OBI"))
        {
            Debug.Log("OBI plugin undetected,Remove OBI DefineSymbols");
            defines.Remove("OBI");
        }

        if (defines.Contains("HYBRID_CLR"))
        {
            Debug.Log("Remove HYBRID_CLR DefineSymbols");
            defines.Remove("HYBRID_CLR");
        }
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines.ToArray());
    }
}
#endif