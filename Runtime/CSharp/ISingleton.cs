using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ISingleton<T>
        where T : new()
    {
        static T _instance;
        public static T Instance
        {
            get => _instance != null ? _instance : _instance = new T();
        }
    }
}
