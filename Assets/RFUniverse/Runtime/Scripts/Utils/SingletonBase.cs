using System;

namespace RFUniverse
{
    public abstract class SingletonBase<T> where T : class
    {
        private static readonly Lazy<T> instance = new Lazy<T>(() => CreateInstance());

        public static T Instance
        {
            get
            {
                return instance.Value;
            }
        }

        private static T CreateInstance()
        {
            return Activator.CreateInstance(typeof(T), true) as T;
        }
    }
}