using System.Collections;
using System.Collections.Generic;
using Hinode.Serialization;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// 入力データを保存することができるComponent
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
    /// <seealso cref="EventSystemFrameInputData"/>
    /// </summary>
    public class InputRecorder : MonoBehaviour
    {
        public enum State
        {
            Stop,
            Recording,
            Replaying,
            PauseingReplay
        }

        State _state;
        /// <summary>
        /// 計算誤差であまり信頼できない値になるかもしれないので注意してください。
        /// </summary>
        float _deltaTime;
        int _currentFrameNo = -1;
        Vector2Int _recordedScreenSize;
        IFrameDataRecorder _frameDataRecorder = new FrameInputData();
        List<InputRecord.Frame> _recordFrames = new List<InputRecord.Frame>();
        Coroutine _updateCoroutine;
        JsonSerializer _jsonSerializer = new JsonSerializer();
        [SerializeField] InputRecord _target;

        public State CurrentState
        {
            get => _state;
            set
            {
                _state = value;
            }
        }

        public bool IsReplaying { get => _state == State.Replaying || _state == State.PauseingReplay; }
        public bool IsStopping { get => _state == State.Stop; }
        public bool IsRecording { get => _state == State.Recording; }
        public bool IsPausingReplay { get => _state == State.PauseingReplay; }

        public int CurrentFrameNo { get => _currentFrameNo; }

        public IFrameDataRecorder FrameDataRecorder
        {
            get => _frameDataRecorder;
            set
            {
                if (IsRecording)
                {
                    throw new System.InvalidOperationException($"Must stop recording when FrameDataRecorder change...");
                }
                Assert.IsNotNull(value);
                _frameDataRecorder = value;
            }
        }

        public InputRecord Target { get => _target; private set => _target = value; }

        private void LateUpdate()
        {
            switch (CurrentState)
            {
                case State.Recording:
                    UpdateRecording();
                    break;
            }
        }

        #region 記録関係
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
            _currentFrameNo = 0;
            _recordedScreenSize.x = -1;
            _recordedScreenSize.y = -1;
            _frameDataRecorder.ResetDatas();
            _recordFrames.Clear();
        }

        /// <summary>
        /// 記録を停止する
        /// </summary>
        public void StopRecord()
        {
            Assert.IsTrue(IsRecording);
            CurrentState = State.Stop;
        }

        /// <summary>
        /// 記録した内容を保存する
        /// </summary>
        /// <param name="inputRecord">nullの場合はTargetに保存します</param>
        public void SaveToTarget(InputRecord inputRecord = null)
        {
            Assert.IsFalse(IsRecording, "Don't save when recording...");

            if (inputRecord == null) inputRecord = Target;
            Assert.IsNotNull(inputRecord, "The Saving target must be not null... ");

            _target.ScreenSize = _recordedScreenSize;
            _target.ClearFrames();
            foreach (var f in _recordFrames)
            {
                _target.Push(f);
            }
        }

        void UpdateRecording()
        {
            if (_recordedScreenSize.x == -1 && _recordedScreenSize.y == -1)
            {//Editor拡張でScreen.width|heightを使うと、Inspectorのサイズを取得してしまうので、ここで画面サイズを設定している
                _recordedScreenSize.x = Screen.width;
                _recordedScreenSize.y = Screen.height;
            }
            else if (_recordedScreenSize.x != Screen.width || _recordedScreenSize.y != Screen.height)
            {
                Debug.LogWarning($"記録中に画面サイズが変更することに対応していません。これまでの記録したものを保存し記録を中止します。({_recordedScreenSize.x}, {_recordedScreenSize.y}) => ({Screen.width}, {Screen.height})");
                StopRecord();
                return;
            }

            _deltaTime += Time.deltaTime;

            _frameDataRecorder.RefleshUpdatedFlags();
            _frameDataRecorder.Record(ReplayableInput.Instance);
            var frame = _frameDataRecorder.WriteToFrame(_jsonSerializer);
            if (!frame.IsEmptyInputText)
            {
                frame.FrameNo = (uint)_currentFrameNo;
                frame.DeltaSecond = _deltaTime;
                _recordFrames.Add(frame);

                _deltaTime = 0f;
            }
            _currentFrameNo++;
        }
        #endregion

        #region 記録の再生関係
        /// <summary>
        /// この関数を呼び出してから次のフレームから入力データが反映されます。
        /// また、最終フレームに到達後、停止状態になるまで一フレームかかります。
        /// </summary>
        /// <param name="newTarget">nullの場合は既に設定されているものを使用します。</param>
        public void StartReplay(InputRecord newTarget = null)
        {
            Assert.IsTrue(IsStopping || IsPausingReplay);
            if (newTarget != null) Target = newTarget;

            Assert.IsNotNull(Target);

            switch (CurrentState)
            {
                case State.Stop:
                    _updateCoroutine = StartCoroutine(UpdateReplay());
                    break;
                case State.PauseingReplay:
                    CurrentState = State.Replaying;
                    break;
                default:
                    throw new System.NotImplementedException();
            }

            //TODO EventSystemは複数存在する可能性があるので、その全てに反映する方法
        }

        public void StopReplay()
        {
            Assert.IsTrue(IsReplaying || IsPausingReplay);
            CurrentState = State.Stop;
            if (_updateCoroutine != null) StopCoroutine(_updateCoroutine);
            _updateCoroutine = null;
        }

        public void PauseReplay()
        {
            Assert.IsTrue(IsReplaying);
            CurrentState = State.PauseingReplay;
        }

        public void NextReplayFrame()
        {
            Assert.IsTrue(IsPausingReplay);
        }

        IEnumerator UpdateReplay()
        {
            CurrentState = State.Replaying;
            _currentFrameNo = -1;
            var deltaTime = 0f;
            var replayingFrameEnumerator = Target.Frames.GetEnumerator();
            if (!replayingFrameEnumerator.MoveNext())
            {
                StopReplay();
                Debug.LogWarning($"空データを再生しようとしました。再生を停止します。");
                yield break;
            }

            while (replayingFrameEnumerator != null)
            {
                if (Target.ScreenSize.x != Screen.width || Target.ScreenSize.y != Screen.height)
                {//Editor拡張でScreen.width|heightを使うと、Inspectorのサイズを取得してしまうので、ここで画面サイズを設定している
                    Debug.LogWarning($"再生中に画面サイズが変更することに対応していません。再生を中断します。");
                    break;
                }

                yield return new WaitForEndOfFrame();

                if (CurrentState != State.Replaying)
                {
                    continue;
                }

                _currentFrameNo++;
                deltaTime += Time.deltaTime;

                var curFrame = replayingFrameEnumerator.Current;
                //Debug.Log($"debug -- frameNo={curFrame.FrameNo}, curFrameIndex={_currentFrameNo}");
                if (curFrame.FrameNo == _currentFrameNo)
                {
                    _frameDataRecorder.RecoverFromFrame(curFrame, _jsonSerializer);
                    _frameDataRecorder.RecoverTo(ReplayableInput.Instance);

                    if (!replayingFrameEnumerator.MoveNext())
                    {
                        replayingFrameEnumerator = null;
                    }
                    deltaTime = 0f;
                }
            }

            //処理の流れの都合上、最終フレームまで到達した時、フレームを待つ処理がループにはないのでここで待つ
            yield return new WaitForEndOfFrame();

            if (replayingFrameEnumerator == null)
            {//最後まで再生されていたら、現在のフレームを最終フレームに合わせる
                _currentFrameNo = Target.FrameCount;
            }
            StopReplay();
        }

        #endregion
    }
}
