using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="IKeyValueDictionary{TKeyValue, T}"/>
    /// </summary>
    [CreateAssetMenu(menuName = "Hinode/KeyValue/Float KeyValueDictionary")]
    public class KeyFloatDictionary : IKeyValueDictionary<KeyFloatObject, float>
    {
        protected override KeyFloatObject CreateObj(string key, float value)
            => new KeyFloatObject(key, value);

        public KeyFloatObject this[string key]
        {
            get => Get(key) as KeyFloatObject;
        }
        public new KeyFloatObject Get(string key) => this[key];
    }
}
