using Michsky.UI.ModernUIPack;
using RFUniverse.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace RFUniverse.EditMode
{
    public class AttributeRigidbodyUI : AttributeBaseUI
    {
        const string uiName = "Rigidbody";
        public CustomInputField mass;
        public CustomToggle toggle;

        RigidbodyData rigidbodyData;
        public override void Init(BaseAttr baseAttr, PropertyInfo info, Action<string, string, object, int> OnValueChange, Action<string, int> OnAttributeSelected)
        {
            base.Init(baseAttr, info, OnValueChange, OnAttributeSelected);
            rigidbodyData = (RigidbodyData)info.GetValue(baseAttr);
            mass.inputText.SetTextWithoutNotify(rigidbodyData.mass.ToString());
            mass.inputText.onSubmit.AddListener((s) =>
            {
                MassChange(s, OnValueChange);
            });
            mass.inputText.onDeselect.AddListener((s) =>
            {
                MassChange(s, OnValueChange);
            });
            toggle.toggleObject.SetIsOnWithoutNotify(rigidbodyData.useGravity);
            toggle.toggleObject.onValueChanged.AddListener((b) =>
            {
                rigidbodyData.useGravity = b;
                ValueChanged(OnValueChange);
            });
        }
        void MassChange(string s, Action<string, string, object, int> OnValueChange)
        {
            float f = 0;
            try
            {
                f = float.Parse(mass.inputText.text);
            }
            catch
            {
                mass.inputText.SetTextWithoutNotify(f.ToString());
            }
            rigidbodyData.mass = f;
            ValueChanged(OnValueChange);
        }
        protected override void ValueChanged(Action<string, string, object, int> OnValueChange)
        {
            OnValueChange(uiName, propertieName, rigidbodyData, -1);
        }
    }
}
