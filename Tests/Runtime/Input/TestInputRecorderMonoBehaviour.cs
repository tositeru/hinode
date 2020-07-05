using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Hinode.Serialization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Input
{
    /// <summary>
    /// <seealso cref="InputRecorderMonoBehaviour"/>
    /// </summary>
    public class TestInputRecorderMonoBehaviour : TestBase
    {
        /// <summary>
        /// ダミー用のフレームレコーダ
        /// 簡単なデータを出力している
        /// </summary>
        [System.Serializable]
        [ContainsSerializationKeyTypeGetter(typeof(DummyFrameDataRecorder))]
        class DummyFrameDataRecorder : IFrameDataRecorder
            , ISerializable
        {
            public static string KeyDummyData = "touchCount";

            UpdateObserver<int> _dummyData = new UpdateObserver<int>();
            public int DummyData { get => _dummyData.Value; }

            public DummyFrameDataRecorder() { }

            public void ResetDatas()
            {
                _dummyData.SetDefaultValue(true);
            }

            public void RefleshUpdatedFlags() { _dummyData.Reset(); }

            public void Record(ReplayableInput input)
            {
                _dummyData.Value = input.TouchCount;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="frameDataRecorder"></param>
            public void CopyUpdatedDatasTo(IFrameDataRecorder other)
            {
                var inst = other as DummyFrameDataRecorder;
                inst._dummyData.Value = DummyData;
            }

            /// <summary>
            /// この関数を呼び出した後は、このインスタンスとReplayableInputのパラメータがFrameのものへ更新されます。
            /// </summary>
            /// <param name="input"></param>
            public void RecoverTo(ReplayableInput input)
            {
                input.RecordedTouchCount = DummyData; //手ごろなプロパティに設定しているだけで、特に意味はない処理
            }

            public IEnumerable<FrameInputDataKeyValue> GetValuesEnumerable()
            {
                return new FrameInputDataKeyValue[]
                {
                    new FrameInputDataKeyValue(KeyDummyData, _dummyData),
                };
            }

            #region ISerializable
            public DummyFrameDataRecorder(SerializationInfo info, StreamingContext context)
            {
                var e = info.GetEnumerator();
                while (e.MoveNext())
                {
                    if (e.Name == KeyDummyData)
                    {
                        _dummyData.Value = (int)e.Value;
                    }
                }
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                foreach (var t in GetValuesEnumerable()
                    .Where(_t => _t.Value.DidUpdated))
                {
                    info.AddValue(t.Key, t.Value.RawValue);
                }
            }
            #endregion

            [SerializationKeyTypeGetter]
            public static System.Type GetKeyType(string key)
            {
                if (KeyDummyData == key)
                {
                    return typeof(int);
                }
                return null;
            }
        }

        /// <summary>
        /// <seealso cref="InputRecorderMonoBehaviour.UseRecorder"/>
        /// <seealso cref="InputRecorderMonoBehaviour.TargetRecord"/>
        /// <seealso cref="InputRecorderMonoBehaviour.IsValid"/>
        /// <seealso cref="InputRecorderMonoBehaviour.CurrentState"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator InitializePasses()
        {
            var recoderObj = new GameObject("recoder").AddComponent<InputRecorderMonoBehaviour>();
            Assert.IsFalse(recoderObj.IsValid);
            Assert.IsNotNull(recoderObj.UseRecorder);
            Debug.Log($"Success to Create immediate!");

            recoderObj.TargetRecord = InputRecord.Create();
            Assert.IsTrue(recoderObj.IsValid);
            Assert.IsNotNull(recoderObj.UseRecorder);
            Assert.IsNotNull(recoderObj.TargetRecord);

            Debug.Log($"Success to Set TargetRecord!");

            yield return null;
        }

        /// <summary>
        /// <seealso cref="InputRecorderMonoBehaviour.StartRecord()"/>
        /// <seealso cref="InputRecorderMonoBehaviour.StopRecord()"/>
        /// <seealso cref="InputRecorderMonoBehaviour.SaveToTarget(InputRecord)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator RecordBasicUsagePasses()
        {
            var recoderObj = new GameObject("recoder").AddComponent<InputRecorderMonoBehaviour>();
            recoderObj.UseRecorder.FrameDataRecorder = new DummyFrameDataRecorder();
            recoderObj.UseRecorder.UseInput = new ReplayableInput() { IsReplaying = true };
            recoderObj.TargetRecord = InputRecord.Create();

            System.Func<int, int> getFrameData = (int i) => i + 1;


            recoderObj.StartRecord(); // <- Start Record
            var loopCount = 5;
            for(var i=0; i<loopCount; ++i)
            {
                recoderObj.UseRecorder.UseInput.RecordedTouchCount = getFrameData(i);
                yield return null; // <- Call InputRecorder#StepFrame at WaitEndOfFrame()
            }
            recoderObj.StopRecord();

            recoderObj.SaveToTarget();

            {
                // validate Record Data
                var record = recoderObj.TargetRecord;
                Assert.AreEqual(loopCount, record.FrameCount);
                for (var i = 0; i < loopCount; ++i)
                {
                    recoderObj.UseRecorder.FrameDataRecorder.RecoverFromFrame(record[i], recoderObj.UseRecorder.UseSerializer);
                    recoderObj.UseRecorder.FrameDataRecorder.RecoverTo(recoderObj.UseRecorder.UseInput);
                    Assert.AreEqual(getFrameData(i), recoderObj.UseRecorder.UseInput.TouchCount);
                }
            }
        }

        /// <summary>
        /// <seealso cref="InputRecorderMonoBehaviour.StartReplay()"/>
        /// <seealso cref="InputRecorderMonoBehaviour.StopReplay()"/>
        /// <seealso cref="InputRecorderMonoBehaviour.PauseReplay()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator ReplayBasicUsagePasses()
        {
            var recoderObj = new GameObject("recoder").AddComponent<InputRecorderMonoBehaviour>();
            recoderObj.UseRecorder.FrameDataRecorder = new DummyFrameDataRecorder();
            recoderObj.UseRecorder.UseInput = new ReplayableInput() { IsReplaying = true };
            recoderObj.TargetRecord = InputRecord.Create();

            System.Func<int, int> getFrameData = (int i) => i + 1;

            recoderObj.UseRecorder.StartRecord(recoderObj.TargetRecord);
            var loopCount = 5;
            for (var i = 0; i < loopCount; ++i)
            {
                recoderObj.UseRecorder.UseInput.RecordedTouchCount = getFrameData(i);
                recoderObj.UseRecorder.StepFrame();
            }
            recoderObj.UseRecorder.StopRecord();
            recoderObj.UseRecorder.SaveToTarget();

            {
                recoderObj.StartReplay();
                for(var i =0; recoderObj.CurrentState != InputRecorder.State.Stop; ++i)
                {
                    yield return null;
                    var validValue = (i < recoderObj.TargetRecord.FrameCount)
                        ? getFrameData(i)
                        : getFrameData(recoderObj.TargetRecord.FrameCount - 1);
                    Assert.AreEqual(validValue, recoderObj.UseRecorder.UseInput.TouchCount);
                }
            }
            Debug.Log($"Success to Replay!");

            {
                recoderObj.StartReplay();
                for (var i = 0; recoderObj.CurrentState != InputRecorder.State.Stop; ++i)
                {
                    recoderObj.PauseReplay(); // must first Pause because not pause finished replay... 
                    for (var j = 0; j < 2; ++j)
                    {
                        yield return null;
                    }
                    recoderObj.StartReplay();

                    yield return null;
                    var validValue = (i < recoderObj.TargetRecord.FrameCount)
                        ? getFrameData(i)
                        : getFrameData(recoderObj.TargetRecord.FrameCount - 1);
                    Assert.AreEqual(validValue, recoderObj.UseRecorder.UseInput.TouchCount);
                }
            }
            Debug.Log($"Success to Pause Replay!");
        }
    }
}
