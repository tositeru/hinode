using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode.MVC
{
    /// <summary>
    /// <seealso cref="EventDispatchStateMap"/>
    /// </summary>
    public class EventDispatchQuery
    {
        HashSet<System.Type> _enabledEventTypes = new HashSet<System.Type>();

        public string Query { get; }
        public OnlyMainIDViewIdentity ViewID { get; }
        public IEnumerable<System.Type> EnabledEventTypes { get => _enabledEventTypes; }

        public EventDispatchQuery(string query, OnlyMainIDViewIdentity viewID)
        {
            Query = query;
            ViewID = viewID;
        }

        public EventDispatchQuery AddIncludedEventType(System.Type type)
        {
            if (!_enabledEventTypes.Contains(type))
            {
                Assert.IsTrue(type.ContainsInterface<IEventHandler>());

                _enabledEventTypes.Add(type);
            }
            return this;
        }
        public EventDispatchQuery AddIncludedEventType<T>()
            where T : IEventHandler
            => AddIncludedEventType(typeof(T));

        public bool DoMatch(Model model, IViewObject viewObj, System.Type eventType)
        {
            Assert.IsNotNull(model);
            Assert.IsTrue(eventType.ContainsInterface<IEventHandler>(), $"Invalid EventType({eventType})...");
            if(viewObj == null)
            {
                return model.DoMatchQuery(Query)
                    && ViewID.IsEmpty
                    && DoEnableEventType(eventType);
            }
            else
            {
                Assert.AreEqual(model, viewObj.UseModel);
                Assert.IsNotNull(viewObj.UseBindInfo);

                return model.DoMatchQuery(Query)
                    && (ViewID.IsEmpty || viewObj.UseBindInfo.ID == ViewID)
                    && DoEnableEventType(eventType);
            }
        }
        public bool DoMatch<T>(Model model, IViewObject viewObj)
            where T : IEventHandler
            => DoMatch(model, viewObj, typeof(T));

        public bool DoEnableEventType(System.Type eventType)
            => (!_enabledEventTypes.Any() || _enabledEventTypes.Contains(eventType));
        public bool DoEnableEventType<T>()
            where T : IEventHandler
            => DoEnableEventType(typeof(T));

    }
}

