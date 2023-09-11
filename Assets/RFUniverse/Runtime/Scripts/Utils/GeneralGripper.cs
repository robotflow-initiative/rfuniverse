using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
                temp.target = Mathf.Clamp(item.position.x, temp.lowerLimit, temp.upperLimit);
                temp.driveType = ArticulationDriveType.Force;
                item.body.xDrive = temp;
            }
        }
        public void Close()
        {
            foreach (var item in fingers)
            {
                ArticulationDrive temp = item.body.xDrive;
                temp.target = Mathf.Clamp(item.position.y, temp.lowerLimit, temp.upperLimit);
                temp.driveType = ArticulationDriveType.Force;
                item.body.xDrive = temp;
            }
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(GeneralGripper))]
    public class GeneralGripperEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GeneralGripper script = target as GeneralGripper;

            if (EditorApplication.isPlaying)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Open"))
                {

                    script.Open();
                }
                if (GUILayout.Button("Close"))
                {
                    script.Close();
                }
                GUILayout.EndHorizontal();
            }
        }
    }
#endif
}
