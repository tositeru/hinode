using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// MouseFrameInputDataをInputRecorderMonoBehaviourに設定するためのComponent
    ///
    /// InputRecorderMonoBehaviour#UseRecorderがFrameInputDataの時のみ設定します。
    /// <seealso cref="InputRecorder"/>
    /// <seealso cref="MouseFrameInputData"/>
    /// <seealso cref="InputRecorderMonoBehaviour"/>
    /// <seealso cref="IAppendFrameInputDataMonoBehaviour"/>
    /// </summary>
    public class AttachMouseInputData : IAppendFrameInputDataMonoBehaviour
    {
        #region override IAppendFrameInputDataMonoBehaviour
        protected override void OnAwake(InputRecorder inputRecorder)
        {
            if (inputRecorder.FrameDataRecorder is FrameInputData)
            {
                MouseFrameInputData.RegistTypeToFrameInputData();

                var frameInputData = inputRecorder.FrameDataRecorder as FrameInputData;
                var touchInputData = new MouseFrameInputData();
                frameInputData.AddChildRecorder(touchInputData);
            }
        }
        #endregion
    }
}