using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Hinode
{
    public static class DictionaryHelperCallback<TKey, TValue>
    {
        public delegate void OnAdded(TKey key, TValue item);
        public delegate void OnRemoved(TKey key, TValue item);
        public delegate void OnSwaped(TKey key, TValue oldValue, TValue newValue);
        public delegate void OnCleared();
        public delegate void OnChangedCount(IReadOnlyDictionaryHelper<TKey, TValue> self, int count);
    }

    public interface IReadOnlyDictionaryHelper<TKey, TValue> : IEnumerable<(TKey, TValue)>, IEnumerable
    {
        IReadOnlyDictionary<TKey, TValue> Items { get; }
        IEnumerable<TKey> Keys { get; }
        IEnumerable<TValue> Values { get; }
        int Count { get; }

        NotInvokableDelegate<DictionaryHelperCallback<TKey, TValue>.OnAdded> OnAdded { get; }
        NotInvokableDelegate<DictionaryHelperCallback<TKey, TValue>.OnRemoved> OnRemoved { get; }
        NotInvokableDelegate<DictionaryHelperCallback<TKey, TValue>.OnSwaped> OnSwaped { get; }
        NotInvokableDelegate<DictionaryHelperCallback<TKey, TValue>.OnCleared> OnCleared { get; }
        NotInvokableDelegate<DictionaryHelperCallback<TKey, TValue>.OnChangedCount> OnChangedCount { get; }

        bool ContainsKey(TKey key);
        bool ContainsValue(TValue value);
    }

    /// <summary>
	/// Dictionary Wrapper
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
    public class DictionaryHelper<TKey, TValue> : IReadOnlyDictionaryHelper<TKey, TValue>
    {
        Dictionary<TKey, TValue> _field = new Dictionary<TKey, TValue>();

        SmartDelegate<DictionaryHelperCallback<TKey, TValue>.OnAdded> _onAdded = new SmartDelegate<DictionaryHelperCallback<TKey, TValue>.OnAdded>();
        SmartDelegate<DictionaryHelperCallback<TKey, TValue>.OnRemoved> _onRemoved = new SmartDelegate<DictionaryHelperCallback<TKey, TValue>.OnRemoved>();
        SmartDelegate<DictionaryHelperCallback<TKey, TValue>.OnSwaped> _onSwaped = new SmartDelegate<DictionaryHelperCallback<TKey, TValue>.OnSwaped>();
        SmartDelegate<DictionaryHelperCallback<TKey, TValue>.OnCleared> _onCleared = new SmartDelegate<DictionaryHelperCallback<TKey, TValue>.OnCleared>();
        SmartDelegate<DictionaryHelperCallback<TKey, TValue>.OnChangedCount> _onChangedCount = new SmartDelegate<DictionaryHelperCallback<TKey, TValue>.OnChangedCount>();

        public IReadOnlyDictionary<TKey, TValue> Items { get => _field; }
        public IEnumerable<(TKey key, TValue value)> TupleItems { get => _field.Select(_t => (_t.Key, _t.Value)); }
        public IEnumerable<TKey> Keys { get => _field.Keys; }
        public IEnumerable<TValue> Values { get => _field.Values; }
        public int Count { get => _field.Count; }

        public NotInvokableDelegate<DictionaryHelperCallback<TKey, TValue>.OnAdded> OnAdded { get => _onAdded; }
        public NotInvokableDelegate<DictionaryHelperCallback<TKey, TValue>.OnRemoved> OnRemoved { get => _onRemoved; }
        public NotInvokableDelegate<DictionaryHelperCallback<TKey, TValue>.OnSwaped> OnSwaped { get => _onSwaped; }
        public NotInvokableDelegate<DictionaryHelperCallback<TKey, TValue>.OnCleared> OnCleared { get => _onCleared; }
        public NotInvokableDelegate<DictionaryHelperCallback<TKey, TValue>.OnChangedCount> OnChangedCount { get => _onChangedCount; }

        public bool ContainsKey(TKey key)
            => _field.ContainsKey(key);
        public bool ContainsValue(TValue value)
            => _field.ContainsValue(value);

        public DictionaryHelper()
        { }

        public DictionaryHelper(params KeyValuePair<TKey, TValue>[] keyValuePairs)
            : this(keyValuePairs.AsEnumerable())
        { }
        public DictionaryHelper(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
        {
            Add(keyValuePairs);
        }

        public TValue this[TKey key]
        {
            get => _field[key];
            set
            {
                bool doChangedCount = InnerAdd(key, value);
                if (doChangedCount)
                {
                    try
                    {
                        _onChangedCount.Instance?.Invoke(this, Count);
                    }
                    catch (System.Exception e)
                    {
                        Logger.LogWarning(Logger.Priority.High, () => $"Exception!! DictionaryHelper#Indexers[{key}]: {e.Message}");
                    }
                }
            }
        }

        public DictionaryHelper<TKey, TValue> Add(TKey key, TValue value)
        {
            if (InnerAdd(key, value))
            {
                try
                {
                    _onChangedCount.Instance?.Invoke(this, Count);
                }
                catch (System.Exception e)
                {
                    Logger.LogWarning(Logger.Priority.High, () => $"Exception!! DictionaryHelper#Add: {e.Message}");
                }
            }
            return this;
        }

        public DictionaryHelper<TKey, TValue> Add(params (TKey key, TValue value)[] keyValuePairs)
            => Add(keyValuePairs.AsEnumerable());
        public DictionaryHelper<TKey, TValue> Add(IEnumerable<(TKey key, TValue value)> keyValuePairs)
        {
            bool doChangedCount = false;
            foreach(var (key, value) in keyValuePairs)
            {
                doChangedCount |= InnerAdd(key, value);
            }

            if (doChangedCount)
            {
                try
                {
                    _onChangedCount.Instance?.Invoke(this, Count);
                }
                catch (System.Exception e)
                {
                    Logger.LogWarning(Logger.Priority.High, () => $"Exception!! DictionaryHelper#Add: {e.Message}");
                }
            }
            return this;
        }

        public DictionaryHelper<TKey, TValue> Add(params KeyValuePair<TKey, TValue>[] keyValuePairs)
            => Add(keyValuePairs.AsEnumerable());
        public DictionaryHelper<TKey, TValue> Add(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
            => Add(keyValuePairs.Select(_t => (_t.Key, _t.Value)));

        bool InnerAdd(TKey key, TValue value)
        {
            if(ContainsKey(key))
            {
                try
                {
                    _onSwaped.Instance?.Invoke(key, _field[key], value);
                }
                catch(System.Exception e)
                {
                    Logger.LogWarning(Logger.Priority.High, () => $"Exception!! DictionaryHelper#Add: {e.Message}");
                }
                _field[key] = value;
                return false;
            }
            else
            {
                _field.Add(key, value);
                try
                {
                    _onAdded.Instance?.Invoke(key, value);
                }
                catch (System.Exception e)
                {
                    Logger.LogWarning(Logger.Priority.High, () => $"Exception!! DictionaryHelper#Add: {e.Message}");
                }
                return true;
            }
        }

        public DictionaryHelper<TKey, TValue> Remove(TKey key)
        {
            if(InnerRemove(key))
            {
                try
                {
                    _onChangedCount.Instance?.Invoke(this, Count);
                }
                catch (System.Exception e)
                {
                    Logger.LogWarning(Logger.Priority.High, () => $"Exception!! DictionaryHelper#Remove: {e.Message}");
                }
            }
            return this;
        }

        public DictionaryHelper<TKey, TValue> Remove(params TKey[] keys)
            => Remove(keys.AsEnumerable());
        public DictionaryHelper<TKey, TValue> Remove(IEnumerable<TKey> keys)
        {
            bool doChangedCount = false;
            foreach(var key in keys)
            {
                doChangedCount |= InnerRemove(key);
            }

            if (doChangedCount)
            {
                try
                {
                    _onChangedCount.Instance?.Invoke(this, Count);
                }
                catch (System.Exception e)
                {
                    Logger.LogWarning(Logger.Priority.High, () => $"Exception!! DictionaryHelper#Remove: {e.Message}");
                }
            }
            return this;
        }

        bool InnerRemove(TKey key)
        {
            if(!ContainsKey(key)) return false;

            try
            {
                _onRemoved.Instance?.Invoke(key, _field[key]);
            }
            catch (System.Exception e)
            {
                Logger.LogWarning(Logger.Priority.High, () => $"Exception!! DictionaryHelper#Remove: {e.Message}");
            }

            _field.Remove(key);
            return true;
        }

        public DictionaryHelper<TKey, TValue> Clear()
        {
            if (Count <= 0) return this;

            foreach(var t in TupleItems)
            {
                try
                {
                    _onRemoved.Instance?.Invoke(t.key, t.value);
                }
                catch (System.Exception e)
                {
                    Logger.LogWarning(Logger.Priority.High, () => $"Exception!! DictionaryHelper#Clear: {e.Message}");
                }
            }

            _field.Clear();

            try
            {
                _onCleared.Instance?.Invoke();
            }
            catch (System.Exception e)
            {
                Logger.LogWarning(Logger.Priority.High, () => $"Exception!! DictionaryHelper#Remove: {e.Message}");
            }

            try
            {
                _onChangedCount.Instance?.Invoke(this, Count);
            }
            catch (System.Exception e)
            {
                Logger.LogWarning(Logger.Priority.High, () => $"Exception!! DictionaryHelper#Remove: {e.Message}");
            }
            return this;
        }

        #region IEnumerable<(TKey, TValue)>
        public IEnumerator<(TKey, TValue)> GetEnumerator()
            => TupleItems.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
        #endregion
    }
}
