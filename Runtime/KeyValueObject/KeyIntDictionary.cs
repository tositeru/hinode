using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="IKeyValueDictionary{TKeyValue, T}"/>
    /// </summary>
    [CreateAssetMenu(menuName = "Hinode/KeyValue/Int KeyValueDictionary")]
    public class KeyIntDictionary : IKeyValueDictionary<KeyIntObject, int>
    {
        protected override KeyIntObject CreateObj(string key, int value)
            => new KeyIntObject(key, value);

        public KeyIntObject this[string key]
        {
            get => Get(key) as KeyIntObject;
        }
        public new KeyIntObject Get(string key) => this[key];
    }
}
