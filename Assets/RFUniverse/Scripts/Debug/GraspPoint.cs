using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace RFUniverse.DebugTool
{
    public class GraspPoint : MonoBehaviour
    {
        public Transform target;
        public Canvas canvas;
        public TextMeshProUGUI text;
        void Update()
        {
            if (target)
            {
                gameObject.SetActive(true);
                transform.position = target.position;
                text.text = $"position:{target.position}\nrotation euler:{target.eulerAngles}\nrotation qura:{target.rotation}\nscale: {target.lossyScale}";
                canvas.transform.rotation = Camera.main.transform.rotation;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
