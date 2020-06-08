using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode.MVC
{
    public enum TransformViewLayoutName
    {
        parent,
        pos,
        rotate,
        localPos,
        localRotate,
        localScale,
    }

    #region Parent
    public interface ITransformParentViewLayout : IViewLayout
    {
        Transform TransformParentLayout { get; set; }
        Transform SelfTransform { get; }

        //#region IViewLayout interface
        //public ViewLayoutUpdateTimeing UpdateTiming { get => ViewLayoutUpdateTimeing.Always; }
        //#endregion
    }

    public class TransformParentViewLayoutAccessor : IViewLayoutAccessor
    {
        public override System.Type ViewLayoutType { get => typeof(ITransformParentViewLayout); }
        public override System.Type ValueType { get => typeof(Transform); }
        public override ViewLayoutAccessorUpdateTiming UpdateTiming { get => ViewLayoutAccessorUpdateTiming.Always; }

        protected override object GetImpl(object viewLayoutObj)
        {
            return (viewLayoutObj as ITransformParentViewLayout).TransformParentLayout;
        }

        protected override void SetImpl(object value, object viewLayoutObj)
        {
            var layout = (viewLayoutObj as ITransformParentViewLayout);
            if (value is Transform)
            {
                layout.TransformParentLayout = (Transform)value;

                SiblingOrderViewLayoutAccessor.Insert(layout.TransformParentLayout, viewLayoutObj);
            }
            else if(value is ModelViewSelector)
            {
                var viewObj = IViewLayoutAccessor.GetViewObject(viewLayoutObj);
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
                        Logger.LogWarning(Logger.Priority.High, () => 
                            $"複数のModelViewがマッチしました。初めに見つかったものを使用します。 model={viewObj.UseModel}, viewLayoutObj={viewLayoutObj.GetType()}");
                    }

                    SiblingOrderViewLayoutAccessor.Insert(parent, viewLayoutObj);
                }
                else
                {
                    Logger.LogWarning(Logger.Priority.High, () =>
                        $"セレクタがマッチしませんでした。セレクタを設定を変更するか、クエリ対象となるModel,IViewObjectがMonoBehaviourかITransformParentViewLayoutを継承しているかどうか確認してください。{selector}");
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
        public override ViewLayoutAccessorUpdateTiming UpdateTiming { get => ViewLayoutAccessorUpdateTiming.AtOnlyModel; }

        protected override object GetImpl(object viewLayoutObj)
        {
            return (viewLayoutObj as ITransformPosViewLayout).TransformPosLayout;
        }

        protected override void SetImpl(object value, object viewLayoutObj)
        {
            (viewLayoutObj as ITransformPosViewLayout).TransformPosLayout = (Vector3)value;
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
        public override ViewLayoutAccessorUpdateTiming UpdateTiming { get => ViewLayoutAccessorUpdateTiming.AtOnlyModel; }

        protected override object GetImpl(object viewLayoutObj)
        {
            return (viewLayoutObj as ITransformRotateViewLayout).TransformRotateLayout;
        }

        protected override void SetImpl(object value, object viewLayoutObj)
        {
            (viewLayoutObj as ITransformRotateViewLayout).TransformRotateLayout = (Vector3)value;
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
        public override ViewLayoutAccessorUpdateTiming UpdateTiming { get => ViewLayoutAccessorUpdateTiming.AtOnlyModel; }

        protected override object GetImpl(object viewLayoutObj)
        {
            return (viewLayoutObj as ITransformLocalPosViewLayout).TransformLocalPosLayout;
        }

        protected override void SetImpl(object value, object viewLayoutObj)
        {
            (viewLayoutObj as ITransformLocalPosViewLayout).TransformLocalPosLayout = (Vector3)value;
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
        public override ViewLayoutAccessorUpdateTiming UpdateTiming { get => ViewLayoutAccessorUpdateTiming.AtOnlyModel; }

        protected override object GetImpl(object viewLayoutObj)
        {
            return (viewLayoutObj as ITransformLocalRotateViewLayout).TransformLocalRotateLayout;
        }

        protected override void SetImpl(object value, object viewLayoutObj)
        {
            (viewLayoutObj as ITransformLocalRotateViewLayout).TransformLocalRotateLayout = (Vector3)value;
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
        public override ViewLayoutAccessorUpdateTiming UpdateTiming { get => ViewLayoutAccessorUpdateTiming.AtOnlyModel; }

        protected override object GetImpl(object viewLayoutObj)
        {
            return (viewLayoutObj as ITransformLocalScaleViewLayout).TransformLocalScaleLayout;
        }

        protected override void SetImpl(object value, object viewLayoutObj)
        {
            (viewLayoutObj as ITransformLocalScaleViewLayout).TransformLocalScaleLayout = (Vector3)value;
        }
    }
    #endregion
}
