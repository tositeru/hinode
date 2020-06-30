using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// AxisButtonFrameInputDataをInputRecorderMonoBehaviourに設定するためのComponent
    ///
    /// InputRecorderMonoBehaviour#UseRecorderがFrameInputDataの時のみ設定します。
    /// <seealso cref="InputRecorder"/>
    /// <seealso cref="AxisButtonFrameInputData"/>
    /// <seealso cref="InputRecorderMonoBehaviour"/>
    /// <seealso cref="IAppendFrameInputDataMonoBehaviour"/>
    /// </summary>
    public class AttachAxisButtonInputData : IAppendFrameInputDataMonoBehaviour
    {
        [SerializeField] string[] _enabledAxisButtons;

        AxisButtonFrameInputData CreateInputData()
        {
            var btn = new AxisButtonFrameInputData();
            btn.AddObservedButtonNames(_enabledAxisButtons);
            return btn;
        }

        #region override IAppendFrameInputDataMonoBehaviour
        protected override void OnAwake(InputRecorder inputRecorder)
        {
            if (inputRecorder.FrameDataRecorder is FrameInputData)
            {
                AxisButtonFrameInputData.RegistTypeToFrameInputData();

                var frameInputData = inputRecorder.FrameDataRecorder as FrameInputData;
                var touchInputData = CreateInputData();
                frameInputData.AddChildRecorder(touchInputData);
            }
        }
        #endregion
    }
}