using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode.MVC
{
    public enum EventDispatchStateName
    {
        disable,
        interrupt,
    }

    /// <summary>
    ///
    /// 
    /// <seealso cref="EventDispatchQuery"/>
    /// </summary>
    public class EventDispatchStateMap : System.IDisposable
    {
        public static readonly string DISABLE_DEFAULT_LAYER_LOGICAL_ID = "#__DISABLE_DEFAULT_LAYER";
        public static readonly string AUTO_ADDED_SWITCHING_MODEL_LOGICAL_ID = "#__AUTO_ADDED_SWITCHING_MODEL_LOGICAL_ID";
        class LayerData
        {
            public bool DoEnabled { get; set; }
            public string LayerLogicalID { get; }
            public LayerData(string layerLogicalID)
                => LayerLogicalID = layerLogicalID;
        }

        Dictionary<string, HashSet<EventDispatchQuery>> _states = new Dictionary<string, HashSet<EventDispatchQuery>>();
        Dictionary<string, LayerData> _layerDict = new Dictionary<string, LayerData>();
        Dictionary<EventDispatchQuery, LayerData> _stateLayerDict = new Dictionary<EventDispatchQuery, LayerData>();

        HashSet<Model> _switchingModels = new HashSet<Model>();

        public IReadOnlyCollection<Model> SwitchingModels { get => _switchingModels; }

        public EventDispatchStateMap() { }

        /// <summary>
        /// ex)
        /// Disable:
        ///     - #Disable: *
        ///     - #DisableTest1: IOnTest1
        /// InjectionYesNoDialog:
        ///     - #ConfirmYesNoDialog,viewID: IOnPointerClick
        ///         
        /// </summary>
        /// <param name="stateSource"></param>
        /// <returns></returns>
        public EventDispatchStateMap Parse(string stateSource)
        {
            throw new System.NotImplementedException();
        }

        #region AddState
        public EventDispatchStateMap AddState(string layerLogicalID, string stateName, EventDispatchQuery state)
        {
            if(!_states.ContainsKey(stateName))
            {
                _states[stateName] = new HashSet<EventDispatchQuery>();
            }
            _states[stateName].Add(state);

            if (!_layerDict.ContainsKey(layerLogicalID))
            {
                _layerDict.Add(layerLogicalID, new LayerData(layerLogicalID));
            }
            _stateLayerDict.Add(state, _layerDict[layerLogicalID]);

            UpdateLayer();
            return this;
        }
        public EventDispatchStateMap AddState(string layerLogicalID, System.Enum stateName, EventDispatchQuery state)
            => AddState(layerLogicalID, stateName.ToString(), state);
        public EventDispatchStateMap AddState(string stateName, EventDispatchQuery state)
            => AddState("", stateName, state);
        public EventDispatchStateMap AddState(System.Enum stateName, EventDispatchQuery state)
            => AddState("", stateName.ToString(), state);
        #endregion

        #region SwitchingModel
        public EventDispatchStateMap AddSwitchingModel(Model model)
        {
            if(!_switchingModels.Contains(model))
            {
                _switchingModels.Add(model);
                model.OnChangedModelIdentities.Add(ModelOnChangedModelIdentities);
                model.OnDestroyed.Add(ModelOnDestroyed);
                UpdateLayer();
            }
            return this;
        }
        public EventDispatchStateMap RemoveSwitchingModel(Model model)
        {
            if (_switchingModels.Contains(model))
            {
                _switchingModels.Remove(model);
                model.OnChangedModelIdentities.Remove(ModelOnChangedModelIdentities);
                model.OnDestroyed.Remove(ModelOnDestroyed);
            }
            return this;
        }

        void UpdateLayer()
        {
            foreach (var data in _layerDict.Values)
            {
                if(data.LayerLogicalID == "")
                {//Case Default Layer
                    data.DoEnabled = !_switchingModels
                        .Any(_m => _m.DoMatchQuery(DISABLE_DEFAULT_LAYER_LOGICAL_ID));
                }
                else
                {//Case Custom Layer
                    data.DoEnabled = _switchingModels
                        .Any(_m => _m.DoMatchQuery(data.LayerLogicalID));
                }
            }
        }

        void ModelOnChangedModelIdentities(Model model)
        {
            _switchingModels.Remove(model);
            UpdateLayer();
        }
        void ModelOnDestroyed(Model model)
            => RemoveSwitchingModel(model);

        #endregion

        #region DoMatch
        public bool DoMatch(string stateName, Model model, IViewObject viewObj, System.Type eventType)
        {
            Assert.IsTrue(eventType.HasInterface<IEventHandler>());

            return _states.ContainsKey(stateName)
                && _states[stateName]
                    .Any(_s => _stateLayerDict[_s].DoEnabled && _s.DoMatch(model, viewObj, eventType));
        }
        public bool DoMatch(System.Enum stateName, Model model, IViewObject viewObj, System.Type eventType)
            => DoMatch(stateName.ToString(), model, viewObj, eventType);
        public bool DoMatch<T>(string stateName, Model model, IViewObject viewObj)
            where T : IEventHandler
            => DoMatch(stateName, model, viewObj, typeof(T));
        public bool DoMatch<T>(System.Enum stateName, Model model, IViewObject viewObj)
            where T : IEventHandler
            => DoMatch(stateName.ToString(), model, viewObj, typeof(T));
        #endregion

        #region MatchStates
        public IEnumerable<string> MatchStates(Model model, IViewObject viewObj, System.Type eventType)
        {
            return _states
                    .Where(_t => _t.Value.Any(_s => _s.DoMatch(model, viewObj, eventType)))
                    .Select(_t => _t.Key);
        }
        public IEnumerable<string> MatchStates<T>(Model model, IViewObject viewObj)
            where T : IEventHandler
            => MatchStates(model, viewObj, typeof(T));
        #endregion

        #region IDisposable interface
        public void Dispose()
        {
            _states.Clear();
            _layerDict.Clear();
            _stateLayerDict.Clear();
            _switchingModels.Clear();
        }
        #endregion
    }
}
