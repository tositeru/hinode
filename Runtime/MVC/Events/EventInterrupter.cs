using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    public class EventInterruptedData
    {
        public static EventInterruptedData Create(Model senderModel, IViewObject senderViewObj, System.Type eventType, object sendEventData, ControllerInfo senderControllerInfo)
            => new EventInterruptedData(senderModel, senderViewObj, eventType, sendEventData, senderControllerInfo);
        public static EventInterruptedData Create<T>(Model senderModel, IViewObject senderViewObj, object sendEventData, ControllerInfo senderControllerInfo)
            where T : IEventHandler
            => Create(senderModel, senderViewObj, typeof(T), sendEventData, senderControllerInfo);

        public Model SenderModel { get; }
        public IViewObject SenderViewObj { get; }
        public object SendEventData { get; }
        public System.Type EventType { get; }
        public ControllerInfo SenderControllerInfo { get; }

        EventInterruptedData(Model senderModel, IViewObject senderViewObj, System.Type eventType, object sendEventData, ControllerInfo senderControllerInfo)
        {
            SenderModel = senderModel;
            SenderViewObj = senderViewObj;
            EventType = eventType;
            SendEventData = (sendEventData is IEventData) ? (sendEventData as IEventData).Clone() : sendEventData;
            SenderControllerInfo = senderControllerInfo;
        }

        public void Send(ModelViewBinderInstanceMap binderInstanceMap)
        {
            foreach (var selector in SenderControllerInfo.RecieverSelectors)
            {
                selector.Send(EventType, SenderModel, SendEventData, binderInstanceMap);
            }
        }
    }

    public delegate (Model createdModel, bool doSendImmediate) OnEventInterruptCallback(ModelViewBinderInstanceMap binderInstanceMap, EventInterruptedData interruptedData);

    /// <summary>
    /// <seealso cref="EventDispatcherMap"/>
    /// </summary>
    public class EventInterrupter
    {
        Dictionary<EventDispatchQuery, OnEventInterruptCallback> _interrupterDict = new Dictionary<EventDispatchQuery, OnEventInterruptCallback>();

        public void Add(EventDispatchQuery eventDispatchQuery, OnEventInterruptCallback onInterruptPredicate)
        {
            Assert.IsFalse(_interrupterDict.ContainsKey(eventDispatchQuery));

            _interrupterDict.Add(eventDispatchQuery, onInterruptPredicate);
        }

        OnEventInterruptCallback GetInterruptCallback(EventInterruptedData eventInterruptedData)
        {
            return _interrupterDict
                .Where(_t => _t.Key.DoMatch(eventInterruptedData.SenderModel, eventInterruptedData.SenderViewObj, eventInterruptedData.EventType))
                .Select(_t => _t.Value)
                .FirstOrDefault();
        }

        public bool DoMatch(EventInterruptedData eventInterruptedData)
            => null != GetInterruptCallback(eventInterruptedData);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binderInstanceMap"></param>
        /// <param name="eventInterruptedData"></param>
        /// <returns>trueなら割り込み対象になったイベントを即時送信することを表します。</returns>
        public bool Interrupt(ModelViewBinderInstanceMap binderInstanceMap, EventInterruptedData eventInterruptedData)
        {
            OnEventInterruptCallback matchPredicate = GetInterruptCallback(eventInterruptedData);
            Assert.IsNotNull(matchPredicate);
            var (createdModel, doSendImmediate) = matchPredicate(binderInstanceMap, eventInterruptedData);
            if(createdModel != null)
            {
                if (!binderInstanceMap.BindInstances.ContainsKey(createdModel))
                {
                    Assert.IsTrue(binderInstanceMap.BinderMap.Binders.Any(_b => _b.DoMatch(createdModel)), $"OnEventInterruptCallbackで生成したModelは必ずModelViewBinderInstanceMapに追加されるようにしてください。");
                    binderInstanceMap.Add(createdModel, false, (binderInstance) => {
                        binderInstance.HoldedEventInterruptedData = eventInterruptedData;
                    });
                }
                else
                {
                    binderInstanceMap.BindInstances[createdModel].HoldedEventInterruptedData = eventInterruptedData;
                }
            }
            return doSendImmediate;
        }
    }
}