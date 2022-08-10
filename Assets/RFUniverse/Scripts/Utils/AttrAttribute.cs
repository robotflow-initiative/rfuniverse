using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RFUniverse.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AttrAttribute : Attribute
    {
        public AttrAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
    }
}
