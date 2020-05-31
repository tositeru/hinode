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
    public class EventDispatchStateMap
    {
        Dictionary<string, HashSet<EventDispatchQuery>> _states = new Dictionary<string, HashSet<EventDispatchQuery>>();

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

        public EventDispatchStateMap AddState(string stateName, EventDispatchQuery state)
        {
            if(!_states.ContainsKey(stateName))
            {
                _states[stateName] = new HashSet<EventDispatchQuery>();
            }
            _states[stateName].Add(state);
            return this;
        }
        public EventDispatchStateMap AddState(System.Enum stateName, EventDispatchQuery state)
            => AddState(stateName.ToString(), state);

        public bool DoMatch(string stateName, Model model, IViewObject viewObj, System.Type eventType)
        {
            Assert.IsTrue(eventType.HasInterface<IEventHandler>());

            return _states.ContainsKey(stateName)
                && _states[stateName].Any(_s => _s.DoMatch(model, viewObj, eventType));
        }
        public bool DoMatch(System.Enum stateName, Model model, IViewObject viewObj, System.Type eventType)
            => DoMatch(stateName.ToString(), model, viewObj, eventType);
        public bool DoMatch<T>(string stateName, Model model, IViewObject viewObj)
            where T : IEventHandler
            => DoMatch(stateName, model, viewObj, typeof(T));
        public bool DoMatch<T>(System.Enum stateName, Model model, IViewObject viewObj)
            where T : IEventHandler
            => DoMatch(stateName.ToString(), model, viewObj, typeof(T));

        public IEnumerable<string> MatchStates(Model model, IViewObject viewObj, System.Type eventType)
        {
            return _states
                    .Where(_t => _t.Value.Any(_s => _s.DoMatch(model, viewObj, eventType)))
                    .Select(_t => _t.Key);
        }
        public IEnumerable<string> MatchStates<T>(Model model, IViewObject viewObj)
            where T : IEventHandler
            => MatchStates(model, viewObj, typeof(T));
    }
}
