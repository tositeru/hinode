using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// UnityのComponentのObjectPool
    ///
    /// GetOrCreate/Pushのタイミングで自動的にactive/deactiveします。
    /// </summary>
    public class UnityComponentPool<T> : ObjectPool<T>
        where T : Component
    {
        public UnityComponentPool(IInstanceCreater creater)
            : base(creater)
        {
        }

        public override T PopOrCreate()
        {
            var obj = base.PopOrCreate();
            obj.gameObject.SetActive(true);
            return obj.GetComponent<T>();
        }

        public override void Push(T obj)
        {
            obj.gameObject.SetActive(false);
            base.Push(obj);
        }
    }
}
