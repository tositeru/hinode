using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.Assertions;
using UnityEditor.UIElements;

namespace Hinode.Editors
{
	public class SerializedPropertyDictionary<T>
	{
		Dictionary<T, SerializedProperty> _dict = new Dictionary<T, SerializedProperty>();

		public SerializedPropertyDictionary(SerializedObject SO, params (T, string)[] propNames)
			: this(SO, propNames.AsEnumerable())
		{ }

		public SerializedPropertyDictionary(SerializedObject SO, IEnumerable<(T, string)> propNames)
		{
			foreach (var (key, name) in propNames)
			{
				var prop = SO.FindProperty(name);
				Assert.IsNotNull(prop, $"Don't found '{name}' property for '{key}'...");
				_dict.Add(key, prop);
			}
		}

        public SerializedPropertyDictionary(SerializedProperty prop, params (T, string)[] propNames)
            : this(prop, propNames.AsEnumerable())
        { }

        public SerializedPropertyDictionary(SerializedProperty parentProp, IEnumerable<(T, string)> propNames)
        {
            foreach (var (key, name) in propNames)
            {
                var prop = parentProp.FindPropertyRelative(name);
                Assert.IsNotNull(prop, $"Don't found '{name}' property for '{key}'...");
                _dict.Add(key, prop);
            }
        }


        public SerializedProperty this[T key]
		{
			get => _dict[key];
		}

        public PropertyField GetPropField(T key, string name, string bindingPath)
        {
            return new PropertyField(this[key])
            {
                name = name,
                bindingPath = bindingPath
            };
        }
    }
}
