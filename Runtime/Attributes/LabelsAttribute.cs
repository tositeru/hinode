using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// Labelにアクセスする時はLabelsでは内部処理的にコピーが発生するため、LabelHashSetの方を使用してください。
    /// <see cref="Labels"/>
    /// <seealso cref="Hinode.Tests.Attribute.TestLabelsAttribute"/>
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public class LabelsAttribute : MemoAttribute
    {
        readonly Labels _labels = new Labels();
        /// <summary>
        /// 内部処理的にコピーが発生するため、Labelにアクセスする時はLabelHashSetの方を使用することを推奨します。
        /// <see cref="LabelHashSet"/>
        /// </summary>
        public string[] Labels
        {
            get => _labels.ToArray();
            set
            {
                _labels.Clear();
                for (var i = 0; i < value.Length; ++i)
                {
                    _labels.Add(value[i]);
                }
            }
        }

        /// <summary>
        /// Labelsよりもこちらのプロパティからラベルを取得するようにしてください。
        /// </summary>
        public IReadOnlyLabels LabelHashSet { get => _labels; }
        public LabelsAttribute()
        {}

        public bool Contains(string label)
            => _labels.Contains(label);

        public bool DoMatch(Labels.MatchOp op, params string[] labels)
            => _labels.DoMatch(op, labels);

        public bool DoMatch(Labels.MatchOp op, IEnumerable<string> labels)
            => _labels.DoMatch(op, labels);

        #region Method

        #endregion
    }

}
