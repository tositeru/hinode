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
    public class AppendTouchInputData : IAppendFrameInputDataMonoBehaviour
    {
        #region override IAppendFrameInputDataMonoBehaviour
        public override IFrameDataRecorder CreateInputData()
        {
            var btn = new TouchFrameInputData();
            return btn;
        }

        protected override void OnAttached(InputRecorder inputRecorder)
        {
            if (inputRecorder.FrameDataRecorder is FrameInputData)
            {
                TouchFrameInputData.RegistTypeToFrameInputData();

                var frameInputData = inputRecorder.FrameDataRecorder as FrameInputData;
                frameInputData.RemoveChildRecorder(TouchFrameInputData.KEY_CHILD_INPUT_DATA_TYPE);

                var touchInputData = CreateInputData();
                frameInputData.AddChildRecorder(touchInputData);
            }
        }
        #endregion
    }
}
