using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ISingleton<T>
        where T : ISingleton<T>, new()
    {
        static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                ResetInstance();
                return _instance;
            }
        }

        protected ISingleton() {}

        protected static void ResetInstance()
        {
            _instance = new T();
            _instance.OnCreated();
        }

        virtual protected void OnCreated() { }
    }
}
