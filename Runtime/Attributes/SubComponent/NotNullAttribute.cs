using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// NullにはできないSerialize対象のメンバに指定するAttribute
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class NotNullAttribute : System.Attribute
        , ISubComponentAttribute
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void RegistToSubComponentAttributeManager()
        {
            SubComponentAttributeManager.AddInitMethod<NotNullAttribute>(ValidInstanceFields);
        }

        public string ErrorMessage { get; }
        public NotNullAttribute(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public bool Valid(FieldInfo fieldInfo, object inst)
        {
            Assert.IsNotNull(inst);

            var field = fieldInfo.GetValue(inst);
            if (field == null)
            {
                Logger.LogWarning(Logger.Priority.High
                    , () => $"Instance must be not Null... {ErrorMessage} : inst={inst.GetType()}, Field name={fieldInfo.Name}:{fieldInfo.FieldType}"
                    , SubComponentDefines.LOGGER_SELECTOR);
                return false;
            }
            return true;
        }

        public static void ValidInstanceFields(object inst)
        {
            if (inst == null) return;

            var type = inst.GetType();
            var publicFields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var serializedFields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(_f => null != _f.GetCustomAttribute<SerializeField>());

            var isValid = true;
            foreach(var f in publicFields.Concat(serializedFields)
                .Where(_f => null != _f.GetCustomAttribute<NotNullAttribute>()))
            {
                var attr = f.GetCustomAttribute<NotNullAttribute>();
                isValid &= attr.Valid(f, inst);
            }

            if(!isValid)
            {
                Logger.LogError(Logger.Priority.High
                    , () => $"Contains Null fields in instance... Please check previous wanring logs!: Instance Type={inst.GetType()}"
                    , SubComponentDefines.LOGGER_SELECTOR);
            }
        }
    }
}
