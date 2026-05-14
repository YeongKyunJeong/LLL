using UnityEngine;

namespace LLL
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T instance;
        private static bool applicationIsQuitting = false;

        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning($"Singleton Instance '{typeof(T)}' was Deleted. Null was returned");
                }

                if (instance == null)
                {

                    instance = FindFirstObjectByType<T>();

                    if (instance == null)
                    {
                        GameObject go = new GameObject();
                        instance = go.AddComponent<T>();
                    }

                    if (FindObjectsByType(typeof(T), FindObjectsSortMode.None).Length > 1)
                    {
                        Debug.LogError($"Other '{typeof(T)}'(Singleton) Instances were Detected");
                        return instance;
                    }
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
    }
}