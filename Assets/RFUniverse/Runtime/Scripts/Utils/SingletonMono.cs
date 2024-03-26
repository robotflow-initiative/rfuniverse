using UnityEngine;
namespace RFUniverse
{
    [DisallowMultipleComponent]
    public abstract class SingletonMono<T> : MonoBehaviour where T : class
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                return instance;
            }
        }
        protected virtual void Awake()
        {
            instance = this as T;
            if (Application.isPlaying)
                DontDestroyOnLoad(gameObject);
        }
    }
}