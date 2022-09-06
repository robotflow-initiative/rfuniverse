using UnityEngine;
using RFUniverse.Attributes;
using System.Collections.Generic;

namespace RFUniverse
{
    public static class RFUniverseUtility
    {
        public static Color EncodeIDAsColor(int instanceId)
        {
            int r = (instanceId * 16807 + 187) % 256;
            int g = (instanceId * 48271 + 79) % 256;
            int b = (instanceId * 95849 + 233) % 256;
            return new Color32((byte)r, (byte)g, (byte)b, 255);
        }

        public static List<T> GetChildComponentFilter<T>(this BaseAttr attr) where T : Component
        {
            List<T> components = new List<T>();
            foreach (var item in attr.GetComponentsInChildren<T>())
            {
                if (item.GetComponentInParent<BaseAttr>() == attr)
                    components.Add(item);
            }
            return components;
        }

        public static Transform FindChlid(this Transform parent, string targetName, bool includeSelf = true)
        {
            if (includeSelf && parent.name == targetName)
                return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = FindChlid(parent.GetChild(i), targetName, true);
                if (child == null)
                    continue;
                else
                    return child;
            }
            return null;
        }
    }
}
