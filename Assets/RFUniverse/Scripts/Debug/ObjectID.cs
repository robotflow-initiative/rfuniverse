using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RFUniverse.Attributes;

namespace RFUniverse.DebugTool
{
    public class ObjectID : MonoBehaviour
    {
        public BaseAttr target;
        public Canvas canvas;
        public TextMeshProUGUI text;
        void FixedUpdate()
        {
            if (target && target.gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
                transform.position = target.transform.position;
                text.text = $"ID:{target.ID}";
                canvas.transform.rotation = Camera.main.transform.rotation;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
