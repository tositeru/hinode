using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
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
    /// TODO ボタンの状態を表示するようにする？
    /// TODO InputとReplayableInputにも対応する
    /// TODO 再生状態を画面に表示する？
    /// </summary>
    public class InputViewer : MonoBehaviour
    {
        public static InputViewer CreateInstance()
        {
            var obj = new GameObject("__inputViewer");
            return obj.AddComponent<InputViewer>();
        }

        HashSet<IInputViewerItem> _inputViewerItems = new HashSet<IInputViewerItem>();
        ReplayableInput _useInput = null;

        [SerializeField] int _sortingOrder = 30000;
        [SerializeField] InputViewerStyleInfo _styleInfo = new InputViewerStyleInfo();

        public ReplayableInput UseInput { get => _useInput ?? ReplayableInput.Instance; set => _useInput = value; }
        public InputViewerStyleInfo StyleInfo { get => _styleInfo; }

        #region IInputViewerItem
        public IReadOnlyCollection<IInputViewerItem> ViewerItems { get => _inputViewerItems; }
        #endregion

        #region Canvas
        Canvas _rootCanvas;

        public int SortingOrder { get => _sortingOrder; set => _sortingOrder = value; }
        public Canvas RootCanvas
        {
            get
            {
                if(null == _rootCanvas)
                {
                    gameObject.AddComponent<RectTransform>();
                    _rootCanvas = gameObject.AddComponent<Canvas>();
                    gameObject.AddComponent<CanvasScaler>();

                    _rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    _rootCanvas.sortingOrder = SortingOrder;
                }
                return _rootCanvas;
            }
        }
        #endregion

        #region TextArea
        RectTransform _textArea;
        public RectTransform TextArea
        {
            get
            {
                if(null == _textArea)
                {
                    _textArea = new GameObject("__textArea").AddComponent<RectTransform>();
                    _textArea.transform.SetParent(RootCanvas.transform);
                    _textArea.anchorMin = _textArea.anchorMax = new Vector2(0, 1);
                    _textArea.offsetMin = Vector2.zero;
                    _textArea.offsetMax = Vector2.zero;
                    _textArea.pivot = new Vector2(0, 1);

                    var rootR = RootCanvas.transform as RectTransform;
                    _textArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rootR.rect.width);
                    _textArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rootR.rect.height / 3f);

                    var layout = _textArea.gameObject.AddComponent<VerticalLayoutGroup>();
                    layout.childForceExpandHeight = false;

                    var image = _textArea.gameObject.AddComponent<Image>();
                    image.raycastTarget = false;

                    var color = Color.black;
                    color.a = 0.4f;
                    image.color = color;
                }
                return _textArea;
            }
        }
        #endregion

        //RectTransform _buttonInfoPanel;
        //Text _textPrefab;

        //Image _replayingCursorInstance;
        //Image _mouseCursor;
        //List<Image> _touchCursors = new List<Image>();

        //Dictionary<ButtonInfoText, Text> _buttonInfoTextDict = new Dictionary<ButtonInfoText, Text>();

        //private void OnGUI()
        //{
        //    if (_defaultFont == null)
        //    {
        //        _defaultFont = GUI.skin.font;
        //    }
        //}

        #region InputViewerItems
        public InputViewer AddItem(IInputViewerItem item)
        {
            if(!_inputViewerItems.Contains(item))
            {
                item.InitItem(this);
                item.ChangedStyle(this.StyleInfo);
                _inputViewerItems.Add(item);
            }
            return this;
        }

        public InputViewer RemoveItem(IInputViewerItem item)
        {
            if(_inputViewerItems.Remove(item))
            {
                _inputViewerItems.Remove(item);
                item.RemoveFromViewer(this);
            }
            return this;
        }

        public void RefleshItems()
        {
            var items = GetComponents<IInputViewerItem>();
            foreach(var i in items.Where(_i => !_inputViewerItems.Contains(_i)))
            {
                i.InitItem(this);
                _inputViewerItems.Add(i);
            }

            UpdateItems();
        }

        public void UpdateItems()
        {
            foreach(var i in _inputViewerItems)
            {
                i.UpdateItem();
            }
        }

        void OnChangedStyleInfo(InputViewerStyleInfo styleInfo)
        {
            foreach (var item in ViewerItems)
            {
                item.ChangedStyle(styleInfo);
            }
        }
        #endregion

        #region Unity Callbacks
        void Awake()
        {
            _styleInfo.OnChanged.Add(OnChangedStyleInfo);
        }

        private void Update()
        {
            UpdateItems();
        }
        #endregion

        //    #region タップ位置のカーソル
        //    Image ReplayingCursorInstance
        //    {
        //        get
        //        {
        //            if (null == _replayingCursorInstance)
        //            {
        //                var obj = new GameObject("__cursorBasePrefab"
        //                    , typeof(RectTransform)
        //                    , typeof(CanvasRenderer)
        //                    , typeof(Image)
        //                    , typeof(Outline));
        //                obj.SetActive(false);
        //                obj.transform.SetParent(transform);
        //                _replayingCursorInstance = obj.GetComponent<Image>();
        //                var rectTransform = obj.transform as RectTransform;
        //                rectTransform.anchorMin = rectTransform.anchorMax = Vector2.zero;
        //                rectTransform.pivot = Vector2.one * 0.5f;
        //                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 10);
        //                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 10);
        //            }
        //            return _replayingCursorInstance;
        //        }
        //    }

        //    public bool HasMouseCursor { get => _mouseCursor != null; }
        //    public Image MouseCursor
        //    {
        //        get
        //        {
        //            if(_mouseCursor == null)
        //            {
        //                _mouseCursor = Instantiate(ReplayingCursorInstance, transform);
        //                _mouseCursor.gameObject.SetActive(true);
        //                _mouseCursor.name = "__mouseCursor";
        //                _mouseCursor.color = MOUSE_CURSOR_COLOR;
        //            }
        //            return _mouseCursor;
        //        }
        //    }

        //    public Image GetTouchCursor(int index)
        //    {
        //        if (Target == null) return null;

        //        ResizeTouchCursor(index);
        //        var cursor = _touchCursors[index];
        //        return cursor;
        //    }

        //    void ResizeTouchCursor(int index)
        //    {
        //        while(_touchCursors.Count <= index)
        //        {
        //            var newCursor = Instantiate(ReplayingCursorInstance, transform);
        //            _touchCursors.Add(newCursor);
        //        }
        //    }
        //    #endregion

        //    #region ボタン情報関連のプロパティ
        //    RectTransform ButtonInfoPanel
        //    {
        //        get
        //        {
        //            if (_buttonInfoPanel != null) return _buttonInfoPanel;

        //            _buttonInfoPanel = new GameObject("__buttonInfo"
        //                , typeof(RectTransform)
        //                , typeof(CanvasRenderer)
        //                , typeof(Image)
        //                , typeof(VerticalLayoutGroup)
        //                , typeof(ContentSizeFitter))
        //                .GetComponent<RectTransform>();
        //            _buttonInfoPanel.SetParent(transform);
        //            _buttonInfoPanel.anchorMin = _buttonInfoPanel.anchorMax = new Vector2(0, 1);
        //            _buttonInfoPanel.anchoredPosition = Vector2.zero;
        //            _buttonInfoPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 200);
        //            _buttonInfoPanel.pivot = new Vector2(0, 1);

        //            var image = _buttonInfoPanel.GetComponent<Image>();
        //            image.color = ColorExtensions.HSVToRGBA(0, 0, 0, 0.1529412f);
        //            image.raycastTarget = false;

        //            var layoutGroup = _buttonInfoPanel.GetComponent<VerticalLayoutGroup>();
        //            var layoutGroupPadding = new RectOffset {
        //                left = 5,
        //                right = 5,
        //                top = 5,
        //                bottom = 5,
        //            };
        //            layoutGroup.padding = layoutGroupPadding;
        //            layoutGroup.childAlignment = TextAnchor.UpperLeft;
        //            layoutGroup.childControlWidth = false;
        //            layoutGroup.childControlHeight = false;
        //            layoutGroup.childForceExpandWidth = false;
        //            layoutGroup.childForceExpandHeight = false;

        //            var contentSizeFitter = _buttonInfoPanel.GetComponent<ContentSizeFitter>();
        //            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        //            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        //            return _buttonInfoPanel;
        //        }
        //    }

        //    Text TextBasePrefab
        //    {
        //        get
        //        {
        //            if (_textPrefab != null) return _textPrefab;

        //            var text = new GameObject("__textPrefab"
        //                , typeof(RectTransform)
        //                , typeof(CanvasRenderer)
        //                , typeof(Text)
        //                , typeof(ContentSizeFitter))
        //                .GetComponent<Text>();
        //            text.transform.SetParent(transform);
        //            text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 22f);
        //            text.gameObject.SetActive(false);

        //            var contentSizeFitter = text.GetComponent<ContentSizeFitter>();
        //            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        //            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        //            text.font = DefaultFont;
        //            text.raycastTarget = false;
        //            _textPrefab = text;
        //            return _textPrefab;
        //        }
        //    }

        //    Text MouseLRMText
        //    {
        //        get
        //        {
        //            if (_buttonInfoTextDict.ContainsKey(ButtonInfoText.MouseButtonLRM))
        //                return _buttonInfoTextDict[ButtonInfoText.MouseButtonLRM];

        //            var text = Instantiate(TextBasePrefab, ButtonInfoPanel);
        //            text.gameObject.SetActive(true);
        //            _buttonInfoTextDict.Add(ButtonInfoText.MouseButtonLRM, text);
        //            return text;
        //        }
        //    }
        //    #endregion

        //    private void Update()
        //    {
        //        if (Target == null) return;

        //        if(Target.mousePresent)
        //        {
        //            MouseCursor.gameObject.SetActive(true);
        //            MouseCursor.rectTransform.anchoredPosition = Target.mousePosition;
        //            var mouseLeftButtonCondition = InputDefines.ToButtonCondition(Target, InputDefines.MouseButton.Left);
        //            SetCursorColor(MouseCursor, mouseLeftButtonCondition);
        //            //動的に読み込むと文字が表示されなかったので、一時的に無刻化している
        //            MouseLRMText.text = $"mouse: L={mouseLeftButtonCondition}, R={InputDefines.ToButtonCondition(Target, InputDefines.MouseButton.Right)}, M={InputDefines.ToButtonCondition(Target, InputDefines.MouseButton.Middle)}";
        //            //MouseLRMText.font.RequestCharactersInTexture(MouseLRMText.text);
        //        }
        //        else
        //        {
        //            if(HasMouseCursor) MouseCursor.gameObject.SetActive(false);
        //        }

        //        if (Target.touchSupported)
        //        {
        //            ResizeTouchCursor(Target.touchCount);
        //            for (var i=0; i<Target.touchCount; ++i)
        //            {
        //                var cursor = _touchCursors[i];
        //                var touch = Target.GetTouch(i);
        //                _touchCursors[i].gameObject.SetActive(true);
        //                cursor.rectTransform.anchoredPosition = touch.position;
        //                SetCursorColor(cursor, InputDefines.ToButtonCondition(touch));
        //            }
        //            for(var i = Target.touchCount; i < _touchCursors.Count; ++i)
        //            {
        //                _touchCursors[i].gameObject.SetActive(false);
        //            }
        //        }
        //        else
        //        {
        //            for (var i = 0; i < _touchCursors.Count; ++i)
        //            {
        //                _touchCursors[i].gameObject.SetActive(false);
        //            }
        //        }
        //    }

        //    void SetCursorColor(Image cursor, InputDefines.ButtonCondition condition)
        //    {
        //        Color color;
        //        switch (condition)
        //        {
        //            case InputDefines.ButtonCondition.Down: color = Color.yellow; break;
        //            case InputDefines.ButtonCondition.Push: color = Color.red; break;
        //            case InputDefines.ButtonCondition.Up: color = Color.green; break;
        //            case InputDefines.ButtonCondition.Free: color = Color.gray; break;
        //            default: color = Color.magenta; break;
        //        }
        //        color.a = 0.4f;
        //        cursor.GetComponent<Outline>().effectColor = color;
        //    }

        //    #region SingletonMonoBehaviour<InputViewer> overrides
        //    override protected string DefaultInstanceName { get => "__inputViewerCanvas"; }
        //    override protected void OnAwaked()
        //    {
        //        gameObject.GetOrAddComponent<RectTransform>();
        //        var canvas = gameObject.GetOrAddComponent<Canvas>();
        //        gameObject.GetOrAddComponent<CanvasScaler>();

        //        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        //        canvas.sortingOrder = 30000;
        //    }

        //    override protected void OnDestroyed(bool isInstance) { }
        //    #endregion
    }
}