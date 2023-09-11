using System;
using System.Collections.Generic;
using UnityEngine;

namespace RFUniverse
{
    [CreateAssetMenu]
    public class HotUpdateAsset : ScriptableObject
    {
        public int patchNumber; 
        public List<TextAsset> dlls;
        public List<TextAsset> aotMetadata;
    }
}

