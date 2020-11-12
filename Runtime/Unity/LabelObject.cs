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
		[SerializeField] string[] _constLabels = new string[] { };
		//HashSet<string> _labels = new HashSet<string>();

		public IReadOnlyCollection<string> ConstLabels { get => _constLabels; }
		public Labels Labels { get; } = new Labels();
		public IEnumerable<string> AllLabels { get => Labels.Concat(ConstLabels); }

		/// <summary>
		/// <seealso cref="Hinode.Tests.TestLabelObject.BasicUsagePasses()"/>
		/// <seealso cref="Hinode.Tests.TestLabelObject.AddAndRemoveRangePasses()"/>
		/// </summary>
		public int Count { get => Labels.Count + _constLabels.Length; }

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
		public bool Contains(IEnumerable<string> labels)
			=> labels.All(_l => Labels.Contains(_l) || ConstLabels.Contains(_l));

		/// <summary>
        /// 指定したラベルとComponentを持っているか確認します
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="getComponent"></param>
        /// <param name="labels"></param>
        /// <returns></returns>
		public bool DoMatch<T>(out T getComponent, params string[] labels)
			where T : Component
        {
			var isOK = DoMatch(out var com, typeof(T), labels);
			getComponent = com as T;
			return isOK;
		}

		/// <summary>
		/// 指定したラベルとComponentを持っているか確認します
		/// </summary>
		/// <param name="getComponent"></param>
		/// <param name="useComponentType"></param>
		/// <param name="labels"></param>
		/// <returns></returns>
		public bool DoMatch(out Component getComponent, System.Type useComponentType, params string[] labels)
		{
			getComponent = null;
			if (useComponentType?.IsSubclassOf(typeof(Component)) ?? false)
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
		public IEnumerator<string> GetEnumerator() => AllLabels.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public static LabelObject GetLabelObject(object obj)
        {
			return GameObjectExtensions.GetComponent<LabelObject>(obj);
		}

		/// <summary>
        /// 
        /// </summary>
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

		/// <summary>
        /// 
        /// </summary>
        [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
        public sealed class LabelListClassAttribute : System.Attribute
        {
            public LabelListClassAttribute()
            { }
        }
	}
}
