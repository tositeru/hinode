using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="IEventDispatcher"/>
    /// </summary>
    public class EventDispatcherMap
    {
        HashSet<IEventDispatcher> _dispatchers = new HashSet<IEventDispatcher>();

        public EventDispatcherMap(params IEventDispatcher[] dispatchers)
            : this(dispatchers.AsEnumerable())
        { }

        public EventDispatcherMap(IEnumerable<IEventDispatcher> dispatchers)
        {
            foreach(var d in dispatchers.Distinct()
                .Where(_d => !Contains(_d.GetType())))
            {
                _dispatchers.Add(d);
            }
        }

        public void Update(ModelViewBinderInstanceMap binderInstanceMap)
        {
            foreach (var dispatcher in _dispatchers)
            {
                dispatcher.Update(binderInstanceMap);
            }
        }

        public void SendTo(ModelViewBinderInstanceMap binderInstanceMap)
        {
            foreach (var dispatcher in _dispatchers)
            {
                dispatcher.SendTo(binderInstanceMap);
            }
        }

        public bool Contains(System.Type dispatcherType)
            => _dispatchers.Any(_d => _d.GetType().Equals(dispatcherType));
        public bool Contains<T>()
            where T : IEventDispatcher
            => _dispatchers.Any(_d => _d.GetType().Equals(typeof(T)));


        public IEventDispatcher Get(System.Type dispatcherType)
        {
            return _dispatchers.First(_d => _d.GetType().Equals(dispatcherType));
        }
        public T Get<T>()
            where T : IEventDispatcher
            => Get(typeof(T)) as T;

        public bool IsCreatableControllerObjects(Model model, IViewObject viewObject, IEnumerable<ControllerInfo> controllerInfos)
        {
            return controllerInfos
                .Any(_c => _dispatchers.Any(_d =>
                    _d.EventInfos.ContainKeyword(_c.Keyword)
                    && _d.IsCreatableControllerObject(model, viewObject)))
                ;
        }

        public  HashSet<IEventDispatcherHelper> CreateEventDispatcherHelpObjects(Model model, IViewObject viewObject, IEnumerable<ControllerInfo> controllerInfos)
        {
            var objs = controllerInfos
                .Select(_c => _dispatchers.FirstOrDefault(_d => _d.EventInfos.ContainKeyword(_c.Keyword)))
                .Where(_d => _d != null && _d.IsCreatableControllerObject(model, viewObject))
                .Distinct()
                ;
            if (objs.Any())
            {
                return new HashSet<IEventDispatcherHelper>(objs.Select(_d => _d.CreateEventDispatcherHelpObject(model, viewObject)));
            }
            else
            {
                return null;
            }
        }

    }
}
