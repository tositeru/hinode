using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
	/// <seealso cref="InputViewer"/>
	/// </summary>
    [System.Serializable]
    public class InputViewerStyleInfo
    {
        public delegate void OnChangedDelegate(InputViewerStyleInfo styleInfo);

        SmartDelegate<OnChangedDelegate> _onChanged = new SmartDelegate<OnChangedDelegate>();

        [SerializeField] Font _font;
        [SerializeField] Color _fontColor = Color.white;

        [SerializeField] Color _buttonColorAtFree = Color.gray;
        [SerializeField] Color _buttonColorAtDown = Color.white;
        [SerializeField] Color _buttonColorAtPush = Color.white;
        [SerializeField] Color _buttonColorAtUp = Color.white;

        public NotInvokableDelegate<OnChangedDelegate> OnChanged { get => _onChanged; }

        public Font Font
        {
            get => _font;
            set
            {
                if (_font == value) return;
                _font = value;
                if(_font == null)
                {
                    _font = Resources.Load<Font>("Arial");
                }
                _onChanged.SafeDynamicInvoke(this, () => $"Font", InputLoggerDefines.SELECTOR_MAIN);
            }
        }

        public Color FontColor
        {
            get => _fontColor;
            set
            {
                if (_fontColor == value) return;
                _fontColor = value;
                _onChanged.SafeDynamicInvoke(this, () => $"Font", InputLoggerDefines.SELECTOR_MAIN);
            }
        }

        public Color ButtonColorAtFree
        {
            get => _buttonColorAtFree;
            set
            {
                if (_buttonColorAtFree == value) return;
                _buttonColorAtFree = value;
                _onChanged.SafeDynamicInvoke(this, () => $"Font", InputLoggerDefines.SELECTOR_MAIN);
            }
        }

        public Color ButtonColorAtDown
        {
            get => _buttonColorAtDown;
            set
            {
                if (_buttonColorAtDown == value) return;
                _buttonColorAtDown = value;
                _onChanged.SafeDynamicInvoke(this, () => $"Font", InputLoggerDefines.SELECTOR_MAIN);
            }
        }

        public Color ButtonColorAtPush
        {
            get => _buttonColorAtPush;
            set
            {
                if (_buttonColorAtPush == value) return;
                _buttonColorAtFree = value;
                _onChanged.SafeDynamicInvoke(this, () => $"Font", InputLoggerDefines.SELECTOR_MAIN);
            }
        }

        public Color ButtonColorAtUp
        {
            get => _buttonColorAtUp;
            set
            {
                if (_buttonColorAtUp == value) return;
                _buttonColorAtUp = value;
                _onChanged.SafeDynamicInvoke(this, () => $"Font", InputLoggerDefines.SELECTOR_MAIN);
            }
        }

        public Color GetButtonCondition(InputDefines.ButtonCondition condition)
        {
            switch(condition)
            {
                case InputDefines.ButtonCondition.Free: return ButtonColorAtFree;
                case InputDefines.ButtonCondition.Down: return ButtonColorAtDown;
                case InputDefines.ButtonCondition.Push: return ButtonColorAtPush;
                case InputDefines.ButtonCondition.Up: return ButtonColorAtUp;
                default:
                    throw new System.NotImplementedException();
            }
        }
    }
}
