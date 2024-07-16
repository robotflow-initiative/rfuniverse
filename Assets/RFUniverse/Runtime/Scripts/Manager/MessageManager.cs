using RFUniverse;
using Robotflow.RFUniverse.SideChannels;
using System.Collections.Generic;
using System.Linq;
using System;

public class MessageManager : SingletonBase<MessageManager>, IReceiveData
{
    private MessageManager()
    {
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
