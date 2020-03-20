using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hinode
{
    /// <summary>
    /// 入力データの再生状況を画面に表示するためのコンポーネント
    ///
    /// 基本的にデバッグ用として作成しました
    /// 現状はBaseInputに対応しています
    ///
    /// TODO InputとReplayableInputにも対応する
    /// TODO 再生状態を画面に表示する？
    /// </summary>
    public class InputViewer : SingletonMonoBehaviour<InputViewer>
    {
        static readonly Color MOUSE_CURSOR_COLOR = ColorExtensions.HSVToRGBA(0f, 0.6f, 0.9f, 0.6f);

        enum ButtonInfoText
        {
            MouseButtonLRM,
        }

#pragma warning disable CS0649
        [SerializeField] BaseInput _target;
#pragma warning restore CS0649

        RectTransform _buttonInfoPanel;
        Text _textPrefab;

        Image _replayingCursorInstance;
        Image _mouseCursor;
        List<Image> _touchCursors = new List<Image>();

        Dictionary<ButtonInfoText, Text> _buttonInfoTextDict = new Dictionary<ButtonInfoText, Text>();

        public BaseInput Target
        {
            get => _target;
            set => _target = value;
        }

        #region タップ位置のカーソル
        Image ReplayingCursorInstance
        {
            get
            {
                if (null == _replayingCursorInstance)
                {
                    var obj = new GameObject("__cursorBasePrefab"
                        , typeof(RectTransform)
                        , typeof(CanvasRenderer)
                        , typeof(Image)
                        , typeof(Outline));
                    obj.SetActive(false);
                    obj.transform.SetParent(transform);
                    _replayingCursorInstance = obj.GetComponent<Image>();
                    var rectTransform = obj.transform as RectTransform;
                    rectTransform.anchorMin = rectTransform.anchorMax = Vector2.zero;
                    rectTransform.pivot = Vector2.one * 0.5f;
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 10);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 10);
                }
                return _replayingCursorInstance;
            }
        }

        Image MouseCursor
        {
            get
            {
                if(_mouseCursor == null)
                {
                    _mouseCursor = Instantiate(ReplayingCursorInstance, transform);
                    _mouseCursor.gameObject.SetActive(true);
                    _mouseCursor.name = "__mouseCursor";
                    _mouseCursor.color = MOUSE_CURSOR_COLOR;
                }
                return _mouseCursor;
            }
        }
        #endregion

        #region ボタン情報関連のプロパティ
        RectTransform ButtonInfoPanel
        {
            get
            {
                if (_buttonInfoPanel != null) return _buttonInfoPanel;

                _buttonInfoPanel = new GameObject("__buttonInfo"
                    , typeof(RectTransform)
                    , typeof(CanvasRenderer)
                    , typeof(Image)
                    , typeof(VerticalLayoutGroup)
                    , typeof(ContentSizeFitter))
                    .GetComponent<RectTransform>();
                _buttonInfoPanel.SetParent(transform);
                _buttonInfoPanel.anchorMin = _buttonInfoPanel.anchorMax = new Vector2(0, 1);
                _buttonInfoPanel.anchoredPosition = Vector2.zero;
                _buttonInfoPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 200);
                _buttonInfoPanel.pivot = new Vector2(0, 1);

                var image = _buttonInfoPanel.GetComponent<Image>();
                image.color = ColorExtensions.HSVToRGBA(0, 0, 0, 0.1529412f);
                image.raycastTarget = false;

                var layoutGroup = _buttonInfoPanel.GetComponent<VerticalLayoutGroup>();
                var layoutGroupPadding = new RectOffset {
                    left = 5,
                    right = 5,
                    top = 5,
                    bottom = 5,
                };
                layoutGroup.padding = layoutGroupPadding;
                layoutGroup.childAlignment = TextAnchor.UpperLeft;
                layoutGroup.childControlWidth = false;
                layoutGroup.childControlHeight = false;
                layoutGroup.childForceExpandWidth = false;
                layoutGroup.childForceExpandHeight = false;

                var contentSizeFitter = _buttonInfoPanel.GetComponent<ContentSizeFitter>();
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                return _buttonInfoPanel;
            }
        }

        Text TextBasePrefab
        {
            get
            {
                if (_textPrefab != null) return _textPrefab;

                var text = new GameObject("__textPrefab"
                    , typeof(RectTransform)
                    , typeof(CanvasRenderer)
                    , typeof(Text)
                    , typeof(ContentSizeFitter))
                    .GetComponent<Text>();
                text.transform.SetParent(transform);
                text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 22f);
                text.gameObject.SetActive(false);

                var contentSizeFitter = text.GetComponent<ContentSizeFitter>();
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                text.font = new Font("Arial");
                text.raycastTarget = false;
                _textPrefab = text;
                return _textPrefab;
            }
        }

        Text MouseLRMText
        {
            get
            {
                if (_buttonInfoTextDict.ContainsKey(ButtonInfoText.MouseButtonLRM))
                    return _buttonInfoTextDict[ButtonInfoText.MouseButtonLRM];

                var text = Instantiate(TextBasePrefab, ButtonInfoPanel);
                text.gameObject.SetActive(true);
                _buttonInfoTextDict.Add(ButtonInfoText.MouseButtonLRM, text);
                return text;
            }
        }

        #endregion

        private void Update()
        {
            if (Target == null) return;

            if(Target.mousePresent)
            {
                MouseCursor.rectTransform.anchoredPosition = Target.mousePosition;
                InputDefines.ButtonCondition mouseLeftButtonCondition = InputDefines.ToButtonCondition(Target, InputDefines.MouseButton.Left);
                SetCursorColor(MouseCursor, mouseLeftButtonCondition);

                //動的に読み込むと文字が表示されなかったので、一時的に無刻化している
                //MouseLRMText.text = $"mouse: L={mouseLeftButtonCondition}, R={InputDefines.ToButtonCondition(Target, InputDefines.MouseButton.Right)}, M={InputDefines.ToButtonCondition(Target, InputDefines.MouseButton.Middle)}";
                //MouseLRMText.font.RequestCharactersInTexture(MouseLRMText.text);
            }


            if (Target.touchSupported)
            {
                for(var i=0; i<Target.touchCount; ++i)
                {
                    if(_touchCursors.Count <= i)
                    {
                        var newCursor = Instantiate(ReplayingCursorInstance, transform);
                        _touchCursors.Add(newCursor);
                    }

                    var cursor = _touchCursors[i];
                    var touch = Target.GetTouch(i);
                    cursor.rectTransform.anchoredPosition = touch.position;
                    SetCursorColor(cursor, InputDefines.ToButtonCondition(touch));
                }
            }
        }

        void SetCursorColor(Image cursor, InputDefines.ButtonCondition condition)
        {
            Color color;
            switch (condition)
            {
                case InputDefines.ButtonCondition.Down: color = Color.yellow; break;
                case InputDefines.ButtonCondition.Push: color = Color.red; break;
                case InputDefines.ButtonCondition.Up: color = Color.green; break;
                case InputDefines.ButtonCondition.Free: color = Color.gray; break;
                default: color = Color.magenta; break;
            }
            color.a = 0.4f;
            cursor.GetComponent<Outline>().effectColor = color;
        }

        #region SingletonMonoBehaviour<InputViewer> overrides
        override protected string DefaultInstanceName { get => "__inputViewerCanvas"; }
        override protected void OnAwaked()
        {
            gameObject.GetOrAddComponent<RectTransform>();
            var canvas = gameObject.GetOrAddComponent<Canvas>();
            gameObject.GetOrAddComponent<CanvasScaler>();

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30000;
        }

        override protected void OnDestroyed(bool isInstance) { }
        #endregion
    }
}