using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RFUniverse
{
    public class RFUniverseMain : MonoBehaviour
    {
        public GameObject ground;
        public bool Ground
        {
            get
            {
                return ground.activeSelf;
            }
            set
            {
                ground.SetActive(value);
            }
        }
    }
}
