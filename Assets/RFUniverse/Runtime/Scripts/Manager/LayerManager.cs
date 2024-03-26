using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace RFUniverse.Manager
{
    public class LayerManager : SingletonBase<LayerManager>
    {
        private LayerManager() { }
        public int tempLayer;
        public Dictionary<int, bool> layerPool = new();
        public void SetLayerPool(LayerMask layerPool)
        {
            this.layerPool.Clear();
            int layerMaskValue = layerPool.value;
            for (int i = 0; i < 32; i++)
            {
                if ((layerMaskValue & (1 << i)) != 0)
                {
                    this.layerPool.Add(i, true);
                }
            }
        }

        public int GetLayer()
        {
            int index = layerPool.First(s => s.Value == true).Key;
            layerPool[index] = false;
            return index;
        }

        public void RevertLayer(int layer)
        {
            if (layerPool.ContainsKey(layer))
                layerPool[layer] = true;
        }

        public void ReceiveDebugData(object[] data)
        {
            string type = (string)data[0];
            data = data.Skip(1).ToArray();
            switch (type)
            {
                default:
                    Debug.LogWarning("Dont have mehond:" + type);
                    break;
            }
        }
    }
}
