using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode.MVC
{
    /// <summary>
    /// 有効なIModelViewParamBinderを指定するAttribute
    /// 簡易的な検証に使用されます。
    /// IViewObjectの派生クラスに指定してください
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class AvailableModelViewParamBinderAttribute : System.Attribute
    {
        HashSet<System.Type> _availableParamBinderTypes = new HashSet<System.Type>();
        public IEnumerable<System.Type> AvailableParamBinders { get => _availableParamBinderTypes; }

        public AvailableModelViewParamBinderAttribute(params System.Type[] paramBinderType)
        {
            foreach(var type in paramBinderType
                .Where(_t => _t.ContainsInterface<IModelViewParamBinder>()))
            {
                _availableParamBinderTypes.Add(type);
            }
        }
    }
}
