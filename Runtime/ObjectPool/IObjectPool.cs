using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hinode
{
    /// <summary>
    /// ObjectPoolのベースクラス
    /// </summary>
    public abstract class IObjectPool
    {
        HashSet<object> _pool = new HashSet<object>();

        public int Count { get => _pool.Count; }
        protected IReadOnlyCollection<object> Pool { get => _pool; }

        public void Clear()
        {
            _pool.Clear();
        }

        /// <summary>
        /// 何らかの理由からPool内にあるobjectが削除される可能性があるのでそれに対応するために作成しました。
        /// </summary>
        /// <param name="obj"></param>
        public void Remove(object obj)
        {
            if (_pool.Contains(obj))
            {
                _pool.Remove(obj);
            }
        }

        protected object Pop()
        {
            if (0 >= _pool.Count) return null;
            var obj = _pool.First();
            _pool.Remove(obj);
            return obj;
        }

        protected void Push(object obj)
        {
            if(_pool.Contains(obj))
            {
                return;
            }

            _pool.Add(obj);
        }
    }
}
