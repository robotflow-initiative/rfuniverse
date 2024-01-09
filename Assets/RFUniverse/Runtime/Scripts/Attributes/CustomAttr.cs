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

            //Register the receiving function of the dynamic object
            PlayerMain.Instance.AddListenerObject("DynamicObject", DynamicObject);
        }

        public override void AddPermanentData(Dictionary<string, object> data)
        {
            //(Optional) If you need, Add base class data.
            base.AddPermanentData(data);
            //Write the data
            data["custom_message"] = "This is instance channel custom message";
        }

        //Methods that add this attribute can be call by python.
        [RFUAPI]
        //new implementation function
        void CustomMessage(string s)
        {
            Debug.Log(s);
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

            //The next frame after adding data will take effect
            CollectData.AddDataNextStep("temp_data", "TempData");

            //The SendObject function can be called anywhere at any time
            //Supported parameter types: string, int, float, bool, List, Dictionary, Tuple, Array
            PlayerMain.Instance.SendObject(
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
                new[] { 7.89f, 1.11f }.ToList(),
                "dict:",
                new Dictionary<string, int>() { { "a", 1 }, { "b", 2 } },
                "tuple:",
                new Tuple<string, int, float>("a", 1, 0.562f)
                );
        }
    }

}