using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode.MVC
{
    [RequireComponent(typeof(RectTransform))]
    [AvailableModelViewParamBinder(typeof(RectTransformViewObject.FixedParamBinder))]
    public class RectTransformViewObject : MonoBehaviourViewObject
        , IEnableToHaveOptionalViewObjects<RectTransformViewObject.IOptionalViewObject, RectTransformViewObject.IOptionalViewObjectParamBinder>
    {
        public static new RectTransformViewObject Create(string name = "rectTransformViewObj")
        {
            var obj = new GameObject(name, typeof(RectTransform));
            return obj.AddComponent<RectTransformViewObject>();
        }
        RectTransform _R;
        public RectTransform R { get => _R != null ? _R : _R = gameObject.GetOrAddComponent<RectTransform>(); }

        #region IEnableToHaveOptionalViewObjects interface
        Dictionary<IOptionalViewObjectParamBinder, IOptionalViewObject> _optionalViewObjects = new Dictionary<IOptionalViewObjectParamBinder, IOptionalViewObject>();
        public IReadOnlyDictionary<IOptionalViewObjectParamBinder, IOptionalViewObject> OptionalViewObjectDict { get => _optionalViewObjects; }

        public IEnableToHaveOptionalViewObjects<IOptionalViewObject, IOptionalViewObjectParamBinder> AddOptionalViewObject(IOptionalViewObjectParamBinder paramBinder, IOptionalViewObject optionalViewObject)
        {
            _optionalViewObjects.Add(paramBinder, optionalViewObject);
            return this;
        }
        #endregion

        #region MonoBehaviourViewObject class
        protected override void OnDestroyViewObj()
        {
            foreach (var optionalViewObj in _optionalViewObjects.Values)
            {
                optionalViewObj.DettachFromMainViewObject();
            }
        }

        protected override void OnUnbind()
        {
            foreach (var optionalViewObj in _optionalViewObjects.Values)
            {
                optionalViewObj.Unbind();
            }
        }
        #endregion

        /// <summary>
        /// RectTransformViewObjectを持つGameObjectに追加できるComponentを表すIViewObject
        ///
        /// LayoutGroupなどが対応しています。
        /// <seealso cref="HVLayoutGroupViewObject"/>
        /// </summary>
        public interface IOptionalViewObject : IViewObject
        {
            void DettachFromMainViewObject();
        }

        /// <summary>
        /// RectTransformViewObjectに追加したいIAppendableViewObjectをRectTransformViewObject#FixedParamBinderに登録する時に使うIModelViewParamBinder
        /// </summary>
        public interface IOptionalViewObjectParamBinder : IModelViewParamBinder
        {
            /// <summary>
            /// </summary>
            /// <param name="target"></param>
            /// <returns></returns>
            IOptionalViewObject AppendTo(GameObject target);
        }

        public class FixedParamBinder : IDictinaryModelViewParamBinder
            , IEnableToHaveOptionalViewObjectParamBinder<IOptionalViewObjectParamBinder>
        {
            protected virtual void UpdateImpl(Model model, IViewObject viewObj) { }

            //public bool Contains(Params paramType)
            //    => Contains(paramType.ToString());

            //public FixedParamBinder Set(Params param, object value)
            //    => Set(param.ToString(), value) as FixedParamBinder;
            //public FixedParamBinder Set<T>(Params param, T value)
            //    => Set(param.ToString(), value) as FixedParamBinder;

            //public T Get<T>(Params param)
            //    => (T)Get(param.ToString());

            //public FixedParamBinder Delete(Params param)
            //    => Delete(param.ToString()) as FixedParamBinder;

            #region IEnableToHaveOptionalViewObjectParamBinder interface
            List<IOptionalViewObjectParamBinder> _optionalViewObjectParamBinders = new List<IOptionalViewObjectParamBinder>();

            public IReadOnlyList<IOptionalViewObjectParamBinder> OptionalViewObjectParamBinders { get => _optionalViewObjectParamBinders; }

            public IEnableToHaveOptionalViewObjectParamBinder<IOptionalViewObjectParamBinder> AddOptionalViewObjectParamBinder(IOptionalViewObjectParamBinder optionalParamBinder)
            {
                _optionalViewObjectParamBinders.Add(optionalParamBinder);
                return this;
            }
            #endregion

            public override void Update(Model model, IViewObject viewObj)
            {
                Assert.IsTrue(viewObj is RectTransformViewObject, $"viewObj Type={viewObj.GetType()}");
                var view = viewObj as RectTransformViewObject;
                var R = view.R;
                //UpdateParams(R);

                UpdateImpl(model, viewObj);

                //Appended View ObjectのUpdate
                foreach (var optionalParamBinder in OptionalViewObjectParamBinders)
                {
                    if (!view.OptionalViewObjectDict.ContainsKey(optionalParamBinder))
                    {
                        var optionalViewObj = optionalParamBinder.AppendTo(R.gameObject);
                        Assert.IsNotNull(optionalViewObj, $"Failed to Append Option ViewObj(paramBinder={optionalParamBinder.GetType()})... view={view}");
                        view.AddOptionalViewObject(optionalParamBinder, optionalViewObj);
                    }
                    var appendedViewObj = view.OptionalViewObjectDict[optionalParamBinder];
                    optionalParamBinder.Update(model, appendedViewObj);
                }
            }

            ////@@ Packages/com.tositeru.hinode/Editor/Assets/MVC/Views/RectTransformViewObject/DefineParamsTemplate.asset
        }
    }
}
