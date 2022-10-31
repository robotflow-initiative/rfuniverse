using Michsky.UI.ModernUIPack;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RFUniverse.EditMode
{
    public class CreateWindowUI : MonoBehaviour
    {
        public ButtonManagerBasicWithIcon oneButton;
        public void SetAttr(List<EditAttrData> attrDatas, Action<string> OnSelectCreate)
        {
            foreach (var item in attrDatas)
            {
                ButtonManagerBasicWithIcon button = Instantiate(oneButton, oneButton.transform.parent);
                button.buttonText = item.displayName;
                button.buttonIcon = item.image;
                button.clickEvent.AddListener(() => OnSelectCreate(item.name));
                button.UpdateUI();
            }
            Destroy(oneButton.gameObject);
        }
    }
}
