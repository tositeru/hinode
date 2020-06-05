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
    public class ViewLayoutSelector
    {
        List<string> _childViewIdentities = new List<string>();

        /// <summary>
        /// セレクタ対象となるModelのQuery.
        /// 親子階層を指定することはできないようにしています.
        /// Model#DoMatchQueryに指定するものになります.
        /// </summary>
        public string Query { get; }
        public string ViewID { get; }
        public IEnumerable<string> ChildViewIdentities { get => _childViewIdentities; }
        public bool HasChildViewId { get => _childViewIdentities.Any(); }

        public ViewLayoutSelector(string query, string viewID)
        {
            Query = query;
            ModelViewBinder.BindInfo.AssertViewID(viewID);
            ViewID = viewID == null ? "" : viewID;

            if (ViewID.Contains('.'))
            {
                var ids = ViewID.Split('.');
                ViewID = ids[0];
                _childViewIdentities = ids.Skip(1).ToList();
            }
            else
            {
                ViewID = ViewID;
            }
        }

        public bool DoMatch(Model model, IViewObject viewObj)
        {
            Assert.IsNotNull(model);
            if (viewObj == null)
            {
                return model.DoMatchQuery(Query)
                    && ViewID == "";
            }
            else
            {
                Assert.AreEqual(model, viewObj.UseModel);
                Assert.IsNotNull(viewObj.UseBindInfo);

                bool doMatchChild = HasChildViewId
                    ? viewObj.QueryChild(ChildViewIdentities) != null
                    : true;

                return model.DoMatchQuery(Query)
                    && (ViewID == "" || (viewObj.UseBindInfo.ID == ViewID && doMatchChild));
            }
        }

        #region System.IEquatable<ViewLayoutSelector> interface
        public bool Equals(ViewLayoutSelector other)
            => Query == other.Query
            && ViewID == other.ViewID;

        public override bool Equals(object obj)
            => obj is ViewLayoutSelector
            ? Equals(obj as ViewLayoutSelector)
            : false;
        public override int GetHashCode()
        {
            return Query.GetHashCode() ^ ViewID.GetHashCode();
        }
        #endregion

        public override string ToString()
        {
            var ChildID = HasChildViewId
                ? ChildViewIdentities.Aggregate(".", (_s, _c) => _s + _c + ".")
                : "";
            return $"{Query}:{ViewID}{ChildID}";
        }
    }
}

