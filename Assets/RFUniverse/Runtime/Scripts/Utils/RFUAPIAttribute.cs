using System;


namespace RFUniverse
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RFUAPIAttribute : Attribute
    {
        public RFUAPIAttribute(string hand = null, bool showLog = true)
        {
            Hand = hand;
            ShowLog = showLog;
        }
        public string Hand { get; set; }
        public bool ShowLog { get; set; }
    }

}
