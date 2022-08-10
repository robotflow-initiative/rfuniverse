using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RFUniverse
{
    public class PlayerMain : RFUniverseMain
    {
        public static PlayerMain Instance = null;
        void Awake()
        {
            Instance = this;
        }
    }
}
