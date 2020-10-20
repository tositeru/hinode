using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// classインスタンスのObjectPool
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T> : IObjectPool
        where T : class
    {
        public interface IInstanceCreater
        {
            T Create();
        }

        IInstanceCreater _creator;
        public IInstanceCreater Creator { get => _creator; }

        public ObjectPool(IInstanceCreater creater)
        {
            Assert.IsNotNull(creater);
            _creator = creater;
        }

        public virtual T PopOrCreate()
        {
            var obj = Pop() as T;
            return obj != null ? obj : _creator.Create();
        }

        public virtual void Push(T obj)
            => base.Push(obj);
    }
}
