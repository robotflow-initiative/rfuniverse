using UnityEngine;
using RFUniverse.Attributes;
using System;
using RFUniverse.Manager;

namespace RFUniverse.DebugTool
{
    public class DDDBBox : MonoBehaviour
    {
        public Renderer render;
        public GameObjectAttr target;
        void FixedUpdate()
        {
            if (DebugManager.Instance.IsDebug3DBBox && target && target.gameObject.activeInHierarchy)
            {
                render.gameObject.SetActive(true);
                render.material.SetColor("_Color", RFUniverseUtility.EncodeIDAsColor(target.ID));

                render.bounds = target.GetAppendBounds();
                Tuple<Vector3, Vector3, Vector3> bound = target.Get3DBBox(false);
                transform.position = bound.Item1;
                transform.eulerAngles = bound.Item2;
                render.material.SetVector("_Size", bound.Item3);
            }
            else
            {
                render.gameObject.SetActive(false);
            }
        }
    }
}
