using System.Collections;
using System.Collections.Generic;
using Hinode.Serialization;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// 入力データを保存することができるClass
    /// 
    /// 入力データはInputRecordクラスへ保存されます
    /// 
    /// IFrameDataRecorderを使用して1フレームのデータを記録しています。
    ///
    /// 記録するデータを変更したい場合は記録を停止した状態でFrameDataRecorderを変更してください。
    /// 
    /// 入力データを記録する時の処理の流れとしては以下のものを想定しています。
    /// 
    /// 1. InputRecorder#StartRecord()で記録を開始。開始した時、以前の入力データの記録は破棄されます。
    /// 1. InputRecorder#StopRecord()で記録を停止。
    /// 1. InputRecorder#SaveToTarget()を使用し、記録しているデータをInputRecordへ保存。
    /// 1. InputRecorder#IsRecordingで記録中っかどうか判定できます。
    ///
    /// 注意点として、InputRecorder#SaveToTarget()を使用する際は必ず、記録を停止するようにしてください。
    /// 
    /// 記録された入力データを再生する際の処理の流れは以下のものを想定しています。
    /// 
    /// 1. 記録中の時は、記録を停止してください。
    /// 1. InputRecorder#Replay()で記録の再生を開始します。
    /// 1. InputRecorder#StopReplay()で再生を停止します。
    /// 1. InputRecorder#PauseReplay()で一時停止できます。
    /// 1. InputRecorder#IsReplayingで再生中かどうか判定できます。
    /// 1. InputRecorder#IsPauseReplayで再生の一時停止かどうか判定できます。
    /// 
    /// <seealso cref="InputRecord"/>
    /// </summary>
    public class InputRecorder
    {
        public enum State
        {
            Stop,
            Recording,
            Replaying,
            PauseingReplay
        }

        /// <summary>
        /// 計算誤差であまり信頼できない値になるかもしれないので注意してください。
        /// </summary>
        float _deltaTime;
        Vector2Int _recordedScreenSize;
        IFrameDataRecorder _frameDataRecorder = new FrameInputData();
        List<InputRecord.Frame> _recordFrames = new List<InputRecord.Frame>();
        IEnumerator _enumerator;
        ReplayableInput _useInput;

        public ISerializer UseSerializer { get; set; } = new JsonSerializer();
        public ReplayableInput UseInput { get => _useInput ?? ReplayableInput.Instance; set => _useInput = value; }

        public State CurrentState { get; set; }

        public bool IsReplaying { get => CurrentState == State.Replaying || CurrentState == State.PauseingReplay; }

        public int CurrentFrameNo { get; private set; } = -1;

        public IFrameDataRecorder FrameDataRecorder
        {
            get => _frameDataRecorder;
            set
            {
                if (CurrentState == State.Recording ||
                    CurrentState == State.Replaying)
                {
                    throw new System.InvalidOperationException($"Must stop recording when FrameDataRecorder change...");
                }
                Assert.IsNotNull(value);
                _frameDataRecorder = value;
            }
        }

        public InputRecord Target { get; private set; }

        public void StepFrame()
        {
            Assert.IsNotNull(_enumerator);

            if (CurrentState == State.PauseingReplay) return;

            var isContinue = _enumerator.MoveNext();

            if(!isContinue)
            {
                _enumerator = null;
            }
        }

        #region Record
        /// <summary>
        /// 記録を開始する
        /// </summary>
        /// <param name="newTarget"></param>
        public void StartRecord(InputRecord newTarget = null)
        {
            if (newTarget != null)
            {
                Target = newTarget;
            }

            CurrentState = State.Recording;
            CurrentFrameNo = 0;
            _recordedScreenSize.x = -1;
            _recordedScreenSize.y = -1;
            _frameDataRecorder.ResetDatas();
            _recordFrames.Clear();

            _enumerator = GetRecordEnumerator();
            _enumerator.MoveNext();
        }

        /// <summary>
        /// 記録を停止する
        /// </summary>
        public void StopRecord()
        {
            Assert.IsTrue(CurrentState == State.Recording);
            CurrentState = State.Stop;
        }

        /// <summary>
        /// 記録した内容を保存する
        /// </summary>
        /// <param name="inputRecord">nullの場合はTargetに保存します</param>
        public void SaveToTarget(InputRecord inputRecord = null)
        {
            Assert.IsFalse(CurrentState == State.Recording, "Don't save when recording...");

            if (inputRecord == null) inputRecord = Target;
            Assert.IsNotNull(inputRecord, "The Saving target must be not null... ");

            Target.ScreenSize = _recordedScreenSize;
            Target.ClearFrames();
            foreach (var f in _recordFrames)
            {
                Target.Push(f);
            }
        }

        IEnumerator GetRecordEnumerator()
        {
            if (_recordedScreenSize.x == -1 && _recordedScreenSize.y == -1)
            {//Editor拡張でScreen.width|heightを使うと、Inspectorのサイズを取得してしまうので、ここで画面サイズを設定している
                _recordedScreenSize.x = Screen.width;
                _recordedScreenSize.y = Screen.height;
            }
            else if (_recordedScreenSize.x != Screen.width || _recordedScreenSize.y != Screen.height)
            {
                Logger.LogWarning(Logger.Priority.High, () =>
                    $"記録中に画面サイズが変更することに対応していません。これまでの記録したものを保存し記録を中止します。({_recordedScreenSize.x}, {_recordedScreenSize.y}) => ({Screen.width}, {Screen.height})",
                    InputLoggerDefines.SELECTOR_MAIN, InputLoggerDefines.SELECTOR_RECORDER);
                StopRecord();
                yield break;
            }
            yield return null; // finish initialize

            while (true)
            {
                _deltaTime += Time.deltaTime;

                _frameDataRecorder.RefleshUpdatedFlags();
                _frameDataRecorder.Record(UseInput);
                var frame = _frameDataRecorder.WriteToFrame(UseSerializer);
                if (!frame.IsEmptyInputText)
                {
                    frame.FrameNo = (uint)CurrentFrameNo;
                    frame.DeltaSecond = _deltaTime;
                    _recordFrames.Add(frame);

                    _deltaTime = 0f;
                }
                CurrentFrameNo++;
                yield return null;
            }
        }
        #endregion

        #region Replay
        /// <summary>
        /// この関数を呼び出してから次のフレームから入力データが反映されます。
        /// また、最終フレームに到達後、停止状態になるまで一フレームかかります。
        /// </summary>
        /// <param name="newTarget">nullの場合は既に設定されているものを使用します。</param>
        public void StartReplay(InputRecord newTarget = null)
        {
            Assert.IsTrue(CurrentState == State.Stop || CurrentState == State.PauseingReplay);
            if (newTarget != null) Target = newTarget;

            Assert.IsNotNull(Target);

            switch (CurrentState)
            {
                case State.Stop:
                    _enumerator = GetReplayEnumerator();
                    CurrentState = State.Replaying;

                    _enumerator.MoveNext();
                    break;
                case State.PauseingReplay:
                    CurrentState = State.Replaying;
                    break;
                default:
                    throw new System.NotImplementedException();
            }
        }

        public void StopReplay()
        {
            Assert.IsTrue(CurrentState == State.Replaying || CurrentState == State.PauseingReplay);
            CurrentState = State.Stop;

            _enumerator = null;
        }

        public void PauseReplay()
        {
            Assert.IsTrue(CurrentState == State.Replaying);
            CurrentState = State.PauseingReplay;
        }

        IEnumerator GetReplayEnumerator()
        {
            CurrentState = State.Replaying;
            CurrentFrameNo = -1;
            var deltaTime = 0f;
            var replayingFrameEnumerator = Target.Frames.GetEnumerator();
            if (!replayingFrameEnumerator.MoveNext())
            {
                StopReplay();
                Logger.LogWarning(Logger.Priority.High, () => $"空データを再生しようとしました。再生を停止します。",
                    InputLoggerDefines.SELECTOR_MAIN, InputLoggerDefines.SELECTOR_RECORDER);
                yield break;
            }

            while (replayingFrameEnumerator != null)
            {
                if (Target.ScreenSize.x != Screen.width || Target.ScreenSize.y != Screen.height)
                {//Editor拡張でScreen.width|heightを使うと、Inspectorのサイズを取得してしまうので、ここで画面サイズを設定している
                    Logger.LogWarning(Logger.Priority.High, () => $"再生中に画面サイズが変更することに対応していません。再生を中断します。",
                        InputLoggerDefines.SELECTOR_MAIN, InputLoggerDefines.SELECTOR_RECORDER);
                    break;
                }

                yield return null;

                if (CurrentState != State.Replaying)
                {
                    continue;
                }

                CurrentFrameNo++;
                deltaTime += Time.deltaTime;

                var curFrame = replayingFrameEnumerator.Current;
                //Debug.Log($"debug -- frameNo={curFrame.FrameNo}, curFrameIndex={_currentFrameNo}");
                if (curFrame.FrameNo == CurrentFrameNo)
                {
                    _frameDataRecorder.RecoverFromFrame(curFrame, UseSerializer);
                    _frameDataRecorder.RecoverTo(UseInput);

                    if (!replayingFrameEnumerator.MoveNext())
                    {
                        replayingFrameEnumerator = null;
                    }
                    deltaTime = 0f;
                }
            }

            //処理の流れの都合上、最終フレームまで到達した時、フレームを待つ処理がループにはないのでここで待つ
            yield return null;

            if (replayingFrameEnumerator == null)
            {//最後まで再生されていたら、現在のフレームを最終フレームに合わせる
                CurrentFrameNo = Target.FrameCount;
            }
            StopReplay();
        }

        #endregion
    }
}
