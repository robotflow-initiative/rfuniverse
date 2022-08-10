using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace RFUniverse.EditMode
{
    public class CreateButtonUI : MonoBehaviour
    {
        public Image image1;
        public Image image2;
        public TextMeshProUGUI text1;
        public TextMeshProUGUI text2;
        public void SetType(Sprite sprite, string name)
        {
            image1.sprite = sprite;
            image2.sprite = sprite;
            text1.text = name;
            text2.text = name;
        }
    }
}
