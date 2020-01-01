using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// 強制的にObjectFieldにしたい時に指定するPropertyAttribute
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class ForcedObjectFieldAttribute : PropertyAttribute
    {
        readonly bool _allowSceneObjects;
        public bool AllowSceneObject { get => _allowSceneObjects; }

        public ForcedObjectFieldAttribute(bool allowSceneObjects=true)
        {
            _allowSceneObjects = allowSceneObjects;
        }

        public bool DoValid(object target)
        {
            return target is Object;
        }
    }
}
