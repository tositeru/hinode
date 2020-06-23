using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace Hinode.Tests.Input
{
    /// <summary>
    /// <seealso cref="BaseInputRecorder"/>
    /// </summary>
    public class TestBaseInputRecorder : TestBase
    {
        /// <summary>
        /// ダミー用のフレームレコーダ
        /// 簡単なデータを出力している
        /// </summary>
        class DummyFrameDataRecorder : BaseInputRecorder.IFrameDataRecorder
        {
            int _dummyData = 0;

            public int DummyData { get => _dummyData; }

            public void ClearRecorder()
            {
                _dummyData = 0;
            }

            public void RecoverFrame(ReplayableBaseInput baseInput, InputRecord.Frame frame)
            {
                var data = int.Parse(frame.InputText);
                baseInput.recordedTouchCount = data; //手ごろなプロパティに設定しているだけで、特に意味はない処理
                _dummyData = data;
            }

            public InputRecord.Frame Update(BaseInput baseInput)
            {
                var frame = new InputRecord.Frame();
                frame.InputText = _dummyData.ToString();
                _dummyData++;
                return frame;
            }
        }

        /// <summary>
        /// 入力データの記録のテスト
        ///
        /// 入力データの記録する際の処理の流れが正しくできているかどうかのみ確認しています。
        /// そのため実際の入力データが正しく記録されているかはこのテストでは検証していません。
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator BasicRecordUsagePasses()
        {
            yield return null;

            var recorderObj = new GameObject("__recorder",
                typeof(BaseInputRecorder));
            var recorder = recorderObj.GetComponent<BaseInputRecorder>();

            var inputRecord = InputRecord.Create(new Vector2Int(Screen.width, Screen.height));

            recorder.FrameDataRecorder = new DummyFrameDataRecorder();
            recorder.StartRecord(inputRecord);
            Assert.IsTrue(recorder.IsRecording);
            Assert.IsFalse(recorder.IsStopping);
            Assert.IsFalse(recorder.IsPausingReplay);
            Assert.IsFalse(recorder.IsReplaying);
            Assert.AreSame(inputRecord, recorder.Target);

            //5フレーム待つ
            int waitFrameCount = 5;
            for (var i=0; i< waitFrameCount; ++i)
            {
                yield return null;
            }
            recorder.StopRecord();
            Assert.IsFalse(recorder.IsRecording);
            Assert.IsTrue(recorder.IsStopping);
            Assert.IsFalse(recorder.IsPausingReplay);
            Assert.IsFalse(recorder.IsReplaying);
            recorder.SaveToTarget();

            Assert.AreSame(inputRecord, recorder.Target);
            Assert.AreEqual(waitFrameCount, recorder.Target.FrameCount);
            foreach (var (frame, data) in recorder.Target.Frames
                .Zip(Enumerable.Range(0, waitFrameCount), (f, d) => (frame: f, data: d)))
            {
                var savedData = int.Parse(frame.InputText);
                Assert.AreEqual(savedData, data);
            }

            //再び記録したときの挙動の確認
            recorder.StartRecord();
            Assert.IsTrue(recorder.IsRecording);
            Assert.IsFalse(recorder.IsStopping);
            Assert.IsFalse(recorder.IsPausingReplay);
            Assert.IsFalse(recorder.IsReplaying);
            Assert.AreSame(inputRecord, recorder.Target, "not keep InputRecorder#Target when not pass argument InputRecorder#StartRecord ... ");

            //3フレーム待つ
            waitFrameCount = 3;
            for (var i = 0; i < waitFrameCount; ++i)
            {
                yield return null;
            }
            recorder.StopRecord();
            Assert.IsFalse(recorder.IsRecording);
            Assert.IsTrue(recorder.IsStopping);
            Assert.IsFalse(recorder.IsPausingReplay);
            Assert.IsFalse(recorder.IsReplaying);

            recorder.SaveToTarget();
            Assert.AreSame(inputRecord, recorder.Target);
            Assert.AreEqual(waitFrameCount, recorder.Target.FrameCount);
            foreach (var (frame, data) in recorder.Target.Frames
                .Zip(Enumerable.Range(0, waitFrameCount), (f, d) => (frame: f, data: d)))
            {
                var savedData = int.Parse(frame.InputText);
                Assert.AreEqual(savedData, data, $"not equal... frameData={frame.InputText}, index={data}");
            }
        }

        /// <summary>
        /// 入力データの再生のテスト
        ///
        /// 入力データを再生する際の処理の流れが正しくできているかどうかのみ確認しています。
        /// そのため実際の入力データが正しく再生されているかはこのテストでは検証していません。
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator BasicReplayUsagePasses()
        {
            yield return null;

            var recorderObj = new GameObject("__recorder",
                typeof(BaseInputRecorder));
            var recorder = recorderObj.GetComponent<BaseInputRecorder>();
            recorder.FrameDataRecorder = new DummyFrameDataRecorder();

            //再生に使用するデータの準備
            var useRecord = InputRecord.Create();
            for(var i=0; i<10; ++i)
            {
                var frame = new InputRecord.Frame((uint)i, 0.016f);
                frame.InputText = $"{i + 10}";
                useRecord.Push(frame);
            }

            {//再生開始と停止のテスト
                recorder.StartReplay(useRecord);
                Assert.AreEqual(BaseInputRecorder.State.Replaying, recorder.CurrentState);
                Assert.AreSame(useRecord, recorder.Target);
                Assert.IsTrue(recorder.IsReplaying);
                Assert.IsFalse(recorder.IsRecording);
                Assert.IsFalse(recorder.IsStopping);
                Assert.IsFalse(recorder.IsPausingReplay);
                Assert.AreEqual(-1, recorder.CurrentFrameNo);

                recorder.StopReplay();
                Assert.AreEqual(BaseInputRecorder.State.Stop, recorder.CurrentState);
                Assert.IsTrue(recorder.IsStopping);
                Assert.IsFalse(recorder.IsReplaying);
                Assert.IsFalse(recorder.IsRecording);
                Assert.IsFalse(recorder.IsPausingReplay);
            }

            {//最終フレームまで再生するテスト
                recorder.StartReplay();
                Assert.AreSame(useRecord, recorder.Target);
                for (var i = 0; i < useRecord.FrameCount; ++i)
                {
                    yield return null;
                    Assert.AreEqual(i, recorder.CurrentFrameNo);
                    var data = int.Parse(useRecord[i].InputText);
                    var frameDataRecorder = recorder.FrameDataRecorder as DummyFrameDataRecorder;
                    Assert.AreEqual(data, frameDataRecorder.DummyData);
                }
                yield return null; //最終フレームに到達後から停止状態になるまで1つフレームかかるので待つ
                Assert.AreEqual(BaseInputRecorder.State.Stop, recorder.CurrentState);
                yield return new WaitUntil(() => recorder.CurrentState == BaseInputRecorder.State.Stop);
                Assert.AreEqual(useRecord.FrameCount, recorder.CurrentFrameNo);
            }

            {//中断のテスト
                recorder.StartReplay();
                yield return new WaitUntil(() => recorder.CurrentFrameNo >= useRecord.FrameCount / 2);
                recorder.PauseReplay();
                var pausingFrameNo = recorder.CurrentFrameNo;
                Assert.AreEqual(BaseInputRecorder.State.PauseingReplay, recorder.CurrentState);
                Assert.IsTrue(recorder.IsReplaying);
                Assert.IsTrue(recorder.IsPausingReplay);
                Assert.IsFalse(recorder.IsRecording);
                Assert.IsFalse(recorder.IsStopping);

                //何もしないフレームを設ける
                for (var i = 0; i < 5; ++i)
                    yield return null;
                Assert.AreEqual(pausingFrameNo, recorder.CurrentFrameNo);

                recorder.StartReplay();
                Assert.AreEqual(BaseInputRecorder.State.Replaying, recorder.CurrentState);
                Assert.IsTrue(recorder.IsReplaying);
                Assert.IsFalse(recorder.IsPausingReplay);
                Assert.IsFalse(recorder.IsRecording);
                Assert.IsFalse(recorder.IsStopping);
                yield return new WaitUntil(() => recorder.CurrentState == BaseInputRecorder.State.Stop);
            }
        }

        /// <summary>
        /// 空データの再生のテスト
        ///
        /// 入力データを再生する際の処理の流れが正しくできているかどうかのみ確認しています。
        /// そのため実際の入力データが正しく再生されているかはこのテストでは検証していません。
        /// </summary>
        /// <returns></returns>
        [UnityTest, Description("空データの再生のテスト")]
        public IEnumerator EmptyReplayPasses()
        {
            yield return null;

            var recorderObj = new GameObject("__recorder",
                typeof(BaseInputRecorder));
            var recorder = recorderObj.GetComponent<BaseInputRecorder>();
            recorder.FrameDataRecorder = new DummyFrameDataRecorder();

            //再生に使用するデータの準備(空)
            var useRecord = InputRecord.Create();

            recorder.StartReplay(useRecord);
            Assert.AreEqual(BaseInputRecorder.State.Stop, recorder.CurrentState);
            Assert.IsTrue(recorder.IsStopping);
            Assert.IsFalse(recorder.IsReplaying);
            Assert.IsFalse(recorder.IsRecording);
            Assert.IsFalse(recorder.IsPausingReplay);
        }

    }
}
