using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Robotflow.RFUniverse.SideChannels;
using RFUniverse.Manager;

public class AssetManagerExt
{
    public void AnalysisMsg(IncomingMessage msg, string type)
    {
        switch (type)
        {
            //添加根据头字符串条跳转接口函数
            //此处CustomMessage对应pyrfuniverse的asset_channel_ext脚本中CustomMessage函数写入的第一个string
            case "CustomMessage":
                CustomMessage(msg);
                return;
        }
    }
    //接口实现
    void CustomMessage(IncomingMessage msg)
    {
        //根据pyrfuniverse的asset_channel_ext脚本中CustomMessage函数写入的参数读取
        string str = msg.ReadString();
        Debug.Log(str);
        //将接收到的参数发送到pyrfuniverse
        OutgoingMessage sendMsg = new OutgoingMessage();
        //第一个参数必须为头字符串
        //此处CustomMessage对应pyrfuniverse的asset_channel_ext脚本中parse_message函数中一个分支
        sendMsg.WriteString("CustomMessage");
        //按顺序写入数据
        sendMsg.WriteString("this is a asset channel unity to python custom message");
        AssetManager.Instance.SendMessage(sendMsg);
    }

}
