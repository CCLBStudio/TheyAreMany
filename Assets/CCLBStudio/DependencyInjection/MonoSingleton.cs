using UnityEngine;

namespace CCLBStudio.DependencyInjection
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : Component
    {
        protected static T instance;

        public static T Instance
        {
            get
            {
                if (!instance)
                {
                    instance = FindAnyObjectByType<T>();
                    if (!instance)
                    {
                        instance = AutoCreateInstance();
                    }
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            instance = null;
            InitializeSingleton();
        }

        protected void InitializeSingleton()
        {
            if(!Application.isPlaying)
            {
                return;
            }

            if (instance && instance != this)
            {
                Debug.LogError($"There is already an instance of {typeof(T).Name} in the scene ! Destroying this one ({name}).");
                Destroy(gameObject);
                return;
            }
            
            instance = this as T;
        }

        private static T AutoCreateInstance()
        {
            var go = new GameObject($"AutoCreatedSingleton-{typeof(T).Name}");
            return go.AddComponent<T>();
        }
    }
}