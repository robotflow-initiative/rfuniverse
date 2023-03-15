using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RFUniverse.Attributes;
using RFUniverse;

namespace RFUniverse.DebugTool
{
    public class DDDBBox : MonoBehaviour
    {
        public Renderer render;
        public BaseAttr target;
        void Update()
        {
            if (target)
            {
                gameObject.SetActive(true);
                Bounds? bounds = null;
                foreach (var item in target.GetChildComponentFilter<Renderer>())
                {
                    if (bounds == null)
                        bounds = item.bounds;
                    else
                        bounds.Value.Encapsulate(item.bounds);
                }
                transform.position = bounds.Value.center;
                render.material.SetVector("_Size", bounds.Value.size);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
