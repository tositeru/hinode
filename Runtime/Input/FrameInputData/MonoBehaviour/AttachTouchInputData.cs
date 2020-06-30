using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// TouchFrameInputDataをInputRecorderMonoBehaviourに設定するためのComponent
    ///
    /// InputRecorderMonoBehaviour#UseRecorderがFrameInputDataの時のみ設定します。
    /// <seealso cref="InputRecorder"/>
    /// <seealso cref="TouchFrameInputData"/>
    /// <seealso cref="InputRecorderMonoBehaviour"/>
    /// <seealso cref="IAppendFrameInputDataMonoBehaviour"/>
    /// </summary>
    public class AttachTouchInputData : IAppendFrameInputDataMonoBehaviour
    {
        #region override IAppendFrameInputDataMonoBehaviour
        protected override void OnAwake(InputRecorder inputRecorder)
        {
            if(inputRecorder.FrameDataRecorder is FrameInputData)
            {
                var frameInputData = inputRecorder.FrameDataRecorder as FrameInputData;
                var touchInputData = new TouchFrameInputData();
                frameInputData.AddChildRecorder(touchInputData);
            }
        }
        #endregion
    }
}
