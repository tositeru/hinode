using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using UnityEngine.UI;
using System.Linq;

namespace Hinode.Tests.Attributes
{
    /// <summary>
    /// test case
    /// ## Constructor
    /// - propName && ComponentType
    /// - IBinderPredicate
    /// ## IsValid
    /// ## EnableBind(MethodInfo targetMethodInfo, object obj)
    /// ## EnableBind (static)
    /// ## Bind
    /// ## Unbind
    /// ## GetMethodInfoAndAttrEnumerable
    /// ## BindWithComTypeAndCallbackName
    /// - UnityEvent with object
    /// - System.Delegate with object
    /// - event with object
    /// - SmartDelegate with object
    /// - miss match case
    /// ## UnbindWithComTypeAndCallbackName
    /// - UnityEvent with object
    /// - System.Delegate with object
    /// - event with object
    /// - SmartDelegate with object
    /// - miss match case
    /// ## BindToGameObject
    /// ## UnbindToGameObject
    /// <seealso cref="BindCallbackAttribute"/>
    /// </summary>
    public class TestBindCallbackAttribute : TestBase
    {
        const int ORDER_CONSTRUCTOR = 0;
        const int ORDER_IsValid = ORDER_CONSTRUCTOR + 100;
        const int ORDER_EnableBind_Instance = ORDER_EnableBind + 100;
        const int ORDER_EnableBind = 0;
        const int ORDER_Bind = ORDER_BindWithTypeAndCallbackName + 100;
        const int ORDER_Unbind = ORDER_Bind;
        const int ORDER_GetMethodInfoAttrEnumerable = ORDER_CONSTRUCTOR + 100;
        const int ORDER_BindWithTypeAndCallbackName = ORDER_GetMethodInfoAttrEnumerable + 100;
        const int ORDER_UnbindWithTypeAndCallbackName = ORDER_BindWithTypeAndCallbackName + 100;
        const int ORDER_BIND_TO_GAMEOBJECT = ORDER_Bind + 100;
        const int ORDER_UNBIND_TO_GAMEOBJECT = ORDER_BIND_TO_GAMEOBJECT + 100;

        #region Constructor
        class A
        {
            public const string LABEL1 = "LABEL1";
            public const string LABEL2 = "LABEL2";

            public const string CALLBACK_NAME = "onClick";

            [BindCallback(typeof(Button), CALLBACK_NAME, Labels = new string[] { LABEL1 })]
            public void Func()
                => Debug.Log("A#Func");

            public class BinderPredicate : BindCallbackAttribute.IBinderPredicate
            {
                public bool EnableBind(MethodInfo methodInfo, object obj) => true;
                public bool AddCallbacks(object target, MethodInfo methodInfo, object obj)
                {
                    if (obj is GameObject)
                    {
                        var btn = (obj as GameObject).GetComponent<Button>();
                        BindCallbackAttribute.BindWithTypeAndCallbackName<Button>(target, methodInfo, btn, "onClick");
                    }
                    return true;
                }

                public bool RemoveCallbacks(object target, MethodInfo methodInfo, object obj)
                {
                    if (obj is GameObject)
                    {
                        var btn = (obj as GameObject).GetComponent<Button>();
                        BindCallbackAttribute.UnbindWithTypeAndCallbackName<Button>(target, methodInfo, btn, "onClick");
                    }
                    return true;
                }
            }
            [BindCallback(typeof(BinderPredicate), Labels = new string[] { LABEL2 })]
            public void Func2()
                => Debug.Log("A#Func2");
        }

