using RFUniverse.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace RFUniverse.EditMode
{
    public abstract class AttributeBaseUI : MonoBehaviour
    {
        protected string propertieName;

        protected Action<string, int> OnAttributeChange;

        public virtual void Init(BaseAttr baseAttr, PropertyInfo info, Action<string, string, object, int> OnValueChange, Action<string, int> OnAttributeChange)
        {
            this.OnAttributeChange = OnAttributeChange;
            propertieName = info.Name;
        }
        public virtual void ReSet()
        {
            OnAttributeChange(string.Empty, -1);
        }
        protected abstract void ValueChanged(Action<string, string, object, int> OnValueChange);
    }
}
