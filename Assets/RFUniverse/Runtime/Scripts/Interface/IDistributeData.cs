using System.Collections.Generic;
using System;

namespace RFUniverse
{
    public interface IDistributeData<T>
    {
        Dictionary<T, Action<object[]>> Receiver { get; set; }

        void RegisterReceiver(T hand, Action<object[]> action)
        {
            if (Receiver == null)
                Receiver = new Dictionary<T, Action<object[]>>();
            Receiver[hand] = action;
        }
        void UnRegisterReceiver(T hand)
        {
            if (Receiver == null)
                Receiver = new Dictionary<T, Action<object[]>>();
            Receiver.Remove(hand);
        }
        void DistributeData(T hand, object[] data)
        {
            if (Receiver.TryGetValue(hand, out Action<object[]> action))
                action?.Invoke(data);
        }
    }
}
