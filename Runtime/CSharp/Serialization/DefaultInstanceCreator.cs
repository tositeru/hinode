using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode.Serialization
{
    /// <summary>
    /// デフォルトのIInstanceCreator
    ///
    /// UnityのVector,Quaternion型に対応しています
    /// <seealso cref="Hinode.Tests.CSharp.TestJsonSerializer.InstanceCreatorPasses()"/>
    /// </summary>
    public class DefaultInstanceCreator : ISerializer.IInstanceCreator
    {
        class TypeInfo
        {
            public delegate object DesirializeInstancePredication(SerializationInfo info, StreamingContext context);
            public delegate bool SerializeInstancePredication(object instance, SerializationInfo info, StreamingContext context);

            public static TypeInfo Create<T>(DesirializeInstancePredication desirializeInstance, SerializeInstancePredication serializeInstance, IReadOnlyDictionary<string, System.Type> fieldKeyAndTypeDict)
                => Create(typeof(T), desirializeInstance, serializeInstance, fieldKeyAndTypeDict);

            public static TypeInfo Create(System.Type target, DesirializeInstancePredication desirializeInstance, SerializeInstancePredication serializeInstance, IReadOnlyDictionary<string, System.Type> fieldKeyAndTypeDict)
            {
                Assert.IsNotNull(target);
                Assert.IsNotNull(desirializeInstance);

                return new TypeInfo()
                {
                    Target = target,
                    DesirializeInstance = desirializeInstance,
                    SerializeInstance = serializeInstance,
                    FieldKeyAndTypeDict = fieldKeyAndTypeDict,
                };
            }
            public System.Type Target { get; private set; }
            public DesirializeInstancePredication DesirializeInstance { get; private set; }
            public SerializeInstancePredication SerializeInstance { get; private set; }
            public IReadOnlyDictionary<string, System.Type> FieldKeyAndTypeDict { get; private set; }

            TypeInfo()
            {}
        }

        static Dictionary<System.Type, TypeInfo> _typeInfoDict = null;

        #region IInstanceCreator insterface
        /// <summary>
        /// type型のインスタンスを生成する
        /// </summary>
        /// <param name="type"></param>
        /// <param name="info"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public object Desirialize(System.Type type, SerializationInfo info, StreamingContext context)
        {
            if (!_typeInfoDict.ContainsKey(type)) return null;
            return _typeInfoDict[type].DesirializeInstance?.Invoke(info, context) ?? null;
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
            if (!_typeInfoDict.ContainsKey(type)) return false;
            return _typeInfoDict[type].SerializeInstance?.Invoke(target, info, context) ?? false;
        }

        /// <summary>
        /// typeに対応したキー名と型の辞書を返す
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IReadOnlyDictionary<string, System.Type> GetFieldKeyAndTypeDict(System.Type type)
        {
            if (!_typeInfoDict.ContainsKey(type)) return null;
            return _typeInfoDict[type].FieldKeyAndTypeDict;
        }
        #endregion

        static DefaultInstanceCreator()
        {
            _typeInfoDict = new Dictionary<System.Type, TypeInfo>();
            _typeInfoDict.Add(typeof(Vector2), TypeInfo.Create<Vector2>(
                (info, context) => {
                    var inst = new Vector2
                    {
                        x = info.GetSingle("x"),
                        y = info.GetSingle("y")
                    };
                    return inst;
                },
                null, null)
            );

            _typeInfoDict.Add(typeof(Vector2Int), TypeInfo.Create<Vector2Int>(
                (info, context) => {
                    var inst = new Vector2Int
                    {
                        x = info.GetInt32("x"),
                        y = info.GetInt32("y")
                    };
                    return inst;
                },
                (inst, info, context) => {
                    var vec2 = (Vector2Int)inst;
                    info.AddValue("x", vec2.x);
                    info.AddValue("y", vec2.y);
                    return true;
                },
                new Dictionary<string, System.Type> {
                    {"x", typeof(int)},
                    {"y", typeof(int)},
                })
            );

            _typeInfoDict.Add(typeof(Vector3), TypeInfo.Create<Vector3>(
                (info, context) => {
                    var inst = new Vector3
                    {
                        x = info.GetSingle("x"),
                        y = info.GetSingle("y"),
                        z = info.GetSingle("z")
                    };
                    return inst;
                },
                null, null)
            );

            _typeInfoDict.Add(typeof(Vector3Int), TypeInfo.Create<Vector3Int>(
                (info, context) => {
                    var inst = new Vector3Int
                    {
                        x = info.GetInt32("x"),
                        y = info.GetInt32("y"),
                        z = info.GetInt32("z")
                    };
                    return inst;
                },
                (inst, info, context) => {
                    var vec = (Vector3Int)inst;
                    info.AddValue("x", vec.x);
                    info.AddValue("y", vec.y);
                    info.AddValue("z", vec.z);
                    return true;
                },
                new Dictionary<string, System.Type> {
                    {"x", typeof(int)},
                    {"y", typeof(int)},
                    {"z", typeof(int)},
                })
            );

            _typeInfoDict.Add(typeof(Vector4), TypeInfo.Create<Vector4>(
                (info, context) => {
                    var inst = new Vector4
                    {
                        x = info.GetSingle("x"),
                        y = info.GetSingle("y"),
                        z = info.GetSingle("z"),
                        w = info.GetSingle("w")
                    };
                    return inst;
                },
                null, null)
            );

            _typeInfoDict.Add(typeof(Quaternion), TypeInfo.Create<Quaternion>(
                (info, context) => {
                    var inst = new Quaternion
                    {
                        x = info.GetSingle("x"),
                        y = info.GetSingle("y"),
                        z = info.GetSingle("z"),
                        w = info.GetSingle("w")
                    };
                    return inst;
                },
                null, null)
            );
        }
    }
}
