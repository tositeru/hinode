using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class IEditorGUIFieldAttribute : PropertyAttribute
    {

    }

    /// <summary>
    /// Editor拡張で表示させたくない時に使用してください。
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class NotVisibleAttribute : IEditorGUIFieldAttribute
    {
        public NotVisibleAttribute()
        { }
    }
}
