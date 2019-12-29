using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="IKeyValueDictionary{TKeyValue, T}"/>
    /// </summary>
    [CreateAssetMenu(menuName = "Hinode/KeyValue/Double KeyValueDictionary")]
    public class KeyDoubleDictionary : IKeyValueDictionary<KeyDoubleObject, double>
    {
        protected override KeyDoubleObject CreateObj(string key, double value)
            => new KeyDoubleObject(key, value);

        public KeyDoubleObject this[string key]
        {
            get => Get(key) as KeyDoubleObject;
        }
        public new KeyDoubleObject Get(string key) => this[key];
    }
}
