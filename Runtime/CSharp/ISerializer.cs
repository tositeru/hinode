using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// シリアライズを行うためのインターフェイス
    /// もし、シリアライズ対象となる型がISerializableインタフェースを継承しておらず、
    /// System.Serializable属性を指定されている時はpublicまたはUnityEngine.SerializeField属性を指定されたFieldをシリアライズの対象にします
    /// <seealso cref="System.Runtime.Serialization.ISerializable"/>。
    /// <seealso cref="System.SerializableAttribute"/>
    /// <seealso cref="UnityEngine.SerializeField"/>。
    /// </summary>
    public abstract class ISerializer
    {
        protected readonly System.Type DEFAULT_TYPE = typeof(Dictionary<string, object>);

        protected abstract void ReadTo(TextReader stream, SerializationInfo outInfo);
        protected abstract void WriteTo(TextWriter stream, SerializationInfo srcInfo);

        public void Serialize(TextWriter stream, object obj)
        {
            var (info, context) = CreateInfoAndContext(obj.GetType());
            if (obj is ISerializable)
            {
                var serializable = obj as ISerializable;
                serializable.GetObjectData(info, context);
            }
            else if (HasSerializableAttribute(obj))
            {
                //System.SerializableAttributeが指定されていたら、publicまたはSerializeFieldなFieldを集計する
                foreach (var fieldInfo in obj.GetSerializedFieldEnumerable())
                {
                    info.AddValue(fieldInfo.FieldInfo.Name, fieldInfo.Value);
                }
            }
            else
            {
                Debug.LogWarning($"This type({obj.GetType().FullName}) don't serialize...");
            }

            if (info.MemberCount <= 0) return;
            WriteTo(stream, info);
        }

        public object Deserialize(TextReader stream, System.Type type)
        {
            var (info, context) = CreateInfoAndContext(type);
            ReadTo(stream, info);

            object inst = null;
            if (type.Equals(DEFAULT_TYPE))
            {
                var dict = new Dictionary<string, object>();
                foreach (var v in info.GetEnumerable())
                {
                    dict.Add(v.Name, v.Value);
                }
            }
            else if (type.GetInterface(typeof(ISerializable).FullName) != null)
            {
                var ctor = type.GetConstructor(new[] { typeof(SerializationInfo), typeof(StreamingContext) });
                Assert.IsNotNull(ctor, $"Not Define Constractor(SerializationInfo, StreamingContext)...");
                inst = ctor.Invoke(new object[] { info, context });
            }
            else if (type.GetCustomAttribute<System.SerializableAttribute>() != null)
            {
                var ctor = type.GetConstructor(new System.Type[] { });
                Assert.IsNotNull(ctor, $"Not Define Default Constractor()...");

                inst = ctor.Invoke(new object[] { });
                foreach (var fieldInfo in inst.GetSerializedFieldEnumerable()
                    .Where(_i => info.GetEnumerable().Any(_e => _e.Name == _i.FieldInfo.Name)))
                {
                    //Debug.Log($"debug -- Desirialize -- {fieldInfo.FieldInfo.Name} type={fieldInfo.FieldInfo.FieldType}");
                    var value = info.GetValue(fieldInfo.FieldInfo.Name, fieldInfo.FieldInfo.FieldType);
                    fieldInfo.FieldInfo.SetValue(inst, value);
                }
            }
            else
            {
                Debug.LogWarning($"This type({type.FullName}) don't deserialize...");
            }
            return inst;
        }

        (SerializationInfo, StreamingContext) CreateInfoAndContext(System.Type type)
        {
            var formatter = new FormatterConverter();
            SerializationInfo info = new SerializationInfo(type, formatter);
            var context = new StreamingContext(StreamingContextStates.File, this);
            return (info, context);
        }

        bool HasSerializableAttribute(object obj)
        {
            return obj.GetType().GetCustomAttribute<System.SerializableAttribute>() != null;
        }

    }
}
