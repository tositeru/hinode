using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Hinode.Serialization;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// UnityEngine.EventSystemの1フレームにおける入力データを記録するためのもの
    /// 一つ前と変化がないデータはシリアライズの対象にならないようになっています。
    /// 
    /// <see cref="IFrameDataRecorder"/>
    /// </summary>
    [System.Serializable, ContainsSerializationKeyTypeGetter(typeof(FrameInputData))]
    public class FrameInputData : IFrameDataRecorder, ISerializable
    {
        Dictionary<string, IFrameDataRecorder> _childFrameInputDatas = new Dictionary<string, IFrameDataRecorder>();

        JsonSerializer _jsonSerializer = new JsonSerializer();

        public IReadOnlyDictionary<string, IFrameDataRecorder> ChildFrameInputDatas { get => _childFrameInputDatas; }

        public FrameInputData()
        {
        }

        public bool ContainsChildRecorder(string key)
            => ChildFrameInputDatas.ContainsKey(key);
        public bool ContainsChildRecorder(System.Type type)
            => ChildFrameInputDatas.Any(_t => _t.Value.GetType().Equals(type));
        public bool ContainsChildRecorder<T>()
            where T : IFrameDataRecorder, ISerializable
            => ContainsChildRecorder(typeof(T));

        public FrameInputData AddChildRecorder(IFrameDataRecorder childFrameInputData)
        {
            Assert.IsTrue(ContainsChildFrameInputDataType(childFrameInputData.GetType()), $"This Type({childFrameInputData.GetType()}) don't regist by FrameInpuData#RegistChildFrameInputDataType()...");

            var key = GetChildFrameInputDataKey(childFrameInputData.GetType());
            Assert.IsFalse(ContainsChildRecorder(key),
                $"This Type({childFrameInputData.GetType()}) already add...");

            _childFrameInputDatas.Add(key, childFrameInputData);
            return this;
        }

        public FrameInputData RemoveChildRecorder(string key)
        {
            if (_childFrameInputDatas.ContainsKey(key))
            {
                _childFrameInputDatas.Remove(key);
            }
            return this;
        }

        public FrameInputData RemoveChildRecorder(System.Type type)
            => RemoveChildRecorder(GetChildFrameInputDataKey(type));
        public FrameInputData RemoveChildRecorder<T>()
            where T : IFrameDataRecorder, ISerializable
            => RemoveChildRecorder(GetChildFrameInputDataKey<T>());

        /// <summary>
        /// 他のインスタンスに自身の値を設定する。
        ///
        /// 設定される値は更新済みのものだけになります。
        /// </summary>
        /// <param name="other"></param>
        public void CopyUpdatedDatasTo(IFrameDataRecorder other)
        {
            Assert.IsTrue(other is FrameInputData);
            var otherInputData = other as FrameInputData;

            foreach (var child in ChildFrameInputDatas.Select(_t => _t.Value))
            {
                var key = FrameInputData.GetChildFrameInputDataKey(child.GetType());
                var otherChild = otherInputData.ChildFrameInputDatas.FirstOrDefault(_t => _t.Key == key);
                child.CopyUpdatedDatasTo(otherChild.Value);
            }
        }

        #region IFrameDataRecorder
        /// <summary>
        /// <see cref="IFrameDataRecorder.ResetDatas()"/>
        /// </summary>
        public void ResetDatas()
        {
            foreach (var child in ChildFrameInputDatas.Select(_t => _t.Value))
            {
                child.ResetDatas();
            }
        }

        public void RefleshUpdatedFlags()
        {
            foreach (var child in ChildFrameInputDatas.Select(_t => _t.Value))
            {
                child.RefleshUpdatedFlags();
            }
        }

        /// <summary>
        /// <see cref="IFrameDataRecorder.Record(ReplayableInput)"/>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public void Record(ReplayableInput input)
        {
            foreach (var child in ChildFrameInputDatas.Select(_t => _t.Value))
            {
                child.Record(input);
            }
        }

        public void RecoverTo(ReplayableInput input)
        {
            foreach (var child in ChildFrameInputDatas.Select(_t => _t.Value))
            {
                child.RecoverTo(input);
            }
        }

        public IEnumerable<FrameInputDataKeyValue> GetValuesEnumerable()
        {
            return new ValuesEnumerable(this);
        }

        class ValuesEnumerable : IEnumerable<FrameInputDataKeyValue>
            , IEnumerable
        {
            FrameInputData _target;
            public ValuesEnumerable(FrameInputData target)
            {
                _target = target;
            }

            public IEnumerator<FrameInputDataKeyValue> GetEnumerator()
            {
                return _target.GetChildRecorderEnumerable()
                    .SelectMany(_childT => _childT.child.GetValuesEnumerable()
                            .Select(_t => new FrameInputDataKeyValue($"{_childT.key}.{_t.Key}", _t.Value))
                    ).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }

        #endregion

        #region Child FrameInputData KeyType
        readonly static Dictionary<string, System.Type> _keyTypeDict = new Dictionary<string, System.Type>();

        public static IReadOnlyDictionary<string, System.Type> RegistedChildFrameInputDataTypes { get => _keyTypeDict; }

        public static void ClearChildFrameInputDataType()
        {
            _keyTypeDict.Clear();
        }

        public static bool ContainsChildFrameInputDataType(string key)
            => _keyTypeDict.ContainsKey(key);
        public static bool ContainsChildFrameInputDataType(System.Type type)
            => GetChildFrameInputDataKey(type) != null;
        public static bool ContainsChildFrameInputDataType<T>()
            where T : IFrameDataRecorder, ISerializable
            => GetChildFrameInputDataKey(typeof(T)) != null;

        public static System.Type GetChildFrameInputDataType(string key)
            => ContainsChildFrameInputDataType(key)
            ? _keyTypeDict[key]
            : null;

        public static string GetChildFrameInputDataKey(System.Type type)
        {
            return _keyTypeDict
                .Where(_t => _t.Value.Equals(type))
                .Select(_t => _t.Key)
                .FirstOrDefault();
        }
        public static string GetChildFrameInputDataKey<T>()
            where T : IFrameDataRecorder, ISerializable
            => GetChildFrameInputDataKey(typeof(T));

        public static void RegistChildFrameInputDataType(string key, System.Type type)
        {
            Assert.IsTrue(type.ContainsInterface<IFrameDataRecorder>(), $"Invalid type({type})...");
            Assert.IsTrue(type.ContainsInterface<ISerializable>(), $"Invalid type({type})...");
            Assert.IsFalse(_keyTypeDict.ContainsKey(key), $"already contains key({key})...");
            Assert.IsFalse(_keyTypeDict.Any(_t => _t.Value.Equals(type)), $"already contains key({key})...");
            _keyTypeDict.Add(key, type);
        }

        public static void RegistChildFrameInputDataType<T>(string key)
            where T : IFrameDataRecorder, ISerializable
            => RegistChildFrameInputDataType(key, typeof(T));

        public static void UnregistChildFrameInputDataType(string key)
        {
            Assert.IsTrue(_keyTypeDict.ContainsKey(key), $"Key({key}) don't contians...");
            _keyTypeDict.Remove(key);
        }
        #endregion

        #region ISerializable
        [SerializationKeyTypeGetter]
        public static System.Type GetKeyType(string key)
            => _keyTypeDict.ContainsKey(key)
            ? _keyTypeDict[key]
            : null;

        public FrameInputData(SerializationInfo info, StreamingContext context)
            : this()
        {
            var e = info.GetEnumerator();
            while (e.MoveNext())
            {
                if (ContainsChildFrameInputDataType(e.Name))
                {
                    var childType = GetChildFrameInputDataType(e.Name);
                    Assert.AreEqual(childType, e.Value.GetType());
                    //var cstr = childType.GetConstructor(new System.Type[] { typeof(SerializationInfo), typeof(StreamingContext) });
                    //Assert.IsNotNull(cstr);
                    //var inst = cstr.Invoke(new object[] { e.Value, context }) as IFrameDataRecorder;
                    AddChildRecorder(e.Value as IFrameDataRecorder);
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (var child in ChildFrameInputDatas)
            {
                info.AddValue(child.Key, child.Value);
            }
        }
        #endregion

        #region IEnumerable
        public IEnumerable<(string key, IFrameDataRecorder child)> GetChildRecorderEnumerable()
        {
            return new ChildRecorderEnumerable(this);
        }

        class ChildRecorderEnumerable : IEnumerable<(string key, IFrameDataRecorder child)>, IEnumerable
        {
            FrameInputData _target;
            public ChildRecorderEnumerable(FrameInputData target)
            {
                _target = target;
            }

            public IEnumerator<(string key, IFrameDataRecorder child)> GetEnumerator()
                => _target.ChildFrameInputDatas.Select(_t => (key: _t.Key, child: _t.Value)).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }
        #endregion
    }
}
