using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Hinode
{
    /// <summary>
    /// 指定したメソッドとDelegate/UnityEventをバインドするためのAttribute
    ///
    /// 以下のものに対応しています。
    /// - UnityEventBase
    /// - System.Delegate
    /// - Event
    /// - Hinode.SmartDelegate
    ///
    /// UnityEventBaseは戻り値を持つことができないことに注意してください。
    /// 指定した関数とコールバックの引数と返り値が異なる場合は何もせず、警告ログを出力します。
    /// その際は例外を発生しないように実装しています。
    ///
    /// Labelsに着きましてはHinode#LabelObjectと兼用をすることを想定しています。
    ///
    /// ## IBinderPredicate
    /// このインターフェイスを利用することで、コールバックの引数に任意のものを指定することができます。
    /// 実装の際はコールバックの追加/削除の処理を実装してください。
    ///
    /// このインターフェイス内で例外が発生した場合はこのクラスでは何もしませんので注意してください。
    /// <seealso cref="LabelObject"/>
    /// <seealso cref="Hinode.Tests.Attributes.TestBindCallbackAttribute"/>
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class BindCallbackAttribute : LabelsAttribute
        , System.IEquatable<BindCallbackAttribute>
    {
        public enum Kind
        {
            TypeAndCallback,
            Binder,
        }

        public interface IBinderPredicate
        {
            bool EnableBind(MethodInfo methodInfo, object obj);
            bool AddCallbacks(object target, MethodInfo methodInfo, object obj);
            bool RemoveCallbacks(object target, MethodInfo methodInfo, object obj);
        }

        public Kind CurrentKind { get => Binder != null ? Kind.Binder : Kind.TypeAndCallback; }

        public System.Type CallbackBaseType { get; }
        public string CallbackName { get; }
        public IBinderPredicate Binder { get; }

        public bool IsValid
        {
            get
            {
                switch (CurrentKind)
                {
                    case Kind.Binder:
                        return Binder != null;
                    case Kind.TypeAndCallback:
                        if (CallbackBaseType == null) return false;
                        return null != CallbackBaseType.GetProperty(CallbackName)
                            || null != CallbackBaseType.GetEvent(CallbackName);
                    default:
                        throw new System.NotImplementedException();
                }
            }
        }

        public BindCallbackAttribute(System.Type type, string callbackName)
        {
            Assert.IsNotNull(type, $"Type is not null...");
            Assert.IsFalse(string.IsNullOrWhiteSpace(callbackName), $"CallbackName is not null, empty or whitespace...");
            CallbackBaseType = type;
            CallbackName = callbackName;
        }

        public BindCallbackAttribute(System.Type binderType)
        {
            Assert.IsTrue(binderType.IsSameOrInheritedType<IBinderPredicate>(), $"BinderType({binderType}) isn't IBinderPredicate...");

            var cstorInfo = binderType.GetConstructor(new System.Type[] { });
            Binder = cstorInfo.Invoke(new object[] { }) as IBinderPredicate;
        }

        #region override Equals
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is BindCallbackAttribute)
                return Equals(obj as BindCallbackAttribute);

            return base.Equals(obj);
        }

        public bool Equals(BindCallbackAttribute obj)
        {
            if (CurrentKind != obj.CurrentKind) return false;
            if (!base.DoMatch(Hinode.Labels.MatchOp.Complete, obj.LabelHashSet))
                return false;

            switch(CurrentKind)
            {
                case Kind.TypeAndCallback:
                    return CallbackBaseType.Equals(obj.CallbackBaseType)
                        && CallbackName == obj.CallbackName;
                case Kind.Binder:
                    return Binder.GetType().Equals(obj.Binder.GetType());
                default:
                    throw new System.NotImplementedException();
            }
        }
        #endregion

        public bool EnableBind(MethodInfo targetMethodInfo, object obj)
        {
            switch (CurrentKind)
            {
                case Kind.TypeAndCallback:
                    if (!obj.GetType().IsSameOrInheritedType(CallbackBaseType))
                        return false;

                    return BindCallbackAttribute.EnableBind(targetMethodInfo, CallbackBaseType, CallbackName);
                case Kind.Binder:
                    return Binder.EnableBind(targetMethodInfo, obj);
                default:
                    throw new System.NotImplementedException();
            }
        }

        public bool Bind(object target, MethodInfo targetMethodInfo, object obj)
        {
            switch (CurrentKind)
            {
                case Kind.TypeAndCallback:
                    return BindWithTypeAndCallbackName(target, targetMethodInfo, obj, CallbackBaseType, CallbackName);
                case Kind.Binder:
                    return Binder.AddCallbacks(target, targetMethodInfo, obj);
                default:
                    throw new System.NotImplementedException();
            }
        }

        public bool Unbind(object target, MethodInfo targetMethodInfo, object obj)
        {
            switch (CurrentKind)
            {
                case Kind.TypeAndCallback:
                    return UnbindWithTypeAndCallbackName(target, targetMethodInfo, obj, CallbackBaseType, CallbackName);
                case Kind.Binder:
                    return Binder.RemoveCallbacks(target, targetMethodInfo, obj);
                default:
                    throw new System.NotImplementedException();
            }
        }

        #region Bind
        public static bool EnableBind<T>(MethodInfo targetMethodInfo, string callbackName)
            => EnableBind(targetMethodInfo, typeof(T), callbackName);

        public static bool EnableBind(MethodInfo targetMethodInfo, System.Type type, string callbackName)
        {
            if (targetMethodInfo == null || type == null || string.IsNullOrEmpty(callbackName))
            {
                return false;
            }

            var propInfo = type.GetProperty(callbackName);
            MethodInfo invokeMethodInfo = null;
            if (propInfo == null)
            {
                var eventInfo = type.GetEvent(callbackName);
                if (eventInfo == null) return false;
                invokeMethodInfo = eventInfo.EventHandlerType.GetMethod("Invoke");
            }
            else
            {
                var callbackType = propInfo.PropertyType;
                if (callbackType.IsSameOrInheritedType<UnityEventBase>())
                {
                    invokeMethodInfo = callbackType.GetMethod("Invoke");
                }
                else if (callbackType.EqualGenericTypeDefinition(typeof(NotInvokableDelegate<>)))
                {
                    var delegateType = callbackType.GetGenericArguments().First();
                    invokeMethodInfo = delegateType.GetMethod("Invoke");
                }
                else if (callbackType.IsSameOrInheritedType<System.Delegate>())
                {
                    if (!propInfo.CanWrite) return false;
                    invokeMethodInfo = callbackType.GetMethod("Invoke");
                }
            }

            if (invokeMethodInfo == null) return false;

            return invokeMethodInfo.DoMatchReturnTypeAndArguments(targetMethodInfo.ReturnType, targetMethodInfo.GetParameters().Select(_p => _p.ParameterType));
        }

        /// <summary>
        /// only public Property or Public Event in obj bind!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="targetMethodInfo"></param>
        /// <param name="obj"></param>
        /// <param name="callbackName"></param>
        public static bool BindWithTypeAndCallbackName<T>(object target, MethodInfo targetMethodInfo, object obj, string callbackName)
            => BindWithTypeAndCallbackName(target, targetMethodInfo, obj, typeof(T), callbackName);

        /// <summary>
        /// only public Property or Public Event in obj bind!
        /// </summary>
        /// <param name="target"></param>
        /// <param name="targetMethodInfo"></param>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        /// <param name="callbackName"></param>
        public static bool BindWithTypeAndCallbackName(object target, MethodInfo targetMethodInfo, object obj, System.Type type, string callbackName)
        {
            if (target == null || targetMethodInfo == null || obj == null || type == null || string.IsNullOrEmpty(callbackName))
            {
                Logger.LogWarning(Logger.Priority.High, () => "Invalid Arguments...");
                return false;
            }

            var info = type.GetProperty(callbackName);
            if (info == null)
            {
                return BindToEvent(target, targetMethodInfo, obj, type, callbackName);
            }

            var callback = info.GetValue(obj);
            if (callback == null)
            {
                Logger.LogWarning(Logger.Priority.High, () => $"Fail to Get Callback Instance... type={type}, callbackName={callbackName}");
                return false;
            }

            try
            {
                var callbackType = callback.GetType();
                if (callbackType.IsSameOrInheritedType<UnityEventBase>())
                {
                    var AddListenerInfo = callbackType.GetMethod("AddListener");
                    var actionType = AddListenerInfo.GetParameters().First().ParameterType;
                    var pred = targetMethodInfo.CreateDelegate(actionType, target);
                    AddListenerInfo.Invoke(callback, new object[] { pred });
                }
                else if (callbackType.EqualGenericTypeDefinition(typeof(NotInvokableDelegate<>)))
                {
                    var delegateType = callbackType.GetGenericArguments().First();
                    var delegateArrayType = delegateType.MakeArrayType();
                    var AddMethodInfo = callbackType.GetMethod("Add"
                        , new System.Type[] { delegateArrayType });
                    var pred = targetMethodInfo.CreateDelegate(delegateType, target);

                    //Make Array[] for NotInvokableDelegate#Add(params T[] predicates)
                    var predArray = delegateArrayType.GetConstructors().First().Invoke(new object[] { 1 });
                    delegateArrayType.GetMethod("Set").Invoke(predArray, new object[] { 0, pred });

                    AddMethodInfo.Invoke(callback, new object[] { predArray });
                }
                else if(callbackType.IsSameOrInheritedType<System.Delegate>())
                {
                    if(info.CanWrite)
                    {
                        var pred = targetMethodInfo.CreateDelegate(callbackType, target);
                        var newCallbacks = System.Delegate.Combine((System.Delegate)callback, pred);
                        info.SetValue(obj, newCallbacks);
                    }
                    else
                    {
                        Logger.LogWarning(Logger.Priority.High, () => $"Don't set {type.Name}{info.Name} prop...");
                        return false;
                    }
                }
                else
                {
                    Logger.LogWarning(Logger.Priority.High, () => $"Don't Support CallbackType... {callbackName}:{callbackType}");
                    return false;
                }
            }
            catch(System.Exception e)
            {
                var br = System.Environment.NewLine;
                Logger.LogWarning(Logger.Priority.High, () =>
                    $"Fail to Bind... {br}"
                    + $"-- target={target.GetType()}{br}"
                    + $"-- methodInfo={targetMethodInfo}{br}"
                    + $"-- obj={obj.GetType()}{br}"
                    + $"-- CallbackType={type}{br}"
                    + $"-- callbackName={callbackName.GetType()}{br}"
                    + $"{br}-- {e}");
                return false;
            }
            return true;
        }

        static bool BindToEvent(object target, MethodInfo targetMethodInfo, object obj, System.Type type, string callbackName)
        {
            var eventInfo = type.GetEvent(callbackName);
            if (eventInfo == null)
            {
                Logger.LogWarning(Logger.Priority.High, () => $"Fail to Get Callback Instance... type={type}, callbackName={callbackName}");
                return false;
            }

            try
            {
                var pred = targetMethodInfo.CreateDelegate(eventInfo.EventHandlerType, target);
                eventInfo.AddEventHandler(obj, pred);
            }
            catch(System.Exception e)
            {
                var br = System.Environment.NewLine;
                Logger.LogWarning(Logger.Priority.High, () =>
                    $"Fail to Bind... {br}"
                    + $"-- target={target.GetType()}{br}"
                    + $"-- methodInfo={targetMethodInfo}{br}"
                    + $"-- obj={obj.GetType()}{br}"
                    + $"-- CallbackType={type}{br}"
                    + $"-- callbackName={callbackName.GetType()}{br}"
                    + $"{br}-- {e}");
                return false;
            }
            return true;
        }
        #endregion

        #region Unbind
        public static bool UnbindWithTypeAndCallbackName<T>(object target, MethodInfo targetMethodInfo, object obj, string callbackName)
            => UnbindWithTypeAndCallbackName(target, targetMethodInfo, obj, typeof(T), callbackName);

        public static bool UnbindWithTypeAndCallbackName(object target, MethodInfo targetMethodInfo, object obj, System.Type type, string callbackName)
        {
            if (target == null || targetMethodInfo == null || obj == null || type == null || string.IsNullOrEmpty(callbackName))
            {
                Logger.LogWarning(Logger.Priority.High, () => "Invalid Arguments...");
                return false;
            }

            var info = type.GetProperty(callbackName);
            if (info == null)
            {
                return UnbindToEvent(target, targetMethodInfo, obj, type, callbackName);
            }

            var callback = info.GetValue(obj);
            if (callback == null)
            {
                Logger.LogWarning(Logger.Priority.High, () => $"Fail to Get Callback Instance... type={type}, callbackName={callbackName}");
                return false;
            }

            try
            {
                var callbackType = callback.GetType();
                if (callbackType.IsSameOrInheritedType<UnityEventBase>())
                {
                    var RemoveListenerInfo = callbackType.GetMethod("RemoveListener");
                    var actionType = RemoveListenerInfo.GetParameters().First().ParameterType;
                    var pred = targetMethodInfo.CreateDelegate(actionType, target);
                    RemoveListenerInfo.Invoke(callback, new object[] { pred });
                }
                else if (callbackType.EqualGenericTypeDefinition(typeof(NotInvokableDelegate<>)))
                {
                    var delegateType = callbackType.GetGenericArguments().First();
                    var delegateArrayType = delegateType.MakeArrayType();
                    var RemoveMethodInfo = callbackType.GetMethod("Remove"
                        , new System.Type[] { delegateArrayType });
                    var pred = targetMethodInfo.CreateDelegate(delegateType, target);

                    //Make Array[] for NotInvokableDelegate#Add(params T[] predicates)
                    var predArray = delegateArrayType.GetConstructors().First().Invoke(new object[] { 1 });
                    delegateArrayType.GetMethod("Set").Invoke(predArray, new object[] { 0, pred });

                    RemoveMethodInfo.Invoke(callback, new object[] { predArray });
                }
                else if (callbackType.IsSameOrInheritedType<System.Delegate>())
                {
                    if (info.CanWrite)
                    {
                        var pred = targetMethodInfo.CreateDelegate(callbackType, target);
                        var newCallbacks = System.Delegate.Remove((System.Delegate)callback, pred);
                        info.SetValue(obj, newCallbacks);
                    }
                    else
                    {
                        Logger.LogWarning(Logger.Priority.High, () => $"Don't set {type.Name}{info.Name} prop...");
                        return false;
                    }
                }
                else
                {
                    Logger.LogWarning(Logger.Priority.High, () => $"Don't Support CallbackType... {callbackName}:{callbackType}");
                    return false;
                }
            }
            catch (System.Exception e)
            {
                var br = System.Environment.NewLine;
                Logger.LogWarning(Logger.Priority.High, () =>
                    $"Fail to Unbind... {br}"
                    + $"-- target={target.GetType()}{br}"
                    + $"-- methodInfo={targetMethodInfo}{br}"
                    + $"-- obj={obj.GetType()}{br}"
                    + $"-- CallbackType={type}{br}"
                    + $"-- callbackName={callbackName.GetType()}{br}"
                    + $"{br}-- {e}");
                return false;
            }
            return true;
        }

        static bool UnbindToEvent(object target, MethodInfo targetMethodInfo, object obj, System.Type type, string callbackName)
        {
            var eventInfo = type.GetEvent(callbackName);
            if (eventInfo == null)
            {
                Logger.LogWarning(Logger.Priority.High, () => $"Fail to Get Callback Instance... type={type}, callbackName={callbackName}");
                return false;
            }

            try
            {
                var pred = targetMethodInfo.CreateDelegate(eventInfo.EventHandlerType, target);
                eventInfo.RemoveEventHandler(obj, pred);
            }
            catch(System.Exception e)
            {
                var br = System.Environment.NewLine;
                Logger.LogWarning(Logger.Priority.High, () =>
                    $"Fail to Bind... {br}"
                    + $"-- target={target.GetType()}{br}"
                    + $"-- methodInfo={targetMethodInfo}{br}"
                    + $"-- obj={obj.GetType()}{br}"
                    + $"-- CallbackType={type}{br}"
                    + $"-- callbackName={callbackName.GetType()}{br}"
                    + $"{br}-- {e}");
                return false;
            }
            return true;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<(MethodInfo methodInfo, IEnumerable<BindCallbackAttribute> attrs)> GetMethodInfoAndAttrEnumerable(System.Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Select(_m => (methodInfo: _m, attrs: _m.GetCustomAttributes<BindCallbackAttribute>()
                    .Distinct()))
                .Where(_t => _t.attrs != null && _t.attrs.Any());
        }
        public static IEnumerable<(MethodInfo methodInfo, IEnumerable<BindCallbackAttribute> attrs)> GetMethodInfoAndAttrEnumerable<T>()
            => GetMethodInfoAndAttrEnumerable(typeof(T));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="op"></param>
        /// <param name="labels"></param>
        /// <returns></returns>
        public static IEnumerable<(MethodInfo methodInfo, IEnumerable<BindCallbackAttribute> attrs)> GetMethodInfoAndAttrEnumerable(System.Type type, Labels.MatchOp op, IEnumerable<string> labels)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Select(_m => (methodInfo: _m, attrs: _m.GetCustomAttributes<BindCallbackAttribute>()
                    .Where(_a => _a.DoMatch(op, labels))
                    .Distinct()))
                .Where(_t => _t.attrs != null && _t.attrs.Any());
        }
        public static IEnumerable<(MethodInfo methodInfo, IEnumerable<BindCallbackAttribute> attrs)> GetMethodInfoAndAttrEnumerable<T>(Labels.MatchOp op, params string[] labels)
            => GetMethodInfoAndAttrEnumerable(typeof(T), op, labels.AsEnumerable());
        public static IEnumerable<(MethodInfo methodInfo, IEnumerable<BindCallbackAttribute> attrs)> GetMethodInfoAndAttrEnumerable<T>(Labels.MatchOp op, IEnumerable<string> labels)
            => GetMethodInfoAndAttrEnumerable(typeof(T), op, labels);
        public static IEnumerable<(MethodInfo methodInfo, IEnumerable<BindCallbackAttribute> attrs)> GetMethodInfoAndAttrEnumerable(System.Type type, Labels.MatchOp op, params string[] labels)
            => GetMethodInfoAndAttrEnumerable(type, op, labels.AsEnumerable());

        #region Unity

        public bool EnableBind(MethodInfo methodInfo, GameObject gameObject)
        {
            switch (CurrentKind)
            {
                case Kind.Binder:
                    return Binder.EnableBind(methodInfo, gameObject);
                case Kind.TypeAndCallback:
                    var com = gameObject.GetComponent(CallbackBaseType);
                    if (com == null) return false;
                    return EnableBind(methodInfo, com);
                default:
                    throw new System.NotImplementedException();
            }
        }

        public bool Bind(object target, MethodInfo methodInfo, GameObject gameObject)
        {
            switch (CurrentKind)
            {
                case Kind.Binder:
                    return Bind(target, methodInfo, (object)gameObject);
                case Kind.TypeAndCallback:
                    var com = gameObject.GetComponent(CallbackBaseType);
                    if (com == null) return false;
                    return Bind(target, methodInfo, com);
                default:
                    throw new System.NotImplementedException();
            }
        }

        public bool Unbind(object target, MethodInfo methodInfo, GameObject gameObject)
        {
            switch (CurrentKind)
            {
                case Kind.Binder:
                    return Unbind(target, methodInfo, (object)gameObject);
                case Kind.TypeAndCallback:
                    var com = gameObject.GetComponent(CallbackBaseType);
                    if (com == null) return false;
                    return Unbind(target, methodInfo, com);
                default:
                    throw new System.NotImplementedException();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="obj"></param>
        public static void BindToGameObject(object target, GameObject obj, Labels.MatchOp op, IEnumerable<string> labels)
        {
            var bindInfos = GetMethodInfoAndAttrEnumerable(target.GetType(), op, labels);
            foreach (var (methodInfo, attrs) in bindInfos)
            {
                foreach(var attr in attrs)
                {
                    attr.Bind(target, methodInfo, obj);
                }
            }
        }
        public static void BindToGameObject(object target, GameObject obj, Labels.MatchOp op, params string[] labels)
            => BindToGameObject(target, obj, op, labels.AsEnumerable());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="obj"></param>
        /// <param name="op"></param>
        /// <param name="labels"></param>
        public static void UnbindToGameObject(object target, GameObject obj, Labels.MatchOp op, IEnumerable<string> labels)
        {
            var bindInfos = GetMethodInfoAndAttrEnumerable(target.GetType(), op, labels);
            foreach (var (methodInfo, attrs) in bindInfos)
            {
                foreach (var attr in attrs)
                {
                    var com = obj.GetComponent(attr.CallbackBaseType);
                    if (com == null) continue;
                    attr.Unbind(target, methodInfo, com);
                }
            }
        }
        public static void UnbindToGameObject(object target, GameObject obj, Labels.MatchOp op, params string[] labels)
            => UnbindToGameObject(target, obj, op, labels.AsEnumerable());
        #endregion
    }
}
