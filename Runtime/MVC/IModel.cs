using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// Model View ControllerのModelに当たるもの
    /// </summary>
    public interface IModel
    {
        /// <summary>
        /// 名前
        /// </summary>
        string Name { get; set; }

        ///// <summary>
        ///// UnityのGameObjectのTagと対応しているプロパティ
        ///// </summary>
        //string Tag { get; set; }

        ///// <summary>
        ///// UnityのGameObjetctのLayerと対応しているプロパティ
        ///// </summary>
        //string LayerName { get; set; }

        /// <summary>
        /// 論理ID.処理の時の識別用と使用してください
        /// </summary>
        ModelIDList LogicalID { get; set; }

        /// <summary>
        /// スタイル用のID.HTMLのclass的なものです。
        /// </summary>
        ModelIDList StylingID { get; set; }

        /// <summary>
        /// 親Model
        /// </summary>
        IModel Parent { get; set; }

        /// <summary>
        /// 子供Model
        /// </summary>
        IEnumerable<IModel> Children { get; set; }

        /// <summary>
        /// 子の個数
        /// </summary>
        int ChildCount { get; }

        /// <summary>
        /// 子を追加する
        /// </summary>
        /// <param name="children"></param>
        void AddChildren(params IModel[] children);

        /// <summary>
        /// 子を追加する
        /// </summary>
        /// <param name="children"></param>
        void AddChildren(IEnumerable<IModel> children);

        /// <summary>
        /// 子を削除する
        /// </summary>
        /// <param name="children"></param>
        void RemoveChildren(params IModel[] children);

        /// <summary>
        /// 子を削除する
        /// </summary>
        /// <param name="children"></param>
        void RemoveChildren(IEnumerable<IModel> children);

        /// <summary>
        /// 子を全て削除する
        /// </summary>
        void ClearChildren();
    }

    public static class ModelExtensions
    {
        const string PARENT_PATH = "..";
        const char LOGICAL_PREFIX_CHAR = '#';
        const char STYLE_PREFIX_CHAR = '.';

        /// <summary>
        /// 指定した添字の子要素を返します
        /// </summary>
        /// <param name="target"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static IModel GetChild(this IModel target, int index)
        {
            return target.Children.ElementAt(index);
        }

        /// <summary>
        /// クエリによる親または子モデルの取得
        /// </summary>
        /// <param name="target"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IEnumerable<IModel> QueryParentOrChildren(this IModel target, string query)
        {
            return new QueryParentOrChildrenEnumerable(target, query);
        }

        class QueryParentOrChildrenEnumerable : IEnumerable<IModel>, IEnumerable
        {
            IModel _target;
            string _query;
            public QueryParentOrChildrenEnumerable(IModel target, string query)
            {
                _target = target;
                _query = query;
            }

            public IEnumerator<IModel> GetEnumerator()
            {
                if (PARENT_PATH == _query)
                {
                    yield return _target.Parent;
                    yield break;
                }

                var queryTerms = _query.Split(' ');
                foreach (var child in _target.Children)
                {
                    bool doMatch = true;
                    foreach(var term in queryTerms)
                    {
                        switch(term[0])
                        {
                            case LOGICAL_PREFIX_CHAR:
                                doMatch &= child.LogicalID.Any(_id => term.Contains(_id));
                                break;
                            case STYLE_PREFIX_CHAR:
                                doMatch &= child.StylingID.Any(_id => term.Contains(_id));
                                break;
                            default:
                                doMatch &= child.Name == term;
                                break;
                        }
                        if (!doMatch) break;
                    }

                    if (doMatch) yield return child;
                }
            }

            IModel GetNext(IModel now, int nextChildIndex = 0)
            {
                if (nextChildIndex >= now.ChildCount)
                {
                    return now == _target
                        ? null
                        : GetNext(now.Parent, now.GetSiblingIndex() + 1);
                }
                if (now.ChildCount > 0)
                {
                    return now.GetChild(nextChildIndex);
                }
                if (now == _target)
                {
                    return null;
                }
                if (now.GetSiblingIndex() < now.Parent.ChildCount - 1)
                {
                    return now.Parent.GetChild(now.GetSiblingIndex() + 1);
                }
                return GetNext(now.Parent, now.GetSiblingIndex() + 1);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }


        /// <summary>
        /// 親モデルの子供の中の添字を返します
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static int GetSiblingIndex(this IModel target)
        {
            if (null == target.Parent) return -1;
            var it = target.Parent.Children.Zip(Enumerable.Range(0, target.Parent.ChildCount), (c, i) => (child: c, index: i))
                .FirstOrDefault(_t => _t.child == target);
            return it.child != null ? it.index : -1;
        }

        /// <summary>
        /// Model階層を表すパスを返す
        /// </summary>
        public static string Path(this IModel target)
        {
            var path = target.Name;
            var model = target.Parent;
            while (model != null)
            {
                path = model.Name + "/" + path;
                model = model.Parent;
            }
            return path;
        }

        /// <summary>
        /// 指定したモデルをルートとして階層内の全てのModelを探索する。(自分自身も含みます)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static IEnumerable<IModel> GetHierarchyEnumerable(this IModel model)
        {
            return new HierarchyEnumerable(model);
        }

        class HierarchyEnumerable : IEnumerable<IModel>, IEnumerable
        {
            IModel _target;
            public HierarchyEnumerable(IModel target)
            {
                _target = target;
            }

            public IEnumerator<IModel> GetEnumerator()
            {
                var it = _target;
                while (it != null)
                {
                    yield return it;
                    it = GetNext(it);
                }
            }

            IModel GetNext(IModel now, int nextChildIndex = 0)
            {
                if (nextChildIndex >= now.ChildCount)
                {
                    return now == _target
                        ? null
                        : GetNext(now.Parent, now.GetSiblingIndex() + 1);
                }
                if (now.ChildCount > 0)
                {
                    return now.GetChild(nextChildIndex);
                }
                if (now == _target)
                {
                    return null;
                }
                if (now.GetSiblingIndex() < now.Parent.ChildCount - 1)
                {
                    return now.Parent.GetChild(now.GetSiblingIndex() + 1);
                }
                return GetNext(now.Parent, now.GetSiblingIndex() + 1);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="queryPath"></param>
        /// <returns></returns>
        static public IEnumerable<IModel> Query(this IModel model, string queryPath)
        {
            return new QueryEnumerable(model, queryPath);
        }

        class QueryEnumerable : IEnumerable<IModel>, IEnumerable
        {
            IModel _target;
            string _queryPath;
            public QueryEnumerable(IModel target, string queryPath)
            {
                _target = target;
                _queryPath = queryPath;
            }

            public IEnumerator<IModel> GetEnumerator()
            {
                return new Enumerator(_target, _queryPath);
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            class Enumerator : IEnumerator<IModel>, IEnumerator, System.IDisposable
            {
                IModel _target;
                string _queryPath;
                IEnumerator<IModel> _enumerator;
                public Enumerator(IModel target, string queryPath)
                {
                    _target = target;
                    _queryPath = queryPath;
                    Reset();
                }
                public IModel Current => _enumerator.Current;
                object IEnumerator.Current => Current;
                public void Dispose() => _enumerator.Dispose();
                public bool MoveNext() => _enumerator.MoveNext();
                public void Reset() => _enumerator = Traverse(_queryPath);
                IEnumerator<IModel> Traverse(string queryPath)
                {
                    var pathTerms = queryPath.Split('/');
                    if (!pathTerms.Any()) yield break;

                    Debug.Log($"debug -- terms={pathTerms.Aggregate((_sum, _cur) => _sum + "," + _cur)}");

                    //"name";
                    //"A/B/C";
                    //"/A/B/C";
                    //"A/#logicalName";
                    //"A/.styleName";
                    //"A/.styleName .styleName2";
                    //"../";
                    // root_keyword = "/"
                    // name = letter, {character}
                    // letter = /a-zA-Z_/
                    // digit = /0-9/
                    // character = letter | digit
                    // space = space_letter, {space_letter}
                    // space_letter = /\s/
                    // logical_prefix = #
                    // style_prefix = "."
                    // parent_identity = ".", "."
                    // path_separator = "/"
                    // path = root_keyword, identity_sequence, { path_separator, identity_sequence } (* /name , /name/B/C *)
                    //      | identity_sequence, { path_separator, identity_sequence } (* name name/name *)
                    // identity_sequence = identity, {space identity}
                    // identity = name
                    //          | logical_prefix, name
                    //          | style_prefix, name
                    //          | parent_identity
                    // 

                }
            }
        }
    }

    /// <summary>
    /// ModelのIDのリスト
    /// </summary>
    public class ModelIDList : List<string>
    {
        public ModelIDList() { }
        public ModelIDList(params string[] ids) : base(ids.AsEnumerable()) { }
        public ModelIDList(IEnumerable<string> ids) : base(ids) { }

        public void Add(params string[] ids)
        {
            AddRange(ids.AsEnumerable());
        }

        public void Remove(params string[] ids)
        {
            foreach(var id in ids)
            {
                Remove(id);
            }
        }
    }

    public class Model : IModel
    {
        IModel _parent;
        List<IModel> _children = new List<IModel>();

        public string Name { get; set; }

        //public string Tag { get; set; }
        //public string LayerName { get; set; }

        public ModelIDList LogicalID { get; set; } = new ModelIDList();
        public ModelIDList StylingID { get; set; } = new ModelIDList();

        public IModel Parent
        {
            get => _parent;
            set
            {
                if(_parent != null)
                {
                    _parent.RemoveChildren(this);
                }
                _parent = value;
                if(_parent != null)
                {
                    _parent.AddChildren(this);
                }
            }
        }
        public IEnumerable<IModel> Children
        {
            get => _children;
            set
            {
                ClearChildren();
                AddChildren(value);
            }
        }

        public int ChildCount { get => _children.Count(); }

        public void AddChildren(params IModel[] children) => AddChildren(children.AsEnumerable());
        public void AddChildren(IEnumerable<IModel> children)
        {
            foreach(var child in children
                .Where(_c => _c.Parent != this))
            {
                child.Parent = this;
            }

            foreach(var child in children
                .Where(_c => !_children.Contains(_c)))
            {
                _children.Add(child);
            }
        }

        public void RemoveChildren(params IModel[] children)
            => RemoveChildren(children.AsEnumerable());
        public void RemoveChildren(IEnumerable<IModel> children)
        {
            foreach(var child in children
                .Where(_c => _c.Parent == this))
            {
                child.Parent = null;
            }
        }

        public void ClearChildren()
            => RemoveChildren(_children);

    }
}
