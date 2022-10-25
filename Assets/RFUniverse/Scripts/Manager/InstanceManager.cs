using RFUniverse.Attributes;
using Robotflow.RFUniverse.SideChannels;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

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
            if (BaseAgent.Instance)
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
            OutgoingMessage msg = new OutgoingMessage();
            List<BaseAttr> activeAttr = BaseAttr.Attrs.Where((s) => s.Value.gameObject.activeInHierarchy).Select((s) => s.Value).ToList();
            msg.WriteInt32(activeAttr.Count);
            foreach (var attr in activeAttr)
            {
                msg.WriteInt32(attr.ID);
                msg.WriteString(attr.Type);
                attr.CollectData(msg);
            }
            channel.SendMetaDataToPython(msg);
        }
    }
}

