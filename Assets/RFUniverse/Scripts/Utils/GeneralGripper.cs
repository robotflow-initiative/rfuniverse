using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RFUniverse
{
    public class GeneralGripper : MonoBehaviour, ICustomGripper
    {
        [System.Serializable]
        public class FingerData
        {
            public ArticulationBody body;
            public Vector2 position;
        }
        public List<FingerData> fingers;
        public void Open()
        {
            foreach (var item in fingers)
            {
                ArticulationDrive temp = item.body.xDrive;
                temp.target = item.position.x;
                item.body.xDrive = temp;
            }
        }
        public void Close()
        {
            foreach (var item in fingers)
            {
                ArticulationDrive temp = item.body.xDrive;
                temp.target = item.position.y;
                item.body.xDrive = temp;
            }
        }
    }
}
