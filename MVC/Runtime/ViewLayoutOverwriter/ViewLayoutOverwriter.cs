using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode.MVC
{
    /// <summary>
    /// <seealso cref="ViewLayoutSelector"/>
    /// <seealso cref="ViewLayoutValueDictionary"/>
    /// </summary>
    public class ViewLayoutOverwriter : System.IDisposable
    {
        Dictionary<ViewLayoutSelector, ViewLayoutValueDictionary> _layoutValueDicts = new Dictionary<ViewLayoutSelector, ViewLayoutValueDictionary>();

        public ViewLayoutOverwriter()
        {

        }

        public ViewLayoutOverwriter Add(ViewLayoutSelector selector, ViewLayoutValueDictionary valueDict)
        {
            if (!_layoutValueDicts.ContainsKey(selector))
            {
                _layoutValueDicts.Add(selector, valueDict);
            }
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="viewObj"></param>
        /// <returns>クエリの優先順位が高いViewLayoutValueDictionaryが先頭の方にきます。</returns>
        public IEnumerable<IReadOnlyViewLayoutValueDictionary> MatchLayoutValueDicts(Model model, IViewObject viewObj)
        {
            return _layoutValueDicts
                .Where(_t => _t.Key.DoMatch(model, viewObj))
                .Select(_t => (viewLayoutDict: _t.Value, priority: model.GetQueryPathPriority(_t.Key.Query)))
                .Where(_t => !_t.priority.IsEmpty)
                .OrderByDescending(_t => _t.priority)
                .Select(_t => _t.viewLayoutDict)
                ;
        }

        #region System.IDisposable interface
        public void Dispose()
        {
            _layoutValueDicts.Clear();
        }
        #endregion
    }
}
