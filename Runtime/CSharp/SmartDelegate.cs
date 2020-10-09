using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// メソッドの追加・削除のみできるDelegate
    /// 登録されたメソッドを実行したい時はInvokableDelegateを使用してください。
    /// <seealso cref="InvokableDelegate{T}"/>
    /// </summary>
    /// <typeparam name="T">System.Delegateからの派生クラス</typeparam>
    public class NotInvokableDelegate<T>
        where T : System.Delegate
    {
        internal protected T _predicate;

        public bool IsValid { get => _predicate != null; }
        public int RegistedDelegateCount
        {
            get => _predicate?.GetInvocationList().Length ?? 0;
        }

        //public T Get { get => _predicate; set => _predicate = value; }

        public NotInvokableDelegate() { }

        public NotInvokableDelegate(params T[] predicates)
        {
            Add(predicates);
        }
        public NotInvokableDelegate(IEnumerable<T> predicates)
        {
            Add(predicates);
        }
        public NotInvokableDelegate(params NotInvokableDelegate<T>[] others)
            : this(others.Select(_o => _o._predicate))
        { }
        public NotInvokableDelegate(IEnumerable<NotInvokableDelegate<T>> others)
            : this(others.Select(_o => _o._predicate))
        { }

        public void Add(params T[] predicates)
            => Add(predicates.AsEnumerable());
        public void Add(IEnumerable<T> predicates)
        {
            System.Delegate newList = _predicate;
            foreach (var p in predicates)
            {
                newList = System.Delegate.Combine(newList, p);
            }
            _predicate = newList as T;
        }
        public void Add(params NotInvokableDelegate<T>[] predicates)
            => Add(predicates.Select(_p => _p._predicate));
        public void Add(IEnumerable<NotInvokableDelegate<T>> predicates)
            => Add(predicates.Select(_p => _p._predicate));

        public void Remove(params T[] predicates)
            => Remove(predicates.AsEnumerable());
        public void Remove(IEnumerable<T> predicates)
        {
            System.Delegate newList = _predicate;
            foreach (var p in predicates)
            {
                newList = System.Delegate.Remove(newList, p);
            }
            _predicate = newList as T;
        }
        public void Remove(params NotInvokableDelegate<T>[] predicates)
            => Add(predicates.Select(_p => _p._predicate));
        public void Remove(IEnumerable<NotInvokableDelegate<T>> predicates)
            => Add(predicates.Select(_p => _p._predicate));

        public void Clear()
        {
            _predicate = _predicate.ClearInvocations();
        }

        public void Set(T predicate)
            => _predicate = predicate;
    }

    /// <summary>
    /// delegateをラップしたクラス。
    /// 登録されたメソッドを実行させたくない時はNotInvokableDelegateにキャストしてください。
    /// <seealso cref="NotInvokableDelegate{T}"/>
    /// </summary>
    /// <typeparam name="T">System.Delegateからの派生クラス</typeparam>
    public class SmartDelegate<T> : NotInvokableDelegate<T>
        where T : System.Delegate
    {
        List<object> _returnValuesCache = new List<object>();

        public T Instance { get => _predicate; }

        public SmartDelegate() { }
        public SmartDelegate(params T[] predicates) : base(predicates) { }
        public SmartDelegate(IEnumerable<T> predicates) : base(predicates) { }
        public SmartDelegate(params NotInvokableDelegate<T>[] others)
            : this(others.Select(_o => _o._predicate))
        { }
        public SmartDelegate(IEnumerable<NotInvokableDelegate<T>> others)
            : this(others.Select(_o => _o._predicate))
        { }

        //Text Template note:
        //public IEnumerable<object> SafeDynamicInvoke$TemplateArgs$($Args$System.Func<string> getLogFunc, params string[] logSelector)
        //{
        //  var list = Instance?.GetInvocationList() ?? null;
        //  if (list == null) return null;
        //  _returnValuesCache.Clear();
        //  foreach (var invocation in list)
        //  {
        //      try
        //      {
        //          var returnValue = invocation?.DynamicInvoke($PassArgs$);
        //          _returnValuesCache.Add(returnValue);
        //      }
        //      catch (System.Exception e)
        //      {
        //          Logger.LogWarning(Logger.Priority.High
        //              , () => $"Exception!! {getLogFunc()}.{System.Environment.NewLine}{e}"
        //              , logSelector);
        //      }
        //  }
        //  return _returnValuesCache;
        //}

        ////@@ Packages/com.tositeru.hinode/Editor/Assets/TextTemplates/SmartDelegate_SafeDynamicInvoke.asset
public IEnumerable<object> SafeDynamicInvoke(System.Func<string> getLogFunc, params string[] logSelector)
{
    var list = Instance?.GetInvocationList() ?? null;
    if (list == null) return null;
    _returnValuesCache.Clear();
    foreach (var invocation in list)
    {
        try
        {
            var returnValue = invocation.DynamicInvoke();
            _returnValuesCache.Add(returnValue);
        }
        catch (System.Exception e)
        {
            Logger.LogWarning(Logger.Priority.High
                , () => $"Exception!! {getLogFunc()}.{System.Environment.NewLine}{e}"
            , logSelector);
        }
    }
    return _returnValuesCache;
}

public IEnumerable<object> SafeDynamicInvoke<T1>(T1 arg1, System.Func<string> getLogFunc, params string[] logSelector)
{
    var list = Instance?.GetInvocationList() ?? null;
    if (list == null) return null;
    _returnValuesCache.Clear();
    foreach (var invocation in list)
    {
        try
        {
            var returnValue = invocation.DynamicInvoke(arg1);
            _returnValuesCache.Add(returnValue);
        }
        catch (System.Exception e)
        {
            Logger.LogWarning(Logger.Priority.High
                , () => $"Exception!! {getLogFunc()}.{System.Environment.NewLine}{e}"
            , logSelector);
        }
    }
    return _returnValuesCache;
}

public IEnumerable<object> SafeDynamicInvoke<T1, T2>(T1 arg1, T2 arg2, System.Func<string> getLogFunc, params string[] logSelector)
{
    var list = Instance?.GetInvocationList() ?? null;
    if (list == null) return null;
    _returnValuesCache.Clear();
    foreach (var invocation in list)
    {
        try
        {
            var returnValue = invocation.DynamicInvoke(arg1, arg2);
            _returnValuesCache.Add(returnValue);
        }
        catch (System.Exception e)
        {
            Logger.LogWarning(Logger.Priority.High
                , () => $"Exception!! {getLogFunc()}.{System.Environment.NewLine}{e}"
            , logSelector);
        }
    }
    return _returnValuesCache;
}

public IEnumerable<object> SafeDynamicInvoke<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3, System.Func<string> getLogFunc, params string[] logSelector)
{
    var list = Instance?.GetInvocationList() ?? null;
    if (list == null) return null;
    _returnValuesCache.Clear();
    foreach (var invocation in list)
    {
        try
        {
            var returnValue = invocation.DynamicInvoke(arg1, arg2, arg3);
            _returnValuesCache.Add(returnValue);
        }
        catch (System.Exception e)
        {
            Logger.LogWarning(Logger.Priority.High
                , () => $"Exception!! {getLogFunc()}.{System.Environment.NewLine}{e}"
            , logSelector);
        }
    }
    return _returnValuesCache;
}

