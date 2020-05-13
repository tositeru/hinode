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
            EntryPair<__legacy.IOnPointerDownSender, __legacy.IOnPointerDownReciever>();
            EntryPair<__legacy.IOnPointerUpSender, __legacy.IOnPointerUpReciever>();
            EntryPair<__legacy.IOnPointerClickSender, __legacy.IOnPointerClickReciever>();
            EntryPair<__legacy.IOnPointerBeginDragSender, __legacy.IOnPointerBeginDragReciever>();
            EntryPair<__legacy.IOnPointerDragSender, __legacy.IOnPointerDragReciever>();
            EntryPair<__legacy.IOnPointerEndDragSender, __legacy.IOnPointerEndDragReciever>();
            EntryPair<__legacy.IOnPointerDropSender, __legacy.IOnPointerDropReciever>();
            EntryPair<__legacy.IOnPointerEnterSender, __legacy.IOnPointerEnterReciever>();
            EntryPair<__legacy.IOnPointerExitSender, __legacy.IOnPointerExitReciever>();

            EntryRecieverExecuter<__legacy.IOnPointerDownReciever, __legacy.OnPointerDownEventData>((reciever, sender, eventData) =>
            {
                reciever.OnPointerDown(sender, eventData);
            });

            EntryRecieverExecuter<__legacy.IOnPointerUpReciever, __legacy.OnPointerUpEventData>((reciever, sender, eventData) =>
            {
                reciever.OnPointerUp(sender, eventData);
            });

            EntryRecieverExecuter<__legacy.IOnPointerClickReciever, __legacy.OnPointerClickEventData>((reciever, sender, eventData) =>
            {
                reciever.OnPointerClick(sender, eventData);
            });

            EntryRecieverExecuter<__legacy.IOnPointerBeginDragReciever, __legacy.OnPointerBeginDragEventData>((reciever, sender, eventData) =>
            {
                reciever.OnPointerBeginDrag(sender, eventData);
            });
            EntryRecieverExecuter<__legacy.IOnPointerDragReciever, __legacy.OnPointerDragEventData>((reciever, sender, eventData) =>
            {
                reciever.OnPointerDrag(sender, eventData);
            });
            EntryRecieverExecuter<__legacy.IOnPointerEndDragReciever, __legacy.OnPointerEndDragEventData>((reciever, sender, eventData) =>
            {
                reciever.OnPointerEndDrag(sender, eventData);
            });
            EntryRecieverExecuter<__legacy.IOnPointerDropReciever, __legacy.OnPointerDropEventData>((reciever, sender, eventData) =>
            {
                reciever.OnPointerDrop(sender, eventData);
            });

            EntryRecieverExecuter<__legacy.IOnPointerEnterReciever, __legacy.OnPointerEnterEventData>((reciever, sender, eventData) =>
            {
                reciever.OnPointerEnter(sender, eventData);
            });
            EntryRecieverExecuter<__legacy.IOnPointerExitReciever, __legacy.OnPointerExitEventData>((reciever, sender, eventData) =>
            {
                reciever.OnPointerExit(sender, eventData);
            });

            MouseEventDispatcher.ConfigControllerType();
            PointerEventDispatcher.ConfigControllerType();
        }

        public static void EntryPair(System.Type sender, System.Type reciever)
        {
            if (_senderRecieverPairDict.ContainsKey(sender))
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
            return (reciever, sender, eventData) =>
            {
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
            Assert.IsTrue(reciever.GetType().HasInterface(useRecieverType));
            _executerDict[useRecieverType](reciever, sender, eventData);
        }


    }
}
