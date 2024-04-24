using RFUniverse.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RFUniverse.Manager
{
    public class InstanceManager : SingletonBase<InstanceManager>, IReceiveData, IDistributeData<int>
    {
        private InstanceManager()
        {
        }

        private Dictionary<int, BaseAttr> attrs = new Dictionary<int, BaseAttr>();
        public Dictionary<int, BaseAttr> Attrs => new Dictionary<int, BaseAttr>(attrs);
        public Dictionary<int, BaseAttr> ActiveAttrs => attrs.Where((s) => s.Value.gameObject.activeInHierarchy).ToDictionary((s) => s.Key, (s) => s.Value);

        public Action OnAttrChange;

        public void AddAttr(BaseAttr attr, Action<object[]> action)
        {
            if (!attrs.ContainsKey(attr.ID))
            {
                attrs.Add(attr.ID, attr);
                (this as IDistributeData<int>).RegisterReceiver(attr.ID, action);
                OnAttrChange?.Invoke();
            }
        }
        public void RemoveAttr(BaseAttr attr)
        {
            if (attrs.ContainsKey(attr.ID))
            {
                attrs.Remove(attr.ID);
                (this as IDistributeData<int>).UnRegisterReceiver(attr.ID);
                OnAttrChange?.Invoke();
            }
        }

        Dictionary<int, Action<object[]>> IDistributeData<int>.Receiver { get; set; }

        public void ReceiveData(object[] data)
        {
            int hand = (int)data[0];
            data = data.Skip(1).ToArray();
            (this as IDistributeData<int>).DistributeData(hand, data);
        }

        public void CollectAllAttrData()
        {
            foreach (var attr in Attrs.Values)
            {
                Dictionary<string, object> data = attr.CollectData.CollectData();
                PlayerMain.Communicator?.SendObject("Instance", attr.ID, attr.GetType().Name, data);
            }
        }
    }
}