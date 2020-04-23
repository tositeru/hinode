using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hinode
{
    public enum DirectionType
    {
        Horizontal,
        Vertical,
    }

    /// <summary>
    /// 
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class HVLayoutGroupViewObject : CanvasViewObject.IAppendableViewObject
    {
        public static HVLayoutGroupViewObject Create(string name = "HVLayoutGroup")
        {
            var obj = new GameObject(name);
            obj.AddComponent<RectTransform>();
            return obj.AddComponent<HVLayoutGroupViewObject>();
        }

        DirectionType _direction;

        public DirectionType Direction
        {
            get => _direction;
            set
            {
                if (_direction == value) return;
                _direction = value;

                var prev = CurrentLayoutGroup;
                var currentLayoutGroup = CreateLayoutGroup(_direction);
                //パラメータを継承する
                if(prev != currentLayoutGroup)
                {
                    currentLayoutGroup.padding = prev.padding;
                    currentLayoutGroup.spacing = prev.spacing;
                    currentLayoutGroup.childAlignment = prev.childAlignment;
                    currentLayoutGroup.childControlWidth = prev.childControlWidth;
                    currentLayoutGroup.childControlHeight = prev.childControlHeight;
                    currentLayoutGroup.childScaleWidth = prev.childScaleWidth;
                    currentLayoutGroup.childScaleHeight = prev.childScaleHeight;
                    currentLayoutGroup.childForceExpandWidth = prev.childForceExpandWidth;
                    currentLayoutGroup.childForceExpandHeight= prev.childForceExpandHeight;
                    Destroy(prev);
                }
            }
        }

        HorizontalOrVerticalLayoutGroup CurrentLayoutGroup
        {
            get
            {
                var layout = GetComponent<HorizontalOrVerticalLayoutGroup>();
                if (layout == null)
                {
                    layout = CreateLayoutGroup(Direction);
                }
                return layout;
            }
        }

        HorizontalOrVerticalLayoutGroup CreateLayoutGroup(DirectionType dir)
        {
            switch (dir)
            {
                case DirectionType.Horizontal: return gameObject.GetOrAddComponent<HorizontalLayoutGroup>();
                case DirectionType.Vertical: return gameObject.GetOrAddComponent<VerticalLayoutGroup>();
                default: throw new System.NotImplementedException();
            }
        }
        private void OnDestroy()
        {
            var layout = GetComponent<HorizontalOrVerticalLayoutGroup>();
            if(layout != null) Destroy(layout);
        }

        public class FixedParamBinder : IDictinaryModelViewParamBinder
            , CanvasViewObject.IAppendableViewObjectParamBinder
        {
            public enum Params
            {
                Direction,
                PaddingX,
                PaddingY,
                Spacing,
                ChildAlignment,
                ControllChildWidth,
                ControllChildHeight,
                UseChildScaleX,
                UseChildScaleY,
                ChildForceExpandWidth,
                ChildForceExpandHeight,
            }

            public bool Contains(Params paramType)
                => Contains(paramType.ToString());

            public FixedParamBinder Set(Params param, object value)
                => Set(param.ToString(), value) as FixedParamBinder;
            public FixedParamBinder Set<T>(Params param, T value)
                => Set(param.ToString(), value) as FixedParamBinder;

            public T Get<T>(Params param)
                => (T)Get(param.ToString());

            public FixedParamBinder Delete(Params param)
                => Delete(param.ToString()) as FixedParamBinder;

            public DirectionType Direction { get => Get<DirectionType>(Params.Direction); set => Set(Params.Direction, value); }
            public Vector2Int PaddingX { get => Get<Vector2Int>(Params.PaddingX); set => Set(Params.PaddingX, value); }
            public Vector2Int PaddingY { get => Get<Vector2Int>(Params.PaddingY); set => Set(Params.PaddingY, value); }
            public float Spacing { get => Get<float>(Params.Spacing); set => Set(Params.Spacing, value); }
            public TextAnchor ChildAlignment { get => Get<TextAnchor>(Params.ChildAlignment); set => Set(Params.ChildAlignment, value); }
            public bool ControllChildWidth { get => Get<bool>(Params.ControllChildWidth); set => Set(Params.ControllChildWidth, value); }
            public bool ControllChildHeight { get => Get<bool>(Params.ControllChildHeight); set => Set(Params.ControllChildHeight, value); }
            public bool UseChildScaleX { get => Get<bool>(Params.UseChildScaleX); set => Set(Params.UseChildScaleX, value); }
            public bool UseChildScaleY { get => Get<bool>(Params.UseChildScaleY); set => Set(Params.UseChildScaleY, value); }
            public bool ChildForceExpandWidth { get => Get<bool>(Params.ChildForceExpandWidth); set => Set(Params.ChildForceExpandWidth, value); }
            public bool ChildForceExpandHeight { get => Get<bool>(Params.ChildForceExpandHeight); set => Set(Params.ChildForceExpandHeight, value); }

            public override void Update(Model model, IViewObject viewObj)
            {
                var layoutView = viewObj as HVLayoutGroupViewObject;
                //Directionの値が変わったら、CurrentLayoutの参照先が切り替わるので先に下のコードを実行
                if (Contains(Params.Direction)) layoutView.Direction = Direction;
                var layout = layoutView.CurrentLayoutGroup;
                if (layout == null) return;
                if (Contains(Params.PaddingX) || Contains(Params.PaddingY))
                {
                    var padding = layout.padding;
                    if (Contains(Params.PaddingX)) { padding.left = PaddingX.x; padding.right = PaddingX.y; }
                    if (Contains(Params.PaddingY)) { padding.top = PaddingY.x; padding.bottom = PaddingY.y; }
                    layout.padding = padding;
                }
                if (Contains(Params.Spacing)) layout.spacing = Spacing;
                if (Contains(Params.ChildAlignment)) layout.childAlignment = ChildAlignment;
                if (Contains(Params.ControllChildHeight)) layout.childControlWidth = ControllChildWidth;
                if (Contains(Params.ControllChildWidth)) layout.childControlHeight = ControllChildHeight;
                if (Contains(Params.UseChildScaleX)) layout.childScaleWidth = UseChildScaleX;
                if (Contains(Params.UseChildScaleY)) layout.childScaleHeight = UseChildScaleY;
                if (Contains(Params.ChildForceExpandWidth)) layout.childForceExpandWidth = ChildForceExpandWidth;
                if (Contains(Params.ChildForceExpandHeight)) layout.childForceExpandHeight = ChildForceExpandHeight;

            }

            #region CanvasViewObject.IAppendableViewObjectParamBinder
            public CanvasViewObject.IAppendableViewObject Append(GameObject target)
            {
                return target.AddComponent<HVLayoutGroupViewObject>();
            }
            #endregion
        }
    }
}
