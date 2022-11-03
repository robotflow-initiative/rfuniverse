using System.Reflection;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CheckPlugins
{
    [InitializeOnLoadMethod]
    //[DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        List<string> defines = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Split(';'));

        bool exist = System.IO.Directory.Exists($"{Application.dataPath}/Plugins/BioIK/Setup");
        if (exist && !defines.Contains("BIOIK"))
        {
            UnityEngine.Debug.Log("BIOIK plugin detected,Add BIOIK DefineSymbols");
            defines.Add("BIOIK");
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
    }
}