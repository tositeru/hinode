using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    #region Parent
    public interface ITransformParentViewLayout : IViewLayout
    {
        Transform TransformParentLayout { get; set; }
        Transform SelfTransform { get; }
        RecieverSelector TransformParentLayoutSelector { get; set; }
    }

    public class TransformParentViewLayoutAccessor : IViewLayoutAccessor
    {
        public override System.Type ViewLayoutType { get => typeof(ITransformParentViewLayout); }
        public override System.Type ValueType { get => typeof(Transform); }

        protected override object GetImpl(IViewObject viewObj)
        {
            return (viewObj as ITransformParentViewLayout).TransformParentLayout;
        }

        protected override void SetImpl(object value, IViewObject viewObj)
        {
            Debug.Log("pass SetImpl");
            var layout = (viewObj as ITransformParentViewLayout);
            if (value is Transform)
            {
                layout.TransformParentLayout = (Transform)value;
            }
            else if(value is ModelViewSelector)
            {
                var selector = value as ModelViewSelector;
                var binderInstanceMap = viewObj.UseBinderInstance != null
                    ? viewObj.UseBinderInstance.UseInstanceMap
                    : null;
                var parents = selector.GetEnumerable(viewObj.UseModel, binderInstanceMap)
                    .Where(_o => _o is MonoBehaviour
                        || _o is ITransformParentViewLayout)
                    .Select(_o => {
                        if (_o is MonoBehaviour) return (_o as MonoBehaviour).transform;
                        if (_o is ITransformParentViewLayout) return (_o as ITransformParentViewLayout).SelfTransform;
                        return null;
                    });
                if(parents.Any())
                {
                    var parent = parents.First();
                    layout.TransformParentLayout = layout.SelfTransform != parent
                        ? parent
                        : null;
                    if(2 <= parents.Count())
                    {
                        Debug.LogWarning($"複数のModelViewがマッチしました。初めに見つかったものを使用します。 model={viewObj.UseModel}, viewObj={viewObj.GetType()}");
                    }
                }
                else
                {
                    Debug.Log($"セレクタがマッチしませんでした。セレクタを設定を変更するか、クエリ対象となるModel,IViewObjectがMonoBehaviourかITransformParentViewLayoutを継承しているかどうか確認してください。{selector}");
                    layout.TransformParentLayout = null;
                }
            }
        }

        public override bool IsVaildValue(object value)
        {
            return base.IsVaildValue(value)
                || value.GetType().Equals(typeof(ModelViewSelector));
        }

    }
    #endregion

    //
    // 以下のクラス定義はHinode/Editors/Assets/MVC/TransformViewLayoutTemplate.assetから生成されています。
    //

    #region Pos
    public interface ITransformPosViewLayout : IViewLayout
    {
        Vector3 TransformPosLayout { get; set; }
    }

    public class TransformPosViewLayoutAccessor : IViewLayoutAccessor
    {
        public override System.Type ViewLayoutType { get => typeof(ITransformPosViewLayout); }
        public override System.Type ValueType { get => typeof(Vector3); }

        protected override object GetImpl(IViewObject viewObj)
        {
            return (viewObj as ITransformPosViewLayout).TransformPosLayout;
        }

        protected override void SetImpl(object value, IViewObject viewObj)
        {
            (viewObj as ITransformPosViewLayout).TransformPosLayout = (Vector3)value;
        }
    }
    #endregion
    #region Rotate
    public interface ITransformRotateViewLayout : IViewLayout
    {
        Vector3 TransformRotateLayout { get; set; }
    }

    public class TransformRotateViewLayoutAccessor : IViewLayoutAccessor
    {
        public override System.Type ViewLayoutType { get => typeof(ITransformRotateViewLayout); }
        public override System.Type ValueType { get => typeof(Vector3); }

        protected override object GetImpl(IViewObject viewObj)
        {
            return (viewObj as ITransformRotateViewLayout).TransformRotateLayout;
        }

        protected override void SetImpl(object value, IViewObject viewObj)
        {
            (viewObj as ITransformRotateViewLayout).TransformRotateLayout = (Vector3)value;
        }
    }
    #endregion
    #region LocalPos
    public interface ITransformLocalPosViewLayout : IViewLayout
    {
        Vector3 TransformLocalPosLayout { get; set; }
    }

    public class TransformLocalPosViewLayoutAccessor : IViewLayoutAccessor
    {
        public override System.Type ViewLayoutType { get => typeof(ITransformLocalPosViewLayout); }
        public override System.Type ValueType { get => typeof(Vector3); }

        protected override object GetImpl(IViewObject viewObj)
        {
            return (viewObj as ITransformLocalPosViewLayout).TransformLocalPosLayout;
        }

        protected override void SetImpl(object value, IViewObject viewObj)
        {
            (viewObj as ITransformLocalPosViewLayout).TransformLocalPosLayout = (Vector3)value;
        }
    }
    #endregion
    #region LocalRotate
    public interface ITransformLocalRotateViewLayout : IViewLayout
    {
        Vector3 TransformLocalRotateLayout { get; set; }
    }

    public class TransformLocalRotateViewLayoutAccessor : IViewLayoutAccessor
    {
        public override System.Type ViewLayoutType { get => typeof(ITransformLocalRotateViewLayout); }
        public override System.Type ValueType { get => typeof(Vector3); }

        protected override object GetImpl(IViewObject viewObj)
        {
            return (viewObj as ITransformLocalRotateViewLayout).TransformLocalRotateLayout;
        }

        protected override void SetImpl(object value, IViewObject viewObj)
        {
            (viewObj as ITransformLocalRotateViewLayout).TransformLocalRotateLayout = (Vector3)value;
        }
    }
    #endregion
    #region LocalScale
    public interface ITransformLocalScaleViewLayout : IViewLayout
    {
        Vector3 TransformLocalScaleLayout { get; set; }
    }

    public class TransformLocalScaleViewLayoutAccessor : IViewLayoutAccessor
    {
        public override System.Type ViewLayoutType { get => typeof(ITransformLocalScaleViewLayout); }
        public override System.Type ValueType { get => typeof(Vector3); }

        protected override object GetImpl(IViewObject viewObj)
        {
            return (viewObj as ITransformLocalScaleViewLayout).TransformLocalScaleLayout;
        }

        protected override void SetImpl(object value, IViewObject viewObj)
        {
            (viewObj as ITransformLocalScaleViewLayout).TransformLocalScaleLayout = (Vector3)value;
        }
    }
    #endregion
}
