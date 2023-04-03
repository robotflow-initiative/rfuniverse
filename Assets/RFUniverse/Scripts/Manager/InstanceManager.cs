using RFUniverse.Attributes;
using Robotflow.RFUniverse.SideChannels;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Drawing.Printing;

namespace RFUniverse.Manager
{
    public class InstanceManager : BaseManager
    {
        const string UUID = "09bfcf57-9120-43dc-99f8-abeeec59df0f";

        private static InstanceManager instance = null;
        public static InstanceManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new InstanceManager(UUID);
                return instance;
            }
        }
        public InstanceManager(string channel_id) : base(channel_id)
        {
            BaseAgent.Instance.OnStepAction += CollectData;
        }
        public override void ReceiveData(IncomingMessage msg)
        {
            int id = msg.ReadInt32();
            if (BaseAttr.Attrs.ContainsKey(id))
                BaseAttr.Attrs[id].ReceiveData(msg);
            else if (AssetManager.Instance.waitingMsg.ContainsKey(id))
            {
                Debug.LogWarning($"ID:{id} is loading, add in waiting msg");
                AssetManager.Instance.waitingMsg[id].Add(new IncomingMessage(msg.GetRawBytes()));
            }
            else
                Debug.LogError($"ID:{id} not exist");
        }
        public virtual void CollectData()
        {
            //OutgoingMessage msg = new OutgoingMessage();
            //msg.WriteInt32(BaseAttr.ActiveAttrs.Count);
            foreach (var attr in BaseAttr.ActiveAttrs.Values)
            {
                OutgoingMessage msg = new OutgoingMessage();
                msg.WriteInt32(attr.ID);
                msg.WriteString(attr.GetType().Name);
                attr.CollectData(msg);
                channel.SendMetaDataToPython(msg);
            }
            //channel.SendMetaDataToPython(msg);
        }
    }
}

