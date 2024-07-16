using UnityEngine;
using RFUniverse.Attributes;
using RFUniverse.Manager;
using UnityEngine.UI;

namespace RFUniverse.DebugTool
{
    public class DDBBox : MonoBehaviour
    {
        public Image image;
        public GameObjectAttr target;
        void FixedUpdate()
        {
            if (DebugManager.Instance.IsDebug2DBBox && target && target.gameObject.activeInHierarchy)
            {
                image.gameObject.SetActive(true);
                image.color = RFUniverseUtility.EncodeIDAsColor(target.ID);
                Rect rect = target.Get2DBBox(PlayerMain.Instance.MainCamera);
                image.rectTransform.position = rect.center;
                image.rectTransform.sizeDelta = rect.size;
            }
            else
            {
                image.gameObject.SetActive(false);
            }
        }

    }
}
