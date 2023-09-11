using Robotflow.RFUniverse.SideChannels;
using RFUniverse.Manager;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

namespace RFUniverse.Attributes
{
    /// <summary>
    /// This is an example of custom attribute class, without actual functions.
    /// </summary>
    public class CustomAttr : BaseAttr
    {
        public override void Init()
        {
            base.Init();
            //Register the receiving function of the dynamic message
            AssetManager.Instance.AddListener("DynamicMessage", DynamicMessage);
            //Register the receiving function of the dynamic object
            AssetManager.Instance.AddListenerObject("DynamicObject", DynamicObject);
        }

        public override Dictionary<string, object> CollectData()
        {
            //1. First, complete the message parsing in parent class.
            Dictionary<string, object> data = base.CollectData();
            //2. Write the message in order.
            data.Add("custom_message", "This is instance channel custom message");
            return data;
        }

        public override void AnalysisData(string type, object[] data)
        {
            //1. Switch to implementation based on message type
            switch (type)
            {
                case "CustomMessage":
                    CustomMessage((string)data[0]);
                    return;
            }
            //2. The message analysis of the base class continues
            base.AnalysisData(type, data);
        }

        void CustomMessage(string s)
        {
            //Read the message from python in order.
            //Note that the reading order here should align with 
            //the writing order in CustomMessage() of custom_attr.py.
            Debug.Log(s);
        }

        void DynamicMessage(IncomingMessage msg)
        {
            //Read the message from python in order.
            //Note that the reading order here should align with 
            //the writing order in env.SendMessage() of test_custom_message.py.
            Debug.Log(msg.ReadString());
            Debug.Log(msg.ReadString());
            Debug.Log(msg.ReadString());
            Debug.Log(msg.ReadInt32());
            Debug.Log(msg.ReadString());
            Debug.Log(msg.ReadBoolean());
            Debug.Log(msg.ReadString());
            Debug.Log(msg.ReadFloat32());
            Debug.Log(msg.ReadString());
            Debug.Log(msg.ReadFloatList());

            //The SendMessage function can be called anywhere at any time
            //Supported parameter types: string, int, float, bool, List<float>
            AssetManager.Instance.SendMessage(
                "DynamicMessage",
                "string:",
                "This is dynamic message",
                "int:",
                123,
                "float:",
                456f,
                "bool:",
                false,
                "list:",
                (new float[] { 7.89f, 1.11f }).ToList()
                );
        }
        void DynamicObject(object[] objs)
        {
            //Read the message from python in order.
            //Note that the reading order here should align with 
            //the writing order in env.SendObject() of test_custom_message.py.
            Debug.Log((string)objs[0]);
            Debug.Log((string)objs[1]);
            Debug.Log((string)objs[2]);
            Debug.Log((int)objs[3]);
            Debug.Log((string)objs[4]);
            Debug.Log((bool)objs[5]);
            Debug.Log((string)objs[6]);
            Debug.Log((float)objs[7]);
            Debug.Log((string)objs[8]);
            Debug.Log(objs[9].ConvertType<List<float>>());
            Debug.Log((string)objs[10]);
            Debug.Log(objs[11].ConvertType<Dictionary<string, int>>());
            Debug.Log((string)objs[12]);
            Debug.Log(objs[13].ConvertType<Tuple<string, int, float>>());
            //The SendObject function can be called anywhere at any time
            //Supported parameter types: string, int, float, bool, List, Dictionary, Tuple, Array
            AssetManager.Instance.SendObject(
                "DynamicObject",
                "string:",
                "This is dynamic object",
                "int:",
                123,
                "float:",
                456f,
                "bool:",
                false,
                "list:",
                (new float[] { 7.89f, 1.11f }).ToList(),
                "dict:",
                new Dictionary<string, int>() { { "a", 1 }, { "b", 2 } },
                "tuple:",
                new Tuple<string, int, float>("a", 1, 0.562f)
                );
        }
    }

}