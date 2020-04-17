﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    public class ControllerTypeManager
    {
        static Dictionary<System.Type, System.Action<IControllerReciever, Model, object>> _executerDict = new Dictionary<System.Type, System.Action<IControllerReciever, Model, object>>();
        static Dictionary<System.Type, System.Type> _senderRecieverPairDict = new Dictionary<System.Type, System.Type>();

        static ControllerTypeManager()
        {
            //Entry Hinode's IControllerRecivers!!
            EntryPair<IOnPointerDownSender, IOnPointerDownReciever>();
            EntryPair<IOnPointerMoveSender, IOnPointerMoveReciever>();
            EntryPair<IOnPointerUpSender, IOnPointerUpReciever>();
            EntryPair<IOnPointerClickSender, IOnPointerClickReciever>();

            EntryRecieverExecuter<IOnPointerDownReciever, OnPointerDownEventData>((reciever, sender, eventData) => {
                reciever.OnPointerDown(sender, eventData);
            });

            EntryRecieverExecuter<IOnPointerMoveReciever, OnPointerMoveEventData>((reciever, sender, eventData) => {
                reciever.OnPointerMove(sender, eventData);
            });

            EntryRecieverExecuter<IOnPointerUpReciever, OnPointerUpEventData>((reciever, sender, eventData) => {
                reciever.OnPointerUp(sender, eventData);
            });

            EntryRecieverExecuter<IOnPointerClickReciever, OnPointerClickEventData>((reciever, sender, eventData) => {
                reciever.OnPointerClick(sender, eventData);
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
            Assert.IsTrue(_executerDict.ContainsKey(useRecieverType), $"Don't entry Type({useRecieverType}) executer...");
            Assert.IsTrue(reciever.GetType().DoHasInterface(useRecieverType));
            _executerDict[useRecieverType](reciever, sender, eventData);
        }


    }
}
