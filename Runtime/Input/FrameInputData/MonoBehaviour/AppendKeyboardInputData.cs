using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    public class AppendKeyboardInputData : IAppendFrameInputDataMonoBehaviour
    {
        [SerializeField] bool _enableArrows = false;
        [SerializeField] bool _enableAlphabets = false;
        [SerializeField] bool _enableNumber = false;
        [SerializeField] bool _enableSymbol = false;
        [SerializeField] bool _enableSystem = false;
        [SerializeField] bool _enableFunction = false;
        [SerializeField] bool _enableJoystick = false;
        [SerializeField] bool _enableMouse = false;
        [SerializeField] bool _enableOther = false;
        [SerializeField] List<KeyCode> _enabledKeyCodes = new List<KeyCode>();

        public bool EnabledArrows { get => _enableArrows; set => _enableArrows = value; }
        public bool EnableAlphabets { get => _enableAlphabets; set => _enableAlphabets = value; }
        public bool EnableNumber { get => _enableNumber; set => _enableNumber = value; }
        public bool EnableSymbol { get => _enableSymbol; set => _enableSymbol = value; }
        public bool EnableSystem { get => _enableSystem; set => _enableSystem = value; }
        public bool EnableFunction { get => _enableFunction; set => _enableFunction = value; }
        public bool EnableJoystick { get => _enableJoystick; set => _enableJoystick = value; }
        public bool EnableMouse { get => _enableMouse; set => _enableMouse = value; }
        public bool EnableOther { get => _enableOther; set => _enableOther = value; }

        #region KeyCodes
        public IEnumerable<KeyCode> EnabledKeyCodes { get => _enabledKeyCodes; }

        public AppendKeyboardInputData AddEnabledKeyCodes(params KeyCode[] keyCodes)
            => AddEnabledKeyCodes(keyCodes.AsEnumerable());
        public AppendKeyboardInputData AddEnabledKeyCodes(IEnumerable<KeyCode> keyCodes)
        {
            var hash = new HashSet<KeyCode>(_enabledKeyCodes?.AsEnumerable() ?? null);
            foreach (var n in keyCodes.Where(_n => !hash.Contains(_n)))
            {
                hash.Add(n);
            }
            _enabledKeyCodes = hash.ToList();
            return this;
        }

        public AppendKeyboardInputData RemoveEnabledKeyCodes(params KeyCode[] keyCodes)
            => RemoveEnabledKeyCodes(keyCodes.AsEnumerable());
        public AppendKeyboardInputData RemoveEnabledKeyCodes(IEnumerable<KeyCode> keyCodes)
        {
            var hash = new HashSet<KeyCode>(_enabledKeyCodes.AsEnumerable());
            foreach (var n in keyCodes.Where(_n => hash.Contains(_n)))
            {
                hash.Remove(n);
            }
            _enabledKeyCodes = hash.ToList();
            return this;
        }

        public AppendKeyboardInputData ClearEnabledKeyCodes()
        {
            _enabledKeyCodes.Clear();
            return this;
        }
        #endregion

        #region override IAppendFrameInputDataMonoBehaviour
        public override IFrameDataRecorder CreateInputData()
        {
            var child = new KeyboardFrameInputData();
            if (_enableArrows) child.AddArrowKeyCode();
            if (_enableAlphabets) child.AddAlphabetKeyCode();
            if (_enableNumber) child.AddKeypadKeyCode();
            if (_enableSymbol) child.AddSymbolKeyCode();
            if (_enableSystem) child.AddSystemKeyCode();
            if (_enableFunction) child.AddFunctionKeyCode();
            if (_enableJoystick) child.AddJoyStickKeyCode();
            if (_enableMouse) child.AddMouseKeyCode();
            if (_enableOther) child.AddOtherKeyCode();
            child.AddEnabledKeyCode(_enabledKeyCodes);
            return child;
        }

        protected override void OnAttached(InputRecorder inputRecorder)
        {
            if (inputRecorder.FrameDataRecorder is FrameInputData)
            {
                KeyboardFrameInputData.RegistTypeToFrameInputData();

                var frameInputData = inputRecorder.FrameDataRecorder as FrameInputData;
                frameInputData.RemoveChildRecorder(KeyboardFrameInputData.KEY_CHILD_INPUT_DATA_TYPE);

                var inputData = CreateInputData();
                frameInputData.AddChildRecorder(inputData);
            }
        }
        #endregion
    }
}