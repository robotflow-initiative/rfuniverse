using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RFUniverse
{
    public interface IHaveAPI
    {
        static List<string> hideName = new List<string>();

        static Dictionary<Type, Dictionary<string, (MethodInfo, bool)>> ApiMap = new Dictionary<Type, Dictionary<string, (MethodInfo, bool)>>();

        void RegisterAPI()
        {
            if (ApiMap.ContainsKey(GetType())) return;
            ApiMap[GetType()] = new Dictionary<string, (MethodInfo, bool)>();
            var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var item in methods)
            {
                RFUAPIAttribute attr = item.GetCustomAttribute<RFUAPIAttribute>(true);
                if (attr != null)
                    ApiMap[GetType()][attr.Hand ?? item.Name] = (item, attr.ShowLog);
            }
        }

        void CallAPI(string hand, object[] data)
        {
            if (ApiMap.TryGetValue(GetType(), out Dictionary<string, (MethodInfo, bool)> apis))
            {
                if (apis.TryGetValue(hand, out (MethodInfo, bool) method))
                {
                    var param = method.Item1.GetParameters();
                    object[] parameters = new object[param.Length];
                    for (int i = 0; i < param.Length; i++)
                    {
                        if (i < data.Length)
                            parameters[i] = data[i].ConvertObjectType(param[i].ParameterType);
                        else
                            parameters[i] = param[i].RawDefaultValue;
                    }
                    method.Item1.Invoke(this, parameters);
                    if (method.Item2)
                        Debug.Log($"{GetType().Name}: {hand}");
                    return;
                }
                else
                {
                    if (!hideName.Contains(hand))
                    {
                        Debug.LogWarning($"Type: {GetType().Name} Method: {hand} not Register");
                        hideName.Add(hand);
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Type: {GetType().Name} not Register");
            }
        }
    }
}
