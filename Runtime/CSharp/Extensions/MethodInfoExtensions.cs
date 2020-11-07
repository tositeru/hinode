using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestMethodInfoExtensions"/>
    /// </summary>
    public static class MethodInfoExtensions
    {
        public static bool DoMatchReturnTypeAndArguments(this MethodInfo info, System.Type returnType, params System.Type[] argTypes)
            => DoMatchReturnTypeAndArguments(info, returnType, argTypes.AsEnumerable());

        public static bool DoMatchReturnTypeAndArguments(this MethodInfo info, System.Type returnType, IEnumerable<System.Type> argTypes)
        {
            if (!returnType.IsSameOrInheritedType(info.ReturnType))
                return false;

            var parameters = info.GetParameters();
            if (parameters.Length != argTypes.Count())
                return false;

            return parameters
                .Zip(argTypes, (_param, _arg) => (param: _param, arg: _arg))
                .All(_tt => _tt.arg.IsSameOrInheritedType(_tt.param.ParameterType));
        }

        /// <summary>
        /// 戻り値の型、引数の型がマッチするMethodInfoを取得します。
        /// 
        /// </summary>
        /// <param name="methodInfos"></param>
        /// <param name="returnType"></param>
        /// <param name="argTypes"></param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetMatchArgsAndReturnType(this IEnumerable<MethodInfo> methodInfos, System.Type returnType, IEnumerable<System.Type> argTypes)
        {
            return methodInfos.Where(_m => _m.DoMatchReturnTypeAndArguments(returnType, argTypes));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="methodInfos"></param>
        /// <param name="returnType"></param>
        /// <param name="argTypes"></param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetMatchArgsAndReturnType(this IEnumerable<MethodInfo> methodInfos, System.Type returnType, params System.Type[] argTypes)
            => GetMatchArgsAndReturnType(methodInfos, returnType, argTypes.AsEnumerable());

        /// <summary>
        /// 指定した返り値と引数と一致する関数を呼び出します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inst"></param>
        /// <param name="label"></param>
        /// <param name="isStatic"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static CallMethodsEnumerable CallMethods(this IEnumerable<MethodInfo> methodInfos, object inst, System.Type returnType, params object[] args)
        {
            return new CallMethodsEnumerable(inst, methodInfos, returnType, args);
        }

        public static CallMethodsEnumerable CallMethods(this IEnumerable<MethodInfo> methodInfos, object inst, System.Type returnType, IEnumerable<object> args)
        {
            return new CallMethodsEnumerable(inst, methodInfos, returnType, args);
        }

        /// <summary>
        /// GetEnumerator()の戻り値は関数の戻り値になります。
        /// </summary>
        public class CallMethodsEnumerable : IEnumerable<object>, IEnumerable
        {
            System.Type _returnType;
            object _inst;
            IEnumerable<MethodInfo> _methodInfos;
            object[] _args;

            IEnumerable<MethodInfo> _matchMethodInfos;
            IEnumerable<MethodInfo> MatchMethodInfos
            {
                get
                {
                    if(_matchMethodInfos == null)
                    {
                        _matchMethodInfos = GetMatchArgsAndReturnType(_methodInfos, _returnType, _args.Select(_a => _a.GetType()));
                    }
                    return _matchMethodInfos;
                }
            }

            internal CallMethodsEnumerable(object inst, IEnumerable<MethodInfo> methodInfos, System.Type returnType, IEnumerable<object> args)
                : this(inst, methodInfos, returnType, args.ToArray())
            { }

            internal CallMethodsEnumerable(object inst, IEnumerable<MethodInfo> methodInfos, System.Type returnType, params object[] args)
            {
                _inst = inst;
                _methodInfos = methodInfos.Where(_m => _m.IsStatic ? inst == null : inst != null);
                _returnType = returnType;
                _args = args;
            }

            public IEnumerator<object> GetEnumerator()
            {
                foreach (var info in MatchMethodInfos)
                {
                    yield return info.Invoke(_inst, _args);
                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            public void CallAll()
            {
                foreach (var _ in this)
                { }
            }
        }
    }
}
