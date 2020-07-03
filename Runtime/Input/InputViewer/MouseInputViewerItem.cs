using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Hinode
{
    /// <summary>
	/// <seealso cref="InputViewer"/>
	/// <seealso cref="IInputViewerItem"/>
	/// </summary>
    public class MouseInputViewerItem : IInputViewerItem
    {
        [SerializeField] float _cursorRadius = 10f;

        public bool DoEnabled { get => UseInput.MousePresent; }

        public float CursorRadius { get => _cursorRadius; set => SetCursorRadius(value); }

        Image _cursor;
        public Image Cursor
        {
            get
            {
                if(_cursor == null)
                {
                    _cursor = CreateImage("__cursor");
                    _cursor.transform.SetParent(UseInputViewer.RootCanvas.transform);

                    SetCursorRadius(CursorRadius);
                }
                return _cursor;
            }
        }

        void SetCursorRadius(float radius)
        {
            _cursorRadius = Mathf.Max(1f, radius);

            var R = Cursor.transform as RectTransform;
            R.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _cursorRadius);
            R.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _cursorRadius);
        }

        Text _buttonsText;
        public Text ButtonsText
        {
            get
            {
                if(_buttonsText == null)
                {
                    _buttonsText = CreateText("__buttonsText");
                    _buttonsText.transform.SetParent(UseInputViewer.TextArea.transform);
                }
                return _buttonsText;
            }
        }

        public static string GetMouseButtonMark(InputDefines.MouseButton btn)
        {
            switch (btn)
            {
                case InputDefines.MouseButton.Left: return "L";
                case InputDefines.MouseButton.Right: return "R";
                case InputDefines.MouseButton.Middle: return "M";
                default: throw new System.NotImplementedException();
            }
        }

        #region override IInputViewerItem
        public override void OnInitItem(InputViewer inputViewer)
        {
        }

        public override void OnRemoveFromViewer(InputViewer inputViewer)
        {

        }

        public override void OnUpdateItem()
        {
            Cursor.gameObject.SetActive(DoEnabled);
            ButtonsText.gameObject.SetActive(DoEnabled);

            if(DoEnabled)
            {
                var cursorR = Cursor.transform as RectTransform;
                var screenRect = (UseInputViewer.RootCanvas.transform as RectTransform).rect; 
                cursorR.anchoredPosition = UseInput.MousePos;
                cursorR.anchoredPosition -= screenRect.size / 2;

                Cursor.color = UseInputViewer.StyleInfo.GetButtonCondition(UseInput.GetMouseButton(InputDefines.MouseButton.Left));

                var text = "MusBtn:";
                foreach(var btn in System.Enum.GetValues(typeof(InputDefines.MouseButton)).OfType<InputDefines.MouseButton>())
                {
                    var condition = UseInput.GetMouseButton(btn);

                    var btnText = GetMouseButtonMark(btn);
                    text += $" {btnText}={GetButtonConditionMark(condition)}";
                }
                ButtonsText.text = text;
            }
        }

        public override void OnChangedStyle(InputViewerStyleInfo styleInfo)
        {
            ButtonsText.font = styleInfo.Font;
            ButtonsText.color = styleInfo.FontColor;
        }

        #endregion
    }
}
