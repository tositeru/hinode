using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="IKeyValueDictionary{TKeyValue, T}"/>
    /// </summary>
    [CreateAssetMenu(menuName = "Hinode/KeyValue/Bool KeyValueDictionary")]
    public class KeyBoolDictionary : IKeyValueDictionary<KeyBoolObject, bool>
    {
        protected override KeyBoolObject CreateObj(string key, bool value)
            => new KeyBoolObject(key, value);

        public KeyBoolObject this[string key]
        {
            get => Get(key) as KeyBoolObject;
        }
        public new KeyBoolObject Get(string key) => this[key];
    }
}
