using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RFUniverse.Attributes;
using RFUniverse.Manager;

namespace RFUniverse.DebugTool
{
    public class ObjectID : MonoBehaviour
    {
        public BaseAttr target;
        public Canvas canvas;
        public TextMeshProUGUI text;
        void FixedUpdate()
        {
            if (DebugManager.Instance.IsDebugObjectID && target && target.gameObject.activeInHierarchy)
            {
                canvas.gameObject.SetActive(true);
                transform.position = target.transform.position;
                text.text = $"ID:{target.ID}";
                canvas.transform.rotation = Camera.main.transform.rotation;
            }
            else
            {
                canvas.gameObject.SetActive(false);
            }
        }
    }
}
