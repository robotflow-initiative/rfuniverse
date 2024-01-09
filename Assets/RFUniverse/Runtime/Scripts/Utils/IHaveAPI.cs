using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RFUniverse
{
    public interface IHaveAPI
    {
        static Dictionary<Type, Dictionary<string, MethodInfo>> ApiMap = new Dictionary<Type, Dictionary<string, MethodInfo>>();
        void RegisterAPI()
        {
            if (ApiMap.ContainsKey(GetType())) return;
            ApiMap[GetType()] = new Dictionary<string, MethodInfo>();
            var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var item in methods)
            {
                RFUAPIAttribute attr = item.GetCustomAttribute<RFUAPIAttribute>(true);
                if (attr != null)
                    ApiMap[GetType()][attr.Hand ?? item.Name] = item;
            }
        }
        void CallAPI(string hand, object[] data)
        {
            if (ApiMap.ContainsKey(GetType()))
            {
                if (ApiMap[GetType()].ContainsKey(hand))
                {
                    var param = ApiMap[GetType()][hand].GetParameters();
                    object[] parameters = new object[param.Length];
                    for (int i = 0; i < param.Length; i++)
                    {
                        if (i < data.Length)
                            parameters[i] = data[i].ConvertObjectType(param[i].ParameterType);
                        else
                            parameters[i] = param[i].RawDefaultValue;
                    }
                    ApiMap[GetType()][hand].Invoke(this, parameters);
                    Debug.Log($"{GetType().Name}: {hand}");
                    return;
                }
                else
                {
                    Debug.LogWarning($"Type: {GetType().Name} Method: {hand} not Register");
                }
            }
            else
            {
                Debug.LogWarning($"Type: {GetType().Name} not Register");
            }
        }
    }
}
