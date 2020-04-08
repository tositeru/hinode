using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        public ModelViewBinderInstance CreateBindInstance(Model model)
        {
            var binder = Binders
                .Select(_b => (binder: _b, priority: model.GetQueryPathPriority(_b.QueryPath)))
                .Where(_t => !_t.priority.IsEmpty)
                .OrderByDescending(_t => _t.priority)
                .Select(_t => _t.binder)
                .FirstOrDefault();
            if (binder == null) return null;

            return binder.CreateBindInstance(model);
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

        /// <summary>
        /// 生成・削除・更新などの処理を遅延させるかどうか?
        /// </summary>
        public bool EnabledDelayOperation { get; set; } = false;

        public ModelViewBinderMap BinderMap { get; }
        Dictionary<Model, ModelViewBinderInstance> _bindInstanceDict = new Dictionary<Model, ModelViewBinderInstance>();
        public IReadOnlyDictionary<Model, ModelViewBinderInstance> BindInstances
        {
            get => _bindInstanceDict;
        }

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

        public void AddImpl(Model model, bool doDelay)
        {
            if (doDelay)
            {
                if (OperationList.ContainsKey(model))
                    OperationList[model].OperationFlags |= Operation.OpType.Add;
                else
                    OperationList.Add(model, new Operation(model, Operation.OpType.Add));
            }
            else
            {
                if (_bindInstanceDict.ContainsKey(model)) return;
                var bindInst = BinderMap.CreateBindInstance(model);
                _bindInstanceDict.Add(model, bindInst);
                bindInst.UpdateViewObjects();
            }
        }

        public void Add(Model model)
        {
            AddImpl(model, EnabledDelayOperation);
        }

        public void Add(params Model[] models)
            => Add(models.AsEnumerable());

        public void Add(IEnumerable<Model> modelEnumerable)
        {
            foreach (var model in modelEnumerable
                .Where(_m => !_bindInstanceDict.ContainsKey(_m)))
            {
                Add(model);
            }
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

        public void UpdateViewObjects()
        {
            foreach (var bindInstance in BindInstances.Values)
            {
                bindInstance.UpdateViewObjects();
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
            }

            public Model Model { get; }
            public OpType OperationFlags { get; set; }

            public Operation(Model model, OpType operationFlags)
            {
                Model = model;
                OperationFlags = operationFlags;
            }

            public void Done(ModelViewBinderInstanceMap instanceMap)
            {
                if (0 != (OperationFlags & Operation.OpType.Remove))
                {
                    instanceMap.RemoveImpl(Model, false);
                }
                else if (0 != (OperationFlags & Operation.OpType.Add))
                {
                    instanceMap.AddImpl(Model, false);
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
