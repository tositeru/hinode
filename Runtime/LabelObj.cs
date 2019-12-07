using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Hinode
{
    /// <summary>
	/// 
	/// </summary>
    public class LabelObj : MonoBehaviour, IEnumerable<string>, IEnumerable
    {
		List<string> _labels = new List<string>();

		public bool Has(string label) => _labels.Contains(label);
		public bool Contains(params string[] labels) => Contains(labels);
		public bool Contains(IEnumerable<string> labels) => labels.All(_l => _labels.Contains(_l));

		public void Add(string label) => _labels.Add(label);
		public void AddRange(params string[] labels) => AddRange(labels);
		public void AddRange(IEnumerable<string> labels) => _labels.AddRange(labels);
        public void Remove(string label) => _labels.Remove(label);
        public void Clear() => _labels.Clear();

        public IEnumerator<string> GetEnumerator() => _labels.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
