using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Hinode
{
    /// <summary>
	/// <seealso cref="IInputViewerItem"/>
	/// </summary>
    public class TouchInputViewerItem : IInputViewerItem
    {
        [SerializeField] float _pointerRadius = 10f;

        public bool DoEnabled { get => UseInput.TouchSupported; }

        public float PointerRadius { get => _pointerRadius; set => SetPointerRadius(value); }

        List<TouchPointer> _pointers = new List<TouchPointer>();
        public IReadOnlyList<TouchPointer> Pointers
        {
            get => _pointers;
        }
        public int PointerCount { get => _pointers.Count; }

        void SetPointerRadius(float radius)
        {
            _pointerRadius = Mathf.Max(1f, radius);

            foreach(var p in Pointers)
            {
                var R = p.transform as RectTransform;
                R.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _pointerRadius);
                R.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _pointerRadius);
            }
        }

        void ResizePointers(int count)
        {
            while(_pointers.Count < count)
            {
                var p = TouchPointer.Create(this, UseInput.GetTouch(_pointers.Count));
                _pointers.Add(p);
            }

            while(count < _pointers.Count)
            {
                var index = _pointers.Count - 1;
                Destroy(_pointers[index]);
                _pointers.RemoveAt(index);
            }
        }

        void DestroyPointers()
        {
            foreach (var p in Pointers)
            {
                Destroy(p.gameObject);
            }
            _pointers.Clear();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            DestroyPointers();
        }

        #region override IInputViewerItem
        public override void OnInitItem(InputViewer inputViewer)
        {
            ResizePointers(inputViewer.UseInput.TouchCount);
        }

        public override void OnRemoveFromViewer(InputViewer inputViewer)
        {
            DestroyPointers();
        }

        public override void OnUpdateItem()
        {
            ResizePointers(UseInput.TouchCount);

            foreach(var (pointer, index) in Pointers
                .Zip(Enumerable.Range(0, PointerCount), (_p, _i) => (pointer: _p, index: _i)))
            {
                pointer.UpdateParam(UseInput.GetTouch(index));
            }
        }

        public override void OnChangedStyle(InputViewerStyleInfo styleInfo)
        {
            foreach(var pointer in Pointers)
            {
                pointer.OnChangedStyle(styleInfo);
            }
        }

        #endregion

        public class TouchPointer : MonoBehaviour
        {
            public static TouchPointer Create(TouchInputViewerItem parent, Touch touch)
            {
                var obj = new GameObject("__touchPointer", typeof(RectTransform), typeof(CanvasRenderer));
                var inst = obj.AddComponent<TouchPointer>();
                obj.transform.SetParent(parent.UseInputViewer.RootCanvas.transform);

                inst.Parent = parent;
                inst.UpdateParam(touch);
                return inst;
            }

            TouchInputViewerItem _parent;
            public TouchInputViewerItem Parent
            {
                get => _parent;
                private set
                {
                    if (_parent == value || value == null) return;
                    _parent = value;

                    SetPointerRadius();
                }
            }

            public int FingerID { get; private set; }

            Image _pointerImage;
            public Image PointerImage
            {
                get
                {
                    if (_pointerImage == null)
                    {
                        _pointerImage = gameObject.AddComponent<Image>();
                        _pointerImage.raycastTarget = false;
                    }
                    return _pointerImage;
                }
            }

            Text _IDText;
            public Text IDText
            {
                get
                {
                    if (_IDText == null)
                    {
                        _IDText = CreateText("fingerIDText");
                        _IDText.transform.SetParent(PointerImage.transform);
                        var R = _IDText.transform as RectTransform;
                        R.anchorMin = Vector2.zero;
                        R.anchorMax = Vector2.one;
                        R.offsetMin = R.offsetMax = Vector2.zero;

                        //_IDText.alignment = ;
                    }
                    return _IDText;
                }
            }

            void SetPointerRadius()
            {
                var R = transform as RectTransform;

                R.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Parent.PointerRadius);
                R.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Parent.PointerRadius);
            }

            public void UpdateParam(Touch touch)
            {
                gameObject.SetActive(Parent.DoEnabled);

                if (!Parent.DoEnabled) return;

                {//PointerImage
                    var R = transform as RectTransform;
                    //Pos
                    var screenRect = (Parent.UseInputViewer.RootCanvas.transform as RectTransform).rect;
                    R.anchoredPosition = touch.position;
                    R.anchoredPosition -= screenRect.size / 2;

                    //Size
                    SetPointerRadius();

                    //Color
                    PointerImage.color = Parent.UseInputViewer.StyleInfo.GetButtonCondition(InputDefines.ToButtonCondition(touch));
                }

                {//IDTest
                    IDText.text = touch.fingerId.ToString();
                }
            }

            public void OnChangedStyle(InputViewerStyleInfo styleInfo)
            {
                IDText.font = styleInfo.Font;
                IDText.color = styleInfo.FontColor;
            }
        }
    }
}
