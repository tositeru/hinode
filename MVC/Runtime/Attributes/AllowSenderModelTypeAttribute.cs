using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode.MVC
{
    /// <summary>
    /// IEventHandlerのメソッドに指定するAttribute
    /// 受け取ることを想定しているModelの型を指定します。
    /// 簡易的な検証に使用されます。
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class AllowSenderModelTypeAttribute : System.Attribute
    {
	    System.Type[] _allowModelTypes;

        public IEnumerable<System.Type> AllowModelTypes { get => _allowModelTypes; }
	    public AllowSenderModelTypeAttribute(params System.Type[] allowModelTypes)
	    {
            foreach(var type in allowModelTypes)
		    {
			    Assert.IsTrue(type.IsSubclassOf(typeof(Model)));
		    }

            _allowModelTypes = allowModelTypes;
	    }
    }
}
