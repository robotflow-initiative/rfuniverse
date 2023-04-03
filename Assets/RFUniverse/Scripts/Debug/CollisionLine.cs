using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RFUniverse.DebugTool
{
    public class CollisionLine : MonoBehaviour
    {
        public Transform point1, point2;
        public LineRenderer line;
        void FixedUpdate()
        {
            if (point1 && point2)
            {
                line.SetPosition(0, point1.transform.position);
                line.SetPosition(1, point2.transform.position);
            }
        }
    }
}
