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
        public List<ModelViewBinder> Binders { get; } = new List<ModelViewBinder>();

        public ModelViewBinderMap() { }

        public ModelViewBinderMap(params ModelViewBinder[] binders)
            : this(binders.AsEnumerable())
        { }

        public ModelViewBinderMap(List<ModelViewBinder> binders)
        {
            Binders = binders;
        }

        public ModelViewBinderMap(IEnumerable<ModelViewBinder> binders)
        {
            Binders = binders.ToList();
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

        public ModelViewBinderInstanceMap CreateBinderInstaceMap()
        {
            return new ModelViewBinderInstanceMap(this);
        }

        public ModelViewBinderInstance CreateBindInstance(Model model, ModelViewBinderInstanceMap binderInstanceMap)
        {
            var binder = Binders
                .Select(_b => (binder: _b, priority: model.GetQueryPathPriority(_b.QueryPath)))
                .Where(_t => !_t.priority.IsEmpty)
                .OrderByDescending(_t => _t.priority)
                .Select(_t => _t.binder)
                .FirstOrDefault();
            if (binder == null) return null;

            return binder.CreateBindInstance(model, binderInstanceMap);
        }
    }

    /// <summary>
    /// BindInstanceをまとめたもの
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
        public IReadOnlyDictionary<Model, ModelViewBinderInstance> BindInstances
        {
            get => _bindInstanceDict;
        }

        public Model RootModel
        {
            get => _rootModel;
            set
            {
                _rootModel = value;
                ClearBindInstances();

                if (_rootModel == null) return;

                Add(true, _rootModel.GetHierarchyEnumerable());
                _rootModel.OnChangedHierarchy.Add((type, target, models) => {
                    switch (type)
                    {
                        case ChangedModelHierarchyType.ChildAdd:
                            Add(true, models.SelectMany(_m => _m.GetHierarchyEnumerable()));
                            break;
                        case ChangedModelHierarchyType.ChildRemove:
                            Remove(models.SelectMany(_m => _m.GetHierarchyEnumerable()));
                            break;
                        case ChangedModelHierarchyType.ParentChange:
                            var prevParent = models.ElementAt(0);
                            var curParent = models.ElementAt(1);
                            var isPrevParentInRootModel = prevParent?.GetTraversedRootEnumerable().Any(_m => _m == RootModel) ?? false;
                            var isCurParentInRootModel = curParent?.GetTraversedRootEnumerable().Any(_m => _m == RootModel) ?? false;
                            if (isCurParentInRootModel)
                            {
                                Add(true, target.GetHierarchyEnumerable());
                            }
                            else if(isPrevParentInRootModel)
                            {
                                Remove(target.GetHierarchyEnumerable());
                            }
                            break;
                        default:
                            throw new System.NotImplementedException();
                    }
                });
                _rootModel.OnChangedModelIdentities.Add(model => {
                    Rebind(model);
                });
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

        public void AddImpl(Model model, bool doDelay, bool allowRebind)
        {
            if (doDelay)
            {
                var addInfo = new Operation.AddParam() { allowRebind = allowRebind };
                if (OperationList.ContainsKey(model))
                {
                    OperationList[model].OperationFlags |= Operation.OpType.Add;
                    OperationList[model].UseAddParam = addInfo;
                }
                else
                    OperationList.Add(model, new Operation(model, addInfo));
            }
            else if(allowRebind && _bindInstanceDict.ContainsKey(model))
            {
                RebindImpl(model, doDelay);
            }
            else if(!_bindInstanceDict.ContainsKey(model))
            {
                var bindInst = BinderMap.CreateBindInstance(model, this);
                if(bindInst != null)
                {
                    bindInst.DettachModelOnUpdated();
                    model.OnUpdated.Add(ModelOnUpdated);
                    _bindInstanceDict.Add(model, bindInst);
                    bindInst.UpdateViewObjects();
                }
            }
        }

        public void Add(Model model, bool allowRebind=false)
        {
            AddImpl(model, EnabledDelayOperation, allowRebind);
        }

        public void Add(bool allowRebind, params Model[] models)
            => Add(allowRebind, models.AsEnumerable());

        public void Add(bool allowRebind, IEnumerable < Model> modelEnumerable)
        {
            foreach (var model in modelEnumerable
                .Where(_m => !_bindInstanceDict.ContainsKey(_m)))
            {
                Add(model, allowRebind);
            }
        }

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
                var bindInst = BinderMap.CreateBindInstance(model, this);
                _bindInstanceDict[model] = bindInst;
                bindInst.UpdateViewObjects();
                return true;
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
                if (!BindInstances.ContainsKey(model))
                {
                    return;
                }
                model.OnUpdated.Remove(ModelOnUpdated);
                _bindInstanceDict.Remove(model);
            }
        }

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

        public void UpdateViewObjects()
        {
            foreach (var bindInstance in BindInstances.Values)
            {
                if(EnabledDelayOperation)
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

        void ModelOnUpdated(Model model)
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
                    instanceMap.AddImpl(Model, false, allowRebind);
                }
                else if (0 != (OperationFlags & Operation.OpType.Rebind))
                {
                    instanceMap.RebindImpl(Model, false);
                }
                else if(0 != (OperationFlags & OpType.Update))
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
        /// 一つのModelにおける遅延操作が実行の優先順位としては以下のものになります。
        ///     - 削除
        ///     - 追加
        ///     - 更新
        /// </summary>
        public void DoDelayOperations()
        {
            foreach(var op in OperationList.Values)
            {
                op.Done(this);
            }
            OperationList.Clear();
        }

        /// <summary>
        /// 遅延操作として登録されたものを順に実行します。
        ///
        /// 一つのModelにおける遅延操作が実行の優先順位としては以下のものになります。
        ///     - 削除
        ///     - 追加
        ///
        /// 戻り値には実行された操作を表すModelViewBinderInstanceMap#Operationクラスのインスタンスが返されます。
        /// 登録されたIModelがない場合は、nullを返します。
        /// が、再び何らかの操作が登録された場合はまたModelViewBinderInstanceMap#Operationクラスのインスタンスが返されます。
        /// </summary>
        public IEnumerator<Operation> GetDoDelayOperationsEnumerator()
        {
            while(true)
            {
                while(0 < OperationList.Count)
                {
                    var op = OperationList.First();
                    op.Value.Done(this);
                    OperationList.Remove(op.Key);
                    yield return op.Value;
                }
                yield return null;
            }
        }
        #endregion
    }
}
