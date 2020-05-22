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
    [AvailableModelViewParamBinder(typeof(HVLayoutGroupViewObject.FixedParamBinder))]
    [DisallowMultipleComponent()]
    public class HVLayoutGroupViewObject : RectTransformViewObject
        , RectTransformViewObject.IOptionalViewObject
    {
        public static new HVLayoutGroupViewObject Create(string name = "HVLayoutGroup")
        {
            var obj = new GameObject(name, typeof(RectTransform));
            return obj.AddComponent<HVLayoutGroupViewObject>();
        }

        DirectionType _direction;
        HorizontalOrVerticalLayoutGroup _currentLayoutGroup;
        Coroutine _changeLayoutGroupCoroutine;

        public DirectionType Direction
        {
            get => _direction;
            set
            {
                if (_direction == value) return;
                this.SafeStartCoroutine(ref _changeLayoutGroupCoroutine, ChangeLayoutGroup(value));
            }
        }
        HorizontalOrVerticalLayoutGroup CurrentLayoutGroup
        {
            get
            {
                if (_currentLayoutGroup == null)
                {
                    _currentLayoutGroup = CreateLayoutGroup(Direction);
                }
                return _currentLayoutGroup;
            }
            set => _currentLayoutGroup = value;
        }

        IEnumerator ChangeLayoutGroup(DirectionType newDirType)
        {
            if (newDirType == _direction) yield break;
            _direction = newDirType;

            var prev = CurrentLayoutGroup;
            Destroy(prev);
            var padding = prev.padding;
            var spacing = prev.spacing;
            var childAlignment = prev.childAlignment;
            var childControlWidth = prev.childControlWidth;
            var childControlHeight = prev.childControlHeight;
            var childScaleWidth = prev.childScaleWidth;
            var childScaleHeight = prev.childScaleHeight;
            var childForceExpandWidth = prev.childForceExpandWidth;
            var childForceExpandHeight = prev.childForceExpandHeight;

            yield return new WaitForEndOfFrame();
            var currentLayoutGroup = CreateLayoutGroup(newDirType);
            //パラメータを継承する
            if (prev != currentLayoutGroup)
            {
                currentLayoutGroup.padding = padding;
                currentLayoutGroup.spacing = spacing;
                currentLayoutGroup.childAlignment = childAlignment;
                currentLayoutGroup.childControlWidth = childControlWidth;
                currentLayoutGroup.childControlHeight = childControlHeight;
                currentLayoutGroup.childScaleWidth = childScaleWidth;
                currentLayoutGroup.childScaleHeight = childScaleHeight;
                currentLayoutGroup.childForceExpandWidth = childForceExpandWidth;
                currentLayoutGroup.childForceExpandHeight = childForceExpandHeight;
            }
            _changeLayoutGroupCoroutine = null;
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

        public new class FixedParamBinder : RectTransformViewObject.FixedParamBinder
            , RectTransformViewObject.IOptionalViewObjectParamBinder
        {
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

            protected override void UpdateImpl(Model model, IViewObject viewObj)
            {
                var layoutView = viewObj as HVLayoutGroupViewObject;
                UpdateParams(layoutView);
            }

            #region RectTransform.IOptionalViewObjectParamBinder
            public RectTransformViewObject.IOptionalViewObject AppendTo(GameObject target)
            {
                return target.AddComponent<HVLayoutGroupViewObject>();
            }
            #endregion

            ////@@ Packages/com.tositeru.hinode/Editor/Assets/MVC/Views/HVLayoutGroupViewObject/DefineParamsTemplate.asset
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

void UpdateParams(HVLayoutGroupViewObject layoutView)
{
var layout = layoutView.CurrentLayoutGroup;
if (layout == null) return;

Vector2Int paddingX = Vector2Int.zero;
Vector2Int paddingY = Vector2Int.zero;
                //Directionの値が変わったら、CurrentLayoutの参照先が切り替わるので先に下のコードを実行
if (Contains(Params.Direction)) layoutView.Direction = Direction;
if (Contains(Params.PaddingX)) paddingX = PaddingX;
if (Contains(Params.PaddingY)) paddingY = PaddingY;
if (Contains(Params.Spacing)) layout.spacing = Spacing;
if (Contains(Params.ChildAlignment)) layout.childAlignment = ChildAlignment;
if (Contains(Params.ControllChildWidth)) layout.childControlWidth = ControllChildWidth;
if (Contains(Params.ControllChildHeight)) layout.childControlHeight = ControllChildHeight;
if (Contains(Params.UseChildScaleX)) layout.childScaleWidth = UseChildScaleX;
if (Contains(Params.UseChildScaleY)) layout.childScaleHeight = UseChildScaleY;
if (Contains(Params.ChildForceExpandWidth)) layout.childForceExpandWidth = ChildForceExpandWidth;
if (Contains(Params.ChildForceExpandHeight)) layout.childForceExpandHeight = ChildForceExpandHeight;
if (Contains(Params.PaddingX) || Contains(Params.PaddingY))
{
    var padding = layout.padding;
    if (Contains(Params.PaddingX)) { padding.left = paddingX.x; padding.right = paddingX.y; }
    if (Contains(Params.PaddingY)) { padding.top = paddingY.x; padding.bottom = paddingY.y; }
    layout.padding = padding;
}

}

////-- Finish Packages/com.tositeru.hinode/Editor/Assets/MVC/Views/HVLayoutGroupViewObject/DefineParamsTemplate.asset
        }
    }
}
