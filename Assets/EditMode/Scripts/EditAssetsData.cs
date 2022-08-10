using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RFUniverse.EditMode
{
    [Serializable]
    public class EditTypeData
    {
        public Sprite image;
        public string name;
        public List<EditAttrData> attrs;
    }
    [Serializable]
    public class EditAttrData
    {
        public Sprite image;
        public string name;
        public string displayName;
    }

    [CreateAssetMenu]
    public class EditAssetsData : ScriptableObject
    {
        public List<EditTypeData> typeData;
        public Sprite GetImageWithName(string name)
        {
            foreach (var item in typeData)
            {
                foreach (var i in item.attrs)
                {
                    if (name == i.name)
                        return item.image;
                }
            }
            return null;
        }
    }


}
