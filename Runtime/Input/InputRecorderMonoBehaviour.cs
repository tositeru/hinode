using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// InputRecorderをラップしたComponent
    /// 
    /// 入力データはInputRecorderMonoBehaviour#TargetRecordへ保存または再生されます
    /// 
    /// 記録するデータを変更したい場合は記録を停止した状態でInputRecorderMonoBehaviour#TargetRecordを変更してください。
    ///
    /// データの記録・再生の更新タイミングはUnityEngine#WaitForEndOfFrameのタイミングで行われます。
    /// 
    /// 入力データを記録する時の処理の流れとしては以下のものを想定しています。
    /// 
    /// 1. InputRecorderMonoBehaviour#StartRecord()で記録を開始。開始した時、以前の入力データの記録は破棄されます。
    /// 1. InputRecorderMonoBehaviour#StopRecord()で記録を停止。
    /// 1. InputRecorderMonoBehaviour#SaveToTarget()を使用し、記録しているデータをInputRecordへ保存。
    ///
    /// 注意点として、InputRecorderMonoBehaviour#SaveToTarget()を使用する際は必ず、記録を停止するようにしてください。
    /// 
    /// 記録された入力データを再生する際の処理の流れは以下のものを想定しています。
    /// 
    /// 1. 記録中の時は、記録を停止してください。
    /// 1. InputRecorderMonoBehaviour#Replay()で記録の再生を開始します。
    /// 1. InputRecorderMonoBehaviour#StopReplay()で再生を停止します。
    /// 1. InputRecorderMonoBehaviour#PauseReplay()で一時停止できます。
    /// 
    /// <seealso cref="InputRecord"/>
    /// <seealso cref="InputRecorder"/>
    /// </summary>
    public class InputRecorderMonoBehaviour : MonoBehaviour
    {
        [System.Flags]
        public enum UseFrameInputData
        {
            Mouse,
            Touch,
            Keyboard,
            Button,
            AxisButton,
        }

        [SerializeField] InputRecord _targetRecord;

        Coroutine _recorderLoopCoroutine;

        public InputRecorder UseRecorder { get; } = new InputRecorder();

        public bool IsValid { get => UseRecorder != null && TargetRecord != null; }
        public InputRecord TargetRecord { get => _targetRecord; set => _targetRecord = value; }
        public InputRecorder.State CurrentState { get => UseRecorder.CurrentState; }

        void StartRecorderLoop()
        {
            if (_recorderLoopCoroutine != null)
                StopCoroutine(_recorderLoopCoroutine);
            _recorderLoopCoroutine = StartCoroutine(RecorderLoop());
        }

        IEnumerator RecorderLoop()
        {
            while(UseRecorder.CurrentState != InputRecorder.State.Stop)
            {
                yield return new WaitForEndOfFrame();
                UseRecorder.StepFrame();
            }
            _recorderLoopCoroutine = null;
        }

        #region Record
        public void StartRecord()
        {
            Assert.IsTrue(IsValid);
            UseRecorder.StartRecord(TargetRecord);

            StartRecorderLoop();
        }

        public void StopRecord()
        {
            Assert.IsTrue(IsValid);
            UseRecorder.StopRecord();
        }

        public void SaveToTarget(InputRecord target = null)
        {
            Assert.IsTrue(IsValid);
            UseRecorder.SaveToTarget(target);
        }
        #endregion

        #region Replay
        public void StartReplay()
        {
            Assert.IsTrue(IsValid);
            UseRecorder.StartReplay(TargetRecord);

            StartRecorderLoop();
        }

        public void StopReplay()
        {
            Assert.IsTrue(IsValid);
            UseRecorder.StopReplay();
        }

        public void PauseReplay()
        {
            Assert.IsTrue(IsValid);
            UseRecorder.PauseReplay();
        }
        #endregion
    }
}
