using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System;
using System.Linq;
using RFUniverse;
using RFUniverse.Attributes;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;

public class PythonClassGenerater
{
    [MenuItem("Assets/Generate Python Class", true)]
    static bool Validate()
    {
        return Selection.activeObject is MonoScript;
    }

    [MenuItem("Assets/Generate Python Class")]
    static void Generate()
    {
        MonoScript obj = (MonoScript)Selection.activeObject;
        Type type = obj.GetClass();
        Debug.Log("Type: " + type.Name);
        string pythonClass = GeneratePythonClass(type);
        string fileName = AssetDatabase.GetAssetPath(obj);
        File.WriteAllText($"{Path.GetDirectoryName(fileName)}/{ConvertCamelCaseToUnderscore(Path.GetFileNameWithoutExtension(fileName))}.py", pythonClass);
    }

    static string GeneratePythonClass(Type type)
    {
        if (type == typeof(BaseAttr))
            throw new NotImplementedException($"BaseAttr can not generate python class");
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"import numpy as np");
        sb.AppendLine($"import pyrfuniverse.attributes as attr");
        sb.AppendLine($"");
        sb.AppendLine($"");

        sb.AppendLine($"class {type.Name}(attr.{type.BaseType.Name}):");
        sb.AppendLine($"    def parse_message(self, data: dict):");
        sb.AppendLine($"        super().parse_message(data)");
        sb.AppendLine($"");

        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Where(m => m.GetCustomAttribute<RFUAPIAttribute>(false) != null);

        foreach (var item in methods)
        {
            RFUAPIAttribute attr = item.GetCustomAttribute<RFUAPIAttribute>(true);
            if (attr != null)
            {
                sb.AppendLine($"    def {attr.Hand ?? item.Name}(self{string.Join("", item.GetParameters().Select(p => $", {ConvertCamelCaseToUnderscore(p.Name)}: {CSharpTypeToPythonType(p.ParameterType)}"))}):");
                sb.AppendLine($"        self._send_data(\"{attr.Hand ?? item.Name}\"{string.Join("", item.GetParameters().Select(p => $", {ConvertCamelCaseToUnderscore(p.Name)}"))})");
                sb.AppendLine($"");
            }
        }
        return sb.ToString();
    }

    private static string CSharpTypeToPythonType(Type type)
    {
        if (type == typeof(int))
            return "int";
        else if (type == typeof(string))
            return "str";
        else if (type == typeof(bool))
            return "bool";
        else if (type == typeof(float))
            return "float";
        else if (type == typeof(Array))
            return "np.ndarray";
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            return "list";
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            return "dict";
        else if (type == typeof(Tuple))
            return "tuple";


        throw new NotImplementedException($"Type not implemented for {type.Name}");
    }

    static string ConvertCamelCaseToUnderscore(string str)
    {
        return Regex.Replace(str, "(?<=.)([A-Z])", "_$0", RegexOptions.Compiled).ToLower();
    }
}