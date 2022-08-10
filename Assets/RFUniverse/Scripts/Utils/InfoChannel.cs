using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Robotflow.RFUniverse;
using Robotflow.RFUniverse.SideChannels;
using RFUniverse.Manager;

public class InfoChannel : SideChannel
{
    //bool isUpdated;
    //public Queue<string> stringParams;
    //public Queue<float> floatParams;
    //public Queue<bool> boolParams;
    //public Queue<int> intParams;
    //public Queue<IList<float>> listParams;

    public BaseManager manager = null;
    public InfoChannel(string uuid)
    {
        // TODO: this guid may not be safe. Need modify later.
        ChannelId = new System.Guid(uuid);
        //InitParams();
    }

    //protected void InitParams()
    //{
    //    isUpdated = false;
    //    stringParams = new Queue<string>();
    //    floatParams = new Queue<float>();
    //    boolParams = new Queue<bool>();
    //    intParams = new Queue<int>();
    //    listParams = new Queue<IList<float>>();
    //}

    protected override void OnMessageReceived(IncomingMessage msg)
    {
        // The IncomingMessage always starts with a msgSign(str),
        // which indicating types of each message by order
        //if (!IsUpdated())
        //{
        //string msgSign = msg.ReadString();
        //foreach (char type in msgSign)
        //{
        //    if (type == 's')
        //        stringParams.Enqueue(msg.ReadString());
        //    else if (type == 'f')
        //        floatParams.Enqueue(msg.ReadFloat32());
        //    else if (type == 'b')
        //        boolParams.Enqueue(msg.ReadBoolean());
        //    else if (type == 'i')
        //        intParams.Enqueue(msg.ReadInt32());
        //    else if (type == 'l')
        //        listParams.Enqueue(msg.ReadFloatList());
        //    else
        //        continue;
        //}
        //IncomingMessage msg2 = new IncomingMessage(msg.GetRawBytes());
        //isUpdated = true;
        //}
        //manager?.ReceiveData(XMLHelper.XMLToObject<List<ManagerData>>(msgSign));
        manager?.ReceiveData(msg);
        //manager?.ParseExecuteAction();
        //manager?.Refresh();
    }

    public void SendMetaDataToPython(OutgoingMessage msg)
    {
        QueueMessageToSend(msg);
    }

    //public bool IsUpdated()
    //{
    //    return isUpdated;
    //}

    //public void Refresh()
    //{
    //    isUpdated = false;
    //    stringParams.Clear();
    //    floatParams.Clear();
    //    boolParams.Clear();
    //    intParams.Clear();
    //    listParams.Clear();
    //}
}
