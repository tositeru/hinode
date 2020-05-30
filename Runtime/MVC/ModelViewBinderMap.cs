using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// ModelViewBinderをまとめたもの
    /// <seealso cref="IModel"/>
    /// <seealso cref="ModelViewBinder"/>
    /// <seealso cref="ModelViewBinderInstance"/>
    /// <seealso cref="ModelViewBinderInstanceMap"/>
    /// </summary>
    public class ModelViewBinderMap
    {
        public delegate void OnAddedCallback(ModelViewBinderInstance binderInstance);

        IViewInstanceCreator _viewInstanceCreator = null;
        HashSet<ModelViewBinder> _binders = new HashSet<ModelViewBinder>();

        public IViewInstanceCreator ViewInstanceCreator
        {
            get => _viewInstanceCreator;
            set
            {
                _viewInstanceCreator = value;

                foreach(var b in Binders)
                {
                    b.ViewInstaceCreator = _viewInstanceCreator;
                }
            }
        }

        public ViewLayouter UseViewLayouter { get; set; }
        public EventDispatcherMap UseEventDispatcherMap { get; set; }
        public EventDispatchStateMap UseEventDispatchStateMap { get; set; }
        public EventInterrupter UseEventInterrupter { get; set; }

        public OnAddedCallback DefaultOnAddedCallback { get; set; }

        public IReadOnlyCollection<ModelViewBinder> Binders { get => _binders; }

        public ModelViewBinderMap() { }

        public ModelViewBinderMap(IViewInstanceCreator creator, params ModelViewBinder[] binders)
            : this(creator, binders.AsEnumerable())
        { }

        public ModelViewBinderMap(IViewInstanceCreator creator, IEnumerable<ModelViewBinder> binders)
        {
            ViewInstanceCreator = creator;
            foreach(var b in binders)
            {
                AddBinder(b);
            }
        }

        /// <summary>
        ///
        /// ## 設定用のソースファイルの構文素案
        /// using <namespace name>
        /// <model queryPath>:
        ///      - <ComponentType>: bind(<viewParamName>=<modelParamName>, ...)
        ///      - <ComponentType>: bind(
        ///         <viewParamName>=<modelParamName>,
        ///         <viewParamName>=<modelParamName>,
        ///         ...)
        ///      - ...
        /// </summary>
        /// <param name="sourceCode"></param>
        public ModelViewBinderMap(string sourceCode)
        {

        }

        #region ModelViewBinder
        public ModelViewBinderMap AddBinder(ModelViewBinder binder)
        {
            Assert.IsFalse(_binders.Contains(binder));

            binder.ViewInstaceCreator = ViewInstanceCreator;
            _binders.Add(binder);
            return this;
        }

        public ModelViewBinderMap RemoveBinder(ModelViewBinder binder)
        {
            if (_binders.Contains(binder))
            {
                _binders.Remove(binder);
            }
            return this;
        }
        #endregion

        public ModelViewBinderInstanceMap CreateBinderInstaceMap()
        {
            return new ModelViewBinderInstanceMap(this);
        }

        public ModelViewBinder MatchBinder(Model model)
        {
            return Binders
                .Where(_b => _b.DoMatch(model))
                .Select(_b => (binder: _b, priority: model.GetQueryPathPriority(_b.Query)))
                .Where(_t => !_t.priority.IsEmpty)
                .OrderByDescending(_t => _t.priority)
                .Select(_t => _t.binder)
                .FirstOrDefault();
        }

        public ModelViewBinderInstance CreateBindInstance(Model model, ModelViewBinderInstanceMap binderInstanceMap)
        {
            var binder = MatchBinder(model);
            if (binder == null) return null;

            return binder.CreateBindInstance(model, binderInstanceMap);
        }
    }

    /// <summary>
    /// BindInstanceをまとめたもの
    ///
    /// EnabledDelayOperationがfalseの場合はViewLayout関係の処理は基本的に自動的には行いません。
    /// そのため、手動でApplyViewLayouts()を呼び出すようにしてください。
    /// <seealso cref="IModel"/>
    /// <seealso cref="ModelViewBinderInstance"/>
    /// <seealso cref="ModelViewBinder"/>
    /// <seealso cref="ModelViewBinderMap"/>
    /// </summary>
    public class ModelViewBinderInstanceMap
    {
        Dictionary<Model, Operation> _operationList;
        Dictionary<Model, ModelViewBinderInstance> _bindInstanceDict = new Dictionary<Model, ModelViewBinderInstance>();
        Model _rootModel;

        /// <summary>
        /// 生成・削除・更新などの処理を遅延させるかどうか?
        /// </summary>
        public bool EnabledDelayOperation { get; set; } = false;

        public ModelViewBinderMap BinderMap { get; }
        public EventDispatcherMap UseEventDispatcherMap { get => BinderMap.UseEventDispatcherMap; }
        public EventDispatchStateMap UseEventDispatchStateMap {get => BinderMap.UseEventDispatchStateMap; }
        public EventInterrupter UseEventInterrupter { get => BinderMap.UseEventInterrupter; }
        public ViewLayouter UseViewLayouter { get => BinderMap.UseViewLayouter; }

        public IReadOnlyDictionary<Model, ModelViewBinderInstance> BindInstances
        {
            get => _bindInstanceDict;
        }

        /// <summary>
        /// EnabledDelayOperationがfalseの場合は処理の最後にApplyViewLayouts()が呼び出されます。
        /// </summary>
        public Model RootModel
        {
            get => _rootModel;
            set
            {
                _rootModel = value;
                ClearBindInstances();

                if (_rootModel == null) return;

                Add(true, null, _rootModel);
                _rootModel.OnChangedHierarchy.Add((type, target, models) =>
                {
                    switch (type)
                    {
                        case ChangedModelHierarchyType.ChildAdd:
                            Add(true, null, models);
                            break;
                        case ChangedModelHierarchyType.ChildRemove:
                            Remove(models);
                            break;
                        case ChangedModelHierarchyType.ParentChange:
                            var prevParent = models.ElementAt(0);
                            var curParent = models.ElementAt(1);
                            var isPrevParentInRootModel = prevParent?.GetTraversedRootEnumerable().Any(_m => _m == RootModel) ?? false;
                            var isCurParentInRootModel = curParent?.GetTraversedRootEnumerable().Any(_m => _m == RootModel) ?? false;
                            if (isCurParentInRootModel)
                            {
                                Add(true, null, target);
                            }
                            else if (isPrevParentInRootModel)
                            {
                                Remove(target);
                            }
                            break;
                        default:
                            throw new System.NotImplementedException();
                    }
                });
                _rootModel.OnChangedModelIdentities.Add(model =>
                {
                    if (BindInstances.ContainsKey(model))
                    {
                        Rebind(model);
                    }
                    else
                    {
                        Add(false, null, model);
                    }
                });

                //ViewLayoutの適応
                if (!EnabledDelayOperation)
                {
                    ApplyViewLayouts(ViewLayoutAccessorUpdateTiming.All);
                }
            }
        }

        public bool EnableAutoBind { get => RootModel != null; }

        /// <summary>
        /// 遅延操作のためのデータ
        /// </summary>
        Dictionary<Model, Operation> OperationList
        {
            get => _operationList != null ? _operationList : _operationList = new Dictionary<Model, Operation>();
        }

        public ModelViewBinderInstanceMap(ModelViewBinderMap binderMap)
        {
            BinderMap = binderMap;
        }

        public ModelViewBinderInstance this[Model model]
        {
            get => BindInstances[model];
        }

        public bool Contains(Model model)
            => BindInstances.ContainsKey(model);

        #region Add
        public void AddImpl(Model model, bool doDelay, bool allowRebind, ModelViewBinderMap.OnAddedCallback onAddedCallback)
        {
            if (doDelay)
            {
                var addInfo = new Operation.AddParam() { allowRebind = allowRebind, onAddedCallback = onAddedCallback };
                if (OperationList.ContainsKey(model))
                {
                    OperationList[model].OperationFlags |= Operation.OpType.Add;
                    OperationList[model].UseAddParam = addInfo;
                }
                else
                    OperationList.Add(model, new Operation(model, addInfo));
            }
            else
            {
                foreach (var m in model.GetHierarchyEnumerable())
                {
                    if (allowRebind && _bindInstanceDict.ContainsKey(m))
                    {
                        RebindImpl(m, doDelay);
                    }
                    else if (!_bindInstanceDict.ContainsKey(m))
                    {
                        try
                        {
                            var bindInst = BinderMap.CreateBindInstance(m, this);
                            if (bindInst != null)
                            {
                                bindInst.UpdateViewObjects();
                                _bindInstanceDict.Add(m, bindInst);

                                Logger.Log(Logger.Priority.Debug, () => $"ModelViewBinderInstanceMap#Add: Add model({m})!! queryPath={bindInst.Binder.Query}");

                                onAddedCallback?.Invoke(bindInst);
                                BinderMap?.DefaultOnAddedCallback?.Invoke(bindInst);
                            }
                            else
                            {
                                Logger.Log(Logger.Priority.Debug, () => $"ModelViewBinderInstanceMap#Add: Don't match queryPathes  model({m})!!");
                            }
                        }
                        catch (System.Exception e)
                        {
                            Logger.LogWarning(Logger.Priority.High, () => $"ModelViewBinderInstanceMap#Add: !!Catch Exception!! Not add model...{System.Environment.NewLine}-- {e}");
                            throw new System.Exception($"Failed to Add model({m})... {System.Environment.NewLine}{System.Environment.NewLine}{e}");
                        }
                    }
                }
            }
        }

        public void Add(Model model, bool allowRebind = false, ModelViewBinderMap.OnAddedCallback onAddedCallback = null)
        {
            AddImpl(model, EnabledDelayOperation, allowRebind, onAddedCallback);
        }

        public void Add(bool allowRebind, ModelViewBinderMap.OnAddedCallback onAddedCallback, params Model[] models)
            => Add(allowRebind, onAddedCallback, models.AsEnumerable());

        public void Add(bool allowRebind, ModelViewBinderMap.OnAddedCallback onAddedCallback, IEnumerable<Model> modelEnumerable)
        {
            foreach (var model in modelEnumerable)
            {
                Add(model, allowRebind, onAddedCallback);
            }
        }

        #endregion

        #region Rebind
        bool RebindImpl(Model model, bool doDelay)
        {
            if (doDelay)
            {
                if (OperationList.ContainsKey(model))
                    OperationList[model].OperationFlags |= Operation.OpType.Rebind;
                else
                {
                    if (!_bindInstanceDict.ContainsKey(model))
                        return false;
                    OperationList.Add(model, new Operation(model, Operation.OpType.Rebind));
                }
                return true;
            }
            else
            {
                if (!_bindInstanceDict.ContainsKey(model)) return false;
                try
                {
                    var prevBinderInstance = _bindInstanceDict[model];
                    var matchBinder = BinderMap.MatchBinder(model);
                    if (matchBinder == prevBinderInstance.Binder)
                    {
                        return false;
                    }

                    // TODO BinderInstanceで同じBindInfoを使っていたらそれを使い回すようにする
                    RemoveImpl(model, false);

                    var bindInst = matchBinder?.CreateBindInstance(model, this) ?? null;
                    if (bindInst != null)
                    {
                        bindInst.UpdateViewObjects();
                        _bindInstanceDict[model] = bindInst;
                        Logger.Log(Logger.Priority.Debug, () => $"ModelViewBinderInstanceMap#Rebind: Rebind model({model})!! queryPath={bindInst.Binder.Query}");
                    }
                    else
                    {
                        Logger.Log(Logger.Priority.Debug, () => $"ModelViewBinderInstanceMap#Rebind: Don't match queryPathes  model({model})!!");
                    }
                    return true;
                }
                catch (System.Exception e)
                {
                    Logger.LogWarning(Logger.Priority.High, () => $"ModelViewBinderInstanceMap#Rebind: !!Catch Exception!! Failed to rebind model...{System.Environment.NewLine}-- {e}");
                    throw new System.Exception($"Failed to Rebind model({model})... {System.Environment.NewLine}{System.Environment.NewLine}{e}");
                }
            }
        }

        /// <summary>
        /// 登録されているModelなら再バインドを行う関数
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Rebind(Model model)
        {
            return RebindImpl(model, EnabledDelayOperation);
        }

        #endregion

        #region Remove
        void RemoveImpl(Model model, bool doDelay)
        {
            if (doDelay)
            {
                if (OperationList.ContainsKey(model))
                    OperationList[model].OperationFlags |= Operation.OpType.Remove;
                else
                    OperationList.Add(model, new Operation(model, Operation.OpType.Remove));
            }
            else
            {
                foreach (var m in model.GetHierarchyEnumerable()
                    .Where(_m => BindInstances.ContainsKey(_m)))
                {
                    if(_bindInstanceDict[m].UseInstanceMap == this)
                    {
                        _bindInstanceDict[m].Dispose();
                        _bindInstanceDict.Remove(m);
                    }
                    else
                    {
                        _bindInstanceDict.Remove(m);
                    }
                }


            }
        }

        /// <summary>
        /// 指定したModelとその子Modelと対応しているModelViewBinderInstance全てを取り除きます。
        /// </summary>
        /// <param name="model"></param>
        public void Remove(Model model)
        {
            RemoveImpl(model, EnabledDelayOperation);
        }

        public void Remove(params Model[] models)
            => Remove(models.AsEnumerable());

        public void Remove(IEnumerable<Model> models)
        {
            foreach (var m in models)
            {
                Remove(m);
            }
        }

        public void ClearBindInstances()
            => Remove(_bindInstanceDict.Keys.ToArray());

        #endregion

        public void UpdateViewObjects()
        {
            foreach (var bindInstance in BindInstances.Values)
            {
                if (EnabledDelayOperation)
                {
                    if (OperationList.ContainsKey(bindInstance.Model))
                    {
                        OperationList[bindInstance.Model].OperationFlags |= Operation.OpType.Update;
                    }
                    else
                        OperationList.Add(bindInstance.Model, new Operation(bindInstance.Model, Operation.OpType.Update));
                }
                else
                {
                    bindInstance.UpdateViewObjects();
                }
            }
        }

        /// <summary>
        /// 処理負荷の問題およびModelの階層状態によって正しくLayoutが設定されない可能性があるため、
        /// 自動的には呼び出すようにはなってませんので、別途呼び出すようにしてください。
        ///
        /// Modelの階層状態によって正しくLayoutが設定されない可能性のケース)
        ///  - 他のModelを参照する必要があるIViewLayout -> TransformParentViewLayoutAccessorの値にModelViewSelectorを指定した時
        /// 
        /// </summary>
        public void ApplyViewLayouts(ViewLayoutAccessorUpdateTiming updateTiming)
        {
            foreach (var bindInstance in BindInstances.Values)
            {
                bindInstance.ApplyViewLayout(updateTiming);
            }
        }

        public void Update(Model model, ViewLayoutAccessorUpdateTiming updateTiming = ViewLayoutAccessorUpdateTiming.AtOnlyModel)
        {
            Assert.IsTrue(BindInstances.ContainsKey(model), $"This BindInstaneMap don't have model({model.GetPath()})...");

            if (EnabledDelayOperation)
            {
                if (OperationList.ContainsKey(model))
                {
                    OperationList[model].OperationFlags |= Operation.OpType.Update;
                }
                else
                    OperationList.Add(model, new Operation(model, Operation.OpType.Update));
            }
            else
            {
                BindInstances[model].UpdateViewObjects();
                BindInstances[model].ApplyViewLayout(updateTiming);
            }
        }

        #region 遅延操作関連

        /// <summary>
        /// 操作のためのデータ
        /// </summary>
        public class Operation
        {
            [System.Flags]
            public enum OpType
            {
                Remove = 0x1 << 0,
                Add = 0x1 << 1,
                Rebind = 0x1 << 2,
                Update = 0x1 << 3,
            }

            public class AddParam
            {
                public bool allowRebind;
                public ModelViewBinderMap.OnAddedCallback onAddedCallback;
            }

            public Model Model { get; }
            public OpType OperationFlags { get; set; }

            public AddParam UseAddParam { get; set; }

            public Operation(Model model, OpType operationFlags)
            {
                Model = model;
                OperationFlags = operationFlags;
            }

            public Operation(Model model, AddParam addParam)
            {
                Model = model;
                OperationFlags = OpType.Add;
                UseAddParam = addParam;
            }

            public void Done(ModelViewBinderInstanceMap instanceMap)
            {
                if (0 != (OperationFlags & Operation.OpType.Remove))
                {
                    instanceMap.RemoveImpl(Model, false);
                }
                else if (0 != (OperationFlags & Operation.OpType.Add))
                {
                    var allowRebind = UseAddParam?.allowRebind ?? false;
                    var onAddedCallback = UseAddParam?.onAddedCallback ?? null;
                    instanceMap.AddImpl(Model, false, allowRebind, onAddedCallback);
                }
                else if (0 != (OperationFlags & Operation.OpType.Rebind))
                {
                    instanceMap.RebindImpl(Model, false);
                }
                else if (0 != (OperationFlags & OpType.Update))
                {
                    instanceMap.BindInstances[Model].UpdateViewObjects();
                }
                else
                {
                    throw new System.NotImplementedException($"Don't support Delay Operation... model={Model.GetPath()}, op={OperationFlags}");
                }
            }
        }

        /// <summary>
        /// 遅延操作として登録されたものを全て実行します。
        ///
        /// 登録された後にViewLayoutの更新も行います。
        /// 
        /// 一つのModelにおける遅延操作の実行の優先順位としては以下のものになります。
        ///     - 削除
        ///     - 追加
        ///     - Rebind
        ///     - 更新
        /// </summary>
        public void DoDelayOperations()
        {
            bool doApplyViewLayout = false;

            //TODO? ループ途中でOperationListに要素が追加・削除された時に対応できるようにする(=> ToArray()を呼び出さなくてもいいようにする)
            foreach (var op in OperationList.Values.ToArray())
            {
                try
                {
                    op.Done(this);
                    var doApply = 0 != (op.OperationFlags & Operation.OpType.Rebind)
                        || 0 != (op.OperationFlags & Operation.OpType.Add)
                        || 0 != (op.OperationFlags & Operation.OpType.Remove);
                    doApplyViewLayout |= doApply;

                    if (doApply && UseViewLayouter != null && BindInstances.ContainsKey(op.Model))
                    {
                        BindInstances[op.Model].ApplyViewLayout(ViewLayoutAccessorUpdateTiming.AtOnlyModel);
                    }
                }
                catch (System.Exception e)
                {
                    Logger.LogError(Logger.Priority.High, () => $"ModelViewBinderInstanceMap#DoDelayOperation: !!Catch Exception!! model={op.Model}, op={op.OperationFlags}...{System.Environment.NewLine}+++ {e}");
                }
            }
            Model.DestoryMarkedModels();

            if (doApplyViewLayout && UseViewLayouter != null)
            {
                //Apply ViewLayout(Only UpdateTiming#Alyways)
                foreach(var bindInstance in BindInstances.Values)
                {
                    try
                    {
                        bindInstance.ApplyViewLayout(ViewLayoutAccessorUpdateTiming.All);
                    }
                    catch (System.Exception e)
                    {
                        Logger.LogError(Logger.Priority.High, () => $"ModelViewBinderInstanceMap#DoDelayOperation: !!Catch Exception at Apply ViewLayout(Always)!! model={bindInstance.Model}...{System.Environment.NewLine}+++{System.Environment.NewLine}{e}{System.Environment.NewLine}+++");
                    }
                }
            }
            OperationList.Clear();
        }

        /// <summary>
        /// 遅延操作として登録されたものを順に実行します。
        ///
        /// 一つのModelにおける遅延操作が実行の優先順位としては以下のものになります。
        ///     - 削除
        ///     - 追加
        ///     - Rebind
        ///     - 更新
        ///
        /// 戻り値には実行された操作を表すModelViewBinderInstanceMap#Operationクラスのインスタンスが返されます。
        /// 登録されたIModelがない場合は、nullを返します。
        /// が、再び何らかの操作が登録された場合はまたModelViewBinderInstanceMap#Operationクラスのインスタンスが返されます。
        ///
        /// この関数内ではViewLayoutの再設定を行いませんので、別途ModelViewBinderInstanceMap#ApplyViewLayoutを呼び出してください。
        /// </summary>
        public IEnumerator<Operation> GetDoDelayOperationsEnumerator()
        {
            while (true)
            {
                while (0 < OperationList.Count)
                {
                    var op = OperationList.First();
                    try
                    {
                        op.Value.Done(this);
                    }
                    catch (System.Exception e)
                    {
                        Logger.LogError(Logger.Priority.High, () => $"ModelViewBinderInstanceMap#DoDelayOperation: !!Catch Exception!! model={op.Value.Model}, op={op.Value.OperationFlags}...{System.Environment.NewLine}+++{System.Environment.NewLine}{e}{System.Environment.NewLine}+++");
                    }

                    OperationList.Remove(op.Key);
                    yield return op.Value;
                }
                yield return null;
            }
        }
        #endregion
    }
}
