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
    public class AxisButtonInputViewerItem : IInputViewerItem
    {
        [SerializeField, Min(1)] int _axisAxisLimitPerText = 4;
        public int AxisLimitPerText
        {
            get => _axisAxisLimitPerText;
            set
            {
                var newLimit = Mathf.Max(1, value);
                if (AxisLimitPerText == newLimit) return;
                _axisAxisLimitPerText = newLimit;
            }
        }

        HashSet<string> _observedAxises = new HashSet<string>();
        public IReadOnlyCollection<string> ObservedAxises { get => _observedAxises; }

        List<AxisText> _axisTexts = new List<AxisText>();
        public IReadOnlyList<AxisText> AxisTexts { get => _axisTexts; }

        public AxisButtonInputViewerItem AddObservedAxis(IEnumerable<string> axises)
        {
            foreach (var k in axises.Where(_k => !_observedAxises.Contains(_k)))
            {
                _observedAxises.Add(k);
            }
            ResizeAxisTexts();
            return this;
        }
        public AxisButtonInputViewerItem AddObservedAxis(params string[] axises)
            => AddObservedAxis(axises.AsEnumerable());

        public AxisButtonInputViewerItem RemoveObservedAxis(IEnumerable<string> axises)
        {
            foreach (var k in axises.Where(_k => _observedAxises.Contains(_k)))
            {
                _observedAxises.Remove(k);
            }
            return this;
        }
        public AxisButtonInputViewerItem RemoveObservedAxis(params string[] axises)
            => RemoveObservedAxis(axises.AsEnumerable());


        void ResizeAxisTexts()
        {
            var count = ObservedAxises.Count / AxisLimitPerText
                + Mathf.Min(1, ObservedAxises.Count % AxisLimitPerText);
            while (_axisTexts.Count < count)
            {
                var inst = AxisText.Create(this);
                inst.OnChangedStyle(UseInputViewer.StyleInfo);
                _axisTexts.Add(inst);
            }

            while (count < _axisTexts.Count)
            {
                var index = _axisTexts.Count - 1;
                Object.Destroy(_axisTexts[index]);
                _axisTexts.RemoveAt(index);
            }
        }

        void RemoveAxisTexts()
        {
            foreach (var text in AxisTexts)
            {
                Destroy(text.gameObject);
            }
            _axisTexts.Clear();
        }

        #region presets
        [SerializeField] List<string> _enabledKeyCodes = new List<string>();
        #endregion

        #region Unity callback
        protected override void Start()
        {
            base.Start();
            AddObservedAxis(_enabledKeyCodes);
        }

        protected override void OnDestroy()
        {
            RemoveAxisTexts();
        }
        #endregion

        #region override IInputViewerItem
        public override void OnInitItem(InputViewer inputViewer)
        {

        }

        public override void OnRemoveFromViewer(InputViewer inputViewer)
        {
            RemoveAxisTexts();
        }

        public override void OnUpdateItem()
        {
            ResizeAxisTexts();

            var axisEnumerator = ObservedAxises.GetEnumerator();
            foreach (var text in AxisTexts)
            {
                text.UpdateParam(axisEnumerator);
            }
        }

        public override void OnChangedStyle(InputViewerStyleInfo styleInfo)
        {
            foreach (var text in AxisTexts)
            {
                text.OnChangedStyle(styleInfo);
            }
        }
        #endregion

        public class AxisText : MonoBehaviour
        {
            public static AxisText Create(AxisButtonInputViewerItem parent)
            {
                var obj = new GameObject($"__axisText"
                    , typeof(RectTransform)
                    , typeof(CanvasRenderer));
                obj.transform.SetParent(parent.UseInputViewer.TextArea.transform);
                var inst = obj.AddComponent<AxisText>();
                inst.Parent = parent;
                return inst;
            }

            List<string> _axises = new List<string>();
            public IReadOnlyList<string> Axises { get => _axises; }

            AxisButtonInputViewerItem _parent;
            public AxisButtonInputViewerItem Parent
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

            public void UpdateParam(IEnumerator<string> observedAxisEnumerator)
            {
                _axises.Clear();
                for (var i = 0; i < Parent.AxisLimitPerText; ++i)
                {
                    if (!observedAxisEnumerator.MoveNext()) break;
                    var axis = (string)observedAxisEnumerator.Current;
                    _axises.Add(axis);
                }

                Text.text = Axises
                    .Select(_k => GetText(_k))
                    .Aggregate("axis:", (_s, _c) => _s + " " + _c);
            }

            string GetText(string axis)
            {
                var value = Parent.UseInput.GetAxis(axis);
                return $"{axis}={value.ToString("F3")}";
            }

        }
    }

}
