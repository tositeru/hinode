using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Serialization;

namespace Hinode.Tests.Input
{
    /// <summary>
    /// <seealso cref="InputRecorder"/>
    /// </summary>
    public class TestInputRecorder
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
                //手ごろなプロパティに設定しているだけで、特に意味はない処理
                _dummyData.Value = input.TouchCount > 0 ? input.GetTouch(0).fingerId : -1;
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
                input.ClearRecordedTouch();
                //手ごろなプロパティに設定しているだけで、特に意味はない処理
                input.SetRecordedTouch(0, new Touch() { fingerId = DummyData });
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
                while(e.MoveNext())
                {
                    if(e.Name == KeyDummyData)
                    {
                        _dummyData.Value = (int)e.Value;
                    }
                }
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                foreach(var t in GetValuesEnumerable()
                    .Where(_t => _t.Value.DidUpdated))
                {
                    info.AddValue(t.Key, t.Value.RawValue);
                }
            }
            #endregion

            [SerializationKeyTypeGetter]
            public static System.Type GetKeyType(string key)
            {
                if(KeyDummyData == key)
                {
                    return typeof(int);
                }
                return null;
            }
        }

        [SetUp]
        public void SetUp()
        {
            ReplayableInput.ResetInstance();
        }

        /// <summary>
        /// <seealso cref="InputRecorder.UseInput"/>
        /// </summary>
        [Test]
        public void UseInputPropertyPasses()
        {
            var recorder = new InputRecorder();
            Assert.AreSame(ReplayableInput.Instance, recorder.UseInput);

            recorder.UseInput = new ReplayableInput();
            Assert.AreNotSame(ReplayableInput.Instance, recorder.UseInput);
        }

        /// <summary>
        /// <seealso cref="InputRecorder.IsReplaying"/>
        /// </summary>
        [Test]
        public void IsReplayingPasses()
        {
            var recorder = new InputRecorder
            {
                FrameDataRecorder = new DummyFrameDataRecorder()
            };

            //再生に使用するデータの準備
            var useRecord = InputRecord.Create();
            for (var i = 0; i < 10; ++i)
            {
                var frame = new InputRecord.Frame((uint)i, 0.016f);
                frame.InputText = $"{i + 10}";
                useRecord.Push(frame);
            }

            {//再生開始と停止のテスト
                recorder.StartReplay(useRecord);
                Assert.IsTrue(recorder.IsReplaying);
            }
            Debug.Log($"Success to IsReplaying when StartReplay!");
            {
                recorder.StopReplay();
                Assert.IsFalse(recorder.IsReplaying);
            }
            Debug.Log($"Success to IsReplaying when StopReplay!");

            {//最終フレームまで再生するテスト
                recorder.StartReplay();
                Assert.IsTrue(recorder.IsReplaying);
                for (var i = 0; i < useRecord.FrameCount; ++i)
                {
                    recorder.StepFrame();
                }
                Assert.IsTrue(recorder.IsReplaying);

                recorder.StepFrame();
                Assert.IsFalse(recorder.IsReplaying);

                Assert.AreEqual(useRecord.FrameCount, recorder.CurrentFrameNo);
            }
            Debug.Log($"Success to IsReplaying when Finish to Replay!");

            {//中断のテスト
                recorder.StartReplay();
                while (recorder.CurrentFrameNo < useRecord.FrameCount / 2)
                {
                    recorder.StepFrame();
                }
                recorder.PauseReplay();
                Assert.IsTrue(recorder.IsReplaying);
            }
            Debug.Log($"Success to IsReplaying when Pause to Replay!");
        }

        /// <summary>
        /// 入力データの記録のテスト
        ///
        /// 入力データの記録する際の処理の流れが正しくできているかどうかのみ確認しています。
        /// そのため実際の入力データが正しく記録されているかはこのテストでは検証していません。
        /// <seealso cref="InputRecorder.StartRecord(InputRecord)"/>
        /// <seealso cref="InputRecorder.StopRecord()"/>
        /// <seealso cref="InputRecorder.SaveToTarget(InputRecord)"/>
        /// </summary>
        /// <returns></returns>
        [Test]
        public void BasicRecordUsagePasses()
        {
            var recorder = new InputRecorder()
            {
                UseSerializer = new JsonSerializer(),
                UseInput = new ReplayableInput() { IsReplaying = true },
                FrameDataRecorder = new DummyFrameDataRecorder(),
            };

            var inputRecord = InputRecord.Create(new Vector2Int(Screen.width, Screen.height));
            {
                recorder.StartRecord(inputRecord);
                Assert.AreEqual(InputRecorder.State.Recording, recorder.CurrentState);
                Assert.AreSame(inputRecord, recorder.Target);
            }
            Debug.Log($"Success to StartRecord!");


            int waitFrameCount;
            System.Func<int, int> getInputData = (int i) => (i + 1) * 10;
            {
                waitFrameCount = 5; //5フレーム待つ
                recorder.UseInput.ClearRecordedTouch();
                for (var i = 0; i < waitFrameCount; ++i)
                {
                    recorder.UseInput.SetRecordedTouch(0, new Touch(){ fingerId = getInputData(i)});
                    recorder.StepFrame();
                }
                recorder.StopRecord();
                Assert.AreEqual(InputRecorder.State.Stop, recorder.CurrentState);
            }
            Debug.Log($"Success to StopRecord!");

            {
                recorder.SaveToTarget();
                Assert.AreSame(inputRecord, recorder.Target);
                Assert.AreEqual(waitFrameCount, recorder.Target.FrameCount);
                foreach (var (frame, frameIndex) in recorder.Target.Frames
                    .Zip(Enumerable.Range(0, waitFrameCount), (f, d) => (frame: f, frameIndex: d)))
                {
                    var recoverData = recorder.UseSerializer.Deserialize<DummyFrameDataRecorder>(frame.InputText);
                    Assert.AreEqual(frameIndex, frame.FrameNo);
                    Assert.IsFalse(frame.IsEmptyInputText);
                    Assert.AreEqual(getInputData(frameIndex), recoverData.DummyData);
                }
            }
            Debug.Log($"Success to SaveToTarget!");

            //再び記録したときの挙動の確認
            {
                recorder.StartRecord();
                Assert.AreEqual(InputRecorder.State.Recording, recorder.CurrentState);
                Assert.AreSame(inputRecord, recorder.Target, "not keep InputRecorder#Target when not pass argument InputRecorder#StartRecord ... ");
                //3フレーム待つ
                waitFrameCount = 3;
                for (var i = 0; i < waitFrameCount; ++i)
                {
                    recorder.UseInput.SetRecordedTouch(0, new Touch() { fingerId = getInputData(i) });
                    recorder.StepFrame();
                }
                recorder.StopRecord();
                Assert.AreEqual(InputRecorder.State.Stop, recorder.CurrentState);

                recorder.SaveToTarget();
                Assert.AreSame(inputRecord, recorder.Target);
                Assert.AreEqual(waitFrameCount, recorder.Target.FrameCount);
                foreach (var (frame, frameIndex) in recorder.Target.Frames
                    .Zip(Enumerable.Range(0, waitFrameCount), (f, d) => (frame: f, frameIndex: d)))
                {
                    var recoverData = recorder.UseSerializer.Deserialize<DummyFrameDataRecorder>(frame.InputText);
                    Assert.AreEqual(recoverData.DummyData, getInputData(frameIndex), $"not equal... frameData={frame.InputText}, index={frameIndex}");
                }
            }
            Debug.Log($"Success to Re-Record!");
        }

        /// <summary>
        /// <seealso cref="InputRecorder.StartRecord(InputRecord)"/>
        /// </summary>
        /// <returns></returns>
        [Test]
        public void RecordToSkipableFramePasses()
        {
            var recorder = new InputRecorder()
            {
                UseSerializer = new JsonSerializer(),
                UseInput = new ReplayableInput() { IsReplaying = true },
                FrameDataRecorder = new DummyFrameDataRecorder(),
            };
            System.Func<int, int> getInputData = (int i) => (i + 1) * 10;
            int frameCount;

            {
                var inputRecord = InputRecord.Create(new Vector2Int(Screen.width, Screen.height));
                recorder.StartRecord(inputRecord);

                frameCount = 5; //5フレーム待つ
                for (var i = 0; i < frameCount; ++i)
                {
                    recorder.UseInput.SetRecordedTouch(0, new Touch() { fingerId = getInputData(i) });
                    recorder.StepFrame();

                    recorder.StepFrame(); // <- not Add Frame!
                }
                recorder.StopRecord();

                recorder.SaveToTarget();
                Assert.AreEqual(frameCount, recorder.Target.FrameCount);

                foreach (var (frame, frameIndex) in recorder.Target.Frames
                    .Zip(Enumerable.Range(0, frameCount), (f, d) => (frame: f, frameIndex: d)))
                {
                    var recoverData = recorder.UseSerializer.Deserialize<DummyFrameDataRecorder>(frame.InputText);
                    Assert.AreEqual(frameIndex*2, frame.FrameNo);
                    Assert.IsFalse(frame.IsEmptyInputText);
                    Assert.AreEqual(getInputData(frameIndex), recoverData.DummyData);
                }
            }
        }

        /// <summary>
        /// 入力データの再生のテスト
        ///
        /// 入力データを再生する際の処理の流れが正しくできているかどうかのみ確認しています。
        /// そのため実際の入力データが正しく再生されているかはこのテストでは検証していません。
        /// <seealso cref="InputRecorder.StartReplay(InputRecord)"/>
        /// <seealso cref="InputRecorder.StopReplay()"/>
        /// <seealso cref="InputRecorder.StepFrame()"/>
        /// </summary>
        /// <returns></returns>
        [Test]
        public void BasicReplayUsagePasses()
        {
            var recorder = new InputRecorder
            {
                UseInput = new ReplayableInput() { IsReplaying = true },
                FrameDataRecorder = new DummyFrameDataRecorder()
            };
            System.Func<int, int> getFrameData = (int i) => i+1;

            //再生に使用するデータの準備
            var useRecord = InputRecord.Create();
            for (var i = 0; i < 10; ++i)
            {
                var frame = new InputRecord.Frame((uint)i, 0.016f);
                recorder.UseInput.SetRecordedTouch(0, new Touch() { fingerId = getFrameData(i) });
                recorder.FrameDataRecorder.Record(recorder.UseInput);
                frame.InputText = recorder.UseSerializer.Serialize(recorder.FrameDataRecorder);
                useRecord.Push(frame);
            }

            {//再生開始と停止のテスト
                recorder.StartReplay(useRecord);
                Assert.AreEqual(InputRecorder.State.Replaying, recorder.CurrentState);
                Assert.AreSame(useRecord, recorder.Target);
                Assert.IsTrue(recorder.IsReplaying);
                Assert.AreEqual(-1, recorder.CurrentFrameNo);

                recorder.StopReplay();
                Assert.AreEqual(InputRecorder.State.Stop, recorder.CurrentState);
                Assert.IsFalse(recorder.IsReplaying);
            }
            Debug.Log($"Success to Start and Stop Replay!");

            {//最終フレームまで再生するテスト
                recorder.StartReplay();
                Assert.AreSame(useRecord, recorder.Target);
                for (var i = 0; i < useRecord.FrameCount; ++i)
                {
                    recorder.StepFrame();

                    Assert.AreEqual(i, recorder.CurrentFrameNo);

                    var frameDataRecorder = recorder.FrameDataRecorder as DummyFrameDataRecorder;
                    Assert.AreEqual(getFrameData(i), frameDataRecorder.DummyData);
                }
                recorder.StepFrame();                
                Assert.AreEqual(useRecord.FrameCount, recorder.CurrentFrameNo);
            }
            Debug.Log($"Success to Replay to Tail!");

            {//中断のテスト
                recorder.StartReplay();
                while (recorder.CurrentFrameNo < useRecord.FrameCount / 2)
                {
                    recorder.StepFrame();
                }
                recorder.PauseReplay();
                var pausingFrameNo = recorder.CurrentFrameNo;
                Assert.AreEqual(InputRecorder.State.PauseingReplay, recorder.CurrentState);
                Assert.IsTrue(recorder.IsReplaying);

                //何もしないフレームを設ける
                for (var i = 0; i < 5; ++i)
                {
                    recorder.StepFrame();
                }
                Assert.AreEqual(pausingFrameNo, recorder.CurrentFrameNo);

                recorder.StartReplay();
                Assert.AreEqual(InputRecorder.State.Replaying, recorder.CurrentState);
                Assert.IsTrue(recorder.IsReplaying);
            }
            Debug.Log($"Success to Pause Replay!");
        }

        /// <summary>
        /// 空データの再生のテスト
        ///
        /// 入力データを再生する際の処理の流れが正しくできているかどうかのみ確認しています。
        /// そのため実際の入力データが正しく再生されているかはこのテストでは検証していません。
        /// <seealso cref="InputRecorder.StartReplay(InputRecord)"/>
        /// <seealso cref="InputRecorder.CurrentState"/>
        /// <seealso cref="InputRecorder.IsReplaying"/>
        /// </summary>
        /// <returns></returns>
        [Test, Description("空データの再生のテスト")]
        public void EmptyReplayPasses()
        {
            var recorder = new InputRecorder
            {
                FrameDataRecorder = new DummyFrameDataRecorder()
            };

            //再生に使用するデータの準備(空)
            var useRecord = InputRecord.Create();

            recorder.StartReplay(useRecord);
            Assert.AreEqual(InputRecorder.State.Stop, recorder.CurrentState);
            Assert.IsFalse(recorder.IsReplaying);
        }
    }
}
