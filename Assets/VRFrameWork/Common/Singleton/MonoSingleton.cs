using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRFrameWork
{
    /// <summary>
    /// Singleton for Class inherit from MonoBehaiour
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        protected static T _instance = null;
        public static T Instance
        {
            get
            {
                if (null == _instance)
                {
                    GameObject go = GameObject.Find("MonoSingleton");
                    if (null == go)
                    {
                        go = new GameObject("MonoSingleton");
                        DontDestroyOnLoad(go);
                    }
                    _instance= go.GetOrAddComponent<T>();
                    _instance.Init();
                }
                return _instance;
            }
        }

        public virtual void Init()
        {

        }

        public virtual void Release()
        {

        }

        private void OnApplicationQuit()
        {
            Release();
            _instance = null;
        }

    }

}
