using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode
{
    public interface IReadOnlyLabels : IEnumerable<string>, IEnumerable
    {
        IReadOnlyCollection<string> LabelHashSet { get; }
        int Count { get; }

        bool Contains(string label);
        bool DoMatch(Labels.MatchOp op, params string[] labels);
        bool DoMatch(Labels.MatchOp op, IEnumerable<string> labels);
    }

    /// <summary>
    /// <seealso cref="Hinode.Tests.TestLabels"/>
    /// </summary>
    public class Labels : IReadOnlyLabels
    {
        public static bool IsValid(string label)
        {
            return !string.IsNullOrEmpty(label)
                && !label.Any(_c => char.IsWhiteSpace(_c));
        }

        public enum MatchOp
        {
            Complete,
            Included,
            Partial,
        }

        readonly HashSet<string> _hash = new HashSet<string>();

        public IReadOnlyCollection<string> LabelHashSet { get => _hash; }
        public int Count { get => _hash.Count; }

        public Labels()
        { }

        public Labels(params string[] labels)
        {
            AddRange(labels);
        }

        public Labels(IEnumerable<string> labels)
        {
            AddRange(labels);
        }

        #region Add
        public Labels Add(string label)
        {
            _hash.Add(label);
            return this;
        }

        public Labels AddRange(params string[] labels)
        {
            for (var i = 0; i < labels.Length; ++i)
            {
                if (!IsValid(labels[i])) continue;
                _hash.Add(labels[i]);
            }
            return this;
        }

        public Labels AddRange(IEnumerable<string> labels)
        {
            foreach (var l in labels)
            {
                if (!IsValid(l)) continue;
                _hash.Add(l);
            }
            return this;
        }
        #endregion

        #region Remove
        public Labels Remove(string label)
        {
            _hash.Remove(label);
            return this;
        }

        public Labels RemoveRange(params string[] labels)
        {
            for (var i = 0; i < labels.Length; ++i)
            {
                _hash.Remove(labels[i]);
            }
            return this;
        }

        public Labels RemoveRange(IEnumerable<string> labels)
        {
            foreach (var l in labels)
            {
                _hash.Remove(l);
            }
            return this;
        }

        public Labels Clear()
        {
            _hash.Clear();
            return this;
        }
        #endregion


        public bool Contains(string label)
            => _hash.Contains(label);

        public bool DoMatch(MatchOp op, params string[] labels)
            => DoMatch(op, labels.AsEnumerable());

        public bool DoMatch(MatchOp op, IEnumerable<string> labels)
        {
            if (LabelHashSet.Count <= 0 && !labels.Any()) return true;

            switch (op)
            {
                case MatchOp.Complete:
                    if (LabelHashSet.Count != labels.Count()) return false;
                    return LabelHashSet.All(_l => 1 == labels.Count(_ll => _ll == _l));
                case MatchOp.Included:
                    return LabelHashSet.All(_l => labels.Any(_ll => _ll == _l));
                case MatchOp.Partial:
                    return LabelHashSet.Any(_l => labels.Any(_ll => _ll == _l));
                default:
                    return false;
            }
        }

        #region IEnumerable interface
        public IEnumerator<string> GetEnumerator()
            => _hash.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _hash.GetEnumerator();
        #endregion
    }
}
