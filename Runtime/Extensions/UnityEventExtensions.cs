using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Hinode
{
    public static class UnityEventExtensions
    {
        public static IEnumerable<System.Delegate> GetInvocationList<T>(this T e)
            where T : UnityEventBase
        {
            return new InvocationListEnumerable<T>(e);
        }

        class InvocationListEnumerable<T> : IEnumerable<System.Delegate>, IEnumerable
            where T : UnityEventBase
        {
            public readonly static FieldInfo UnityEventBase_CallsInfo;
            public readonly static MethodInfo CallsInfo_PrepareInvoke;
            public readonly static PropertyInfo PrepareInvoke_Count;
            public readonly static MethodInfo PrepareInvoke_get_Item;
            public readonly static MethodInfo Delegate_GetInvocationList;

            public static FieldInfo PrepareInvokeElement_Delegate;

            static InvocationListEnumerable()
            {
                UnityEventBase_CallsInfo = typeof(UnityEventBase).GetField("m_Calls", BindingFlags.Instance | BindingFlags.NonPublic);
                CallsInfo_PrepareInvoke = UnityEventBase_CallsInfo.FieldType.GetMethod("PrepareInvoke");
                PrepareInvoke_Count = CallsInfo_PrepareInvoke.ReturnType.GetProperty("Count");
                PrepareInvoke_get_Item = CallsInfo_PrepareInvoke.ReturnType.GetMethod("get_Item");

                //PrepareInvokeElement_Delegate = PrepareInvoke_get_Item.ReturnType.GetField("Delegate", BindingFlags.NonPublic | BindingFlags.Instance);

                Delegate_GetInvocationList = typeof(T).GetMethod("AddListener").GetParameters()[0].ParameterType.GetMethod("GetInvocationList");

                Assert.IsNotNull(UnityEventBase_CallsInfo);
                Assert.IsNotNull(CallsInfo_PrepareInvoke);
                Assert.IsNotNull(PrepareInvoke_Count);
                Assert.IsNotNull(PrepareInvoke_get_Item);
//                Assert.IsNotNull(PrepareInvokeElement_Delegate);
                Assert.IsNotNull(Delegate_GetInvocationList);
            }

            UnityEventBase _target;
            public InvocationListEnumerable(UnityEventBase target)
            {

                _target = target;
            }

            public IEnumerator<System.Delegate> GetEnumerator()
            {
                var emptyArgs = new object[] { };
                var oneArgs = new object[1];
                var calls = UnityEventBase_CallsInfo.GetValue(_target);

                var prepareInvokes = CallsInfo_PrepareInvoke.Invoke(calls, emptyArgs);

                var prepareInvokeCount = (int)PrepareInvoke_Count.GetValue(prepareInvokes);
                for (var i = 0; i < prepareInvokeCount; ++i)
                {
                    oneArgs[0] = i;
                    var invoke = PrepareInvoke_get_Item.Invoke(prepareInvokes, oneArgs);

                    //実行時のみでしか取得できなかったのでここで取得している。
                    if(PrepareInvokeElement_Delegate == null)
                    {
                        PrepareInvokeElement_Delegate = invoke.GetType().GetField("Delegate", BindingFlags.NonPublic | BindingFlags.Instance);
                    }

                    var delegate_ = PrepareInvokeElement_Delegate.GetValue(invoke);
                    foreach (var m in Delegate_GetInvocationList.Invoke(delegate_, emptyArgs) as System.Delegate[])
                    {
                        yield return m;
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }

    }
}
