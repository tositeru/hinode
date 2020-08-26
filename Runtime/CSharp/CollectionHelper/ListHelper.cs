using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Hinode
{
    public static class ListHelperCallback<T>
    {
        public delegate void OnAdded(T item, int index);
        public delegate void OnMoved(T item, int index);
        public delegate void OnRemoved(T item, int index);
        public delegate void OnCleared();
        public delegate void OnChangedCount(IReadOnlyListHelper<T> self, int count);
    }

    public interface IReadOnlyListHelper<T> : IEnumerable<T>, IEnumerable
    {
        IReadOnlyList<T> Items { get; }
        int Count { get; }

        NotInvokableDelegate<ListHelperCallback<T>.OnAdded> OnAdded { get; }
        NotInvokableDelegate<ListHelperCallback<T>.OnMoved> OnMoved { get; }
        NotInvokableDelegate<ListHelperCallback<T>.OnRemoved> OnRemoved { get; }
        NotInvokableDelegate<ListHelperCallback<T>.OnCleared> OnCleared { get; }
        NotInvokableDelegate<ListHelperCallback<T>.OnChangedCount> OnChangedCount { get; }

        T this[int index] { get; }

        bool Contains(T item);
        bool IsValidIndex(int index);

        int IndexOf(T value, int start = 0, int count = -1);
        int LastIndexOf(T value, int start = 0, int count = -1);
        int FindIndex(System.Predicate<T> predicate, int start = 0, int count = -1);
        int FindLastIndex(System.Predicate<T> predicate, int start = 0, int count = -1);
    }

    /// <summary>
	/// List Wrapper
	/// </summary>
	/// <typeparam name="T"></typeparam>
    public class ListHelper<T> : IReadOnlyListHelper<T>
    {
        List<T> _field = new List<T>();
        SmartDelegate<ListHelperCallback<T>.OnAdded> _onAdded = new SmartDelegate<ListHelperCallback<T>.OnAdded>();
        SmartDelegate<ListHelperCallback<T>.OnMoved> _onMoved = new SmartDelegate<ListHelperCallback<T>.OnMoved>();
        SmartDelegate<ListHelperCallback<T>.OnRemoved> _onRemoved = new SmartDelegate<ListHelperCallback<T>.OnRemoved>();
        SmartDelegate<ListHelperCallback<T>.OnCleared> _onCleared = new SmartDelegate<ListHelperCallback<T>.OnCleared>();
        SmartDelegate<ListHelperCallback<T>.OnChangedCount> _onChangedCount = new SmartDelegate<ListHelperCallback<T>.OnChangedCount>();

        public IReadOnlyList<T> Items { get => _field; }
        public int Count { get => _field.Count; }

        public NotInvokableDelegate<ListHelperCallback<T>.OnAdded> OnAdded { get => _onAdded; }
        public NotInvokableDelegate<ListHelperCallback<T>.OnMoved> OnMoved { get => _onMoved; }
        public NotInvokableDelegate<ListHelperCallback<T>.OnRemoved> OnRemoved { get => _onRemoved; }
        public NotInvokableDelegate<ListHelperCallback<T>.OnCleared> OnCleared { get => _onCleared; }
        public NotInvokableDelegate<ListHelperCallback<T>.OnChangedCount> OnChangedCount { get => _onChangedCount; }

        public ListHelper()
        { }

        public ListHelper(params T[] values)
            : this(values.AsEnumerable())
        { }

        public ListHelper(IEnumerable<T> values)
        {
            this.Add(values);
        }

        public T this[int index]
        {
            get => _field[index];
            set
            {
                _onRemoved.SafeDynamicInvoke(_field[index], index, () => $"ListHelper#Indexers[{index}] Remove PrevElement");
                _field[index] = value;

                _onAdded.SafeDynamicInvoke(value, index, () => "ListHelper#Indexers[{index}] Add NewElement");
            }
        }

        public bool Contains(T item) => _field.Contains(item);
        public bool IsValidIndex(int index) => 0 <= index && index < Count;

        public ListHelper<T> Resize(int size)
        {
            if(InnerResize(size))
            {
                _onChangedCount.SafeDynamicInvoke(this, Count, () => $"ListHelper#Resize");
            }
            return this;
        }

        bool InnerResize(int size)
        {
            size = Mathf.Max(0, size);

            bool doChangedCount = false;
            while (Count < size)
            {
                _field.Add(default(T));
                doChangedCount |= true;
            }
            while (size < Count)
            {
                doChangedCount |= InnerRemoveAt(Count - 1);
            }
            return doChangedCount;
        }

        public ListHelper<T> Add(T item)
        {
            if(InnerAdd(item))
            {
                _onChangedCount.SafeDynamicInvoke(this, Count, () => $"ListHelper#Add");
            }
            return this;
        }

        public ListHelper<T> Add(params T[] items)
            => Add(items.AsEnumerable());
        public ListHelper<T> Add(IEnumerable<T> items)
        {
            bool isAdd = false;
            foreach (var item in items)
            {
                isAdd |= InnerAdd(item);
            }
            if (isAdd)
            {
                _onChangedCount.SafeDynamicInvoke(this, Count, () => $"ListHelper#Add");
            }
            return this;
        }

        bool InnerAdd(T item)
        {
            _field.Add(item);
            _onAdded.SafeDynamicInvoke(item, Count - 1, () => $"ListHelper#Add");
            return true;
        }

        public ListHelper<T> InsertTo(int index, params T[] values)
            => InsertTo(index, values.AsEnumerable());
        public ListHelper<T> InsertTo(int index, IEnumerable<T> values)
        {
            if(index < 0 || Count < index) throw new System.ArgumentOutOfRangeException($"Invalid insert Index({index})...");

            if(Count == index)
            {
                return this.Add(values);
            }
            else
            {
                var addCount = values.Count();
                var prevCount = Count;
                var sumCount = Count + addCount;
                bool doChangedCount = InnerResize(sumCount);

                var moveCount = prevCount - index;
                for(var i=0; i<moveCount; ++i)
                {
                    var fromIndex = index + moveCount - i - 1;
                    var toIndex = sumCount - i - 1;
                    MoveTo(fromIndex, toIndex);
                }

                foreach(var (v, i) in values
                    .Zip(Enumerable.Range(0, addCount), (_v, _i) => (_v, _i + index)))
                {
                    _field[i] = v;
                    _onAdded.SafeDynamicInvoke(v, i, () => $"ListHelper#InsertTo insert Index={i}");
                }

                if(doChangedCount)
                {
                    _onChangedCount.SafeDynamicInvoke(this, Count, () => $"ListHelper#InsertTo");
                }
            }
            return this;
        }

        public ListHelper<T> Remove(T item)
        {
            var index = _field.IndexOf(item);
            return RemoveAt(index);
        }

        public ListHelper<T> RemoveAt(int index)
        {
            if(InnerRemoveAt(index))
            {
                _onChangedCount.SafeDynamicInvoke(this, Count, () => $"ListHelper#Remove");
            }
            return this;
        }

        public ListHelper<T> Remove(params T[] items)
            => Remove(items.AsEnumerable());
        public ListHelper<T> Remove(IEnumerable<T> items)
            => RemoveAt(items
                .Select(_v => IndexOf(_v))
                .Where(_i => -1 != _i));

        public ListHelper<T> RemoveAt(params int[] indecies)
            => RemoveAt(indecies.AsEnumerable());
        public ListHelper<T> RemoveAt(IEnumerable<int> indecies)
        {
            bool isRemove = false;
            foreach (var index in indecies.OrderByDescending(_i => _i))
            {
                isRemove |= InnerRemoveAt(index);
            }

            if(isRemove)
            {
                _onChangedCount.SafeDynamicInvoke(this, Count, () => $"ListHelper#RemoveAt");
            }
            return this;
        }

        bool InnerRemoveAt(int index)
        {
            if (!IsValidIndex(index))
                return false;

            var target =_field[index];
            _field.RemoveAt(index);
            _onRemoved.SafeDynamicInvoke(target, index, () => $"ListHelper#RemoveAt");
            return true;
        }

        public ListHelper<T> Clear()
        {
            if (Count <= 0)
                return this;

            while (0 < Count)
            {
                InnerRemoveAt(Count-1);
            }

            _onCleared.SafeDynamicInvoke(() => $"ListHelper#Clear");

            _onChangedCount.SafeDynamicInvoke(this, Count, () => $"ListHelper#Remove");
            return this;
        }

        public ListHelper<T> MoveTo(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex) return this;

            if (!IsValidIndex(fromIndex)) throw new System.ArgumentOutOfRangeException($"Invalid fromIndex({fromIndex})...");
            if (!IsValidIndex(toIndex)) throw new System.ArgumentOutOfRangeException($"Invalid toIndex({toIndex})...");

            _onRemoved.SafeDynamicInvoke(_field[toIndex], toIndex, () => $"ListHelper#MoveTo(from={fromIndex}, to={toIndex})");

            _field[toIndex] = _field[fromIndex];
            _field[fromIndex] = default(T);

            _onMoved.SafeDynamicInvoke(_field[toIndex], toIndex, () => $"ListHelper#MoveTo(from={fromIndex}, to={toIndex})");

            return this;
        }

        public ListHelper<T> Swap(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex) return this;

            if (!IsValidIndex(fromIndex)) throw new System.ArgumentOutOfRangeException($"Invalid fromIndex({fromIndex})...");
            if (!IsValidIndex(toIndex)) throw new System.ArgumentOutOfRangeException($"Invalid toIndex({toIndex})...");

            var swap = _field[toIndex];
            _field[toIndex] = _field[fromIndex];
            _field[fromIndex] = swap;

            _onMoved.SafeDynamicInvoke(_field[fromIndex], fromIndex, () => $"ListHelper#MoveTo in from(from={fromIndex}, to={toIndex})");
            _onMoved.SafeDynamicInvoke(_field[toIndex], toIndex, () => $"ListHelper#MoveTo in to(from={fromIndex}, to={toIndex})");

            return this;
        }

        public ListHelper<T> Sort(IComparer<T> comparer = null)
        {
            if (comparer == null) comparer = Comparer<T>.Default;

            _field.Sort(comparer);

            for(var i=0; i<Count; ++i)
            {
                _onMoved.SafeDynamicInvoke(_field[i], i, () => $"ListHelper#Sort(index={i})");
            }
            return this;
        }

        public int IndexOf(T value, int start=0, int count=-1)
        {
            if(count < 0) return _field.IndexOf(value, start);
            else return _field.IndexOf(value, start, count);
        }

        public int LastIndexOf(T value, int start=0, int count=-1)
        {
            start = System.Math.Max(0, Count - start - 1);
            if (count < 0) return _field.LastIndexOf(value, start);
            else return _field.LastIndexOf(value, start, count);
        }

        public int FindIndex(System.Predicate<T> predicate, int start=0, int count=-1)
        {
            if (count < 0) return _field.FindIndex(start, predicate);
            else return _field.FindIndex(start, count, predicate);
        }

        public int FindLastIndex(System.Predicate<T> predicate, int start = 0, int count = -1)
        {
            start = System.Math.Max(0, Count - start - 1);
            if (count < 0) return _field.FindLastIndex(start, predicate);
            else return _field.FindLastIndex(start, count, predicate);
        }

        #region IEnumerable<T>
        public IEnumerator<T> GetEnumerator()
            => _field.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
        #endregion
    }
}
