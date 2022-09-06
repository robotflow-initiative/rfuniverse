using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RFUniverse
{
    public class RFUniverseMain : MonoBehaviour
    {
        public Camera mainCamera;
        public GameObject ground;
        public bool Ground
        {
            get
            {
                if (ground == null) return false;
                return ground.activeSelf;
            }
            set
            {
                ground.SetActive(value);
            }
        }

        public LayerMask simulation = 1 << 0;
        public LayerMask axis = 1 << 6;
        public int tempLayer = 21;
    }
}
