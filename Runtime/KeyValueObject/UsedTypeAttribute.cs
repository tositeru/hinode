using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    public interface IHasTypeName
    {
        System.Type HasType { get; set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class UsedTypeAttribute : PropertyAttribute
    {
        readonly System.Type _usedBaseType;
        public System.Type UsedBaseType
        {
            get
            {
                return _usedBaseType;
            }
        }

        readonly System.Type _usedType;
        public System.Type UsedType
        {
            get
            {
                return _usedType;
            }
        }

        public UsedTypeAttribute(System.Type usedType, System.Type baseType = null, int order=-1)
        {
            _usedBaseType = baseType;
            Assert.IsTrue(usedType.IsSubclassOf(_usedBaseType), $"'{usedType.FullName}' is not the derrived type of '{baseType.FullName}'.");
            _usedType = usedType;
            base.order = order;
        }

        public void SetType(IHasTypeName typeName)
        {
            typeName.HasType = UsedType;
        }

        public static bool SetTypeFromFieldInfo(FieldInfo info, IHasTypeName hasType)
        {
            var usedTypeAttr = info.GetCustomAttributes(true)
                .OfType<UsedTypeAttribute>()
                .FirstOrDefault(_a => _a != null);
            if (usedTypeAttr == null)
            {
                return false;
            }
            usedTypeAttr.SetType(hasType);
            return true;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class UsedEnumAttribute : UsedTypeAttribute
    {
        public UsedEnumAttribute(System.Type usedType, int order = -1)
            : base(usedType, typeof(System.Enum), order)
        {
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class UsedUnityObjectAttribute : UsedTypeAttribute
    {
        public UsedUnityObjectAttribute(System.Type usedType, int order = -1)
            : base(usedType, typeof(Object), order)
        {
        }
    }

}
