using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// TODO Singletonに変更
    /// IoC的なものに変更?
    /// </summary>
    public class ControllerTypeManager
    {
        static Dictionary<System.Type, System.Action<IControllerReciever, Model, object>> _executerDict = new Dictionary<System.Type, System.Action<IControllerReciever, Model, object>>();
        static Dictionary<System.Type, System.Type> _senderRecieverPairDict = new Dictionary<System.Type, System.Type>();

        static ControllerTypeManager()
        {
            //Entry Hinode's IControllerRecivers!!
            EntryPair<IOnPointerDownSender, IOnPointerDownReciever>();
            EntryPair<IOnPointerUpSender, IOnPointerUpReciever>();
            EntryPair<IOnPointerClickSender, IOnPointerClickReciever>();
            EntryPair<IOnPointerBeginDragSender, IOnPointerBeginDragReciever>();
            EntryPair<IOnPointerDragSender, IOnPointerDragReciever>();
            EntryPair<IOnPointerEndDragSender, IOnPointerEndDragReciever>();
            EntryPair<IOnPointerDropSender, IOnPointerDropReciever>();
            EntryPair<IOnPointerEnterSender, IOnPointerEnterReciever>();
            EntryPair<IOnPointerExitSender, IOnPointerExitReciever>();

            EntryRecieverExecuter<IOnPointerDownReciever, OnPointerDownEventData>((reciever, sender, eventData) => {
                reciever.OnPointerDown(sender, eventData);
            });

            EntryRecieverExecuter<IOnPointerUpReciever, OnPointerUpEventData>((reciever, sender, eventData) => {
                reciever.OnPointerUp(sender, eventData);
            });

            EntryRecieverExecuter<IOnPointerClickReciever, OnPointerClickEventData>((reciever, sender, eventData) => {
                reciever.OnPointerClick(sender, eventData);
            });

            EntryRecieverExecuter<IOnPointerBeginDragReciever, OnPointerBeginDragEventData>((reciever, sender, eventData) => {
                reciever.OnPointerBeginDrag(sender, eventData);
            });
            EntryRecieverExecuter<IOnPointerDragReciever, OnPointerDragEventData>((reciever, sender, eventData) => {
                reciever.OnPointerDrag(sender, eventData);
            });
            EntryRecieverExecuter<IOnPointerEndDragReciever, OnPointerEndDragEventData>((reciever, sender, eventData) => {
                reciever.OnPointerEndDrag(sender, eventData);
            });
            EntryRecieverExecuter<IOnPointerDropReciever, OnPointerDropEventData>((reciever, sender, eventData) => {
                reciever.OnPointerDrop(sender, eventData);
            });

            EntryRecieverExecuter<IOnPointerEnterReciever, OnPointerEnterEventData>((reciever, sender, eventData) => {
                reciever.OnPointerEnter(sender, eventData);
            });
            EntryRecieverExecuter<IOnPointerExitReciever, OnPointerExitEventData>((reciever, sender, eventData) => {
                reciever.OnPointerExit(sender, eventData);
            });

        }

        public static void EntryPair(System.Type sender, System.Type reciever)
        {
            if(_senderRecieverPairDict.ContainsKey(sender))
            {
                _senderRecieverPairDict[sender] = reciever;
            }
            else
            {
                _senderRecieverPairDict.Add(sender, reciever);
            }
        }

        public static void EntryPair<TSender, TReciever>()
            where TSender : IControllerSender
            where TReciever : IControllerReciever
            => EntryPair(typeof(TSender), typeof(TReciever));

        public static System.Type GetRecieverType(System.Type senderType)
        {
            return _senderRecieverPairDict[senderType];
        }
        public static System.Type GetRecieverType<TSender>()
            where TSender : IControllerSender
            => GetRecieverType(typeof(TSender));

        public static System.Type GetSenderType(System.Type recieverType)
        {
            return _senderRecieverPairDict.First(_t => _t.Value == recieverType).Key;
        }

        public static System.Type GetSenderType<TReciever>()
            where TReciever : IControllerReciever
            => GetSenderType(typeof(TReciever));


        public static void EntryRecieverExecuter<TReciever, TEventData>(System.Action<TReciever, Model, TEventData> action)
            where TReciever : class, IControllerReciever
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

        static System.Action<IControllerReciever, Model, object> CreateExecuter<TReciever, TEventData>(System.Action<TReciever, Model, TEventData> action)
            where TReciever : class, IControllerReciever
        {
            return (reciever, sender, eventData) => {
                Assert.IsTrue(reciever is TReciever);
                Assert.IsTrue(eventData is TEventData);
                var r = reciever as TReciever;
                var d = (TEventData)eventData;
                action(r, sender, d);
            };
        }

        public static void DoneRecieverExecuter(System.Type useRecieverType, IControllerReciever reciever, Model sender, object eventData)
        {
            Assert.IsTrue(_executerDict.ContainsKey(useRecieverType), $"Don't entry Type({useRecieverType}) executer... Please Use ControllerTypeManager#EntryRecieverExecuter()!!");
            Assert.IsTrue(reciever.GetType().DoHasInterface(useRecieverType));
            _executerDict[useRecieverType](reciever, sender, eventData);
        }


    }
}
