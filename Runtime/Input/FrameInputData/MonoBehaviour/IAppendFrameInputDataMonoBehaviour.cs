using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// InputRecorderMonoBehaviourに任意のIFrameInputDateRecorderを追加するためのComponent
    ///
    /// <seealso cref="InputRecorderMonoBehaviour"/>
    /// <seealso cref="IFrameDataRecorder"/>
    /// <seealso cref="AttachTouchInputData"/>
    /// </summary>
    public abstract class IAppendFrameInputDataMonoBehaviour : MonoBehaviour
    {
        protected abstract void OnAwake(InputRecorder inputRecorder);

        private void Awake()
        {
            if(TryGetComponent<InputRecorderMonoBehaviour>(out var inputRecorder))
            {
                Assert.IsNotNull(inputRecorder.UseRecorder);
                OnAwake(inputRecorder.UseRecorder);
            }
            else
            {
                Assert.IsTrue(false, "Require to Attach InputRecorderMonoBehaviour...");
            }
        }
    }
}