        /// <summary>
        /// <seealso cref="BindCallbackAttribute.BindCallbackAttribute(string, System.Type)"/>
        /// </summary>
        /// <returns></returns>
        [Test, Order(ORDER_CONSTRUCTOR), Description("Type And CallbackName")]
        public void Constructor_PropNameAndComponentType_Pass()
        {
            var type = typeof(A);
            var funcInfo = type.GetMethod("Func");
            var attr = funcInfo.GetCustomAttribute<BindCallbackAttribute>();

            Assert.AreEqual(BindCallbackAttribute.Kind.TypeAndCallback, attr.CurrentKind);
            Assert.AreEqual(typeof(Button), attr.CallbackBaseType);
            Assert.AreEqual(A.CALLBACK_NAME, attr.CallbackName);
            Assert.IsNull(attr.Binder);

            AssertionUtils.AssertEnumerableByUnordered(
                new string[] { A.LABEL1 }
                , attr.Labels
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="BindCallbackAttribute.BindCallbackAttribute(System.Type)"/>
        /// </summary>
        /// <returns></returns>
        [Test, Order(ORDER_CONSTRUCTOR), Description("IBinderPredicate")]
        public void Constructor_IBinderPredicate_Pass()
        {
            var type = typeof(A);
            var funcInfo = type.GetMethod("Func2");
            var attr = funcInfo.GetCustomAttribute<BindCallbackAttribute>();

            Assert.AreEqual(BindCallbackAttribute.Kind.Binder, attr.CurrentKind);
            Assert.IsNull(attr.CallbackBaseType);
            Assert.IsTrue(string.IsNullOrEmpty(attr.CallbackName));
            Assert.AreEqual(typeof(A.BinderPredicate), attr.Binder.GetType());
        }
        #endregion

        #region IsValid
        class IsValidTest
        {
            public const string CALLBACK_Delegate = "Delegate";
            public const string CALLBACK_Event = "Event";
            public const string CALLBACK_SmartDelegate = "SmartDelegate";
            public const string CALLBACK_UnityEvent = "UnityEvent";

            [BindCallback(typeof(Callbacks), CALLBACK_Delegate)]
            [BindCallback(typeof(Callbacks), CALLBACK_SmartDelegate)]
            [BindCallback(typeof(Callbacks), CALLBACK_Event)]
            [BindCallback(typeof(Callbacks), CALLBACK_UnityEvent)]
            public void ValidFunc()
                => Debug.Log("A#Func");

            [BindCallback(typeof(Callbacks), "2XXX2")]
            [BindCallback(typeof(Callbacks), "Field")]
            [BindCallback(typeof(Callbacks), "Prop")]
            [BindCallback(typeof(Callbacks), "Func")]
            public void InvalidFunc()
                => Debug.Log("A#Func");

            public class BinderPredicate : BindCallbackAttribute.IBinderPredicate
            {
                public bool EnableBind(MethodInfo methodInfo, object obj) => true;
                public bool AddCallbacks(object target, MethodInfo methodInfo, object obj) => true;
                public bool RemoveCallbacks(object target, MethodInfo methodInfo, object obj) => true;
            }
            [BindCallback(typeof(BinderPredicate))]
            public void Predicate()
                => Debug.Log("A#Func2");


            public class Callbacks
            {
                public delegate void TestDelegate();

                public TestDelegate Delegate { get; set; }
                public event TestDelegate Event;
                public NotInvokableDelegate<TestDelegate> SmartDelegate { get; set; }
                public UnityEvent UnityEvent { get; set; }

                public int Field = 0;
                public int Prop { get; set; }
                public void Func() { }
            }
        }

        /// <summary>
        /// <seealso cref="BindCallbackAttribute.IsValid"/>
        /// </summary>
        /// <returns></returns>
        [Test, Order(ORDER_IsValid), Description("Type And CallbackName")]
        public void IsValid_TypeAndCallback_Pass()
        {
            var type = typeof(IsValidTest);
            {
                var funcInfo = type.GetMethod("ValidFunc");
                var attrs = funcInfo.GetCustomAttributes<BindCallbackAttribute>();
                Assert.IsTrue(attrs.All(_a => _a.IsValid));
            }

            {
                var funcInfo = type.GetMethod("InvalidFunc");
                var attrs = funcInfo.GetCustomAttributes<BindCallbackAttribute>();
                Assert.IsFalse(attrs.All(_a => _a.IsValid));
            }
        }

        /// <summary>
        /// <seealso cref="BindCallbackAttribute.IsValid"/>
        /// </summary>
        /// <returns></returns>
        [Test, Order(ORDER_IsValid), Description("IBinderPredicate")]
        public void IsValid_IBinderPredicate_Pass()
        {
            var type = typeof(IsValidTest);
            var funcInfo = type.GetMethod("Predicate");
            var attrs = funcInfo.GetCustomAttributes<BindCallbackAttribute>();
            Assert.IsTrue(attrs.All(_a => _a.IsValid));
        }
        #endregion

        #region EnableBind(MethodInfo targetMethodInfo, object obj)
        class EnableBind_Instance_Test
        {
            [BindCallback(typeof(Callbacks), "Delegate1")]
            [BindCallback(typeof(Callbacks), "Event1")]
            [BindCallback(typeof(Callbacks), "SmartDelegate1")]
            [BindCallback(typeof(Callbacks), "UnityEvent1")]
            public void Func() { }

            [BindCallback(typeof(Callbacks), "Delegate2")]
            [BindCallback(typeof(Callbacks), "Event2")]
            [BindCallback(typeof(Callbacks), "SmartDelegate2")]
            public int Func2() => 0;

            [BindCallback(typeof(Callbacks), "Delegate3")]
            [BindCallback(typeof(Callbacks), "Event3")]
            [BindCallback(typeof(Callbacks), "SmartDelegate3")]
            public string Func3(int a, string b) => "";

            [BindCallback(typeof(Callbacks), "UnityEvent4")]
            public void Func4(int a, string b) { }

            public class BinderPredicate : BindCallbackAttribute.IBinderPredicate
            {
                public bool EnableBind(MethodInfo methodInfo, object obj)
                {
                    if (obj.GetType() != typeof(Callbacks)) return false;

                    return methodInfo.DoMatchReturnTypeAndArguments(typeof(void));
                }

                public bool AddCallbacks(object target, MethodInfo methodInfo, object obj) => true;
                public bool RemoveCallbacks(object target, MethodInfo methodInfo, object obj) => true;
            }
            [BindCallback(typeof(BinderPredicate))]
            public void BinderFunc() { }

            public class Callbacks
            {
                public delegate void TestDelegate();
                public delegate int TestDelegate2();
                public delegate string TestDelegate3(int a, string b);

                public TestDelegate Delegate1 { get; set; }
                public event TestDelegate Event1;
                public NotInvokableDelegate<TestDelegate> SmartDelegate1 { get; set; }
                public UnityEvent UnityEvent1 { get; }

                public TestDelegate2 Delegate2 { get; set; }
                public event TestDelegate2 Event2;
                public NotInvokableDelegate<TestDelegate2> SmartDelegate2 { get; set; }

                public TestDelegate3 Delegate3 { get; set; }
                public event TestDelegate3 Event3;
                public NotInvokableDelegate<TestDelegate3> SmartDelegate3 { get; set; }

                public class UnityEvent_4 : UnityEvent<int, string> { }
                public UnityEvent_4 UnityEvent4 { get; }
            }
        }
        [Test, Order(ORDER_EnableBind_Instance), Description("")]
        public void EnableBind_Instance_Passes()
        {
            var type = typeof(EnableBind_Instance_Test);
            var callbackType = typeof(EnableBind_Instance_Test.Callbacks);
            var funcName = "Func";
            var func2Name = "Func2";
            var func3Name = "Func3";
            var func4Name = "Func4";
            var binderFuncName = "BinderFunc";
            var callbackInst = new EnableBind_Instance_Test.Callbacks();
            var badInst = new EnableBindTest();
            var testData = new (bool enabled, string funcName, object callbackInst)[]
            {
                (true, funcName, callbackInst),
                (true, func2Name, callbackInst),
                (true, func3Name, callbackInst),
                (true, func4Name, callbackInst),
                (true, binderFuncName, callbackInst),

                (false, funcName, badInst),
                (false, func2Name, badInst),
                (false, func3Name, badInst),
                (false, func4Name, badInst),
                (false, binderFuncName, badInst),
            };
            foreach (var data in testData)
            {
                var methodInfo = type.GetMethod(data.funcName);
                var attrs = methodInfo.GetCustomAttributes<BindCallbackAttribute>();

                var br = System.Environment.NewLine;
                Assert.AreEqual(data.enabled, attrs.Any(_a => _a.EnableBind(methodInfo, data.callbackInst))
                    , $"-- method={methodInfo}{br}-- callback={data.callbackInst.GetType()}");
            }
        }

        #endregion

        #region ORDER_EnableBind
        class EnableBindTest
        {
            public void Func() { }
            public int Func2() => 0;
            public string Func3(int a, string b) => "";
            public void Func4(int a, string b) { }

            public class Callbacks
            {
                public delegate void TestDelegate();
                public delegate int TestDelegate2();
                public delegate string TestDelegate3(int a, string b);

                public TestDelegate Delegate1 { get; set; }
                public event TestDelegate Event1;
                public NotInvokableDelegate<TestDelegate> SmartDelegate1 { get; set; }
                public UnityEvent UnityEvent1 { get; }

                public TestDelegate2 Delegate2 { get; set; }
                public event TestDelegate2 Event2;
                public NotInvokableDelegate<TestDelegate2> SmartDelegate2 { get; set; }

                public TestDelegate3 Delegate3 { get; set; }
                public event TestDelegate3 Event3;
                public NotInvokableDelegate<TestDelegate3> SmartDelegate3 { get; set; }

                public class UnityEvent_4 : UnityEvent<int, string> { }
                public UnityEvent_4 UnityEvent4 { get; }
            }
        }

        [Test, Order(ORDER_EnableBind), Description("")]
        public void EnableBind_Passes()
        {
            var type = typeof(EnableBindTest);
            var callbackType = typeof(EnableBindTest.Callbacks);
            var funcName = "Func";
            var func2Name = "Func2";
            var func3Name = "Func3";
            var func4Name = "Func4";
            var testData = new (bool enabled, string funcName, System.Type callbackType, string callbackName)[]
            {
                (true, funcName, callbackType, "Delegate1"),
                (true, funcName, callbackType, "Event1"),
                (true, funcName, callbackType, "SmartDelegate1"),
                (true, funcName, callbackType, "UnityEvent1"),
                (true, func2Name, callbackType, "Delegate2"),
                (true, func2Name, callbackType, "Event2"),
                (true, func2Name, callbackType, "SmartDelegate2"),
                (true, func3Name, callbackType, "Delegate3"),
                (true, func3Name, callbackType, "Event3"),
                (true, func3Name, callbackType, "SmartDelegate3"),
                (true, func4Name, callbackType, "UnityEvent4"),

                (false, funcName, callbackType, "Delegate2"),
                (false, funcName, callbackType, "Event2"),
                (false, funcName, callbackType, "SmartDelegate2"),
                (false, funcName, callbackType, "UnityEvent2"),
                (false, func2Name, callbackType, "Delegate1"),
                (false, func2Name, callbackType, "Event3"),
                (false, func2Name, callbackType, "SmartDelegate1"),
                (false, func2Name, callbackType, "UnityEvent"),
                (false, func3Name, callbackType, "Delegate1"),
                (false, func3Name, callbackType, "Event1"),
                (false, func3Name, callbackType, "SmartDelegate1"),
                (false, func4Name, callbackType, "UnityEvent1"),
            };
            foreach (var data in testData)
            {
                var methodInfo = type.GetMethod(data.funcName);

                var br = System.Environment.NewLine;
                Assert.AreEqual(data.enabled, BindCallbackAttribute.EnableBind(methodInfo, data.callbackType, data.callbackName)
                    , $"-- method={methodInfo}{br}-- callback={data.callbackType}{br}-- callback name={data.callbackName}");
            }
        }
        #endregion

        #region Bind
        class BindTest
        {
            public const string LABEL1 = "LABEL1";
            public const string LABEL2 = "LABEL2";

            public const string CALLBACK_NAME = "UnityEvent";

            public int FuncCounter { get; set; }
            [BindCallback(typeof(CallbackClass), CALLBACK_NAME, Labels = new string[] { LABEL1 })]
            public void Func() => FuncCounter++;

            public class BinderPredicate : BindCallbackAttribute.IBinderPredicate
            {
                public bool EnableBind(MethodInfo methodInfo, object obj) => true;
                public bool AddCallbacks(object target, MethodInfo methodInfo, object obj)
                {
                    BindCallbackAttribute.BindWithTypeAndCallbackName<CallbackClass>(target, methodInfo, obj, "UnityEvent");
                    return true;
                }
                public bool RemoveCallbacks(object target, MethodInfo methodInfo, object obj)
                {
                    BindCallbackAttribute.UnbindWithTypeAndCallbackName<CallbackClass>(target, methodInfo, obj, "UnityEvent");
                    return true;
                }
            }
            [BindCallback(typeof(BinderPredicate), Labels = new string[] { LABEL2 })]
            public void Func2() => Func2Counter++;
            public int Func2Counter { get; set; }

            public class CallbackClass
            {
                public UnityEvent UnityEvent { get; } = new UnityEvent();
            }
        }

        /// <summary>
        /// <seealso cref="BindCallbackAttribute.Bind(object, MethodInfo, object)"/>
        /// </summary>
        [Test, Order(ORDER_Bind), Description("")]
        public void Bind_Passes()
        {
            {// test Func()
                var inst = new BindTest();
                var callback = new BindTest.CallbackClass();

                var funcInfo = inst.GetType().GetMethod("Func");
                var attr = funcInfo.GetCustomAttribute<BindCallbackAttribute>();
                Assert.IsTrue(attr.Bind(inst, funcInfo, callback));

                callback.UnityEvent.Invoke();

                Assert.AreEqual(1, inst.FuncCounter);
            }
            Logger.Log(Logger.Priority.High, () => "Success Type And CallbackName!");

            {//test Func2()
                var inst = new BindTest();
                var callback = new BindTest.CallbackClass();

                var funcInfo = inst.GetType().GetMethod("Func2");
                var attr = funcInfo.GetCustomAttribute<BindCallbackAttribute>();
                Assert.IsTrue(attr.Bind(inst, funcInfo, callback));

                callback.UnityEvent.Invoke();

                Assert.AreEqual(1, inst.Func2Counter);
            }
            Logger.Log(Logger.Priority.High, () => "Success IBinderPredicate!");
        }
        #endregion


        #region Unbind
        class UnbindTest
        {
            public const string LABEL1 = "LABEL1";
            public const string LABEL2 = "LABEL2";

            public const string CALLBACK_NAME = "onClick";

            public int FuncCounter { get; set; }
            [BindCallback(typeof(CallbackClass), CALLBACK_NAME, Labels = new string[] { LABEL1 })]
            public void Func() => FuncCounter++;

            public class BinderPredicate : BindCallbackAttribute.IBinderPredicate
            {
                public bool EnableBind(MethodInfo methodInfo, object obj) => true;
                public bool AddCallbacks(object target, MethodInfo methodInfo, object obj)
                {
                    BindCallbackAttribute.BindWithTypeAndCallbackName<CallbackClass>(target, methodInfo, obj, "UnityEvent");
                    return true;
                }
                public bool RemoveCallbacks(object target, MethodInfo methodInfo, object obj)
                {
                    BindCallbackAttribute.UnbindWithTypeAndCallbackName<CallbackClass>(target, methodInfo, obj, "UnityEvent");
                    return true;
                }
            }
            [BindCallback(typeof(BinderPredicate), Labels = new string[] { LABEL2 })]
            public void Func2() => Func2Counter++;
            public int Func2Counter { get; set; }

            public class CallbackClass
            {
                public UnityEvent UnityEvent { get; } = new UnityEvent();
            }
        }

        [Test, Order(ORDER_Unbind), Description("")]
        public void Unbind_Passes()
        {
            {// test Func()
                var inst = new BindTest();
                var callback = new BindTest.CallbackClass();

                var funcInfo = inst.GetType().GetMethod("Func");
                var attr = funcInfo.GetCustomAttribute<BindCallbackAttribute>();

                attr.Bind(inst, funcInfo, callback);
                Assert.IsTrue(attr.Unbind(inst, funcInfo, callback));

                callback.UnityEvent.Invoke();

                Assert.AreEqual(0, inst.FuncCounter);
            }
            Logger.Log(Logger.Priority.High, () => "Success Type And CallbackName!");

            {//test Func2()
                var inst = new BindTest();
                var callback = new BindTest.CallbackClass();

                var funcInfo = inst.GetType().GetMethod("Func2");
                var attr = funcInfo.GetCustomAttribute<BindCallbackAttribute>();

                attr.Bind(inst, funcInfo, callback);
                Assert.IsTrue(attr.Unbind(inst, funcInfo, callback));

                callback.UnityEvent.Invoke();
                Assert.AreEqual(0, inst.Func2Counter);
            }
            Logger.Log(Logger.Priority.High, () => "Success IBinderPredicate!");
        }
        #endregion

        #region GetMethodInfoAttrEnumerable
        class GetMethodInfoAttrEnumerableTest
        {
            public const string LABEL1 = "LABEL1";
            public const string LABEL2 = "LABEL2";

            public const string UNITY_EVENT_CALLBACK_NAME = "UnityEvent";
            public const string DELEGATE_CALLBACK_NAME = "Delegate";
            public const string SMART_DELEGATE_CALLBACK_NAME = "SmartDelegate";

            private void F() { }

            [BindCallback(typeof(CallbackClass), UNITY_EVENT_CALLBACK_NAME, Labels = new string[] { LABEL1 })]
            private void F2() { }

            [BindCallback(typeof(CallbackClass), UNITY_EVENT_CALLBACK_NAME, Labels = new string[] { LABEL1 })]
            [BindCallback(typeof(CallbackClass), DELEGATE_CALLBACK_NAME, Labels = new string[] { LABEL1 })]
            [BindCallback(typeof(CallbackClass), SMART_DELEGATE_CALLBACK_NAME, Labels = new string[] { LABEL1 })]
            public void Func()
                => Debug.Log("A#Func");

            public class BinderPredicate : BindCallbackAttribute.IBinderPredicate
            {
                public bool EnableBind(MethodInfo methodInfo, object obj) => true;
                public bool AddCallbacks(object target, MethodInfo methodInfo, object obj)
                {
                    return BindCallbackAttribute.BindWithTypeAndCallbackName<CallbackClass>(target, methodInfo, obj, UNITY_EVENT_CALLBACK_NAME);
                }
                public bool RemoveCallbacks(object target, MethodInfo methodInfo, object obj)
                {
                    return BindCallbackAttribute.UnbindWithTypeAndCallbackName<CallbackClass>(target, methodInfo, obj, UNITY_EVENT_CALLBACK_NAME);
                }
            }

            [BindCallback(typeof(BinderPredicate), Labels = new string[] { LABEL2 })]
            public void Func2()
                => Debug.Log("A#Func2");

            public void Func3() { }

            public class CallbackClass
            {
                public UnityEvent UnityEvent { get; } = new UnityEvent();

                public delegate void TestDelegate();
                TestDelegate _delegate = () => { };
                public System.Delegate Delegate { get => _delegate; }

                SmartDelegate<TestDelegate> _smartDelegate = new SmartDelegate<TestDelegate>();
                public NotInvokableDelegate<TestDelegate> SmartDelegate { get => _smartDelegate; }
            }
        }

        /// <summary>
        /// <seealso cref="BindCallbackAttribute.GetMethodInfoAndAttrEnumerable(System.Type)"/>
        /// <seealso cref="BindCallbackAttribute.GetMethodInfoAndAttrEnumerable{T}"/>
        /// </summary>
        [Test, Order(ORDER_GetMethodInfoAttrEnumerable), Description("")]
        public void GetMethodInfoAttrEnumerable_Passes()
        {
            var inst = new GetMethodInfoAttrEnumerableTest();
            var type = inst.GetType();

            AssertionUtils.AssertEnumerableByUnordered(
                //private Methodは除外されるようにしてください
                type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Select(_m => (methodInfo: _m, attrs: _m.GetCustomAttributes<BindCallbackAttribute>()))
                    .Where(_t => _t.attrs != null && _t.attrs.Any())
                , BindCallbackAttribute.GetMethodInfoAndAttrEnumerable(type)
                , ""
                , (_c, _g) =>
                    _c.methodInfo.Equals(_g.methodInfo)
                    && _c.attrs.Count() == _g.attrs.Count()
                    && _c.attrs.Zip(_g.attrs, (_ca, _ga) => (ca: _ca, ga: _ga))
                        .All(_t => _t.ca.Equals(_t.ga))
            );
        }
        #endregion

        #region BindWithTypeAndCallbackName

        class BindWithTypeAndCallbackNameTest
        {
            public const string LABEL1 = "LABEL1";
            public const string LABEL2 = "LABEL2";

            public const string UNITY_EVENT_CALLBACK_NAME = "UnityEvent";
            public const string DELEGATE_CALLBACK_NAME = "Delegate";
            public const string EVENT_NAME = "TestEvent";
            public const string SMART_DELEGATE_CALLBACK_NAME = "SmartDelegate";

            public int FuncCounter { get; set; }
            public int Func2Counter { get; set; }

            private void F() { }

            [BindCallback(typeof(CallbackClass), UNITY_EVENT_CALLBACK_NAME, Labels = new string[] { LABEL1 })]
            private void F2() { }

            [BindCallback(typeof(CallbackClass), UNITY_EVENT_CALLBACK_NAME, Labels = new string[] { LABEL1 })]
            [BindCallback(typeof(CallbackClass), DELEGATE_CALLBACK_NAME, Labels = new string[] { LABEL1 })]
            [BindCallback(typeof(CallbackClass), EVENT_NAME, Labels = new string[] { LABEL1 })]
            [BindCallback(typeof(CallbackClass), SMART_DELEGATE_CALLBACK_NAME, Labels = new string[] { LABEL1 })]
            public void Func() => FuncCounter++;

            public class BinderPredicate : BindCallbackAttribute.IBinderPredicate
            {
                public bool EnableBind(MethodInfo methodInfo, object obj) => true;
                public bool AddCallbacks(object target, MethodInfo methodInfo, object obj)
                {
                    return BindCallbackAttribute.BindWithTypeAndCallbackName<CallbackClass>(target, methodInfo, obj, UNITY_EVENT_CALLBACK_NAME);
                }
                public bool RemoveCallbacks(object target, MethodInfo methodInfo, object obj)
                {
                    return BindCallbackAttribute.UnbindWithTypeAndCallbackName<CallbackClass>(target, methodInfo, obj, UNITY_EVENT_CALLBACK_NAME);
                }
            }
            [BindCallback(typeof(BinderPredicate), Labels = new string[] { LABEL2 })]
            public void Func2() => Func2Counter++;

            public void Func3() { }

            public class CallbackClass
            {
                public UnityEvent UnityEvent { get; } = new UnityEvent();

                public delegate void TestDelegate();
                TestDelegate _delegate = () => { };
                public TestDelegate Delegate { get => _delegate; set => _delegate = value; }

                public event TestDelegate TestEvent;
                public void CallTestEvent()
                {
                    TestEvent?.DynamicInvoke(new object[] { });
                }

                SmartDelegate<TestDelegate> _smartDelegate = new SmartDelegate<TestDelegate>();
                public NotInvokableDelegate<TestDelegate> SmartDelegate { get => _smartDelegate; }
                public void CallSmartDelegate()
                {
                    _smartDelegate.SafeDynamicInvoke(() => "");
                }
            }
        }

        /// <summary>
        ///<seealso cref="BindCallbackAttribute.BindWithTypeAndCallbackName(object, MethodInfo, object, System.Type, string)"/>
        ///<seealso cref="BindCallbackAttribute.BindWithTypeAndCallbackName{T}(object, MethodInfo, object, string)"/>
        /// </summary>
        [Test, Order(ORDER_BindWithTypeAndCallbackName), Description("UnityEvent with object")]
        public void BindWithTypeAndCallbackName_ToUnityEvent_Passes()
        {
            var inst = new BindWithTypeAndCallbackNameTest();
            var type = inst.GetType();
            var callbackInst = new BindWithTypeAndCallbackNameTest.CallbackClass();

            var instMethodName = "Func";
            var isOK = BindCallbackAttribute.BindWithTypeAndCallbackName<BindWithTypeAndCallbackNameTest.CallbackClass>(
                inst, type.GetMethod(instMethodName), callbackInst, "UnityEvent"
            );
            Assert.IsTrue(isOK);

            callbackInst.UnityEvent.Invoke();
            Assert.AreEqual(1, inst.FuncCounter);
        }

        /// <summary>
        ///<seealso cref="BindCallbackAttribute.BindWithTypeAndCallbackName(object, MethodInfo, object, System.Type, string)"/>
        ///<seealso cref="BindCallbackAttribute.BindWithTypeAndCallbackName{T}(object, MethodInfo, object, string)"/>
        /// </summary>
        [Test, Order(ORDER_BindWithTypeAndCallbackName), Description("System.Delegate with object")]
        public void BindWithTypeAndCallbackName_ToSystemDelegate_Passes()
        {
            var inst = new BindWithTypeAndCallbackNameTest();
            var type = inst.GetType();
            var callbackInst = new BindWithTypeAndCallbackNameTest.CallbackClass();

            var instMethodName = "Func";
            var isOK = BindCallbackAttribute.BindWithTypeAndCallbackName<BindWithTypeAndCallbackNameTest.CallbackClass>(
                inst, type.GetMethod(instMethodName), callbackInst, "Delegate"
            );
            Assert.IsTrue(isOK);

            var list = callbackInst.Delegate.GetInvocationList();
            var pred = type.GetMethod(instMethodName)
                .CreateDelegate(typeof(BindWithTypeAndCallbackNameTest.CallbackClass.TestDelegate), inst);
            Assert.AreEqual(1, list.Count(_l => _l.Equals(pred)));

            callbackInst.Delegate.DynamicInvoke(new object[] { });
            Assert.AreEqual(1, inst.FuncCounter);
        }

        /// <summary>
        ///<seealso cref="BindCallbackAttribute.BindWithTypeAndCallbackName(object, MethodInfo, object, System.Type, string)"/>
        ///<seealso cref="BindCallbackAttribute.BindWithTypeAndCallbackName{T}(object, MethodInfo, object, string)"/>
        /// </summary>
        [Test, Order(ORDER_BindWithTypeAndCallbackName), Description("event with object")]
        public void BindWithTypeAndCallbackName_ToEvent_Passes()
        {
            var inst = new BindWithTypeAndCallbackNameTest();
            var type = inst.GetType();
            var callbackInst = new BindWithTypeAndCallbackNameTest.CallbackClass();

            var instMethodName = "Func";
            var isOK = BindCallbackAttribute.BindWithTypeAndCallbackName<BindWithTypeAndCallbackNameTest.CallbackClass>(
                inst, type.GetMethod(instMethodName), callbackInst, "TestEvent"
            );
            Assert.IsTrue(isOK);

            callbackInst.CallTestEvent();
            Assert.AreEqual(1, inst.FuncCounter);
        }

        /// <summary>
        ///<seealso cref="BindCallbackAttribute.BindWithTypeAndCallbackName(object, MethodInfo, object, System.Type, string)"/>
        ///<seealso cref="BindCallbackAttribute.BindWithTypeAndCallbackName{T}(object, MethodInfo, object, string)"/>
        /// </summary>
        [Test, Order(ORDER_BindWithTypeAndCallbackName), Description("SmartDelegate with object")]
        public void BindWithTypeAndCallbackName_ToSmartDelegate_Passes()
        {
            var inst = new BindWithTypeAndCallbackNameTest();
            var type = inst.GetType();
            var callbackInst = new BindWithTypeAndCallbackNameTest.CallbackClass();

            var instMethodName = "Func";
            var isOK = BindCallbackAttribute.BindWithTypeAndCallbackName<BindWithTypeAndCallbackNameTest.CallbackClass>(
                inst, type.GetMethod(instMethodName), callbackInst, "SmartDelegate"
            );
            Assert.IsTrue(isOK);

            Assert.IsTrue(callbackInst.SmartDelegate.Contains(inst.Func));
            Assert.AreEqual(1, callbackInst.SmartDelegate.RegistedDelegateCount);

            callbackInst.CallSmartDelegate();
            Assert.AreEqual(1, inst.FuncCounter);
        }

        /// <summary>
        ///<seealso cref="BindCallbackAttribute.BindWithTypeAndCallbackName(object, MethodInfo, object, System.Type, string)"/>
        ///<seealso cref="BindCallbackAttribute.BindWithTypeAndCallbackName{T}(object, MethodInfo, object, string)"/>
        /// </summary>
        [Test, Order(ORDER_BindWithTypeAndCallbackName), Description("miss match case")]
        public void BindWithTypeAndCallbackName_MissMatchCase_Passes()
        {
            Assert.DoesNotThrow(() => {
                var inst = new BindWithTypeAndCallbackNameTest();
                var type = inst.GetType();
                var callbackInst = new BindWithTypeAndCallbackNameTest.CallbackClass();

                var instMethodName = "1XXX1";
                var isOK = BindCallbackAttribute.BindWithTypeAndCallbackName<BindWithTypeAndCallbackNameTest.CallbackClass>(
                    inst, type.GetMethod(instMethodName), callbackInst, "SmartDelegate"
                );
                Assert.IsFalse(isOK);

                Assert.AreEqual(0, callbackInst.SmartDelegate.RegistedDelegateCount);
            }, "Invalid Instance MethodName...");
            Logger.Log(Logger.Priority.High, () => "Success to Invali Instance MethodName!");

            Assert.DoesNotThrow(() => {
                var inst = new BindWithTypeAndCallbackNameTest();
                var type = inst.GetType();
                var callbackInst = new BindWithTypeAndCallbackNameTest.CallbackClass();

                var instMethodName = "1XXX1";
                var isOK = BindCallbackAttribute.BindWithTypeAndCallbackName<BindWithTypeAndCallbackNameTest.CallbackClass>(
                    null, type.GetMethod(instMethodName), callbackInst, "SmartDelegate"
                );
                Assert.IsFalse(isOK);

                Assert.AreEqual(0, callbackInst.SmartDelegate.RegistedDelegateCount);
            }, "target is null...");
            Logger.Log(Logger.Priority.High, () => "Success target is null!");

            Assert.DoesNotThrow(() => {
                var inst = new BindWithTypeAndCallbackNameTest();
                var type = inst.GetType();
                var callbackInst = new BindWithTypeAndCallbackNameTest.CallbackClass();

                var instMethodName = "1XXX1";
                var isOK = BindCallbackAttribute.BindWithTypeAndCallbackName<BindWithTypeAndCallbackNameTest.CallbackClass>(
                    inst, type.GetMethod(instMethodName), null, "SmartDelegate"
                );
                Assert.IsFalse(isOK);

                Assert.AreEqual(0, callbackInst.SmartDelegate.RegistedDelegateCount);
            }, "callbackInstance is null...");
            Logger.Log(Logger.Priority.High, () => "Success callbackInstance is null!");

            Assert.DoesNotThrow(() => {
                var inst = new BindWithTypeAndCallbackNameTest();
                var type = inst.GetType();
                var callbackInst = new BindWithTypeAndCallbackNameTest.CallbackClass();

                var instMethodName = "1XXX1";
                var isOK = BindCallbackAttribute.BindWithTypeAndCallbackName<BindWithTypeAndCallbackNameTest.CallbackClass>(
                    inst, type.GetMethod(instMethodName), callbackInst, "1XXX1"
                );
                Assert.IsFalse(isOK);

            }, "Invalid callbackName...");
            Logger.Log(Logger.Priority.High, () => "Success to Invalid callbackName!");
        }

        class BindWithTypeAndCallbackNameTest2
        {
            public const string UNITY_EVENT_CALLBACK_NAME = "UnityEvent";
            public const string DELEGATE_CALLBACK_NAME = "Delegate";
            public const string EVENT_NAME = "TestEvent";
            public const string SMART_DELEGATE_CALLBACK_NAME = "SmartDelegate";

            [BindCallback(typeof(CallbackClass), UNITY_EVENT_CALLBACK_NAME)]
            public void UnityEventFunc(int a, string b)
            {
                UnityEventFuncCounter++;
            }
            public int UnityEventFuncCounter { get; set; }

            [BindCallback(typeof(CallbackClass), DELEGATE_CALLBACK_NAME)]
            [BindCallback(typeof(CallbackClass), EVENT_NAME)]
            [BindCallback(typeof(CallbackClass), SMART_DELEGATE_CALLBACK_NAME)]
            public int Func(int a, string b)
            {
                FuncCounter++;
                return a + b.Length;
            }
            public int FuncCounter { get; set; }

            public class CallbackClass
            {
                public class UnityEventEX : UnityEvent<int, string>
                { }

                public UnityEvent<int, string> UnityEvent { get; } = new UnityEventEX();

                public delegate int TestDelegate(int a, string b);
                TestDelegate _delegate = (_, __) => 0;
                public TestDelegate Delegate { get => _delegate; set => _delegate = value; }

                public event TestDelegate TestEvent;
                public int CallTestEvent(int a, string b)
                {
                    return (int)TestEvent?.DynamicInvoke(new object[] { a, b });
                }

                SmartDelegate<TestDelegate> _smartDelegate = new SmartDelegate<TestDelegate>();
                public NotInvokableDelegate<TestDelegate> SmartDelegate { get => _smartDelegate; }
                public IEnumerable<int> CallSmartDelegate(int a, string b)
                {
                    return _smartDelegate.SafeDynamicInvoke(a, b, () => "").OfType<int>();
                }
            }
        }

        /// <summary>
        ///<seealso cref="BindCallbackAttribute.BindWithTypeAndCallbackName(object, MethodInfo, object, System.Type, string)"/>
        ///<seealso cref="BindCallbackAttribute.BindWithTypeAndCallbackName{T}(object, MethodInfo, object, string)"/>
        /// </summary>
        [Test, Order(ORDER_BindWithTypeAndCallbackName), Description("With Arguments and ReturnValue")]
        public void BindWithTypeAndCallbackName_WithArgumentAndReturnValue_Passes()
        {
            {//Test UnityEvent<int, string>
                var inst = new BindWithTypeAndCallbackNameTest2();
                var type = inst.GetType();
                var callbackInst = new BindWithTypeAndCallbackNameTest2.CallbackClass();

                var instMethodName = "UnityEventFunc";
                var isOK = BindCallbackAttribute.BindWithTypeAndCallbackName<BindWithTypeAndCallbackNameTest2.CallbackClass>(
                    inst, type.GetMethod(instMethodName), callbackInst, "UnityEvent"
                );
                Assert.IsTrue(isOK);

                // note: UnityEvent don't have ReturnValue
                var a = 1;
                var b = "abc";
                callbackInst.UnityEvent.Invoke(a, b);
                Assert.AreEqual(1, inst.UnityEventFuncCounter);
            }
            Logger.Log(Logger.Priority.High, () => "Success UnityEvent<int, string>");

            {//Test System.Delegate
                var inst = new BindWithTypeAndCallbackNameTest2();
                var type = inst.GetType();
                var callbackInst = new BindWithTypeAndCallbackNameTest2.CallbackClass();

                var instMethodName = "Func";
                var isOK = BindCallbackAttribute.BindWithTypeAndCallbackName<BindWithTypeAndCallbackNameTest2.CallbackClass>(
                    inst, type.GetMethod(instMethodName), callbackInst, "Delegate"
                );
                Assert.IsTrue(isOK);

                Assert.AreEqual(2, callbackInst.Delegate.GetInvocationList().Length);//include default callback

                var a = 1;
                var b = "abc";
                var ret = callbackInst.Delegate.Invoke(a, b);
                Assert.AreEqual(1, inst.FuncCounter);
                Assert.AreEqual(a + b.Length, ret);
            }
            Logger.Log(Logger.Priority.High, () => "Success Delegate");

            {//Test event
                var inst = new BindWithTypeAndCallbackNameTest2();
                var type = inst.GetType();
                var callbackInst = new BindWithTypeAndCallbackNameTest2.CallbackClass();

                var instMethodName = "Func";
                var isOK = BindCallbackAttribute.BindWithTypeAndCallbackName<BindWithTypeAndCallbackNameTest2.CallbackClass>(
                    inst, type.GetMethod(instMethodName), callbackInst, "TestEvent"
                );
                Assert.IsTrue(isOK);

                var a = 1;
                var b = "abc";
                var ret = callbackInst.CallTestEvent(a, b);
                Assert.AreEqual(1, inst.FuncCounter);
                Assert.AreEqual(a + b.Length, ret);
            }
            Logger.Log(Logger.Priority.High, () => "Success Event");

            {//Test SmartDelegate
                var inst = new BindWithTypeAndCallbackNameTest2();
                var type = inst.GetType();
                var callbackInst = new BindWithTypeAndCallbackNameTest2.CallbackClass();

                var instMethodName = "Func";
                var isOK = BindCallbackAttribute.BindWithTypeAndCallbackName<BindWithTypeAndCallbackNameTest2.CallbackClass>(
                    inst, type.GetMethod(instMethodName), callbackInst, "SmartDelegate"
                );
                Assert.IsTrue(isOK);

                Assert.IsTrue(callbackInst.SmartDelegate.Contains(inst.Func));
                Assert.AreEqual(1, callbackInst.SmartDelegate.RegistedDelegateCount);

                var a = 1;
                var b = "abc";
                var ret = callbackInst.CallSmartDelegate(a, b);
                Assert.AreEqual(1, inst.FuncCounter);
                AssertionUtils.AssertEnumerable(
                    new int[] { a+b.Length }
                    , ret
                    , ""
                );
            }
            Logger.Log(Logger.Priority.High, () => "Success SmartDelegate");
        }

        class BindWithTypeAndCallbackNameTestNotBind
        {
            public const string UNITY_EVENT_CALLBACK_NAME = "UnityEvent";
            public const string DELEGATE_CALLBACK_NAME = "Delegate";
            public const string EVENT_NAME = "TestEvent";
            public const string SMART_DELEGATE_CALLBACK_NAME = "SmartDelegate";

            [BindCallback(typeof(CallbackClass), UNITY_EVENT_CALLBACK_NAME)]
            public void UnityEventFunc(int a, string b)
            {
                UnityEventFuncCounter++;
            }
            public int UnityEventFuncCounter { get; set; }

            [BindCallback(typeof(CallbackClass), DELEGATE_CALLBACK_NAME)]
            [BindCallback(typeof(CallbackClass), EVENT_NAME)]
            [BindCallback(typeof(CallbackClass), SMART_DELEGATE_CALLBACK_NAME)]
            public int Func(int a, string b)
            {
                FuncCounter++;
                return a + b.Length;
            }
            public int FuncCounter { get; set; }

            public string InvalidFunc(int a, float b, double c)
            {
                InvalidFuncCounter++;
                return "INVALID_FUNC";
            }
            public int InvalidFuncCounter { get; set; }

            public class CallbackClass
            {
                public class UnityEventEX : UnityEvent<int, string>
                { }

                public UnityEvent<int, string> UnityEvent { get; } = new UnityEventEX();

                public delegate int TestDelegate(int a, string b);
                TestDelegate _delegate = (_, __) => 0;
                public TestDelegate Delegate { get => _delegate; set => _delegate = value; }

                public event TestDelegate TestEvent;
                public int CallTestEvent(int a, string b)
                {
                    return (int)(TestEvent?.DynamicInvoke(new object[] { a, b }) ?? 0);
                }

                SmartDelegate<TestDelegate> _smartDelegate = new SmartDelegate<TestDelegate>();
                public NotInvokableDelegate<TestDelegate> SmartDelegate { get => _smartDelegate; }
                public IEnumerable<int> CallSmartDelegate(int a, string b)
                {
                    var e = _smartDelegate.SafeDynamicInvoke(a, b, () => "");
                    return e?.OfType<int>() ?? null;
                }
            }
        }
        /// <summary>
        ///<seealso cref="BindCallbackAttribute.BindWithTypeAndCallbackName(object, MethodInfo, object, System.Type, string)"/>
        ///<seealso cref="BindCallbackAttribute.BindWithTypeAndCallbackName{T}(object, MethodInfo, object, string)"/>
        /// </summary>
        [Test, Order(ORDER_BindWithTypeAndCallbackName), Description("Not Bind Case(but Nothrow Exception)")]
        public void BindWithTypeAndCallbackName_NotBindCase_Passes()
        {
            {//Test UnityEvent
                var inst = new BindWithTypeAndCallbackNameTestNotBind();
                var type = inst.GetType();
                var callbackInst = new BindWithTypeAndCallbackNameTestNotBind.CallbackClass();

                var invalidMethodName = "InvalidFunc";
                var isOK = BindCallbackAttribute.BindWithTypeAndCallbackName<BindWithTypeAndCallbackNameTestNotBind.CallbackClass>(
                    inst
                    , type.GetMethod(invalidMethodName)
                    , callbackInst
                    , BindWithTypeAndCallbackNameTestNotBind.UNITY_EVENT_CALLBACK_NAME
                );
                Assert.IsFalse(isOK);

                callbackInst.UnityEvent.Invoke(0, "");
                Assert.AreEqual(0, inst.InvalidFuncCounter);
            }
            Logger.Log(Logger.Priority.High, () => "Success UnityEvent");

            {//Test System.Delegate
                var inst = new BindWithTypeAndCallbackNameTestNotBind();
                var type = inst.GetType();
                var callbackInst = new BindWithTypeAndCallbackNameTestNotBind.CallbackClass();

                var invalidMethodName = "InvalidFunc";
                var isOK = BindCallbackAttribute.BindWithTypeAndCallbackName<BindWithTypeAndCallbackNameTestNotBind.CallbackClass>(
                    inst
                    , type.GetMethod(invalidMethodName)
                    , callbackInst
                    , BindWithTypeAndCallbackNameTestNotBind.DELEGATE_CALLBACK_NAME
                );
                Assert.IsFalse(isOK);

                //include default callback
                Assert.AreEqual(1, callbackInst.Delegate.GetInvocationList().Length);
            }
            Logger.Log(Logger.Priority.High, () => "Success Delegate");

            {//Test event
                var inst = new BindWithTypeAndCallbackNameTestNotBind();
                var type = inst.GetType();
                var callbackInst = new BindWithTypeAndCallbackNameTestNotBind.CallbackClass();

                var invalidMethodName = "InvalidFunc";
                var isOK = BindCallbackAttribute.BindWithTypeAndCallbackName<BindWithTypeAndCallbackNameTestNotBind.CallbackClass>(
                    inst
                    , type.GetMethod(invalidMethodName)
                    , callbackInst
                    , BindWithTypeAndCallbackNameTestNotBind.EVENT_NAME
                );
                Assert.IsFalse(isOK);

                var a = 1;
                var b = "abc";
                var ret = callbackInst.CallTestEvent(a, b);
                Assert.AreEqual(0, inst.FuncCounter);
            }
            Logger.Log(Logger.Priority.High, () => "Success Event");

            
            {//Test SmartDelegate
                var inst = new BindWithTypeAndCallbackNameTestNotBind();
                var type = inst.GetType();
                var callbackInst = new BindWithTypeAndCallbackNameTestNotBind.CallbackClass();

                var invalidMethodName = "InvalidFunc";
                var isOK = BindCallbackAttribute.BindWithTypeAndCallbackName<BindWithTypeAndCallbackNameTestNotBind.CallbackClass>(
                    inst
                    , type.GetMethod(invalidMethodName)
                    , callbackInst
                    , BindWithTypeAndCallbackNameTestNotBind.SMART_DELEGATE_CALLBACK_NAME
                );
                Assert.IsFalse(isOK);

                Assert.IsFalse(callbackInst.SmartDelegate.Contains(inst.Func));
                Assert.AreEqual(0, callbackInst.SmartDelegate.RegistedDelegateCount);

                var a = 1;
                var b = "abc";
                var ret = callbackInst.CallSmartDelegate(a, b);
                AssertionUtils.AssertEnumerable(
                    null
                    , ret
                    , ""
                );
            }
            Logger.Log(Logger.Priority.High, () => "Success SmartDelegate");
        }
        #endregion

        #region UnbindWithTypeAndCallbackName

        class UnbindWithTypeAndCallbackNameTest
        {
            public const string LABEL1 = "LABEL1";
            public const string LABEL2 = "LABEL2";

            public const string UNITY_EVENT_CALLBACK_NAME = "UnityEvent";
            public const string DELEGATE_CALLBACK_NAME = "Delegate";
            public const string EVENT_NAME = "TestEvent";
            public const string SMART_DELEGATE_CALLBACK_NAME = "SmartDelegate";

            public int FuncCounter { get; set; }
            public int Func2Counter { get; set; }

            private void F() { }

            [BindCallback(typeof(CallbackClass), UNITY_EVENT_CALLBACK_NAME, Labels = new string[] { LABEL1 })]
            private void F2() { }

            [BindCallback(typeof(CallbackClass), UNITY_EVENT_CALLBACK_NAME, Labels = new string[] { LABEL1 })]
            [BindCallback(typeof(CallbackClass), DELEGATE_CALLBACK_NAME, Labels = new string[] { LABEL1 })]
            [BindCallback(typeof(CallbackClass), EVENT_NAME, Labels = new string[] { LABEL1 })]
            [BindCallback(typeof(CallbackClass), SMART_DELEGATE_CALLBACK_NAME, Labels = new string[] { LABEL1 })]
            public void Func() => FuncCounter++;

            public class BinderPredicate : BindCallbackAttribute.IBinderPredicate
            {
                public bool EnableBind(MethodInfo methodInfo, object obj) => true;
                public bool AddCallbacks(object target, MethodInfo methodInfo, object obj)
                {
                    return BindCallbackAttribute.BindWithTypeAndCallbackName<CallbackClass>(target, methodInfo, obj, UNITY_EVENT_CALLBACK_NAME);
                }
                public bool RemoveCallbacks(object target, MethodInfo methodInfo, object obj)
                {
                    return BindCallbackAttribute.UnbindWithTypeAndCallbackName<CallbackClass>(target, methodInfo, obj, UNITY_EVENT_CALLBACK_NAME);
                }
            }
            [BindCallback(typeof(BinderPredicate), Labels = new string[] { LABEL2 })]
            public void Func2() => Func2Counter++;

            public void Func3() { }

            public class CallbackClass
            {
                public UnityEvent UnityEvent { get; } = new UnityEvent();

                public delegate void TestDelegate();
                TestDelegate _delegate = () => { };
                public TestDelegate Delegate { get => _delegate; set => _delegate = value; }

                public event TestDelegate TestEvent;
                public void CallTestEvent()
                {
                    TestEvent?.DynamicInvoke(new object[] { });
                }

                SmartDelegate<TestDelegate> _smartDelegate = new SmartDelegate<TestDelegate>();
                public NotInvokableDelegate<TestDelegate> SmartDelegate { get => _smartDelegate; }
                public void CallSmartDelegate()
                {
                    _smartDelegate.SafeDynamicInvoke(() => "");
                }
            }
        }

        /// <summary>
        ///<seealso cref="BindCallbackAttribute.UnbindWithTypeAndCallbackName(object, MethodInfo, object, System.Type, string)"/>
        ///<seealso cref="BindCallbackAttribute.UnbindWithTypeAndCallbackName{T}(object, MethodInfo, object, string)"/>
        /// </summary>
        [Test, Order(ORDER_UnbindWithTypeAndCallbackName), Description("UnityEvent with object")]
        public void UnbindWithTypeAndCallbackName_ToUnityEvent_Passes()
        {
            var inst = new UnbindWithTypeAndCallbackNameTest();
            var type = inst.GetType();
            var callbackInst = new UnbindWithTypeAndCallbackNameTest.CallbackClass();

            // Unbind after Bind
            var instMethodName = "Func";
            BindCallbackAttribute.BindWithTypeAndCallbackName<UnbindWithTypeAndCallbackNameTest.CallbackClass>(
                inst, type.GetMethod(instMethodName), callbackInst, "UnityEvent"
            );
            var isOK = BindCallbackAttribute.UnbindWithTypeAndCallbackName<UnbindWithTypeAndCallbackNameTest.CallbackClass>(
                inst, type.GetMethod(instMethodName), callbackInst, "UnityEvent"
            );
            Assert.IsTrue(isOK);

            callbackInst.UnityEvent.Invoke();
            Assert.AreEqual(0, inst.FuncCounter);
        }

        /// <summary>
        ///<seealso cref="BindCallbackAttribute.UnbindWithTypeAndCallbackName(object, MethodInfo, object, System.Type, string)"/>
        ///<seealso cref="BindCallbackAttribute.UnbindWithTypeAndCallbackName{T}(object, MethodInfo, object, string)"/>
        /// </summary>
        [Test, Order(ORDER_UnbindWithTypeAndCallbackName), Description("System.Delegate with object")]
        public void UnbindWithTypeAndCallbackName_ToSystemDelegate_Passes()
        {
            var inst = new UnbindWithTypeAndCallbackNameTest();
            var type = inst.GetType();
            var callbackInst = new UnbindWithTypeAndCallbackNameTest.CallbackClass();

            var instMethodName = "Func";
            BindCallbackAttribute.BindWithTypeAndCallbackName<UnbindWithTypeAndCallbackNameTest.CallbackClass>(
                inst, type.GetMethod(instMethodName), callbackInst, "Delegate"
            );
            var isOK = BindCallbackAttribute.UnbindWithTypeAndCallbackName<UnbindWithTypeAndCallbackNameTest.CallbackClass>(
                inst, type.GetMethod(instMethodName), callbackInst, "Delegate"
            );
            Assert.IsTrue(isOK);

            var list = callbackInst.Delegate.GetInvocationList();
            var pred = type.GetMethod(instMethodName)
                .CreateDelegate(typeof(UnbindWithTypeAndCallbackNameTest.CallbackClass.TestDelegate), inst);
            Assert.AreEqual(0, list.Count(_l => _l.Equals(pred)));

            callbackInst.Delegate.DynamicInvoke(new object[] { });
            Assert.AreEqual(0, inst.FuncCounter);
        }

        /// <summary>
        ///<seealso cref="BindCallbackAttribute.UnbindWithTypeAndCallbackName(object, MethodInfo, object, System.Type, string)"/>
        ///<seealso cref="BindCallbackAttribute.UnbindWithTypeAndCallbackName{T}(object, MethodInfo, object, string)"/>
        /// </summary>
        [Test, Order(ORDER_UnbindWithTypeAndCallbackName), Description("event with object")]
        public void UnbindWithTypeAndCallbackName_ToEvent_Passes()
        {
            var inst = new UnbindWithTypeAndCallbackNameTest();
            var type = inst.GetType();
            var callbackInst = new UnbindWithTypeAndCallbackNameTest.CallbackClass();

            var instMethodName = "Func";
            BindCallbackAttribute.BindWithTypeAndCallbackName<UnbindWithTypeAndCallbackNameTest.CallbackClass>(
                inst, type.GetMethod(instMethodName), callbackInst, "TestEvent"
            );
            var isOK = BindCallbackAttribute.UnbindWithTypeAndCallbackName<UnbindWithTypeAndCallbackNameTest.CallbackClass>(
                inst, type.GetMethod(instMethodName), callbackInst, "TestEvent"
            );
            Assert.IsTrue(isOK);

            callbackInst.CallTestEvent();
            Assert.AreEqual(0, inst.FuncCounter);
        }

        /// <summary>
        ///<seealso cref="BindCallbackAttribute.UnbindWithTypeAndCallbackName(object, MethodInfo, object, System.Type, string)"/>
        ///<seealso cref="BindCallbackAttribute.UnbindWithTypeAndCallbackName{T}(object, MethodInfo, object, string)"/>
        /// </summary>
        [Test, Order(ORDER_UnbindWithTypeAndCallbackName), Description("SmartDelegate with object")]
        public void UnbindWithTypeAndCallbackName_ToSmartDelegate_Passes()
        {
            var inst = new UnbindWithTypeAndCallbackNameTest();
            var type = inst.GetType();
            var callbackInst = new UnbindWithTypeAndCallbackNameTest.CallbackClass();

            var instMethodName = "Func";
            BindCallbackAttribute.BindWithTypeAndCallbackName<UnbindWithTypeAndCallbackNameTest.CallbackClass>(
                inst, type.GetMethod(instMethodName), callbackInst, "SmartDelegate"
            );
            var isOK = BindCallbackAttribute.UnbindWithTypeAndCallbackName<UnbindWithTypeAndCallbackNameTest.CallbackClass>(
                inst, type.GetMethod(instMethodName), callbackInst, "SmartDelegate"
            );
            Assert.IsTrue(isOK);

            Assert.IsFalse(callbackInst.SmartDelegate.Contains(inst.Func));
            Assert.AreEqual(0, callbackInst.SmartDelegate.RegistedDelegateCount);

            callbackInst.CallSmartDelegate();
            Assert.AreEqual(0, inst.FuncCounter);
        }

        /// <summary>
        ///<seealso cref="BindCallbackAttribute.UnbindWithTypeAndCallbackName(object, MethodInfo, object, System.Type, string)"/>
        ///<seealso cref="BindCallbackAttribute.UnbindWithTypeAndCallbackName{T}(object, MethodInfo, object, string)"/>
        /// </summary>
        [Test, Order(ORDER_UnbindWithTypeAndCallbackName), Description("miss match case")]
        public void UnbindWithTypeAndCallbackName_MissMatchCase_Passes()
        {
            Assert.DoesNotThrow(() => {
                var inst = new UnbindWithTypeAndCallbackNameTest();
                var type = inst.GetType();
                var callbackInst = new UnbindWithTypeAndCallbackNameTest.CallbackClass();

                var instMethodName = "Func";
                BindCallbackAttribute.BindWithTypeAndCallbackName<UnbindWithTypeAndCallbackNameTest.CallbackClass>(
                    inst, type.GetMethod(instMethodName), callbackInst, "SmartDelegate"
                );
                Assert.AreEqual(1, callbackInst.SmartDelegate.RegistedDelegateCount);

                var isOK = BindCallbackAttribute.UnbindWithTypeAndCallbackName<UnbindWithTypeAndCallbackNameTest.CallbackClass>(
                    inst, type.GetMethod("1XXX1"), callbackInst, "SmartDelegate"
                );
                Assert.IsFalse(isOK);
                Assert.AreEqual(1, callbackInst.SmartDelegate.RegistedDelegateCount);
            }, "Invalid Instance MethodName...");
            Logger.Log(Logger.Priority.High, () => "Success to Invali Instance MethodName!");

            Assert.DoesNotThrow(() => {
                var inst = new UnbindWithTypeAndCallbackNameTest();
                var type = inst.GetType();
                var callbackInst = new UnbindWithTypeAndCallbackNameTest.CallbackClass();

                var instMethodName = "Func";
                BindCallbackAttribute.BindWithTypeAndCallbackName<UnbindWithTypeAndCallbackNameTest.CallbackClass>(
                    inst, type.GetMethod(instMethodName), callbackInst, "SmartDelegate"
                );
                Assert.AreEqual(1, callbackInst.SmartDelegate.RegistedDelegateCount);

                var isOK = BindCallbackAttribute.UnbindWithTypeAndCallbackName<UnbindWithTypeAndCallbackNameTest.CallbackClass>(
                    null, type.GetMethod(instMethodName), callbackInst, "SmartDelegate"
                );
                Assert.IsFalse(isOK);

                Assert.AreEqual(1, callbackInst.SmartDelegate.RegistedDelegateCount);
            }, "target is null...");
            Logger.Log(Logger.Priority.High, () => "Success target is null!");

            Assert.DoesNotThrow(() => {
                var inst = new UnbindWithTypeAndCallbackNameTest();
                var type = inst.GetType();
                var callbackInst = new UnbindWithTypeAndCallbackNameTest.CallbackClass();

                var instMethodName = "Func";
                BindCallbackAttribute.BindWithTypeAndCallbackName<UnbindWithTypeAndCallbackNameTest.CallbackClass>(
                    inst, type.GetMethod(instMethodName), callbackInst, "SmartDelegate"
                );
                Assert.AreEqual(1, callbackInst.SmartDelegate.RegistedDelegateCount);

                var isOK = BindCallbackAttribute.UnbindWithTypeAndCallbackName<UnbindWithTypeAndCallbackNameTest.CallbackClass>(
                    inst, type.GetMethod(instMethodName), null, "SmartDelegate"
                );
                Assert.IsFalse(isOK);

                Assert.AreEqual(1, callbackInst.SmartDelegate.RegistedDelegateCount);
            }, "callbackInstance is null...");
            Logger.Log(Logger.Priority.High, () => "Success callbackInstance is null!");

            Assert.DoesNotThrow(() => {
                var inst = new UnbindWithTypeAndCallbackNameTest();
                var type = inst.GetType();
                var callbackInst = new UnbindWithTypeAndCallbackNameTest.CallbackClass();

                var instMethodName = "Func";
                BindCallbackAttribute.BindWithTypeAndCallbackName<UnbindWithTypeAndCallbackNameTest.CallbackClass>(
                    inst, type.GetMethod(instMethodName), callbackInst, "SmartDelegate"
                );
                Assert.AreEqual(1, callbackInst.SmartDelegate.RegistedDelegateCount);

                var isOK = BindCallbackAttribute.UnbindWithTypeAndCallbackName<UnbindWithTypeAndCallbackNameTest.CallbackClass>(
                    inst, type.GetMethod(instMethodName), callbackInst, "1XXX1"
                );
                Assert.IsFalse(isOK);

                Assert.AreEqual(1, callbackInst.SmartDelegate.RegistedDelegateCount);
            }, "Invalid callbackName...");
            Logger.Log(Logger.Priority.High, () => "Success to Invalid callbackName!");
        }
        #endregion

        #region BindToGameObject
        class BindToGameObjectTest
        {
            [BindCallback(typeof(Button), "onClick")]
            public void OnClick()
            {
                OnClickCounter++;
            }
            public int OnClickCounter { get; set; }

            [BindCallback(typeof(Button), "onClick")]
            [BindCallback(typeof(Button), "onClick")]
            public void OnClick2()
            {
                OnClick2Counter++;
            }
            public int OnClick2Counter { get; set; }
        }

        [UnityTest, Order(ORDER_BIND_TO_GAMEOBJECT), Description("")]
        public IEnumerator BindToGameObject_Passes()
        {
            var inst = new BindToGameObjectTest();
            var gameObject = new GameObject("__obj");
            var btn = gameObject.AddComponent<Button>();

            BindCallbackAttribute.BindToGameObject(inst, gameObject, Labels.MatchOp.Complete);

            btn.onClick.Invoke();
            Assert.AreEqual(1, inst.OnClickCounter);
            Assert.AreEqual(1, inst.OnClick2Counter);

            yield break;
        }
        #endregion

        #region UnbindToGameObject
        class UnbindToGameObjectTest
        {
            [BindCallback(typeof(Button), "onClick")]
            public void OnClick()
            {
                OnClickCounter++;
            }
            public int OnClickCounter { get; set; }

            [BindCallback(typeof(Button), "onClick")]
            [BindCallback(typeof(Button), "onClick")]
            public void OnClick2()
            {
                OnClick2Counter++;
            }
            public int OnClick2Counter { get; set; }
        }
        [UnityTest, Order(ORDER_UNBIND_TO_GAMEOBJECT), Description("")]
        public IEnumerator UnbindToGameObject_Passes()
        {
            var inst = new UnbindToGameObjectTest();
            var gameObject = new GameObject("__obj");
            var btn = gameObject.AddComponent<Button>();

            BindCallbackAttribute.BindToGameObject(inst, gameObject, Labels.MatchOp.Complete);
            BindCallbackAttribute.UnbindToGameObject(inst, gameObject, Labels.MatchOp.Complete);

            btn.onClick.Invoke();
            Assert.AreEqual(0, inst.OnClickCounter);
            Assert.AreEqual(0, inst.OnClick2Counter);
            yield break;
        }
        #endregion
    }
}
