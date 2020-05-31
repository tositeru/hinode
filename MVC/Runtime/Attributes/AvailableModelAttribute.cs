using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode.MVC
{
    /// <summary>
    /// IViewObjectのターゲットにできるModelを指定するAttribute
    /// 簡易的な検証に使用されます。
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class AvailableModelAttribute : System.Attribute
    {
        HashSet<System.Type> _availableModels = new HashSet<System.Type>();
        public IEnumerable<System.Type> AvailableModels { get => _availableModels; }

        public AvailableModelAttribute(params System.Type[] modelTypes)
        {
            foreach (var type in modelTypes
                .Where(_t => _t.IsSubclassOf(typeof(Model))))
            {
                _availableModels.Add(type);
            }
        }
    }
}
