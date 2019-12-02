using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode.MVC
{
    /// <summary>
    /// 
    /// </summary>
    public interface IViewLayoutable
    { }

    /// <summary>
    /// 
    /// </summary>
    public interface IViewLayout
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public class ViewLayouter
    {
        Dictionary<string, IViewLayoutAccessor> _layoutAccessors = new Dictionary<string, IViewLayoutAccessor>();
        Dictionary<string, IAutoViewObjectCreator> _autoCreatorDict = new Dictionary<string, IAutoViewObjectCreator>();

        public abstract class IAutoViewObjectCreator
        {
            public abstract IEnumerable<System.Type> GetSupportedIViewLayouts();
            protected abstract IAutoViewLayoutObject CreateImpl(IViewObject viewObj);
            public IAutoViewLayoutObject Create(IViewObject viewObj)
            {
                var inst = CreateImpl(viewObj);
                Assert.IsNotNull(inst);
                foreach (var supportedType in GetSupportedIViewLayouts())
                {
                    Assert.IsTrue(inst.GetType().HasInterface(supportedType),
                        $"'{inst.GetType()}' don't support IViewLayout({supportedType})... creatorType='{this.GetType()}'");
                }
                inst.Attach(viewObj);
                return inst;
            }

        }

        public IReadOnlyDictionary<string, IViewLayoutAccessor> Accessors { get => _layoutAccessors; }
        public bool DoEnableToAutoCreateViewObject { get => _autoCreatorDict.Any(); }

        public ViewLayouter(params (string keyword, IViewLayoutAccessor layout)[] layouts)
            : this(layouts.AsEnumerable())
        { }
        public ViewLayouter(IEnumerable<(string, IViewLayoutAccessor)> layouts)
        {
            AddKeywords(layouts);
        }

        public ViewLayouter AddKeywords(params (string keyword, IViewLayoutAccessor layout)[] layouts)
            => AddKeywords(layouts.AsEnumerable());
        public ViewLayouter AddKeywords(IEnumerable<(string keyword, IViewLayoutAccessor layout)> layouts)
        {
            foreach (var (keyword, layout) in layouts)
            {
                Assert.IsFalse(_layoutAccessors.ContainsKey(keyword), $"同じキーワード名が既に存在しています。keyword={keyword}");
                Assert.IsFalse(_layoutAccessors.Values.Any(_l => _l.GetType().Equals(layout.GetType())), $"同じViewLayoutAccessor型を持つキーワードは追加できません。keyword={keyword}, accessorType={layout.GetType()}");

                _layoutAccessors.Add(keyword, layout);
            }
            return this;
        }

        public bool ContainsKeyword(string keyword)
            => _layoutAccessors.ContainsKey(keyword);

        public bool IsVaildViewObject(string keyword, IViewLayoutable viewLayoutObj)
        {
            if (!ContainsKeyword(keyword)) return false;

            var accessor = _layoutAccessors[keyword];
            return accessor.IsVaildViewLayoutType(viewLayoutObj.GetType());
        }

        public bool IsVaildValue(string keyword, object value)
        {
            if (!ContainsKeyword(keyword)) return false;

            var accessor = _layoutAccessors[keyword];
            return accessor.IsVaildValue(value);
        }

        public void Set(string keyword, object value, IViewObject viewObj)
        {
            if (viewObj == null) return;

            bool doUpdate = false;
            if (IsVaildViewObject(keyword, viewObj))
            {
                doUpdate = true;
                Set(keyword, value, viewObj as IViewLayoutable);
            }
            if(viewObj.ContainsAutoViewLayoutObjects())
            {
                foreach (var autoViewObj in viewObj.GetAutoViewLayoutObjects()
                    .Where(_a => IsVaildViewObject(keyword, _a)))
                {
                    Set(keyword, value, autoViewObj);
                    doUpdate = true;
                }
            }

            if (doUpdate)
            {
                Logger.Log(Logger.Priority.Low, () => {
                    return $"ViewLayouter#Set(IViewObject) -> {viewObj}: layout=[{keyword}={value}, layoutType={_layoutAccessors[keyword].GetType()}]";
                });
            }
        }

        public void Set(string keyword, object value, IViewLayoutable viewLayoutObj)
        {
            var accessor = _layoutAccessors[keyword];
            accessor.Set(value, viewLayoutObj);
        }

        public object Get(string keyword, IViewLayoutable viewLayoutObj)
        {
            var accessor = _layoutAccessors[keyword];
            return accessor.Get(viewLayoutObj);
        }

        /// <summary>
        /// ModelViewBinderInstanceがBindされていた場合は対応しているIAutoViewLayoutObjectもチェック対象に含めます
        /// </summary>
        /// <param name="updateTimingFlags"></param>
        /// <param name="target"></param>
        /// <param name="keyAndValues"></param>
        /// <returns></returns>
        public bool DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming updateTimingFlags, IViewObject target, ViewLayoutState layoutState)
        {
            var getKeyAndValues = GetMatchKeyAndValues(updateTimingFlags, target, layoutState);
            if(target.ContainsAutoViewLayoutObjects())
            {
                var autoViewLayouts = target.UseBinderInstance?.AutoLayoutViewObjects[target];
                getKeyAndValues = getKeyAndValues.Concat(
                    autoViewLayouts.SelectMany(_o => GetMatchKeyAndValues(updateTimingFlags, _o, layoutState))
                );
            }
            return getKeyAndValues.Any();
        }

        public bool DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming updateTimingFlags, IAutoViewLayoutObject target, ViewLayoutState layoutState)
            => GetMatchKeyAndValues(updateTimingFlags, target, layoutState).Any();

        /// <summary>
        /// 一致しているLayoutを設定します。
        ///
        /// もしtargetがIAutoViewLayoutObjectを持っており、あるキーがtargetと一致していない場合は、
        /// そのキーと一致しているIAutoViewLayoutObjectに値を設定します。
        /// </summary>
        /// <param name="updateTimingFlags"></param>
        /// <param name="target"></param>
        /// <param name="keyAndValues"></param>
        public void SetAllMatchLayouts(ViewLayoutAccessorUpdateTiming updateTimingFlags, IViewObject target, ViewLayoutState viewLayoutState)
        {
            foreach (var (targetObj, value, layoutAccessor) in viewLayoutState
                .Where(_t => ContainsKeyword(_t.key))
                .Select(_t => {
                    var accessor = Accessors[_t.key];

                    object targetObj = accessor.IsVaildViewLayoutType(target.GetType())
                        ? target
                        : (object)(target.ContainsAutoViewLayoutObjects()
                            ? target.GetAutoViewLayoutObjects()
                                .FirstOrDefault(_a => accessor.IsVaildViewLayoutType(_a.GetType()))
                            : null);
                    //targetObjがnullならループから弾くようにしてください
                    return (targetObj, value: _t.value, layoutAccessor: accessor);
                })
                .Where(_t => _t.targetObj != null
                        && _t.layoutAccessor.IsVaildValue(_t.value)
                        && 0 != (updateTimingFlags & _t.layoutAccessor.UpdateTiming)
                ))
            {
                layoutAccessor.Set(value, targetObj);
            }
        }

        IEnumerable<(string key, object value)> GetMatchKeyAndValues(ViewLayoutAccessorUpdateTiming updateTimingFlags, IViewLayoutable target, ViewLayoutState layoutState)
            => layoutState.Where(_t => {
                if (!ContainsKeyword(_t.key)) return false;
                var layout = Accessors[_t.key];
                return layout.IsVaildViewLayoutType(target.GetType())
                    && layout.IsVaildValue(_t.value)
                    && 0 != (updateTimingFlags & layout.UpdateTiming);
            });

        #region AutoViewObject
        public ViewLayouter AddAutoCreateViewObject(IAutoViewObjectCreator creator, params string[] keywords)
            => AddAutoCreateViewObject(creator, keywords.AsEnumerable());
        public ViewLayouter AddAutoCreateViewObject(IAutoViewObjectCreator creator, IEnumerable<string> keywords)
        {
            Assert.IsNotNull(creator);
            var supportedLayoutTypes = creator.GetSupportedIViewLayouts();
            foreach (var key in keywords
                .Where(_k => ContainsKeyword(_k) && !_autoCreatorDict.ContainsKey(_k)))
            {
                var viewLayout = _layoutAccessors[key].ViewLayoutType;
                Assert.IsTrue(supportedLayoutTypes.Contains(viewLayout), $"Not support IViewLayout Type({viewLayout}) for AutoViewObjectCreator{creator.GetType()}... keyword={key}");
                _autoCreatorDict.Add(key, creator);
            }
            return this;
        }

        public IEnumerable<IAutoViewObjectCreator> GetAutoViewObjectCreator(IViewObject viewObj, params string[] keywords)
            => GetAutoViewObjectCreator(viewObj, keywords.AsEnumerable());
        public IEnumerable<IAutoViewObjectCreator> GetAutoViewObjectCreator(IViewObject viewObj, IEnumerable<string> keywords)
        {
            return keywords
                .Where(_k => !IsVaildViewObject(_k, viewObj))
                .Select(_k => _autoCreatorDict[_k])
                .Distinct();
        }

        public bool ContainAutoViewObjectCreator(params string[] keywords)
            => ContainAutoViewObjectCreator(keywords.AsEnumerable());
        public bool ContainAutoViewObjectCreator(IEnumerable<string> keywords)
        {
            return keywords.All(_k => _autoCreatorDict.ContainsKey(_k));
        }
        #endregion
    }
}
