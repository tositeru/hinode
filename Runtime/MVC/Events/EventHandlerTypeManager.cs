using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// TODO IoC的なものに変更?
    /// </summary>
    public class EventHandlerTypeManager : ISingleton<EventHandlerTypeManager>
    {
        Dictionary<System.Type, System.Action<IEventHandler, Model, object>> _executerDict = new Dictionary<System.Type, System.Action<IEventHandler, Model, object>>();

        override protected void OnCreated()
        {
            MouseEventDispatcher.ConfigControllerType(this);
            PointerEventDispatcher.ConfigControllerType(this);
        }

        public void EntryEventHandlerExecuter<TReciever, TEventData>(System.Action<TReciever, Model, TEventData> action)
            where TReciever : class, IEventHandler
        {
            if (_executerDict.ContainsKey(typeof(TReciever)))
            {
                _executerDict[typeof(TReciever)] = CreateExecuter(action);
            }
            else
            {
                _executerDict.Add(typeof(TReciever), CreateExecuter(action));
            }
        }

        System.Action<IEventHandler, Model, object> CreateExecuter<TReciever, TEventData>(System.Action<TReciever, Model, TEventData> action)
            where TReciever : class, IEventHandler
        {
            return (reciever, sender, eventData) =>
            {
                Assert.IsTrue(reciever is TReciever);
                Assert.IsTrue(eventData is TEventData);
                var r = reciever as TReciever;
                var d = (TEventData)eventData;
                action(r, sender, d);
            };
        }

        public void DoneRecieverExecuter(System.Type useRecieverType, IEventHandler reciever, Model sender, object eventData)
        {
            Assert.IsTrue(_executerDict.ContainsKey(useRecieverType), $"Don't entry Type({useRecieverType}) executer... Please Use EventHandlerTypeManager#EntryRecieverExecuter()!!");
            Assert.IsTrue(reciever.GetType().HasInterface(useRecieverType));
            _executerDict[useRecieverType](reciever, sender, eventData);
        }


    }
}
