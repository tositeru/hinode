using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
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
            protected abstract IViewObject CreateImpl(IViewObject viewObj);
            public IViewObject Create(IViewObject viewObj)
            {
                var inst = CreateImpl(viewObj);
                Assert.IsNotNull(inst);
                foreach(var supportedType in GetSupportedIViewLayouts())
                {
                    Assert.IsTrue(inst.GetType().DoHasInterface(supportedType),
                        $"'{inst.GetType()}' don't support IViewLayout({supportedType})... creatorType='{this.GetType()}'");
                }
                inst.UseModel = viewObj.UseModel;
                inst.UseBindInfo = viewObj.UseBindInfo;
                inst.UseBinderInstance = viewObj.UseBinderInstance;
                return inst;
            }

        }

        public IReadOnlyDictionary<string, IViewLayoutAccessor> Accessors { get => _layoutAccessors; }
        public bool DoEnableToAutoCreateViewObject { get => _autoCreatorDict.Any(); }

        public ViewLayouter(params (string keyword, IViewLayoutAccessor layout)[] layouts)
            : this(layouts.AsEnumerable())
        {}
        public ViewLayouter(IEnumerable<(string, IViewLayoutAccessor)> layouts)
        {
            AddKeywords(layouts);
        }

        public void AddKeywords(params (string keyword, IViewLayoutAccessor layout)[] layouts)
            => AddKeywords(layouts.AsEnumerable());
        public void AddKeywords(IEnumerable<(string keyword, IViewLayoutAccessor layout)> layouts)
        {
            foreach (var (keyword, layout) in layouts)
            {
                Assert.IsFalse(_layoutAccessors.ContainsKey(keyword), $"同じキーワード名が既に存在しています。keyword={keyword}");
                Assert.IsFalse(_layoutAccessors.Values.Any(_l => _l.GetType().Equals(layout.GetType())), $"同じViewLayoutAccessor型を持つキーワードは追加できません。keyword={keyword}, accessorType={layout.GetType()}");

                _layoutAccessors.Add(keyword, layout);
            }
        }

        public bool ContainsKeyword(string keyword)
            => _layoutAccessors.ContainsKey(keyword);

        public bool IsVaildViewObject(string keyword, IViewObject viewObj)
        {
            if (!ContainsKeyword(keyword)) return false;

            var accessor = _layoutAccessors[keyword];
            return accessor.IsVaildViewObject(viewObj);
        }

        public bool IsVaildValue(string keyword, object value)
        {
            if (!ContainsKeyword(keyword)) return false;

            var accessor = _layoutAccessors[keyword];
            return accessor.IsVaildValue(value);
        }

        public void Set(string keyword, object value, IViewObject viewObj)
        {
            var accessor = _layoutAccessors[keyword];
            accessor.Set(value, viewObj);
        }

        public object Get(string keyword, IViewObject viewObj)
        {
            var accessor = _layoutAccessors[keyword];
            return accessor.Get(viewObj);
        }

        public void SetAllMatchLayouts(IViewObject target, IReadOnlyDictionary<string, object> keyAndValues)
        {
            foreach (var (value, layoutAccessor) in keyAndValues
                .Where(_t => {
                    if (!ContainsKeyword(_t.Key)) return false;
                    var layout = Accessors[_t.Key];
                    return layout.IsVaildViewObject(target) && layout.IsVaildValue(_t.Value);
                })
                .Select(_t => (value: _t.Value, layout: Accessors[_t.Key])))
            {
                layoutAccessor.Set(value, target);
            }

        }

        #region AutoViewObject
        public void AddAutoCreateViewObject(IAutoViewObjectCreator creator, params string[] keywords)
            => AddAutoCreateViewObject(creator, keywords.AsEnumerable());
        public void AddAutoCreateViewObject(IAutoViewObjectCreator creator, IEnumerable<string> keywords)
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
