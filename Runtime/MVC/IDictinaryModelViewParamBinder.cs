using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// 辞書としてパラメータを保持できるIModelViewParamBinder
    /// <seealso cref="IModelViewParamBinder"/>
    /// </summary>
    public abstract class IDictinaryModelViewParamBinder : IModelViewParamBinder
    {
        public abstract void Update(Model model, IViewObject viewObj);

        Dictionary<string, object> _fixedParams = new Dictionary<string, object>();

        public bool Contains(string keyword) => _fixedParams.ContainsKey(keyword);

        public IDictinaryModelViewParamBinder Set(string keyword, object value)
        {
            if (_fixedParams.ContainsKey(keyword))
            {
                _fixedParams[keyword] = value;
            }
            else
            {
                _fixedParams.Add(keyword, value);
            }
            return this;
        }

        public IDictinaryModelViewParamBinder Set<T>(string keyword, T value)
            => Set(keyword, (object)value);

        public object Get(string keyword)
        {
            if (_fixedParams.ContainsKey(keyword))
            {
                return _fixedParams[keyword];
            }
            else
            {
                return default;
            }
        }
        public T Get<T>(string keyword)
            => (T)Get(keyword);

        public IDictinaryModelViewParamBinder Delete(string keyword)
        {
            if (_fixedParams.ContainsKey(keyword))
            {
                _fixedParams.Remove(keyword);
            }
            return this;
        }

    }
}
