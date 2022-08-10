using Robotflow.RFUniverse.SideChannels;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace RFUniverse.Manager
{
    public abstract class BaseManager
    {
        public BaseManager(string channel_id)
        {
            channel = new InfoChannel(channel_id);
            channel.manager = this;
            SideChannelManager.RegisterSideChannel(channel);
        }
        public InfoChannel channel;
        public virtual void ReceiveData(IncomingMessage msg) { }
    }
}
