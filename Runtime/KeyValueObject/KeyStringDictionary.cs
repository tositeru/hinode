using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="IKeyValueDictionary{TKeyValue, T}"/>
    /// </summary>
    [CreateAssetMenu(menuName = "Hinode/KeyValue/String KeyValueDictionary")]
    public class KeyStringDictionary : IKeyValueDictionary<KeyStringObject, string>
    {
        protected override KeyStringObject CreateObj(string key, string value)
            => new KeyStringObject(key, value);

        public KeyStringObject this[string key]
        {
            get => Get(key) as KeyStringObject;
        }
        public new KeyStringObject Get(string key) => this[key];
    }
}
