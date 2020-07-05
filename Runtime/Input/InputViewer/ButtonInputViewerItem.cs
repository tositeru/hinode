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
    public class ButtonInputViewerItem : IInputViewerItem
    {
        [SerializeField, Min(1)] int _buttonLimitPerText = 4;
        public int ButtonLimitPerText
        {
            get => _buttonLimitPerText;
            set
            {
                var newLimit = Mathf.Max(1, value);
                if (ButtonLimitPerText == newLimit) return;
                _buttonLimitPerText = newLimit;
            }
        }

        HashSet<string> _observedButtons = new HashSet<string>();
        public IReadOnlyCollection<string> ObservedButtons { get => _observedButtons; }

        List<ButtonText> _buttonTexts = new List<ButtonText>();
        public IReadOnlyList<ButtonText> ButtonTexts { get => _buttonTexts; }

        public ButtonInputViewerItem AddObservedButton(IEnumerable<string> buttons)
        {
            foreach (var k in buttons.Where(_k => !_observedButtons.Contains(_k)))
            {
                _observedButtons.Add(k);
            }
            ResizeButtonTexts();
            return this;
        }
        public ButtonInputViewerItem AddObservedButton(params string[] buttons)
            => AddObservedButton(buttons.AsEnumerable());

        public ButtonInputViewerItem RemoveObservedButton(IEnumerable<string> buttons)
        {
            foreach (var k in buttons.Where(_k => _observedButtons.Contains(_k)))
            {
                _observedButtons.Remove(k);
            }
            return this;
        }
        public ButtonInputViewerItem RemoveObservedButton(params string[] buttons)
            => RemoveObservedButton(buttons.AsEnumerable());


        void ResizeButtonTexts()
        {
            var count = ObservedButtons.Count / ButtonLimitPerText
                + Mathf.Min(1, ObservedButtons.Count % ButtonLimitPerText);
            while (_buttonTexts.Count < count)
            {
                var inst = ButtonText.Create(this);
                inst.OnChangedStyle(UseInputViewer.StyleInfo);
                _buttonTexts.Add(inst);
            }

            while (count < _buttonTexts.Count)
            {
                var index = _buttonTexts.Count - 1;
                Object.Destroy(_buttonTexts[index]);
                _buttonTexts.RemoveAt(index);
            }
        }

        void RemoveButtonTexts()
        {
            foreach (var text in ButtonTexts)
            {
                Destroy(text.gameObject);
            }
            _buttonTexts.Clear();
        }

        #region presets
        [SerializeField] List<string> _enabledKeyCodes = new List<string>();
        #endregion

        #region Unity callback
        protected override void Start()
        {
            base.Start();
            AddObservedButton(_enabledKeyCodes);
        }

        protected override void OnDestroy()
        {
            RemoveButtonTexts();
        }
        #endregion

        #region override IInputViewerItem
        public override void OnInitItem(InputViewer inputViewer)
        {

        }

        public override void OnRemoveFromViewer(InputViewer inputViewer)
        {
            RemoveButtonTexts();
        }

        public override void OnUpdateItem()
        {
            ResizeButtonTexts();

            var buttonEnumerator = ObservedButtons.GetEnumerator();
            foreach (var text in ButtonTexts)
            {
                text.UpdateParam(buttonEnumerator);
            }
        }

        public override void OnChangedStyle(InputViewerStyleInfo styleInfo)
        {
            foreach (var text in ButtonTexts)
            {
                text.OnChangedStyle(styleInfo);
            }
        }
        #endregion

        public class ButtonText : MonoBehaviour
        {
            public static ButtonText Create(ButtonInputViewerItem parent)
            {
                var obj = new GameObject($"__buttonText"
                    , typeof(RectTransform)
                    , typeof(CanvasRenderer));
                obj.transform.SetParent(parent.UseInputViewer.TextArea.transform);
                var inst = obj.AddComponent<ButtonText>();
                inst.Parent = parent;
                return inst;
            }

            List<string> _buttons = new List<string>();
            public IReadOnlyList<string> Buttons { get => _buttons; }

            ButtonInputViewerItem _parent;
            public ButtonInputViewerItem Parent
            {
                get => _parent;
                set
                {
                    if (value == null || _parent == value) return;
                    _parent = value;
                }
            }

            Text _text;
            public Text Text
            {
                get
                {
                    if (_text == null)
                    {
                        _text = gameObject.AddComponent<Text>();
                        OnChangedStyle(Parent.UseInputViewer.StyleInfo);
                        _text.raycastTarget = false;
                    }
                    return _text;
                }
            }

            public void OnChangedStyle(InputViewerStyleInfo styleInfo)
            {
                Text.font = styleInfo.Font;
                Text.color = styleInfo.FontColor;
            }

            public void UpdateParam(IEnumerator<string> observedButtonEnumerator)
            {
                _buttons.Clear();
                for (var i = 0; i < Parent.ButtonLimitPerText; ++i)
                {
                    if (!observedButtonEnumerator.MoveNext()) break;
                    var button = (string)observedButtonEnumerator.Current;
                    _buttons.Add(button);
                }

                Text.text = Buttons
                    .Select(_k => GetText(_k))
                    .Aggregate("button:", (_s, _c) => _s + " " + _c);
            }

            string GetText(string button)
            {
                var condition = Parent.UseInput.GetButtonCondition(button);
                return $"{button}={IInputViewerItem.GetButtonConditionMark(condition)}";
            }

        }
    }
}
