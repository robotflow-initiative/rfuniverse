using System.Reflection;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;

public class CheckPlugins
{
    [InitializeOnLoadMethod]
    //[DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        Assembly assembly = Assembly.Load("Assembly-CSharp-firstpass");
        Type type = assembly.GetType("BioIK.BioObjective");
        List<string> defines = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Split(';'));
        if (type != null && !defines.Contains("BIOIK"))
        {
            UnityEngine.Debug.Log("BIOIK plugin detected,Add BIOIK DefineSymbols");
            defines.Add("BIOIK");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines.ToArray());
        }
    }
}
