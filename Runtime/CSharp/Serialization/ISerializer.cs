using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode.Serialization
{
    /// <summary>
    /// シリアライズを行うためのインターフェイス
    /// もし、シリアライズ対象となる型がISerializableインタフェースを継承しておらず、
    /// System.Serializable属性を指定されている時はpublicまたはUnityEngine.SerializeField属性を指定されたFieldをシリアライズの対象にします
    /// <seealso cref="System.Runtime.Serialization.ISerializable"/>。
    /// <seealso cref="System.SerializableAttribute"/>
    /// <seealso cref="UnityEngine.SerializeField"/>
    /// <seealso cref="Hinode.Tests.CSharp.Serialization.TestJsonSerializer"/>
    /// </summary>
    public abstract class ISerializer
    {
        /// <summary>
        /// <seealso cref="HasKeyAndTypeDictionaryGetterAttribute"/>
        /// </summary>
        public interface IInstanceCreator
        {
            /// <summary>
            /// infoとcontextを元に指定した型のインスタンスを生成します。
            /// </summary>
            /// <param name="type"></param>
            /// <param name="info"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            object Desirialize(System.Type type, SerializationInfo info, StreamingContext context);

            /// <summary>
            /// targetをinfoとcontextを元にシリアライズします。
            /// </summary>
            /// <param name="target"></param>
            /// <param name="info"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            bool Serialize(object target, SerializationInfo info, StreamingContext context);

            /// <summary>
            /// 引数に渡された型と対応するISerializationKeyTypeGetterを返すようにしてください。
            /// このISerializationKeyTypeGetterを使用することで、フィールド名とは異なったキーを指定することができます。
            /// 
            /// もし、ある型がこの関数が返す辞書と一致するキー名がない時は、
            /// ISerialize内でその型がContainsSerializationKeyTypeGetterAttributeを持っているか判定し、
            /// 持っている場合はそのAttributeからISerializationKeyTypeGetterを取得します。
            /// ex) 
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            ISerializationKeyTypeGetter GetKeyTypeGetter(System.Type type);
        }

        protected static readonly System.Type DEFAULT_TYPE = typeof(Dictionary<string, object>);

        IInstanceCreator _instanceCreator = null;

        protected abstract void ReadTo(TextReader stream, SerializationInfo outInfo, ISerializationKeyTypeGetter keyTypeGetter);
        protected abstract void WriteTo(TextWriter stream, SerializationInfo srcInfo);

        public ISerializer(IInstanceCreator instanceCreator = null)
        {
            _instanceCreator = instanceCreator ?? new DefaultInstanceCreator();
        }

        /// <summary>
        /// constructorDictに値を渡す時はISerializer#CreateDefaultConstructorDictionary関数の戻り値をベースにしたものを利用することを推奨します。
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        /// <param name="constructorDict">nullの場合はCreateDefaultConstructorDictionary()の戻り値が使用されます。</param>
        public void Serialize(TextWriter stream, object obj)
        {
            var (info, context) = CreateInfoAndContext(obj.GetType());
            var doneSerialized = _instanceCreator.Serialize(obj, info, context);
            if (doneSerialized)
            {
            }
            else if (obj is ISerializable)
            {
                var serializable = obj as ISerializable;
                serializable.GetObjectData(info, context);
            }
            else if (obj.GetType().GetCustomAttribute<System.SerializableAttribute>() != null
                || obj.GetType().IsStruct())
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

            //if (info.MemberCount <= 0) return;
            WriteTo(stream, info);
        }

        public object Deserialize(TextReader stream, System.Type type)
        {
            var keyTypeGetter = GetKeyTypeGetter(type);
            var (info, context) = CreateInfoAndContext(type);
            ReadTo(stream, info, keyTypeGetter);

            object inst = _instanceCreator?.Desirialize(type, info, context);
            if (inst != null)
            {
                return inst;
            }
            else if (type.Equals(DEFAULT_TYPE))
            {
                var dict = new Dictionary<string, object>();
                foreach (var v in info.GetEnumerable())
                {
                    dict.Add(v.Name, v.Value);
                }
            }
            else if (type.ContainsInterface<ISerializable>())
            {
                var ctor = type.GetConstructor(new[] { typeof(SerializationInfo), typeof(StreamingContext) });
                Assert.IsNotNull(ctor, $"Not Define Constractor(SerializationInfo, StreamingContext)... type={type}");
                inst = ctor.Invoke(new object[] { info, context });
            }
            else if (type.GetCustomAttribute<System.SerializableAttribute>() != null
                || type.IsStruct())
            {
                //空引数のコンストラクを持つものだけ対応している
                var ctor = type.GetConstructor(new System.Type[] { });
                //var ctor = type.GetConstructor(null);
                Assert.IsNotNull(ctor, $"Not Define Default Constractor()... type={type}");
                Assert.AreEqual(0, ctor.GetParameters().Length, $"Not Define Default Constractor()... type={type} argsCount={ctor.GetParameters().Length}");

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

        /// <summary>
        /// 型に結び付けられたキー名とその型の辞書を返します。
        /// この辞書を使用することで、フィールド名とは異なったキーを指定することができます。
        ///
        /// 使用する辞書の優先順位としては以下のものになります。
        /// 1. IInstanceCreator#GetKeyTypeGetter
        /// 1. typeに付けられたContainsSerializationKeyTypeGetterAttribute
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        ISerializationKeyTypeGetter GetKeyTypeGetter(System.Type type)
        {
            var getter = _instanceCreator.GetKeyTypeGetter(type);
            if (getter != null) return getter;

            var attr = type.GetCustomAttribute<ContainsSerializationKeyTypeGetterAttribute>();
            return attr?.CreateKeyTypeGetter(type) ?? null;
        }
    }

    public static partial class ISerializerExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Serialize(this ISerializer serializer, object obj)
        {
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);
                return writer.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Deserialize(this ISerializer serializer, string json, System.Type type)
        {
            using (var reader = new StringReader(json))
            {
                return serializer.Deserialize(reader, type);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T Deserialize<T>(this ISerializer serializer, string json)
        {
            return (T)serializer.Deserialize(json, typeof(T));
        }

    }
}

