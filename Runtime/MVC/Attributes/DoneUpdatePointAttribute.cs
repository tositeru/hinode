using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// Hinode.Model#DoneUpdateを行うことを示すAttribute
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method | System.AttributeTargets.Property
        , Inherited = false
        , AllowMultiple = false)]
    public sealed class DoneUpdatePointAttribute : System.Attribute
    {
        public bool Enable { get; }
        public DoneUpdatePointAttribute(bool enable=true)
        {
            Enable = enable;
        }
    }
}
