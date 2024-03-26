using RFUniverse;
using Robotflow.RFUniverse.SideChannels;
using System.Collections.Generic;
using System.Linq;
using System;
using RFUniverse.Manager;

public class MessageManager : SingletonBase<MessageManager>, IReceiveData
{
    private MessageManager()
    {
    }

    Dictionary<string, Action<IncomingMessage>> registeredMessages = new Dictionary<string, Action<IncomingMessage>>();
    public void ReceiveMessageData(object[] data)
    {
        string message = (string)data[0];
        data = data.Skip(1).ToArray();
        if (registeredMessages.TryGetValue(message, out Action<IncomingMessage> action))
        {

            action?.Invoke(new IncomingMessage((byte[])data[0]));
        }
    }

    [Obsolete("AddListener is the older interface, and AddListenerObject is the recommended interface for dynamic messaging")]
    public void AddListener(string message, Action<IncomingMessage> action)
    {
        if (registeredMessages.ContainsKey(message))
        {
            registeredMessages[message] = action;
        }
        else
        {
            registeredMessages.Add(message, action);
        }
    }

    [Obsolete("RemoveListener is the older interface, and RemoveListenerObject is the recommended interface for dynamic messaging")]
    public void RemoveListener(string message)
    {
        if (registeredMessages.ContainsKey(message))
        {
            registeredMessages.Remove(message);
        }
    }
    [Obsolete("SendMessage is the older interface, and SendObject is the recommended interface for dynamic messaging")]
    public void SendMessage(string message, params object[] objects)
    {
        OutgoingMessage msg = new OutgoingMessage();
        foreach (var item in objects)
        {
            if (item is int)
                msg.WriteInt32((int)item);
            if (item is float)
                msg.WriteFloat32((float)item);
            if (item is string)
                msg.WriteString((string)item);
            if (item is bool)
                msg.WriteBoolean((bool)item);
            if (item is List<float>)
                msg.WriteFloatList((List<float>)item);
            if (item is List<bool>)
            {
                List<bool> data = (List<bool>)item;
                msg.WriteInt32(data.Count);
                foreach (var i in data)
                {
                    msg.WriteBoolean(i);
                }
            }
        }
        PlayerMain.Communicator?.SendObject("Message", message, msg.buffer);
    }

    Dictionary<string, Action<object[]>> registeredObjects = new Dictionary<string, Action<object[]>>();
    public void ReceiveData(object[] data)
    {
        string head = (string)data[0];
        data = data.Skip(1).ToArray();
        if (registeredObjects.TryGetValue(head, out Action<object[]> action))
        {
            action?.Invoke(data);
        }
    }
    public void AddListenerObject(string head, Action<object[]> action)
    {
        if (registeredObjects.ContainsKey(head))
        {
            registeredObjects[head] = action;
        }
        else
        {
            registeredObjects.Add(head, action);
        }
    }
    public void RemoveListenerObject(string head)
    {
        if (registeredObjects.ContainsKey(head))
        {
            registeredObjects.Remove(head);
        }
    }
    public void SendObject(string head, params object[] objects)
    {
        object[] data = new[] { "Object", head };
        PlayerMain.Communicator?.SendObject(data.Concat(objects).ToArray());
    }
}
