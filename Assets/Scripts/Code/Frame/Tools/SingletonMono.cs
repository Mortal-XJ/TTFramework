
using UnityEngine;


    public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject tools = GameObject.Find("SingletonMono");
                    if (tools == null)
                    {
                        tools = new GameObject("SingletonMono");
                        DontDestroyOnLoad(tools);
                    }

                    _instance = new GameObject(typeof(T).Name).AddComponent<T>();
                    _instance.transform.SetParent(tools.transform);

                }

                return _instance;
            }
        }

        public virtual void Init()
        {
        }
    }
