using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// Unityの仕様上シーンオブジェクトは設定できません。
    /// シーンオブジェクトを参照できるようにしたい時はList<KeyObjectRefObject>かKeyObjectRefObjectの配列を使用してください。
    /// <seealso cref="IKeyValueDictionary{TKeyValue, T}"/>
    /// <seealso cref="IKeyValueDictionaryWithTypeName{TKeyValue, T}"/>
    /// </summary>
    [CreateAssetMenu(menuName = "Hinode/KeyValue/ObjectRef KeyValueDictionary")]
    public class KeyObjectRefDictionary : IKeyValueDictionaryWithTypeName<KeyObjectRefObject, Object>
    {
        public static KeyObjectRefDictionary Create<TObject>()
            where TObject : Object
        {
            var inst = CreateInstance<KeyObjectRefDictionary>();
            Assert.IsTrue(inst.IsValidType(typeof(TObject)));
            inst.CurrentType = typeof(TObject);
            return inst;
        }

        public KeyObjectRefObject this[string key]
        {
            get => Get(key) as KeyObjectRefObject;
        }
        public new KeyObjectRefObject Get(string key) => this[key];

        #region IKeyValueDictionary
        protected override KeyObjectRefObject CreateObj(string key, Object value)
            => new KeyObjectRefObject(key, value, CurrentType);

        public override void Refresh()
        {
            CurrentType = FindType();
            base.Refresh();
        }
        #endregion

        #region IKeyValueDictionaryWithTypeName
        protected override bool IsValidType(System.Type type) => type.IsSubclassOf(typeof(Object));
        #endregion
    }
}
