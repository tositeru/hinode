using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Hinode
{
    public static class TypeExtensions
    {
        public static FieldInfo GetFieldInHierarchy(this System.Type t, string name, BindingFlags flags = BindingFlags.Default)
        {
            while(t != null)
            {
                var info = t.GetField(name, flags);
                if (info != null)
                    return info;
                t = t.BaseType;
            }
            return null;
        }
    }
}