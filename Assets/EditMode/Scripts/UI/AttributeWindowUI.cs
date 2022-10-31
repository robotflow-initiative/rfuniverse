using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Michsky.UI.ModernUIPack;
using RFUniverse.Attributes;
using System.Reflection;
using RFUniverse.Manager;

namespace RFUniverse.EditMode
{
    public class AttributeWindowUI : MonoBehaviour
    {
        public Image image;
        public TextMeshProUGUI title;
        public ButtonManagerBasicIcon delete;
        public List<Toggle> attributesToggle = new List<Toggle>();
        Action<string, string, object, int> OnValueChange;
        Action<string, int> OnAttributeChange;

        public void Init(Action OnDeleteUnit, Action<string, string, object, int> OnValueChange, Action<string, int> OnAttributeChange)
        {
            delete.clickEvent.AddListener(() =>
            {
                OnDeleteUnit();
            });
            this.OnValueChange = OnValueChange;
            this.OnAttributeChange = OnAttributeChange;
        }

        List<AttributeBaseUI> attributeBaseUIs = new List<AttributeBaseUI>();
        public void SetAttr(BaseAttr baseAttr)
        {
            foreach (var item in attributeBaseUIs)
            {
                Destroy(item.gameObject);
            }
            attributeBaseUIs.Clear();

            foreach (var item in attributesToggle)
            {
                item.gameObject.SetActive(false);
            }
            attributesToggle[0].isOn = false;

            Type type = baseAttr.GetType();
            PropertyInfo[] ps = type.GetProperties();
            foreach (var item in ps)
            {
                EditableAttrAttribute a = item.GetCustomAttribute<EditableAttrAttribute>(false);
                if (a == null) continue;
                AssetManager.Instance.GetGameObject(a.Name, (gameObject) =>
                 {
                     AttributeBaseUI ui = Instantiate(gameObject, transform).GetComponent<AttributeBaseUI>();
                     ui.gameObject.SetActive(false);
                     attributeBaseUIs.Add(ui);
                     ui.Init(baseAttr, item, OnValueChange, OnAttributeChange);

                     if (attributesToggle.Count <= attributeBaseUIs.Count - 1)
                     {
                         attributesToggle.Add(Instantiate(attributesToggle[0], attributesToggle[0].transform.parent));
                     }
                     attributesToggle[attributeBaseUIs.Count - 1].gameObject.SetActive(true);
                     attributesToggle[attributeBaseUIs.Count - 1].GetComponentInChildren<TextMeshProUGUI>().text = a.Name;
                     attributesToggle[attributeBaseUIs.Count - 1].onValueChanged.RemoveAllListeners();
                     attributesToggle[attributeBaseUIs.Count - 1].onValueChanged.AddListener((b) =>
                     {
                         if (b)
                         {
                             ui.gameObject.SetActive(true);
                             ui.ReSet();
                         }
                         else
                             ui.gameObject.SetActive(false);
                     });
                     attributesToggle[0].isOn = true;
                 });
            }

        }
    }
}
