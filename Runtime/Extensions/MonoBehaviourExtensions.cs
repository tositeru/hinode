using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="Hinode.Tests.Extensions.TestMonoBehaviourExtensions"/>
    /// </summary>
    public static class MonoBehaviourExtensions
    {
        /// <summary>
        /// <seealso cref="Hinode.Tests.Extensions.TestMonoBehaviourExtensions.SafeStartCoroutinePasses()"/>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="coroutine"></param>
        /// <param name="routine"></param>
        public static void SafeStartCoroutine(this MonoBehaviour target, ref Coroutine coroutine, IEnumerator routine)
        {
            if(coroutine != null)
            {
                target.StopCoroutine(coroutine);
            }
            coroutine = target.StartCoroutine(routine);
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Extensions.TestMonoBehaviourExtensions.AssertObjectReferencePasses()"/>
        /// </summary>
        /// <param name="mono"></param>
        /// <param name="objHash"></param>
        public static void AssertObjectReference(this MonoBehaviour mono, HashSet<object> objHash = null)
        {
            AssertObjectReference(mono as object, objHash);
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Extensions.TestMonoBehaviourExtensions.AssertObjectReferencePasses()"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="objHash"></param>
        public static void AssertObjectReference(object obj, HashSet<object> objHash = null)
        {
            if (objHash == null) objHash = new HashSet<object>();
            objHash.Add(obj);

            var objReferences = obj.GetType().GetRuntimeFields()
                .Where(_f => !_f.IsStatic && (_f.IsPublic || _f.GetCustomAttribute<SerializeField>() != null));

            foreach (var (inst, info) in objReferences
                .Where(_f => _f.FieldType.IsSubclassOf(typeof(Object)))
                .Select(_f => (inst: _f.GetValue(obj) as Object, info: _f)))
            {
                Assert.IsNotNull(inst, $"'{info.Name}' must be not Null... type={info.DeclaringType.Name}");
            }

            foreach (var (inst, info) in objReferences
                .Where(_f => !_f.FieldType.IsPrimitive || _f.FieldType.IsEnum)
                .Where(_f => _f.FieldType.IsSerializable)
                .Select(_f => (inst: _f.GetValue(obj), info: _f))
                .Where(pair => pair.inst != null && !objHash.Contains(pair.inst)))
            {
                //Debug.Log($"deep into {info.Name},{info.FieldType.Name}. {obj.GetType().Name}");
                AssertObjectReference(inst, objHash);
            }
        }
    }
}
