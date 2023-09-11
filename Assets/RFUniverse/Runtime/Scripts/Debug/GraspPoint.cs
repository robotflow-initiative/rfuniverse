using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RFUniverse.Manager;

namespace RFUniverse.DebugTool
{
    public class GraspPoint : MonoBehaviour
    {
        public Transform target;
        public Canvas canvas;
        public TextMeshProUGUI text;
        void FixedUpdate()
        {
            if (DebugManager.Instance.IsDebugGraspPoint && target && target.gameObject.activeInHierarchy)
            {
                canvas.gameObject.SetActive(true);
                transform.position = target.position;
                text.text = $"position:{target.position.ToString("f2")}\nrotation euler:{target.eulerAngles.ToString("f2")}\nrotation qura:{target.rotation.ToString("f2")}";
                canvas.transform.rotation = Camera.main.transform.rotation;
            }
            else
            {
                canvas.gameObject.SetActive(false);
            }
        }
    }
}
