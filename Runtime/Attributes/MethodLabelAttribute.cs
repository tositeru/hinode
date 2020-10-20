using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// MethodにLabelを指定するためのAttribute
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class MethodLabelAttribute : System.Attribute
    {
        HashSet<string> _labels = new HashSet<string>();
        public IReadOnlyCollection<string> Labels { get => _labels; }
        public MethodLabelAttribute(params string[] labels)
        {
            foreach(var l in labels
                .Where(_l => !_labels.Contains(_l))
            ) {
                _labels.Add(l);
            }
        }

        public bool Contains(string label)
            => _labels.Contains(label);

        /// <summary>
        /// labelが指定されたMethodLabelAttributeを持つメソッドの情報を取得します。
        /// 
        /// publicメソッドのみが処理対象になります。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public static IEnumerable<(MethodInfo methodInfo, MethodLabelAttribute labelAttr)> GetMethodInfos(System.Type type, string label, bool isStatic)
        {
            var bindingFlags = BindingFlags.Public;
            bindingFlags |= isStatic ? BindingFlags.Static : BindingFlags.Instance;
            return type.GetMethods(bindingFlags)
                .Select(_m => (method: _m, attr: _m.GetCustomAttribute<MethodLabelAttribute>()))
                .Where(_t => _t.attr != null
                    && _t.attr.Contains(label)
                );
        }
        public static IEnumerable<(MethodInfo methodInfo, MethodLabelAttribute labelAttr)> GetMethodInfos<T>(string label, bool isStatic)
            => GetMethodInfos(typeof(T), label, isStatic);

        /// <summary>
        /// labelと戻り値の型、引数の型がマッチするMethodLabelAttributeを持つメソッドの情報を取得します。
        /// 
        /// publicメソッドのみが処理対象になります。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public static IEnumerable<(MethodInfo methodInfo, MethodLabelAttribute labelAttr)> GetMethodInfosWithArgsAndReturnType(System.Type type, string label, bool isStatic, System.Type returnType, IEnumerable<System.Type> argTypes)
        {
            return GetMethodInfos(type, label, isStatic)
                .Where(_t =>
                {
                    if (!_t.methodInfo.ReturnType.IsSameOrInheritedType(returnType))
                        return false;

                    var parameters = _t.methodInfo.GetParameters();
                    if (parameters.Length != argTypes.Count())
                        return false;

                    return parameters
                        .Zip(argTypes, (_param, _arg) => (param: _param, arg: _arg))
                        .All(_tt => _tt.param.ParameterType.IsSameOrInheritedType(_tt.arg));
                });
        }
        public static IEnumerable<(MethodInfo methodInfo, MethodLabelAttribute labelAttr)> GetMethodInfosWithArgsAndReturnType<T, TReturnType>(string label, bool isStatic, IEnumerable<System.Type> argTypes)
            => GetMethodInfosWithArgsAndReturnType(typeof(T), label, isStatic, typeof(TReturnType), argTypes);

        /// <summary>
        /// labelが指定されたMethodLabelAttributeを持つ全てのpublicメソッドを呼び出します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inst"></param>
        /// <param name="label"></param>
        /// <param name="isStatic"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static CallMethodsEnumerable CallMethods<TReturnType>(object inst, string label, bool isStatic, params object[] args)
        {
            return new CallMethodsEnumerable(typeof(TReturnType), inst, label, isStatic, args);
        }

        /// <summary>
        /// labelが指定されたMethodLabelAttributeを持つ全てのpublicメソッドを呼び出します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inst"></param>
        /// <param name="label"></param>
        /// <param name="isStatic"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static CallMethodsEnumerable CallMethods(System.Type returnType, object inst, string label, bool isStatic, params object[] args)
        {
            return new CallMethodsEnumerable(returnType, inst, label, isStatic, args);
        }

        public class CallMethodsEnumerable : IEnumerable<object>, IEnumerable
        {
            System.Type _returnType;
            object _inst;
            string _label;
            bool _isStatic;
            object[] _args;

            public CallMethodsEnumerable(System.Type returnType, object inst, string label, bool isStatic, params object[] args)
            {
                _returnType = returnType;
                _inst = inst;
                _label = label;
                _isStatic = isStatic;
                _args = args;
            }

            public IEnumerator<object> GetEnumerator()
            {
                var obj = _isStatic ? null : _inst;
                foreach (var (info, __) in GetMethodInfosWithArgsAndReturnType(
                    _inst.GetType(), _label, _isStatic, _returnType, _args.Select(_a => _a.GetType()))
                )
                {
                    yield return info.Invoke(obj, _args);
                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            public void CallAll()
            {
                foreach(var _ in this)
                { }
            }
        }
    }
}
