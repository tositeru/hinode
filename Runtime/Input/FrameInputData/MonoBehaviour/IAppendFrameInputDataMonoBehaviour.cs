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
        public abstract IFrameDataRecorder CreateInputData();
        protected abstract void OnAttached(InputRecorder inputRecorder);

        public void Attach()
        {
            if (TryGetComponent<InputRecorderMonoBehaviour>(out var inputRecorder))
            {
                Assert.IsNotNull(inputRecorder.UseRecorder);

                OnAttached(inputRecorder.UseRecorder);
            }
        }

        private void Start()
        {
            Attach();
        }
    }
}
