using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// シーン上に一つしか存在しないMonoBehaviour
    /// 
    /// DontDestroyOnLoadには自動的に設定しませんので、別途設定してください
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour
        where T : SingletonMonoBehaviour<T>
    {
        static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                var obj = new GameObject("", typeof(T));
                _instance = obj.GetComponent<T>();
                _instance.name = _instance.DefaultInstanceName;
                return _instance;
            }
        }
        public static bool DoExistInstance { get => _instance != null; }

        /// <summary>
        /// デフォルトの名前
        /// </summary>
        abstract protected string DefaultInstanceName { get; }

        /// <summary>
        /// Awakeが呼び出されてかつ、シングルトンのインスタンスとして扱われている時に呼び出される関数
        /// </summary>
        abstract protected void OnAwaked();

        /// <summary>
        /// OnDestroyが呼ばれた時に呼び出される関数
        /// </summary>
        /// <param name="isInstance">シングルトンのインスタンスとして設定されていたかどうか</param>
        abstract protected void OnDestroyed(bool isInstance);

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }
            _instance = this as T;

            OnAwaked();
        }

        private void OnDestroy()
        {
            bool isInstance = _instance == this;
            if (isInstance)
            {
                _instance = null;
            }
            OnDestroyed(isInstance);
        }
    }
}
