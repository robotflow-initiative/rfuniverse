using Robotflow.RFUniverse.SideChannels;
using RFUniverse.Manager;
using UnityEngine;
using System.Linq;

namespace RFUniverse.Attributes
{
    public class CustomAttr : BaseAttr
    {
        public override void Init()
        {
            base.Init();
            AssetManager.Instance.AddListener("DynamicMessage", ListenerMessage);
        }
        //数据发送示例
        public override void CollectData(OutgoingMessage msg)
        {
            //先完成所继承的基类的数据写入
            base.CollectData(msg);
            //按顺序写入数据
            //此处写入顺序对应pyrfuniverse的custom_attr脚本parse_message函数中的读取顺序
            msg.WriteString("this is a instance channel unity to python custom message");
        }
        //新增接口示例
        public override void AnalysisMsg(IncomingMessage msg, string type)
        {
            //根据头字符串分支
            switch (type)
            {
                case "CustomMessage":
                    CustomMessage(msg);
                    return;
            }
            base.AnalysisMsg(msg, type);
        }
        //接口实现
        void CustomMessage(IncomingMessage msg)
        {
            //按顺序读取数据
            //此处写入读取对应pyrfuniverse的custom_attr脚本CustomMessage函数中的写入顺序
            string str = msg.ReadString();
            Debug.Log(str);
        }

        //ListenerMessage实现
        void ListenerMessage(IncomingMessage msg)
        {
            Debug.Log(msg.ReadInt32());
            Debug.Log(msg.ReadString());
            Debug.Log(msg.ReadBoolean());
            Debug.Log(msg.ReadFloat32());
            Debug.Log(msg.ReadFloatList());

            AssetManager.Instance.SendMessage(
                "DynamicMessage",
                "this is a unity to python dynamic message",
                987654,
                649849f,
                false,
                (new float[] { 1321f, 989898f }).ToList()
                );
        }
    }

}