using UnityEngine;
using RFUniverse.Attributes;
using System.Collections.Generic;

namespace RFUniverse
{
    public static class RFUniverseUtility
    {
        public static int SparsifyBits(byte value, int sparse)
        {
            int retVal = 0;
            for (int bits = 0; bits < 8; bits++, value >>= 1)
            {
                retVal |= (value & 1);
                retVal <<= sparse;
            }
            return retVal >> sparse;
        }

        public static Color EncodeIDAsColor(int instanceId)
        {
            var uid = instanceId * 2;
            if (uid < 0)
                uid = -uid + 1;

            var sid =
                (SparsifyBits((byte)(uid >> 16), 3) << 2) |
                (SparsifyBits((byte)(uid >> 8), 3) << 1) |
                 SparsifyBits((byte)(uid), 3);

            var r = (byte)(sid >> 8);
            var g = (byte)(sid >> 16);
            var b = (byte)(sid);
            return new Color32(r, g, b, 255);
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
