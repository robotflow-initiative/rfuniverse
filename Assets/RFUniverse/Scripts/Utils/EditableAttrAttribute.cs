using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RFUniverse.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class EditableAttrAttribute : Attribute
    {
        public EditableAttrAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
    }
}
