﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Hinode
{
    public static class HashSetHelperCallback<T>
    {
        public delegate void OnAdded(T item);
        public delegate void OnRemoved(T item);
        public delegate void OnCleared();
        public delegate void OnChangedCount(IReadOnlyHashSetHelper<T> self, int count);
    }

    public interface IReadOnlyHashSetHelper<T> : IEnumerable<T>, IEnumerable
    {
        IReadOnlyCollection<T> Items { get; }
        int Count { get; }

        NotInvokableDelegate<HashSetHelperCallback<T>.OnAdded> OnAdded { get; }
        NotInvokableDelegate<HashSetHelperCallback<T>.OnRemoved> OnRemoved { get; }
        NotInvokableDelegate<HashSetHelperCallback<T>.OnCleared> OnCleared { get; }

        bool Contains(T item);
    }

    /// <summary>
	/// HashSet Wrapper
	/// </summary>
	/// <typeparam name="T"></typeparam>
    public class HashSetHelper<T> : IReadOnlyHashSetHelper<T>
    {
        HashSet<T> _field = new HashSet<T>();
        SmartDelegate<HashSetHelperCallback<T>.OnAdded> _onAdded = new SmartDelegate<HashSetHelperCallback<T>.OnAdded>();
        SmartDelegate<HashSetHelperCallback<T>.OnRemoved> _onRemoved = new SmartDelegate<HashSetHelperCallback<T>.OnRemoved>();
        SmartDelegate<HashSetHelperCallback<T>.OnCleared> _onCleared = new SmartDelegate<HashSetHelperCallback<T>.OnCleared>();
        SmartDelegate<HashSetHelperCallback<T>.OnChangedCount> _onChangedCount = new SmartDelegate<HashSetHelperCallback<T>.OnChangedCount>();

        public IReadOnlyCollection<T> Items { get => _field; }
        public int Count { get => _field.Count; }

        public NotInvokableDelegate<HashSetHelperCallback<T>.OnAdded> OnAdded { get => _onAdded; }
        public NotInvokableDelegate<HashSetHelperCallback<T>.OnRemoved> OnRemoved { get => _onRemoved; }
        public NotInvokableDelegate<HashSetHelperCallback<T>.OnCleared> OnCleared { get => _onCleared; }
        public NotInvokableDelegate<HashSetHelperCallback<T>.OnChangedCount> OnChangedCount { get => _onChangedCount; }

        public bool Contains(T item) => _field.Contains(item);

        public HashSetHelper<T> Add(T item)
        {
            if(InnerAdd(item))
            {
                _onChangedCount.SafeDynamicInvoke(this, Count, () => $"HashSetHelper#Add");
            }
            return this;
        }

        public HashSetHelper<T> Add(params T[] items)
            => Add(items.AsEnumerable());
        public HashSetHelper<T> Add(IEnumerable<T> items)
        {
            var isAdd = false;
            foreach (var item in items)
            {
                isAdd |= InnerAdd(item);
            }

            if(isAdd)
            {
                _onChangedCount.SafeDynamicInvoke(this, Count, () => $"HashSetHelper#Add");
            }
            return this;
        }

        bool InnerAdd(T item)
        {
            if (item == null || Contains(item))
                return false;

            _field.Add(item);

            _onAdded.SafeDynamicInvoke(item, () => $"HashSetHelper#Add");
            return true;
        }

        public HashSetHelper<T> Remove(T item)
        {
            if (InnerRemove(item))
            {
                _onChangedCount.SafeDynamicInvoke(this, Count, () => $"HashSetHelper#Remove");
            }
            return this;
        }

        public HashSetHelper<T> Remove(params T[] items)
            => Remove(items.AsEnumerable());
        public HashSetHelper<T> Remove(IEnumerable<T> items)
        {
            bool isRemove = false;
            foreach (var item in items)
            {
                isRemove |= InnerRemove(item);
            }

            if(isRemove)
            {
                _onChangedCount.SafeDynamicInvoke(this, Count, () => $"HashSetHelper#Remove");
            }

            return this;
        }

        bool InnerRemove(T item)
        {
            if (item == null || !Contains(item))
                return false;

            _field.Remove(item);

            _onRemoved.SafeDynamicInvoke(item, () => $"HashSetHelper#Remove");
            return true;
        }

        public HashSetHelper<T> Clear()
        {
            if (_field.Count <= 0)
                return this;

            bool isRemove = false;
            while(0 < _field.Count)
            {
                isRemove |= InnerRemove(_field.First());
            }

            if(isRemove)
            {
                _onCleared.SafeDynamicInvoke(() => $"HashSetHelper#Clear");

                _onChangedCount.SafeDynamicInvoke(this, 0, () => $"HashSetHelper#Clear");
            }
            return this;
        }

        #region IEnumerable<T>
        public IEnumerator<T> GetEnumerator()
            => _field.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
        #endregion
    }
}
