using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode.MVC
{
    /// <summary>
    /// Modelが更新された時に呼び出されるイベント
    /// </summary>
    /// <param name="model"></param>
    public delegate void OnUpdatedCallback(Model model);

    /// <summary>
    /// Modelの削除された時に呼び出されるイベント
    /// </summary>
    /// <param name="model"></param>
    public delegate void OnDestroyedCallback(Model model);

    /// <summary>
    /// Model#Name,Model#LogicalID,Model#StyleIDのいずれかが変更された時に呼び出されるイベント
    /// </summary>
    public delegate void OnChangedModelIdentitiesCallback(Model model);

    /// <summary>
    /// Modelの階層が変更された時のイベント
    /// typeによってmodelsに渡されるものが代わります。
    /// - ChildAdd     追加された全Model
    /// - ChildRemove  削除された全Model
    /// - ParentSet    以前の親と設定された親
    /// </summary>
    /// <param name="type"></param>
    /// <param name="model"></param>
    public delegate void OnChangedModelHierarchyCallback(ChangedModelHierarchyType type, Model target, IEnumerable<Model> models);

    /// <summary>
    /// Modelの階層が変更された時の種類
    /// </summary>
    public enum ChangedModelHierarchyType
    {
        ChildAdd,
        ChildRemove,
        ParentChange,
    }

    /// <summary>
    /// ModelのIDのリスト
    /// </summary>
    public class ModelIDList : HashSet<string>
    {
        public ModelIDList() { }
        public ModelIDList(params string[] ids) : base(ids.AsEnumerable()) { }
        public ModelIDList(IEnumerable<string> ids) : base(ids) { }

        public void Add(params string[] ids)
        {
            foreach(var id in ids)
            {
                base.Add(id);
            }
        }

        public void Remove(params string[] ids)
        {
            foreach(var id in ids)
            {
                base.Remove(id);
            }
        }
    }

    /// <summary>
    /// Model View ControllerのModelに当たるもの
    /// </summary>
    public class Model
    {
        public const string QUERY_ALL_PATH = "*";
        public const char QUERY_LOGICAL_PREFIX_CHAR = '#';
        public const char QUERY_STYLE_PREFIX_CHAR = '.';
        public const string QUERY_ONLY_AT_ROOT = "/";
        public const string QUERY_EXCULDED_AT_ROOT = "~";

        private Model _parent;
        private List<Model> _children = new List<Model>();
        private string _name = "";
        private HashSet<string> _logicalIDs = new HashSet<string>();
        private HashSet<string> _stylingIDs = new HashSet<string>();

        private SmartDelegate<OnUpdatedCallback> _onUpdatedCallback = new SmartDelegate<OnUpdatedCallback>();
        private SmartDelegate<OnDestroyedCallback> _onDestroyedCallback = new SmartDelegate<OnDestroyedCallback>();

        private SmartDelegate<OnChangedModelIdentitiesCallback> _onChangedIdentitiesCallback = new SmartDelegate<OnChangedModelIdentitiesCallback>();
        private SmartDelegate<OnChangedModelHierarchyCallback> _onChangedHierarchyCallback = new SmartDelegate<OnChangedModelHierarchyCallback>();

        public NotInvokableDelegate<OnUpdatedCallback> OnUpdated { get => _onUpdatedCallback; }
        public NotInvokableDelegate<OnDestroyedCallback> OnDestroyed { get => _onDestroyedCallback; }

        /// <summary>
        /// Model#Name,Model#LogicalID,Model#StyleIDのいずれかが変更された時に呼び出されるDelegate
        /// </summary>
        public NotInvokableDelegate<OnChangedModelIdentitiesCallback> OnChangedModelIdentities { get => _onChangedIdentitiesCallback; }

        public NotInvokableDelegate<OnChangedModelHierarchyCallback> OnChangedHierarchy { get => _onChangedHierarchyCallback; }

        public string Name
        {
            get => _name;
            set
            {
                var isSame = _name == value;
                _name = value;
                if (!isSame) InvokeChangedIdentities(this);
            }
        }

        /// <summary>
        /// Model階層を表すパスを返す
        /// </summary>
        public string GetPath()
        {
            var path = this.Name;
            var model = this.Parent;
            while (model != null)
            {
                path = model.Name + "/" + path;
                model = model.Parent;
            }
            return path;
        }

        public override string ToString()
        {
            string logicalIds = "";
            if (LogicalID.Any())
                logicalIds = LogicalID.Aggregate(" ", (_sum, _cur) => _sum + " #" + _cur);
            string stylingIds = "";
            if (StylingID.Any())
                stylingIds = StylingID.Aggregate(" ", (_sum, _cur) => _sum + " ." + _cur);
            return $"{GetPath()}{logicalIds}{stylingIds}:{GetType().FullName}";
        }
        #region OnUpdated callback
        /// <summary>
        /// 更新したことを知らせるための関数
        /// OnUpdatedイベントが発生します。
        ///
        /// デフォルト実装ではModelのもつ値が変更されたかどうかの判定は行っていませんので、
        /// もしその判定を行いたい時はoverrideして自前で実装してください。
        /// </summary>
        public virtual void DoneUpdate()
        {
            _onUpdatedCallback.Instance?.Invoke(this);
        }
        #endregion

        #region Destroy
        public static HashSet<Model> _markedDestrotModels = new HashSet<Model>();

        public static bool IsMarkedDestroy(Model model)
            => _markedDestrotModels.Contains(model);
        public static void DestoryMarkedModels()
        {
            foreach (var model in _markedDestrotModels.ToArray())
            {
                Logger.Log(Logger.Priority.Middle, () => $"Destory model={model}");
                model.Destroy();
            }
            _markedDestrotModels.Clear();
        }

        public bool IsMarkedDestory { get => Model.IsMarkedDestroy(this); }

        /// <summary>
        /// 破棄されるようにマーキングします。
        /// 実際の破棄はModel#DestoryMarkedModels()が呼び出されるまで行われません。
        /// 
        /// 破棄されるタイミングよっては他の処理で問題(EventDispatcher周り)が発生したため、マーキング形式にしています。
        /// </summary>
        public void MarkDestroy()
        {
            if (IsMarkedDestroy(this)) return;

            _markedDestrotModels.Add(this);
            foreach (var child in Children)
            {
                child.MarkDestroy();
            }
        }

        void Destroy()
        {
            Parent = null;
            ClearChildren();
            _onUpdatedCallback.Clear();
            _onChangedHierarchyCallback.Clear();
            _onChangedIdentitiesCallback.Clear();

            _onDestroyedCallback.Instance?.Invoke(this);
            _onDestroyedCallback.Clear();
        }

        #endregion

        #region Logical && Styling ID

        public IReadOnlyCollection<string> LogicalID
        {
            get => _logicalIDs;
            set
            {
                _logicalIDs.Clear();
                if(value != null) AddIDs(_logicalIDs, value.AsEnumerable());
            }
        }
        public IReadOnlyCollection<string> StylingID
        {
            get => _stylingIDs;
            set
            {
                _stylingIDs.Clear();
                if (value != null) AddIDs(_stylingIDs, value.AsEnumerable());
            }
        }

        public Model AddLogicalID(params string[] idList)
            => AddIDs(_logicalIDs, idList.AsEnumerable());
        public Model AddLogicalID(IEnumerable<string> idList)
            => AddIDs(_logicalIDs, idList.AsEnumerable());

        public Model RemoveLogicalID(params string[] idList)
            => RemoveIDs(_logicalIDs, idList.AsEnumerable());
        public Model RemoveLogicalID(IEnumerable<string> idList)
            => RemoveIDs(_logicalIDs, idList.AsEnumerable());

        public Model AddStylingID(params string[] idList)
            => AddIDs(_stylingIDs, idList.AsEnumerable());
        public Model AddStylingID(IEnumerable<string> idList)
            => AddIDs(_stylingIDs, idList.AsEnumerable());

        public Model RemoveStylingID(params string[] idList)
            => RemoveIDs(_stylingIDs, idList.AsEnumerable());
        public Model RemoveStylingID(IEnumerable<string> idList)
            => RemoveIDs(_stylingIDs, idList.AsEnumerable());

        Model AddIDs(HashSet<string> list, IEnumerable<string> idList)
        {
            bool doCallEvent = false;
            foreach (var id in TrimStart(idList, '#', '.')
                .Where(_i => !list.Contains(_i)))
            {
                list.Add(id);
                doCallEvent = true;
            }
            if (doCallEvent) InvokeChangedIdentities(this);
            return this;
        }

        Model RemoveIDs(HashSet<string> list, IEnumerable<string> idList)
        {
            bool doCallEvent = false;
            foreach (var id in TrimStart(idList, '#', '.')
                .Where(_i => list.Contains(_i)))
            {
                list.Remove(id);
                doCallEvent = true;
            }
            if (doCallEvent) InvokeChangedIdentities(this);
            return this;
        }

        IEnumerable<string> TrimStart(IEnumerable<string> idList, params char[] charList)
            => idList
                .Where(_i => _i != null && _i != "")
                .Select(_i => _i.TrimStart(charList));

        void InvokeChangedIdentities(Model model)
        {
            _onChangedIdentitiesCallback.Instance?.Invoke(model);

            if(Parent != null)
            {
                Parent.InvokeChangedIdentities(model);
            }
        }

        #endregion

        #region Parent && Children

        public Model Parent
        {
            get => _parent;
            set
            {
                if (_parent == value) return;

                var prevParent = _parent;
                _parent = value;

                if (prevParent != null)
                {
                    prevParent.RemoveChildren(this);
                }
                if(_parent != null)
                {
                    _parent.AddChildren(this);
                }

                InvokeChangedParentHierarchy(ChangedModelHierarchyType.ParentChange, this, new Model[] { prevParent, _parent });
            }
        }

        public IEnumerable<Model> Children
        {
            get => _children;
            set
            {
                ClearChildren();
                AddChildren(value);
            }
        }

        public int ChildCount { get => _children.Count(); }

        public void AddChildren(params Model[] models) => AddChildren(models.AsEnumerable());
        public void AddChildren(IEnumerable<Model> models)
        {
            var addToListModels = models.Where(_c => _c != null && !_children.Contains(_c));
            bool doCallEvent = addToListModels.Any();
            foreach (var child in addToListModels)
            {
                _children.Add(child);
                if(child.Parent != this) child.Parent = this;
            }

            if(doCallEvent)
            {
                InvokeChangedChildHierarchy(ChangedModelHierarchyType.ChildAdd, this, _children.Where(_c => models.Contains(_c)));
            }
        }

        public void RemoveChildren(params Model[] models)
            => RemoveChildren(models.AsEnumerable());
        public void RemoveChildren(IEnumerable<Model> models)
        {
            var removeFromListModels = models.Where(_c => _c != null && _children.Contains(_c));
            var doCallEvent = removeFromListModels.Any();
            foreach (var child in removeFromListModels)
            {
                _children.Remove(child);
                if(child.Parent == this) child.Parent = null;
            }
            if(doCallEvent)
            {
                InvokeChangedChildHierarchy(ChangedModelHierarchyType.ChildRemove, this, models.Where(_c => !_children.Contains(_c)));
            }
        }

        public void ClearChildren()
            => RemoveChildren(_children.ToArray());


        void InvokeChangedParentHierarchy(ChangedModelHierarchyType typeOfParent, Model target, IEnumerable<Model> models)
        {
            Assert.IsTrue(ChangedModelHierarchyType.ParentChange == typeOfParent,
                $"親に対する変更のみ実行できるようにしています... type={typeOfParent}");

            _onChangedHierarchyCallback.Instance?.Invoke(typeOfParent, target, models);

            foreach (var child in Children)
            {
                child.InvokeChangedParentHierarchy(typeOfParent, target, models);
            }
        }

        void InvokeChangedChildHierarchy(ChangedModelHierarchyType typeOfChild, Model target, IEnumerable<Model> models)
        {
            Assert.IsTrue(ChangedModelHierarchyType.ChildAdd == typeOfChild
                || ChangedModelHierarchyType.ChildRemove == typeOfChild,
                $"子に対する変更のみ実行できるようにしています... type={typeOfChild}");

            _onChangedHierarchyCallback.Instance?.Invoke(typeOfChild, target, models);

            if (Parent != null)
            {
                Parent.InvokeChangedChildHierarchy(typeOfChild, target, models);
            }
        }

        /// <summary>
        /// ルートModelを返します。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public Model GetRoot()
        {
            var m = this;
            while (m.Parent != null)
            {
                m = m.Parent;
            }
            return m;
        }

        /// <summary>
        /// 指定した添字の子要素を返します
        /// </summary>
        /// <param name="target"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public Model GetChild(int index)
        {
            return Children.ElementAt(index);
        }

        /// <summary>
        /// 親モデルの子供の中の添字を返します
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public int GetSiblingIndex()
        {
            if (null == Parent) return -1;
            return Parent._children.IndexOf(this);
        }

        #endregion

        #region Query & QueryPath
        /// <summary>
        /// クエリとマッチしているか調べる関数
        /// 親子階層をまたいでマッチしているかどうかを調べたい時はDoMatchQueryPath()を使用してください。
        /// 
        /// クエリの構文としては以下のものがあります。
        /// - query = nameKey = <任意の文字列>
        ///         | logicalKey = #<任意の文字列>
        ///         | styleKey = .<任意の文字列>
        ///         | groupingKey = (nameKey|logicalKey|styleKey) (nameKey|logicalKey|styleKey)...
        ///         | allKey = *
        /// </summary>
        /// <param name="target"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public bool DoMatchQuery(string query)
        {
            var queryTerms = query.Split(' ');
            return DoMatchQuery(queryTerms);
        }

        bool DoMatchQuery(string[] queryTerms)
        {
            bool doMatch = true;
            foreach (var term in queryTerms.Where(_t => _t != ""))
            {
                if (QUERY_ALL_PATH == term)
                {
                    doMatch = true;
                }
                else
                {
                    switch (term[0])
                    {
                        case QUERY_LOGICAL_PREFIX_CHAR:
                            doMatch &= LogicalID.Any(_id => (_id.Length + 1) == term.Length && 1 == term.IndexOf(_id));
                            break;
                        case QUERY_STYLE_PREFIX_CHAR:
                            doMatch &= StylingID.Any(_id => (_id.Length + 1) == term.Length && 1 == term.IndexOf(_id));
                            break;
                        default:
                            doMatch &= Name == term;
                            break;
                    }
                }
                if (!doMatch) break;
            }
            return doMatch;
        }

        /// <summary>
        /// クエリによる子モデルの取得
        ///  親子階層をまたいでマッチしているかどうかを調べたい時はDoMatchQueryPath()を使用してください。
        ///  
        /// クエリの構文としては以下のものがあります。
        /// - query = nameKey = <任意の文字列>
        ///         | logicalKey = #<任意の文字列>
        ///         | styleKey = .<任意の文字列>
        ///         | allChildKey = * <- 全ての子要素
        ///     
        /// またnameKey, logicalKey, styleKeyは組み合わせることが可能です。
        /// 組み合わせる時は一つの空白文字で区切るようにしてください。
        ///     - groupingKey = (nameKey|logicalKey|styleKey) (nameKey|logicalKey|styleKey)...
        /// </summary>
        /// <param name="target"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<Model> QueryChildren(string query)
        {
            return new QueryChildrenEnumerable(this, query);
        }

        class QueryChildrenEnumerable : IEnumerable<Model>, IEnumerable
        {
            Model _target;
            string _query;
            public QueryChildrenEnumerable(Model target, string query)
            {
                _target = target;
                _query = query;
            }

            public IEnumerator<Model> GetEnumerator()
            {
                if (QUERY_ALL_PATH == _query)
                {
                    foreach (var child in _target.Children)
                    {
                        yield return child;
                    }
                }
                else
                {
                    var queryTerms = _query.Split(' ');
                    foreach (var child in _target.Children
                        .Where(_c => _c.DoMatchQuery(queryTerms)))
                    {
                        yield return child;
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        /// <summary>
        /// Root Modelに到達するまでParentを辿っていくEnumerableを返します。
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IEnumerable<Model> GetTraversedRootEnumerable()
        {
            return new TraversedRootEnumerable(this);
        }

        class TraversedRootEnumerable : IEnumerable<Model>, IEnumerable
        {
            Model _target;
            public TraversedRootEnumerable(Model target)
            {
                _target = target;
            }

            public IEnumerator<Model> GetEnumerator()
            {
                var model = _target;
                while (model != null)
                {
                    yield return model;
                    model = model.Parent;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }


        /// <summary>
        /// 指定したモデルをルートとして階層内の全てのModelを探索する。(自分自身も含みます)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IEnumerable<Model> GetHierarchyEnumerable()
        {
            return new HierarchyEnumerable(this);
        }

        class HierarchyEnumerable : IEnumerable<Model>, IEnumerable
        {
            Model _target;
            public HierarchyEnumerable(Model target)
            {
                _target = target;
            }

            public IEnumerator<Model> GetEnumerator()
            {
                var it = _target;
                while (it != null)
                {
                    yield return it;
                    it = GetNext(it);
                }
            }

            Model GetNext(Model now, int nextChildIndex = 0)
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
        /// クエリパスとマッチしているか調べる関数
        /// この関数では指定したModelを検索ルートとしてマッチ判定を行います。
        /// 
        /// 指定したModelのみだけ調べたい時はDoMatchQuery()を使用してください。
        /// 
        /// クエリパスの構文としては以下のものがあります。
        ///     - queryPath = <query>/<query>/<query>...
        ///                 | /<query>/<query>/<query>... <- クエリ操作時のルートModelからのパスのみを含める
        ///                 | ~<query>/<query>/<query>... <- クエリ操作時のルートModelからのパスは除く
        ///     - query = nameKey <- Model#Nameを指定するもの
        ///             | logicalKey <- Model#LogicalIDを指定するもの
        ///             | styleKey <- Model#StyleIDを指定するもの
        ///             | allChildKey <- Model#Children
        ///             | groupingKey <- nameKey, logicalKey, styleKeyを組み合わせて指定するもの
        ///     - nameKey = <任意の文字列>
        ///     - logicalKey = #<任意の文字列>
        ///     - styleKey = .<任意の文字列>
        ///     - allChildKey = * <- 全ての要素
        ///     - groupingKey = (nameKey|logicalKey|styleKey) (nameKey|logicalKey|styleKey)...
        ///
        /// ## クエリパスルートからのクエリパスのみを指定したい、または除外したい場合
        ///
        /// デフォルトではクエリパスを指定した場合はクエリのルートModel以下のModel階層内を検索します。
        /// そのため、ルートModelとのその子要素に指定したクエリパスと一致するModelがある場合は両方が結果として返されます。
        ///
        /// もし、クエリパスルートModelからのModelだけを返すようにしたい場合は、クエリパスの先頭に'/'を挿入してください。
        /// その反対に子要素からのModelだけを返すようにしたい場合は、クエリパスの先頭に'~'を挿入してください。
        ///
        /// ## 全ての子Modelを指定したい時
        /// 
        /// 現在のクエリパスからみたModelの全ての子Modelを指定したい場合は'*'を指定してください
        /// 
        /// ## groupingKey
        ///
        /// nameKey, logicalKey, styleKeyは組み合わせることが可能です。
        /// これをgroupingKeyと名付けています。
        /// groupingKeyを使用する時は一つの空白文字で各キーを区切るようにしてください。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="queryPath"></param>
        /// <returns></returns>
        public bool DoMatchQueryPath(string queryPath)
        {
            var queryEnumerable = new QueryEnumerable(this, queryPath);
            return queryEnumerable.FirstOrDefault() == this;
        }

        /// <summary>
        /// クエリパスの優先順位を返します。
        /// この関数では指定したModelを検索ルートとしてマッチ判定を行います。
        ///
        /// 返された優先順位は値が高いほど順位が高くなります。
        /// 
        /// 指定したModelのみだけ調べたい時はDoMatchQuery()を使用してください。
        /// 
        /// クエリパスの構文としては以下のものがあります。
        ///     - queryPath = <query>/<query>/<query>...
        ///                 | /<query>/<query>/<query>... <- クエリ操作時のルートModelからのパスのみを含める
        ///                 | ~<query>/<query>/<query>... <- クエリ操作時のルートModelからのパスは除く
        ///     - query = nameKey <- Model#Nameを指定するもの
        ///             | logicalKey <- Model#LogicalIDを指定するもの
        ///             | styleKey <- Model#StyleIDを指定するもの
        ///             | allChildKey <- Model#Children
        ///             | groupingKey <- nameKey, logicalKey, styleKeyを組み合わせて指定するもの
        ///     - nameKey = <任意の文字列>
        ///     - logicalKey = #<任意の文字列>
        ///     - styleKey = .<任意の文字列>
        ///     - allChildKey = * <- 全ての要素
        ///     - groupingKey = (nameKey|logicalKey|styleKey) (nameKey|logicalKey|styleKey)...
        ///
        /// ## クエリパスルートからのクエリパスのみを指定したい、または除外したい場合
        ///
        /// デフォルトではクエリパスを指定した場合はクエリのルートModel以下のModel階層内を検索します。
        /// そのため、ルートModelとのその子要素に指定したクエリパスと一致するModelがある場合は両方が結果として返されます。
        ///
        /// もし、クエリパスルートModelからのModelだけを返すようにしたい場合は、クエリパスの先頭に'/'を挿入してください。
        /// その反対に子要素からのModelだけを返すようにしたい場合は、クエリパスの先頭に'~'を挿入してください。
        ///
        /// ## 全ての子Modelを指定したい時
        /// 
        /// 現在のクエリパスからみたModelの全ての子Modelを指定したい場合は'*'を指定してください
        /// 
        /// ## groupingKey
        ///
        /// nameKey, logicalKey, styleKeyは組み合わせることが可能です。
        /// これをgroupingKeyと名付けています。
        /// groupingKeyを使用する時は一つの空白文字で各キーを区切るようにしてください。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="queryPath"></param>
        /// <returns></returns>
        public ModelViewQueryPathPriority GetQueryPathPriority(string queryPath)
        {
            return new ModelViewQueryPathPriority(this, queryPath);
        }

        /// <summary>
        /// クエリパスによるModel取得。
        /// この関数では指定したModelを検索ルートとしてその全ての子要素に対してマッチ判定を行います。
        /// 
        /// 指定したModelの子要素のみを検索対象にしたい時はQueryChildren()を使用してください。
        /// 
        /// クエリパスの構文としては以下のものがあります。
        ///     - queryPath = <query>/<query>/<query>...
        ///                 | /<query>/<query>/<query>... <- クエリ操作時のルートModelからのパスのみを含める
        ///                 | ~<query>/<query>/<query>... <- クエリ操作時のルートModelからのパスは除く
        ///     - query = nameKey <- Model#Nameを指定するもの
        ///             | logicalKey <- Model#LogicalIDを指定するもの
        ///             | styleKey <- Model#StyleIDを指定するもの
        ///             | allChildKey <- Model#Children
        ///             | groupingKey <- nameKey, logicalKey, styleKeyを組み合わせて指定するもの
        ///     - nameKey = <任意の文字列>
        ///     - logicalKey = #<任意の文字列>
        ///     - styleKey = .<任意の文字列>
        ///     - allChildKey = * <- 全ての要素
        ///     - groupingKey = (nameKey|logicalKey|styleKey) (nameKey|logicalKey|styleKey)...
        ///
        /// ## クエリパスルートからのクエリパスのみを指定したい、または除外したい場合
        ///
        /// デフォルトではクエリパスを指定した場合はクエリのルートModel以下のModel階層内を検索します。
        /// そのため、ルートModelとのその子要素に指定したクエリパスと一致するModelがある場合は両方が結果として返されます。
        ///
        /// もし、クエリパスルートModelからのModelだけを返すようにしたい場合は、クエリパスの先頭に'/'を挿入してください。
        /// その反対に子要素からのModelだけを返すようにしたい場合は、クエリパスの先頭に'~'を挿入してください。
        ///
        /// ## 全ての子Modelを指定したい時
        /// 
        /// 現在のクエリパスからみたModelの全ての子Modelを指定したい場合は'*'を指定してください
        /// 
        /// ## groupingKey
        ///
        /// nameKey, logicalKey, styleKeyは組み合わせることが可能です。
        /// これをgroupingKeyと名付けています。
        /// groupingKeyを使用する時は一つの空白文字で各キーを区切るようにしてください。
        /// </summary>
        /// <param name="model"></param>
        /// <param name="queryPath"></param>
        /// <returns></returns>
        public IEnumerable<Model> Query(string queryPath)
        {
            return new QueryEnumerable(this, queryPath);
        }

        class QueryEnumerable : IEnumerable<Model>, IEnumerable
        {
            Model _target;
            string _queryPath;
            public QueryEnumerable(Model target, string queryPath)
            {
                _target = target;
                _queryPath = queryPath;
            }

            public IEnumerator<Model> GetEnumerator()
            {
                return new Enumerator(_target, _queryPath);
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            class Enumerator : IEnumerator<Model>, IEnumerator, System.IDisposable
            {
                Model _target;
                string _queryPath;
                IEnumerator<Model> _enumerator;
                public Enumerator(Model target, string queryPath)
                {
                    _target = target;
                    _queryPath = queryPath;
                    Reset();
                }
                public Model Current => _enumerator.Current;
                object IEnumerator.Current => Current;
                public void Dispose() => _enumerator.Dispose();
                public bool MoveNext() => _enumerator.MoveNext();
                public void Reset() => _enumerator = Traverse(_queryPath);
                IEnumerator<Model> Traverse(string queryPath)
                {
                    var pathTermsList = queryPath.Split('/');
                    if (!pathTermsList.Any()) yield break;

                    //Prefixの確認
                    bool isOnlyAtRoot = pathTermsList.First() == "";
                    //処理の都合上QUERY_EXCULDED_AT_ROOTがクエリと一緒になっているので、それを取り除く必要がある
                    bool doExcludedAtRoot = 0 == pathTermsList.First().IndexOf(QUERY_EXCULDED_AT_ROOT);
                    if (doExcludedAtRoot)
                    {
                        pathTermsList[0] = pathTermsList[0].Substring(1);
                    }

                    var pathTerms = pathTermsList.AsEnumerable();
                    if (isOnlyAtRoot)
                        pathTerms = pathTerms.Skip(1);

                    //Debug.Log($"debug -- queryPath={queryPath} terms={pathTerms.Aggregate((_sum, _cur) => _sum + "," + _cur)}　isOnlyAtRoot?={isOnlyAtRoot}, doExcludedAtRoot={doExcludedAtRoot}");

                    //Model階層の都合上クエリパスを反対から辿った方が簡単だったのでそうしてる
                    pathTerms = pathTerms.Reverse();
                    var pathTermCount = pathTerms.Count();
                    foreach (var model in _target.GetHierarchyEnumerable())
                    {
                        var modelDepthCount = model.GetTraversedRootEnumerable().Count();
                        var termModelEnumerable = pathTerms.Zip(model.GetTraversedRootEnumerable(), (_t, _m) => (term: _t, model: _m));
                        if (pathTermCount <= modelDepthCount
                            && termModelEnumerable.All((_t) =>
                            _t.model == null
                                ? false
                                : _t.model.DoMatchQuery(_t.term)
                            ))
                        {
                            var headModel = termModelEnumerable.Last().model;
                            var isHeadModel = headModel == _target || headModel == _target.Parent;
                            if (isOnlyAtRoot && isHeadModel
                                || doExcludedAtRoot && !isHeadModel
                                || !isOnlyAtRoot && !doExcludedAtRoot)
                            {
                                //Debug.Log($"model={model.GetPath()} headModel={headModel.GetPath()}");
                                yield return model;
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// クエリパスの優先順位を表すクラス
    ///
    /// 優先順位としては以下のものの方が高くなります。
    /// 1. クエリパスの階層が深い方
    /// 2. 検索ルート修飾子が指定されている方
    /// 3. Model#Nameが指定されている方
    /// 4. Model#LogicalIDが指定されている方でかつ、その個数が多い方
    /// 5. Model#StyleIDが指定されている方でかつ、その個数が多い方
    /// 6. 全称指定子(*)のみのもの
    ///
    /// クエリパス階層の各階層は3.から6.までを元に判定を行います。
    /// また、3.から6.までの判定は親の階層のものが優先されます。
    /// </summary>
    public class ModelViewQueryPathPriority
        : System.IComparable<ModelViewQueryPathPriority>, System.IComparable
        , System.IEquatable<ModelViewQueryPathPriority>
    {
        class PathTerm
        {
            public bool DoExistName { get; set; }
            public bool OnlyAllPath { get; set; }
            public int LogicalIDCount { get; set; }
            public int StyleIDCount { get; set; }
        }

        List<PathTerm> _pathTerms = new List<PathTerm>();

        public Model Target { get; }
        public string QueryPath { get; }
        public bool DoExistRootModifiers { get; }
        public bool IsEmpty { get => Target == null || !_pathTerms.Any(); }
        public int HierarchyDepth { get => _pathTerms.Count; }

        public ModelViewQueryPathPriority()
        {
        }

        public ModelViewQueryPathPriority(Model target, string queryPath)
        {
            if (!target.DoMatchQueryPath(queryPath))
            {
                return;
            }

            var pathTerms = queryPath.Split('/');
            if (!pathTerms.Any())
            {
                Debug.LogWarning($"invalid queryPath={queryPath}");
                return;
            }

            Target = target;
            QueryPath = queryPath;
            if(0 == queryPath.IndexOf(Model.QUERY_EXCULDED_AT_ROOT))
            {
                pathTerms[0] = pathTerms[0].Substring(1);
                DoExistRootModifiers = true;
            }
            else if(0 == queryPath.IndexOf(Model.QUERY_ONLY_AT_ROOT))
            {
                DoExistRootModifiers = true;
            }

            foreach(var pathTerm in pathTerms
                .Where(_t => _t != ""))
            {
                var elements = pathTerm.Split(' ');
                var info = new PathTerm();
                foreach(var e in elements.Where(_e => _e != ""))
                {
                    switch(e[0])
                    {
                        case Model.QUERY_LOGICAL_PREFIX_CHAR: info.LogicalIDCount++; break;
                        case Model.QUERY_STYLE_PREFIX_CHAR: info.StyleIDCount++; break;
                        default:
                            if (e == Model.QUERY_ALL_PATH) info.OnlyAllPath = true;
                            else info.DoExistName = true;
                            break;
                    }
                }
                info.OnlyAllPath = info.OnlyAllPath
                    && !info.DoExistName
                    && info.LogicalIDCount == 0
                    && info.StyleIDCount == 0;
                _pathTerms.Add(info);
            }
        }

        #region IComparable<ModelViewQueryPathPriority>, IComparable and System.IEquatable<ModelViewQueryPathPriority> interface
        public int CompareTo(ModelViewQueryPathPriority other)
        {
            //階層の深さ
            //クエリパスルート修飾子の有無
            //Nameの指定の有無
            //LogicalIDの個数
            //StyleIDの個数
            //AllPathのみか？
            if (HierarchyDepth > other.HierarchyDepth) return 1;
            if (HierarchyDepth < other.HierarchyDepth) return -1;

            if (DoExistRootModifiers && !other.DoExistRootModifiers) return 1;
            if (!DoExistRootModifiers && other.DoExistRootModifiers) return -1;

            foreach(var (selfTerm, otherTerm) in _pathTerms
                .Zip(other._pathTerms, (s, o) => (selfTerm: s, otherTerm: o)))
            {
                if (selfTerm.DoExistName && !otherTerm.DoExistName) return 1;
                if (!selfTerm.DoExistName && otherTerm.DoExistName) return -1;

                if (selfTerm.LogicalIDCount > otherTerm.LogicalIDCount) return 1;
                if (selfTerm.LogicalIDCount < otherTerm.LogicalIDCount) return -1;

                if (selfTerm.StyleIDCount > otherTerm.StyleIDCount) return 1;
                if (selfTerm.StyleIDCount < otherTerm.StyleIDCount) return -1;

                if (selfTerm.OnlyAllPath && !otherTerm.OnlyAllPath) return 1;
                if (!selfTerm.OnlyAllPath && otherTerm.OnlyAllPath) return -1;
            }

            return 0;
        }

        public int CompareTo(object other)
        {
            if (other is ModelViewQueryPathPriority)
                return CompareTo(other as ModelViewQueryPathPriority);
            else
                throw new System.NotImplementedException();
        }

        public bool Equals(ModelViewQueryPathPriority other)
        {
            return CompareTo(other) == 0;
        }
        #endregion
    }

}
