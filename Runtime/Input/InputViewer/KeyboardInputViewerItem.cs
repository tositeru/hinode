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
    public class KeyboardInputViewerItem : IInputViewerItem
    {
        [SerializeField, Min(1)] int _keyCodeLimitPerText = 8;
        public int KeyCodeLimitPerText
        {
            get => _keyCodeLimitPerText;
            set
            {
                var newLimit = Mathf.Max(1, value);
                if (_keyCodeLimitPerText == newLimit) return;
                _keyCodeLimitPerText = newLimit;
            }
        }

        HashSet<KeyCode> _observedKeyCodes = new HashSet<KeyCode>();
        public IReadOnlyCollection<KeyCode> ObservedKeyCodes { get => _observedKeyCodes; }

        List<KeyCodeText> _keyCodeTexts = new List<KeyCodeText>();
        public IReadOnlyList<KeyCodeText> KeyCodeTexts { get => _keyCodeTexts; }

        public KeyboardInputViewerItem AddObservedKey(IEnumerable<KeyCode> keyCodes)
        {
            foreach(var k in keyCodes.Where(_k => !_observedKeyCodes.Contains(_k)))
            {
                _observedKeyCodes.Add(k);
            }
            ResizeKeyCodeTexts();
            return this;
        }
        public KeyboardInputViewerItem AddObservedKey(params KeyCode[] keyCodes)
            => AddObservedKey(keyCodes.AsEnumerable());

        public KeyboardInputViewerItem RemoveObservedKey(IEnumerable<KeyCode> keyCodes)
        {
            foreach (var k in keyCodes.Where(_k => _observedKeyCodes.Contains(_k)))
            {
                _observedKeyCodes.Remove(k);
            }
            return this;
        }
        public KeyboardInputViewerItem RemoveObservedKey(params KeyCode[] keyCodes)
            => RemoveObservedKey(keyCodes.AsEnumerable());


        void ResizeKeyCodeTexts()
        {
            var count = ObservedKeyCodes.Count / KeyCodeLimitPerText
                + Mathf.Min(1, ObservedKeyCodes.Count % KeyCodeLimitPerText);
            while (_keyCodeTexts.Count < count)
            {
                var inst = KeyCodeText.Create(this);
                inst.OnChangedStyle(UseInputViewer.StyleInfo);
                _keyCodeTexts.Add(inst);
            }

            while(count < _keyCodeTexts.Count)
            {
                var index = _keyCodeTexts.Count - 1;
                Object.Destroy(_keyCodeTexts[index]);
                _keyCodeTexts.RemoveAt(index);
            }
        }

        void RemoveKeyCodeTexts()
        {
            foreach(var text in KeyCodeTexts)
            {
                Destroy(text.gameObject);
            }
            _keyCodeTexts.Clear();
        }

        #region presets
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

        void SetEnabled(ref bool flag, bool enabled, IEnumerable<KeyCode> keyCodes)
        {
            if (flag == enabled) return;
            flag = enabled;
            if (enabled) AddObservedKey(keyCodes);
            else RemoveObservedKey(keyCodes);
            AddObservedKey(_enabledKeyCodes);
        }

        public bool EnabledArrows { get => _enableArrows; set => SetEnabled(ref _enableArrows, value, KeyCodeDefines.ArrowKeyCodes); }
        public bool EnableAlphabets { get => _enableArrows; set => SetEnabled(ref _enableAlphabets, value, KeyCodeDefines.AlphabetKeyCodes); }
        public bool EnableNumber { get => _enableNumber; set => SetEnabled(ref _enableNumber, value, KeyCodeDefines.KeypadKeyCodes); }
        public bool EnableSymbol { get => _enableSymbol; set => SetEnabled(ref _enableSymbol, value, KeyCodeDefines.SymbolKeyCodes); }
        public bool EnableSystem { get => _enableSystem; set => SetEnabled(ref _enableSystem, value, KeyCodeDefines.SystemKeyCodes); }
        public bool EnableFunction { get => _enableFunction; set => SetEnabled(ref _enableFunction, value, KeyCodeDefines.FunctionKeyCodes); }
        public bool EnableJoystick { get => _enableJoystick; set => SetEnabled(ref _enableJoystick, value, KeyCodeDefines.JoyStickKeyCodes); }
        public bool EnableMouse { get => _enableMouse; set => SetEnabled(ref _enableMouse, value, KeyCodeDefines.MouseKeyCodes); }
        public bool EnableOther { get => _enableOther; set => SetEnabled(ref _enableOther, value, KeyCodeDefines.OtherKeyCodes); }
        #endregion

        #region Unity callback
        protected override void Start()
        {
            base.Start();

            if (_enableArrows) AddObservedKey(KeyCodeDefines.ArrowKeyCodes);
            if (_enableAlphabets) AddObservedKey(KeyCodeDefines.AlphabetKeyCodes);
            if (_enableNumber) AddObservedKey(KeyCodeDefines.KeypadKeyCodes);
            if (_enableSymbol) AddObservedKey(KeyCodeDefines.SymbolKeyCodes);
            if (_enableSystem) AddObservedKey(KeyCodeDefines.SystemKeyCodes);
            if (_enableFunction) AddObservedKey(KeyCodeDefines.FunctionKeyCodes);
            if (_enableJoystick) AddObservedKey(KeyCodeDefines.JoyStickKeyCodes);
            if (_enableMouse) AddObservedKey(KeyCodeDefines.MouseKeyCodes);
            if (_enableOther) AddObservedKey(KeyCodeDefines.OtherKeyCodes);
            AddObservedKey(_enabledKeyCodes);
        }

        protected override void OnDestroy()
        {
            RemoveKeyCodeTexts();
        }
        #endregion

        #region override IInputViewerItem
        public override void OnInitItem(InputViewer inputViewer)
        {

        }

        public override void OnRemoveFromViewer(InputViewer inputViewer)
        {
            RemoveKeyCodeTexts();
        }

        public override void OnUpdateItem()
        {
            ResizeKeyCodeTexts();

            var keyCodeEnumerator = ObservedKeyCodes.GetEnumerator();
            foreach(var text in KeyCodeTexts)
            {
                text.UpdateParam(keyCodeEnumerator);
            }
        }

        public override void OnChangedStyle(InputViewerStyleInfo styleInfo)
        {
            foreach (var text in KeyCodeTexts)
            {
                text.OnChangedStyle(styleInfo);
            }
        }
        #endregion

        public class KeyCodeText : MonoBehaviour
        {
            public static KeyCodeText Create(KeyboardInputViewerItem parent)
            {
                var obj = new GameObject($"__keyCodeText"
                    , typeof(RectTransform)
                    , typeof(CanvasRenderer));
                obj.transform.SetParent(parent.UseInputViewer.TextArea.transform);
                var inst = obj.AddComponent<KeyCodeText>();
                inst.Parent = parent;
                return inst;
            }

            List<KeyCode> _keyCodes = new List<KeyCode>();
            public IReadOnlyList<KeyCode> KeyCodes { get => _keyCodes; }

            KeyboardInputViewerItem _parent;
            public KeyboardInputViewerItem Parent
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
                    if(_text == null)
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

            public void UpdateParam(IEnumerator<KeyCode> observedKeyCodeEnumerator)
            {
                _keyCodes.Clear();
                for (var i=0; i<Parent.KeyCodeLimitPerText; ++i)
                {
                    if (!observedKeyCodeEnumerator.MoveNext()) break;
                    var keyCode = (KeyCode)observedKeyCodeEnumerator.Current;
                    _keyCodes.Add(keyCode);
                }

                Text.text = KeyCodes
                    .Select(_k => GetText(_k))
                    .Aggregate("key:", (_s, _c) => _s + " " + _c);
            }

            string GetText(KeyCode keyCode)
            {
                var condition = Parent.UseInput.GetKeyCondition(keyCode);
                return $"{keyCode}={IInputViewerItem.GetButtonConditionMark(condition)}";
            }

        }
    }
}
