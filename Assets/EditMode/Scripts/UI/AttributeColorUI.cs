using Michsky.UI.ModernUIPack;
using RFUniverse.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace RFUniverse.EditMode
{
    public class AttributeColorUI : AttributeBaseUI
    {
        const string uiName = "Color";
        public SliderManager rSlider;
        public SliderManager gSlider;
        public SliderManager bSlider;
        public SliderManager aSlider;
        public override void Init(BaseAttr baseAttr, PropertyInfo info, Action<string, string, object, int> OnValueChange, Action<string, int> OnAttributeSelected)
        {
            base.Init(baseAttr, info, OnValueChange, OnAttributeSelected);
            Color c = (Color)info.GetValue(baseAttr);
            rSlider.mainSlider.SetValueWithoutNotify(c.r);
            gSlider.mainSlider.SetValueWithoutNotify(c.g);
            bSlider.mainSlider.SetValueWithoutNotify(c.b);
            aSlider.mainSlider.SetValueWithoutNotify(c.a);
            rSlider.mainSlider.onValueChanged.AddListener((f) =>
            {
                ValueChanged(OnValueChange);
            });
            gSlider.mainSlider.onValueChanged.AddListener((f) =>
            {
                ValueChanged(OnValueChange);
            });
            bSlider.mainSlider.onValueChanged.AddListener((f) =>
            {
                ValueChanged(OnValueChange);
            });
            aSlider.mainSlider.onValueChanged.AddListener((f) =>
            {
                ValueChanged(OnValueChange);
            });
        }
        protected override void ValueChanged(Action<string, string, object, int> OnValueChange)
        {
            Color c = new Color(rSlider.mainSlider.value, gSlider.mainSlider.value, bSlider.mainSlider.value, aSlider.mainSlider.value);
            OnValueChange(uiName, propertieName, c, -1);
        }
    }
}
