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
    public class AppendMouseInputData : IAppendFrameInputDataMonoBehaviour
    {
        #region override IAppendFrameInputDataMonoBehaviour
        public override IFrameDataRecorder CreateInputData()
        {
            var btn = new MouseFrameInputData();
            return btn;
        }

        protected override void OnAttached(InputRecorder inputRecorder)
        {
            if (inputRecorder.FrameDataRecorder is FrameInputData)
            {
                MouseFrameInputData.RegistTypeToFrameInputData();

                var frameInputData = inputRecorder.FrameDataRecorder as FrameInputData;
                frameInputData.RemoveChildRecorder(MouseFrameInputData.KEY_CHILD_INPUT_DATA_TYPE);

                var inputData = CreateInputData();
                frameInputData.AddChildRecorder(inputData);
            }
        }
        #endregion
    }
}