public IEnumerable<object> SafeDynamicInvoke<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, System.Func<string> getLogFunc, params string[] logSelector)
{
    var list = Instance?.GetInvocationList() ?? null;
    if (list == null) return null;
    _returnValuesCache.Clear();
    foreach (var invocation in list)
    {
        try
        {
            var returnValue = invocation.DynamicInvoke(arg1, arg2, arg3, arg4);
            _returnValuesCache.Add(returnValue);
        }
        catch (System.Exception e)
        {
            Logger.LogWarning(Logger.Priority.High
                , () => $"Exception!! {getLogFunc()}.{System.Environment.NewLine}{e}"
            , logSelector);
        }
    }
    return _returnValuesCache;
}

public IEnumerable<object> SafeDynamicInvoke<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, System.Func<string> getLogFunc, params string[] logSelector)
{
    var list = Instance?.GetInvocationList() ?? null;
    if (list == null) return null;
    _returnValuesCache.Clear();
    foreach (var invocation in list)
    {
        try
        {
            var returnValue = invocation.DynamicInvoke(arg1, arg2, arg3, arg4, arg5);
            _returnValuesCache.Add(returnValue);
        }
        catch (System.Exception e)
        {
            Logger.LogWarning(Logger.Priority.High
                , () => $"Exception!! {getLogFunc()}.{System.Environment.NewLine}{e}"
            , logSelector);
        }
    }
    return _returnValuesCache;
}

////-- Finish Packages/com.tositeru.hinode/Editor/Assets/TextTemplates/SmartDelegate_SafeDynamicInvoke.asset
    }
}
