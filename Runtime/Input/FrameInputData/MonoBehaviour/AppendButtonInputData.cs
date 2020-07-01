using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    public class AppendButtonInputData : IAppendFrameInputDataMonoBehaviour
    {
        [SerializeField] List<string> _enabledButtons = new List<string>();

        #region Buttons
        public IEnumerable<string> EnabledButtons { get => _enabledButtons; }

        public AppendButtonInputData AddEnabledButtons(params string[] names)
            => AddEnabledButtons(names.AsEnumerable());
        public AppendButtonInputData AddEnabledButtons(IEnumerable<string> names)
        {
            var hash = new HashSet<string>(_enabledButtons.AsEnumerable());
            foreach (var n in names.Where(_n => !hash.Contains(_n)))
            {
                hash.Add(n);
            }
            _enabledButtons = hash.ToList();
            return this;
        }

        public AppendButtonInputData RemoveEnabledButtons(params string[] names)
            => RemoveEnabledButtons(names.AsEnumerable());
        public AppendButtonInputData RemoveEnabledButtons(IEnumerable<string> names)
        {
            var hash = new HashSet<string>(_enabledButtons.AsEnumerable());
            foreach (var n in names.Where(_n => hash.Contains(_n)))
            {
                hash.Remove(n);
            }
            _enabledButtons = hash.ToList();
            return this;
        }

        public AppendButtonInputData ClearEnabledButton()
        {
            _enabledButtons.Clear();
            return this;
        }
        #endregion

        #region override IAppendFrameInputDataMonoBehaviour
        public override IFrameDataRecorder CreateInputData()
        {
            var btn = new ButtonFrameInputData();
            btn.AddObservedButtonNames(_enabledButtons);
            return btn;
        }

        protected override void OnAttached(InputRecorder inputRecorder)
        {
            if (inputRecorder.FrameDataRecorder is FrameInputData)
            {
                ButtonFrameInputData.RegistTypeToFrameInputData();

                var frameInputData = inputRecorder.FrameDataRecorder as FrameInputData;
                frameInputData.RemoveChildRecorder(ButtonFrameInputData.KEY_CHILD_INPUT_DATA_TYPE);

                var touchInputData = CreateInputData();
                frameInputData.AddChildRecorder(touchInputData);
            }
        }
        #endregion
    }
}