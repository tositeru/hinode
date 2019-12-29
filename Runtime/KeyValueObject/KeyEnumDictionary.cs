using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// 定義されていたEnum値が削除されたなどの時に対応するため内部データではint型(Enumの名前への添字)を持つように指定います。
    /// <seealso cref="IKeyValueDictionary{TKeyValue, T}"/>
    /// <seealso cref="IKeyValueDictionaryWithTypeName{TKeyValue, T}"/>
    /// </summary>
    [CreateAssetMenu(menuName = "Hinode/KeyValue/Enum KeyValueDictionary")]
    public class KeyEnumDictionary : IKeyValueDictionaryWithTypeName<KeyEnumObject, int>
    {
        public static KeyEnumDictionary Create<TEnum>()
            where TEnum : System.Enum
        {
            var inst = CreateInstance<KeyEnumDictionary>();
            Assert.IsTrue(inst.IsValidType(typeof(TEnum)));
            inst.CurrentType = typeof(TEnum);
            return inst;
        }

        public KeyEnumObject this[string key]
        {
            get => Get(key) as KeyEnumObject;
        }
        public new KeyEnumObject Get(string key) => this[key];

        #region IKeyValueDictionary
        protected override KeyEnumObject CreateObj(string key, int value)
            => new KeyEnumObject(key, (System.Enum)(object)value, CurrentType);

        public override void Refresh()
        {
            CurrentType = FindType();
            base.Refresh();
        }
        #endregion

        #region IKeyValueDictionaryWithTypeName
        protected override bool IsValidType(System.Type type) => type?.IsSubclassOf(typeof(System.Enum)) ?? false;
        #endregion
    }
}
