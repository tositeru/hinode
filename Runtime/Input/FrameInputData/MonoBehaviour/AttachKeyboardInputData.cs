using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// KeyboardFrameInputDataをInputRecorderMonoBehaviourに設定するためのComponent
    ///
    /// InputRecorderMonoBehaviour#UseRecorderがFrameInputDataの時のみ設定します。
    /// <seealso cref="InputRecorder"/>
    /// <seealso cref="KeyboardFrameInputData"/>
    /// <seealso cref="InputRecorderMonoBehaviour"/>
    /// <seealso cref="IAppendFrameInputDataMonoBehaviour"/>
    /// </summary>
    public class AttachKeyboardInputData : IAppendFrameInputDataMonoBehaviour
    {
        [SerializeField] bool _EnableArrows = true;
        [SerializeField] bool _EnableAlphabets = true;
        [SerializeField] bool _EnableNumber = true;
        [SerializeField] bool _EnableSymbol = true;
        [SerializeField] bool _EnableSystem = true;
        [SerializeField] bool _EnableFunction = true;
        [SerializeField] bool _EnableJoystick = true;
        [SerializeField] bool _EnableMouse = true;
        [SerializeField] bool _EnableOther = true;
        [SerializeField] KeyCode[] _enabledKeyCodes;

        KeyboardFrameInputData CreateKeyboardInputData()
        {
            var child = new KeyboardFrameInputData();
            if (_EnableArrows) child.AddArrowKeyCode();
            if (_EnableAlphabets) child.AddAlphabetKeyCode();
            if (_EnableNumber) child.AddKeypadKeyCode();
            if (_EnableSymbol) child.AddSymbolKeyCode();
            if (_EnableSystem) child.AddSystemKeyCode();
            if (_EnableFunction) child.AddFunctionKeyCode();
            if (_EnableJoystick) child.AddJoyStickKeyCode();
            if (_EnableMouse) child.AddMouseKeyCode();
            if (_EnableOther) child.AddOtherKeyCode();
            child.AddEnabledKeyCode(_enabledKeyCodes);
            return child;
        }

        #region override IAppendFrameInputDataMonoBehaviour
        protected override void OnAwake(InputRecorder inputRecorder)
        {
            if (inputRecorder.FrameDataRecorder is FrameInputData)
            {
                KeyboardFrameInputData.RegistTypeToFrameInputData();

                var frameInputData = inputRecorder.FrameDataRecorder as FrameInputData;
                var child = CreateKeyboardInputData();
                frameInputData.AddChildRecorder(child);
            }
        }
        #endregion
    }
}