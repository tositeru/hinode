using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.Assertions;

namespace Hinode
{
	/// <summary>
	/// <seealso cref="Hinode.Tests.TestLabelObject"/>
	/// </summary>
	public class LabelObject : MonoBehaviour, IEnumerable<string>, IEnumerable
    {
		static Regex _validRegex = new Regex(@"^[\w_]+$");

		public static bool IsValidLabel(string label)
			=> _validRegex.IsMatch(label);

		HashSet<string> _labels = new HashSet<string>();

		/// <summary>
		/// <seealso cref="Hinode.Tests.TestLabelObject.BasicUsagePasses()"/>
		/// <seealso cref="Hinode.Tests.TestLabelObject.AddAndRemoveRangePasses()"/>
		/// </summary>
		public int Count { get => _labels.Count; }

		/// <summary>
		/// <seealso cref="Hinode.Tests.TestLabelObject.BasicUsagePasses()"/>
		/// <seealso cref="Hinode.Tests.TestLabelObject.AddAndRemoveRangePasses()"/>
		/// </summary>
		/// <param name="labels"></param>
		/// <returns></returns>
		public bool Contains(params string[] labels) => Contains(labels.AsEnumerable());

		/// <summary>
		/// <seealso cref="Hinode.Tests.TestLabelObject.BasicUsagePasses()"/>
		/// <seealso cref="Hinode.Tests.TestLabelObject.AddAndRemoveRangePasses()"/>
		/// </summary>
		/// <param name="labels"></param>
		/// <returns></returns>
		public bool Contains(IEnumerable<string> labels) => labels.All(_l => _labels.Contains(_l));

		/// <summary>
		/// <seealso cref="Hinode.Tests.TestLabelObject.BasicUsagePasses()"/>
		/// <seealso cref="Hinode.Tests.TestLabelObject.ValidLabelPasses()"/>
		/// </summary>
		/// <param name="label"></param>
		/// <returns></returns>
		public LabelObject Add(string label)
		{
			Assert.IsTrue(IsValidLabel(label), $"'{label}' is not valid... use only numeric, alphabet and _.");
			_labels.Add(label);
			return this;
		}

		/// <summary>
		/// <seealso cref="Hinode.Tests.TestLabelObject.AddAndRemoveRangePasses()"/>
		/// </summary>
		/// <param name="labels"></param>
		/// <returns></returns>
		public LabelObject AddRange(params string[] labels) => AddRange(labels.AsEnumerable());

		/// <summary>
		/// <seealso cref="Hinode.Tests.TestLabelObject.AddAndRemoveRangePasses()"/>
		/// </summary>
		/// <param name="labels"></param>
		/// <returns></returns>
		public LabelObject AddRange(IEnumerable<string> labels)
		{
			foreach(var l in labels)
            {
				Add(l);
            }
			return this;
		}

		/// <summary>
		/// <seealso cref="Hinode.Tests.TestLabelObject.BasicUsagePasses()"/>
		/// </summary>
		/// <param name="label"></param>
		/// <returns></returns>
		public LabelObject Remove(string label)
		{
			_labels.Remove(label);
			return this;
		}

		/// <summary>
		/// <seealso cref="Hinode.Tests.TestLabelObject.AddAndRemoveRangePasses()"/>
		/// </summary>
		/// <param name="labels"></param>
		/// <returns></returns>
		public LabelObject RemoveRange(params string[] labels)
			=> RemoveRange(labels.AsEnumerable());
		public LabelObject RemoveRange(IEnumerable<string> labels)
        {
			foreach(var l in labels)
            {
				Remove(l);
            }
			return this;
        }

		/// <summary>
		/// <seealso cref="Hinode.Tests.TestLabelObject.AddAndRemoveRangePasses()"/>
		/// </summary>
		/// <returns></returns>
		public LabelObject Clear()
		{
			_labels.Clear();
			return this;
		}

		/// <summary>
		/// <seealso cref="Hinode.Tests.TestLabelObject.AddAndRemoveRangePasses()"/>
		/// </summary>
		/// <returns></returns>
		public IEnumerator<string> GetEnumerator() => _labels.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
