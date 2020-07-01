using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public class AppendAxisButtonInputData : IAppendFrameInputDataMonoBehaviour
    {
        [SerializeField] List<string> _enabledAxisButtons = new List<string>();

        #region AxisButtons
        public IEnumerable<string> EnabledAxisButtons { get => _enabledAxisButtons; }

        public AppendAxisButtonInputData AddEnabledAxisButtons(params string[] names)
            => AddEnabledAxisButtons(names.AsEnumerable());
        public AppendAxisButtonInputData AddEnabledAxisButtons(IEnumerable<string> names)
        {
            var hash = new HashSet<string>(_enabledAxisButtons?.AsEnumerable() ?? null);
            foreach (var n in names.Where(_n => !hash.Contains(_n)))
            {
                hash.Add(n);
            }
            _enabledAxisButtons = hash.ToList();
            return this;
        }

        public AppendAxisButtonInputData RemoveEnabledAxisButtons(params string[] names)
            => RemoveEnabledAxisButtons(names.AsEnumerable());
        public AppendAxisButtonInputData RemoveEnabledAxisButtons(IEnumerable<string> names)
        {
            var hash = new HashSet<string>(_enabledAxisButtons.AsEnumerable());
            foreach (var n in names.Where(_n => hash.Contains(_n)))
            {
                hash.Remove(n);
            }
            _enabledAxisButtons = hash.ToList();
            return this;
        }

        public AppendAxisButtonInputData ClearEnabledAxisButton()
        {
            _enabledAxisButtons.Clear();
            return this;
        }
        #endregion

        #region override IAppendFrameInputDataMonoBehaviour
        public override IFrameDataRecorder CreateInputData()
        {
            var btn = new AxisButtonFrameInputData();
            btn.AddObservedButtonNames(EnabledAxisButtons);
            return btn;
        }

        protected override void OnAttached(InputRecorder inputRecorder)
        {
            if (inputRecorder.FrameDataRecorder is FrameInputData)
            {
                AxisButtonFrameInputData.RegistTypeToFrameInputData();
                var frameInputData = inputRecorder.FrameDataRecorder as FrameInputData;
                frameInputData.RemoveChildRecorder(AxisButtonFrameInputData.KEY_CHILD_INPUT_DATA_TYPE);

                var touchInputData = CreateInputData();
                frameInputData.AddChildRecorder(touchInputData);

            }
        }
        #endregion
    }
}