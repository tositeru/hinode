using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// ButtonFrameInputDataをInputRecorderMonoBehaviourに設定するためのComponent
    ///
    /// InputRecorderMonoBehaviour#UseRecorderがFrameInputDataの時のみ設定します。
    /// <seealso cref="InputRecorder"/>
    /// <seealso cref="ButtonFrameInputData"/>
    /// <seealso cref="InputRecorderMonoBehaviour"/>
    /// <seealso cref="IAppendFrameInputDataMonoBehaviour"/>
    /// </summary>
    public class AttachButtonInputData : IAppendFrameInputDataMonoBehaviour
    {
        [SerializeField] string[] _enabledButtons;

        ButtonFrameInputData CreateInputData()
        {
            var btn = new ButtonFrameInputData();
            btn.AddObservedButtonNames(_enabledButtons);
            return btn;
        }

        #region override IAppendFrameInputDataMonoBehaviour
        protected override void OnAwake(InputRecorder inputRecorder)
        {
            if (inputRecorder.FrameDataRecorder is FrameInputData)
            {
                ButtonFrameInputData.RegistTypeToFrameInputData();

                var frameInputData = inputRecorder.FrameDataRecorder as FrameInputData;
                var touchInputData = CreateInputData();
                frameInputData.AddChildRecorder(touchInputData);
            }
        }
        #endregion
    }
}