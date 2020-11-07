using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// 
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class MemoAttribute : System.Attribute
    {
        public string Memo { get; set; }

        public MemoAttribute()
        {}
    }
}
