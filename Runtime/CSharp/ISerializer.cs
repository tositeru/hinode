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
    /// <seealso cref="UnityEngine.SerializeField"/>
    /// <seealso cref="Hinode.Tests.CSharp.TestJsonSerializer"/>
    /// </summary>
    public abstract class ISerializer
    {
        public interface IInstanceCreator
        {
            object Create(System.Type type, SerializationInfo info, StreamingContext context);
            bool Serialize(object target, SerializationInfo info, StreamingContext context);
            IReadOnlyDictionary<string, System.Type> GetKeyAndTypeDict(System.Type type);
        }

        protected static readonly System.Type DEFAULT_TYPE = typeof(Dictionary<string, object>);

        IInstanceCreator _instanceCreator = null;

        protected abstract void ReadTo(TextReader stream, SerializationInfo outInfo, IReadOnlyDictionary<string, System.Type> keyAndTypeDict);
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
            else if (HasSerializableAttribute(obj)
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
            var keyAndTypeDict = GetKeyAndTypeDictionary(type);
            var (info, context) = CreateInfoAndContext(type);
            ReadTo(stream, info, keyAndTypeDict);

            object inst = _instanceCreator?.Create(type, info, context);
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
            else if (type.GetInterface(typeof(ISerializable).FullName) != null)
            {
                var ctor = type.GetConstructor(new[] { typeof(SerializationInfo), typeof(StreamingContext) });
                Assert.IsNotNull(ctor, $"Not Define Constractor(SerializationInfo, StreamingContext)... type={type}");
                inst = ctor.Invoke(new object[] { info, context });
            }
            else if (type.GetCustomAttribute<System.SerializableAttribute>() != null)
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

        bool HasSerializableAttribute(object obj)
        {
            return obj.GetType().GetCustomAttribute<System.SerializableAttribute>() != null;
        }

        /// <summary>
        /// 型に結び付けられたキー名とその型の辞書を返します。
        /// この辞書を使用することで、フィールド名とは異なったキーを指定することができます。
        ///
        /// 使用する辞書の優先順位としては以下のものになります。
        /// 1. IInstanceCreator#GetKeyAndTypeDict
        /// 1. typeに付けられたHasKeyAndTypeDictionaryGetterAttribute
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IReadOnlyDictionary<string, System.Type> GetKeyAndTypeDictionary(System.Type type)
        {
            var dict = _instanceCreator.GetKeyAndTypeDict(type);
            if (dict != null) return dict;

            var dictGetter = type.GetCustomAttribute<HasKeyAndTypeDictionaryGetterAttribute>();
            return dictGetter?.GetDictionary(type) ?? null;
        }

        /// <summary>
        /// デフォルトのIInstanceCreator
        ///
        /// UnityのVector,Quaternion型に対応しています
        /// </summary>
        public class DefaultInstanceCreator : IInstanceCreator
        {
            static IReadOnlyDictionary<System.Type, System.Func<SerializationInfo, StreamingContext, object>> _instanceCreateDict;
            static IReadOnlyDictionary<System.Type, System.Func<object, SerializationInfo, StreamingContext, bool>> _instanceSerializerDict;
            static IReadOnlyDictionary<System.Type, IReadOnlyDictionary<string, System.Type>> _instanceKeyAndTypeDict;

            /// <summary>
            /// 指定した型のインスタンスを生成する関数の辞書
            ///
            /// 派生クラスでのインスタンス生成関数の辞書のベースに使用することを推奨します。
            /// </summary>
            static protected IReadOnlyDictionary<System.Type, System.Func<SerializationInfo, StreamingContext, object>> InstanceCreatorDict
            {
                get
                {
                    if (null == _instanceCreateDict)
                    {
                        _instanceCreateDict = CreateDictinary();
                    }
                    return _instanceCreateDict;
                }
            }
            /// <summary>
            /// 指定したインスタンスをシリアライズを行う関数の辞書
            ///
            /// 派生クラスでのシリアライズ関数の辞書のベースに使用することを推奨します。
            /// </summary>
            static protected IReadOnlyDictionary<System.Type, System.Func<object, SerializationInfo, StreamingContext, bool>> InstanceSerializerDict
            {
                get => _instanceSerializerDict != null
                    ? _instanceSerializerDict
                    : (_instanceSerializerDict = CreateSerializer());
            }

            static protected IReadOnlyDictionary<System.Type, IReadOnlyDictionary<string, System.Type>> InstanceKeyAndTypeDict
            {
                get => _instanceKeyAndTypeDict != null
                    ? _instanceKeyAndTypeDict
                    : (_instanceKeyAndTypeDict = CreateKeyAndTypeDict());
            }

            /// <summary>
            /// type型のインスタンスを生成する
            /// </summary>
            /// <param name="type"></param>
            /// <param name="info"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            public object Create(System.Type type, SerializationInfo info, StreamingContext context)
            {
                if (!InstanceCreatorDict.ContainsKey(type)) return null;
                return InstanceCreatorDict[type](info, context);
            }

            /// <summary>
            /// targetのシリアライズを行う
            /// </summary>
            /// <param name="target"></param>
            /// <param name="info"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            public bool Serialize(object target, SerializationInfo info, StreamingContext context)
            {
                var type = target.GetType();
                if (!InstanceSerializerDict.ContainsKey(type)) return false;
                return InstanceSerializerDict[type](target, info, context);
            }

            /// <summary>
            /// typeに対応したキー名と型の辞書を返す
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            public IReadOnlyDictionary<string, System.Type> GetKeyAndTypeDict(System.Type type)
            {
                if (!InstanceKeyAndTypeDict.ContainsKey(type)) return null;
                return InstanceKeyAndTypeDict[type];
            }

            /// <summary>
            /// 型に合わせたインスタンス生成関数の辞書を生成する
            /// </summary>
            /// <returns></returns>
            static IReadOnlyDictionary<System.Type, System.Func<SerializationInfo, StreamingContext, object>> CreateDictinary()
            {
                return new Dictionary<System.Type, System.Func<SerializationInfo, StreamingContext, object>>
                {
                    {
                        typeof(Vector2), (info, context) =>
                        {
                            var inst = new Vector2();
                            inst.x = info.GetSingle("x");
                            inst.y = info.GetSingle("y");
                            return inst;
                        }
                    },
                    {
                        typeof(Vector2Int), (info, context) =>
                        {
                            var inst = new Vector2Int();
                            inst.x = info.GetInt32("x");
                            inst.y = info.GetInt32("y");
                            return inst;
                        }
                    },
                    {
                        typeof(Vector3), (info, context) =>
                        {
                            var inst = new Vector3();
                            inst.x = info.GetSingle("x");
                            inst.y = info.GetSingle("y");
                            inst.z = info.GetSingle("z");
                            return inst;
                        }
                    },
                    {
                        typeof(Vector3Int), (info, context) =>
                        {
                            var inst = new Vector3Int();
                            inst.x = info.GetInt32("x");
                            inst.y = info.GetInt32("y");
                            inst.z = info.GetInt32("z");
                            return inst;
                        }
                    },
                    {
                        typeof(Vector4), (info, context) =>
                        {
                            var inst = new Vector4();
                            inst.x = info.GetSingle("x");
                            inst.y = info.GetSingle("y");
                            inst.z = info.GetSingle("z");
                            inst.w = info.GetSingle("w");
                            return inst;
                        }
                    },
                    {
                        typeof(Quaternion), (info, context) =>
                        {
                            var inst = new Quaternion();
                            inst.x = info.GetSingle("x");
                            inst.y = info.GetSingle("y");
                            inst.z = info.GetSingle("z");
                            inst.w = info.GetSingle("w");
                            return inst;
                        }
                    },
                };
            }
        }

        /// <summary>
        /// 型に合わせたシリアライズ関数の辞書を生成する
        ///
        /// この辞書に登録した型はDefaultInstanceCreator#CreateKeyAndTypeDictにも登録することを推奨します。
        /// <seealso cref="CreateKeyAndTypeDict"/>
        /// </summary>
        /// <returns></returns>
        static IReadOnlyDictionary<System.Type, System.Func<object, SerializationInfo, StreamingContext, bool>> CreateSerializer()
        {
            return new Dictionary<System.Type, System.Func<object, SerializationInfo, StreamingContext, bool>>
                {
                    {
                        typeof(Vector2Int), (inst, info, context) =>
                        {
                            var vec2 = (Vector2Int)inst;
                            info.AddValue("x", vec2.x);
                            info.AddValue("y", vec2.y);
                            return true;
                        }
                    },
                    {
                        typeof(Vector3Int), (inst, info, context) =>
                        {
                            var vec3 = (Vector3Int)inst;
                            info.AddValue("x", vec3.x);
                            info.AddValue("y", vec3.y);
                            info.AddValue("z", vec3.z);
                            return true;
                        }
                    },
                };
        }

        /// <summary>
        /// 指定した型のキー名と型の辞書の辞書を生成する。
        ///
        /// この辞書に登録した型はDefaultInstanceCreator#CreateSerializerにも登録することを推奨します。
        /// </summary>
        /// <returns></returns>
        static IReadOnlyDictionary<System.Type, IReadOnlyDictionary<string, System.Type>> CreateKeyAndTypeDict()
        {
            return new Dictionary<System.Type, IReadOnlyDictionary<string, System.Type>>
                {
                    {
                        typeof(Vector2Int), new Dictionary<string, System.Type>
                        {
                            {"x", typeof(float)},
                            {"y", typeof(float)},
                        }
                    },
                    {
                        typeof(Vector3Int), new Dictionary<string, System.Type>
                        {
                            {"x", typeof(float)},
                            {"y", typeof(float)},
                            {"z", typeof(float)},
                        }
                    },
            };
        }

    }
}

