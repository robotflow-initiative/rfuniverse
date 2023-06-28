using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RFUniverse.Attributes;
using RFUniverse;
using System;
using DG.Tweening.Core.Easing;
using RFUniverse.Manager;
using UnityEngine.UI;

namespace RFUniverse.DebugTool
{
    public class DDBBox : MonoBehaviour
    {
        public Image image;
        public GameObjectAttr target;
        int frame = 0;
        public static int total = 50;
        int index;
        private void Awake()
        {
            index = UnityEngine.Random.Range(0, total);
        }
        void FixedUpdate()
        {
            if ((frame++ % total) != index) return;
            if (target && target.gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
                image.color = RFUniverseUtility.EncodeIDAsColor(target.ID);
                Rect rect = target.Get2DBBox(PlayerMain.Instance.MainCamera);
                image.rectTransform.position = rect.center;
                image.rectTransform.sizeDelta = rect.size;
            }
            else
            {
                gameObject.SetActive(false);

            }
        }
    }
}
