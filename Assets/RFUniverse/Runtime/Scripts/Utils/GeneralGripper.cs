using RFUniverse.Attributes;
using System;
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

        public void OpenDirectly()
        {
            foreach (var item in fingers)
            {
                ArticulationDrive temp = item.body.xDrive;
                item.body.GetUnit().SetJointPositionDirectly(Mathf.Clamp(item.position.x, temp.lowerLimit, temp.upperLimit));
            }
        }
        public void CloseDirectly()
        {
            foreach (var item in fingers)
            {
                ArticulationDrive temp = item.body.xDrive;
                item.body.GetUnit().SetJointPositionDirectly(Mathf.Clamp(item.position.y, temp.lowerLimit, temp.upperLimit));
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

            if (GUILayout.Button("GetMoveableJointData"))
            {
                script.fingers = new List<GeneralGripper.FingerData>();
                foreach (var item in script.GetComponent<ControllerAttr>().jointParameters)
                {
                    if (item.moveable)
                        script.fingers.Add(new GeneralGripper.FingerData
                        {
                            body = item.body,
                            position = new Vector2(item.body.xDrive.lowerLimit, item.body.xDrive.upperLimit)
                        }); ;
                    ;
                }
            }
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
