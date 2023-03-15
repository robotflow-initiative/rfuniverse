using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RFUniverse.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class EditAttrAttribute : Attribute
    {
        public EditAttrAttribute(string name, string type)
        {
            Name = name;
            Type = type;
        }
        public string Name { get; set; }
        public string Type { get; set; }
    }

}
