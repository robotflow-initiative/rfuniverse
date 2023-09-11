using System;


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
