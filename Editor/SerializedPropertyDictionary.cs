using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.Assertions;

namespace Hinode.Editors
{
	public class SerializedPropertyDictionary<T>
	{
		readonly SerializedObject _SO;
		public SerializedObject SO { get => _SO; }
		Dictionary<T, SerializedProperty> _dict = new Dictionary<T, SerializedProperty>();

		public SerializedPropertyDictionary(SerializedObject SO, params (T, string)[] propNames)
			: this(SO, propNames.AsEnumerable())
		{ }

		public SerializedPropertyDictionary(SerializedObject SO, IEnumerable<(T, string)> propNames)
		{
			_SO = SO;
			foreach (var (key, name) in propNames)
			{
				var prop = SO.FindProperty(name);
				Assert.IsNotNull(prop, $"Don't found '{name}' property for '{key}'...");
				_dict.Add(key, prop);
			}
		}

		public SerializedProperty this[T key]
		{
			get => _dict[key];
		}
    }
}
