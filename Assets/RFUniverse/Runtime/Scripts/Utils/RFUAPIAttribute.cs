using System;


namespace RFUniverse
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RFUAPIAttribute : Attribute
    {
        public RFUAPIAttribute(string hand = null)
        {
            Hand = hand;
        }
        public string Hand { get; set; }
    }

}
