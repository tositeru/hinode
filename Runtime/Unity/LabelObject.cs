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

		[SerializeField] string[] _initialLabels;
		HashSet<string> _labels = new HashSet<string>();

		public IReadOnlyCollection<string> InitialLabels { get => _initialLabels; }
		public IReadOnlyCollection<string> Labels { get => _labels; }

		/// <summary>
		/// <seealso cref="Hinode.Tests.TestLabelObject.BasicUsagePasses()"/>
		/// <seealso cref="Hinode.Tests.TestLabelObject.AddAndRemoveRangePasses()"/>
		/// </summary>
		public int Count { get => Labels.Count; }

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
		public bool Contains(IEnumerable<string> labels) => labels.All(_l => Labels.Contains(_l) || InitialLabels.Contains(_l));

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

		public bool DoMatch<T>(out T getComponent, params string[] labels)
			where T : Component
        {
			var isOK = DoMatch(out var com, typeof(T), labels);
			getComponent = com as T;
			return isOK;
		}

		public bool DoMatch(out Component getComponent, System.Type useComponentType, params string[] labels)
		{
			getComponent = null;
			if (useComponentType.IsSubclassOf(typeof(Component)))
            {
				var doContainsCom = gameObject.TryGetComponent(useComponentType, out getComponent);
				if (labels.Length == 0)
					return doContainsCom;
				else
					return Contains(labels) && doContainsCom;
			}
			else
            {
				return Contains(labels);
			}
		}

		/// <summary>
		/// <seealso cref="Hinode.Tests.TestLabelObject.AddAndRemoveRangePasses()"/>
		/// </summary>
		/// <returns></returns>
		public IEnumerator<string> GetEnumerator() => Labels.Concat(InitialLabels).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public static LabelObject GetLabelObject(object obj)
        {
			LabelObject label = null;
			if (obj is Component)
				label = (obj as Component).GetComponent<LabelObject>();
			else if (obj is GameObject)
				label = (obj as GameObject).GetComponent<LabelObject>();
			else if (obj is Transform)
				label = (obj as Transform).GetComponent<LabelObject>();
			return label;
		}

		//public static void MatchAndAction(object obj, IEnumerable<LabelFilter> filters)
  //      {
		//	var label = GetLabelObject(obj);
		//	if (label != null)
		//	{
		//		label.MatchAndAction(filters);
		//	}
		//}
		public class LabelFilter
		{
			public System.Type ComponentType { get; }
			public string[] Labels { get; }

			public LabelFilter(System.Type componentType, params string[] labels)
			{
				ComponentType = componentType;
				Labels = labels;
			}

			public static LabelFilter Create(System.Type comType, params string[] labels)
			{
				return new LabelFilter(comType, labels);
			}

			public static LabelFilter Create<T>(params string[] labels)
				where T : Component
            {
				return new LabelFilter(typeof(T), labels);
            }

			public bool DoMatch<T>(LabelObject label, out T component)
				where T : Component
			{
				return label.DoMatch(out component, Labels);
			}
			public bool DoMatch(LabelObject label, out Component component)
			{
				return label.DoMatch(out component, ComponentType, Labels);
			}
		}
	}
}
