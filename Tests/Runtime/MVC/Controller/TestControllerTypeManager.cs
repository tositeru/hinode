using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC.Controller
{
    /// <summary>
    /// <seealso cref="ControllerTypeManager"/>
    /// <seealso cref="IControllerSender"/>
    /// <seealso cref="IControllerReciever"/>
    /// </summary>
    public class TestControllerTypeManager
    {
        class OnPointerRecievers
            : IOnPointerDownReciever
            , IOnPointerMoveReciever
            , IOnPointerUpReciever
            , IOnPointerClickReciever
        {
            public Model Sender { get; private set; }
            public object EventData { get; private set; }
            public void OnPointerClick(Model sender, OnPointerClickEventData eventData)
            {
                Sender = sender;
                EventData = eventData;
            }

            public void OnPointerDown(Model sender, OnPointerDownEventData eventData)
            {
                Sender = sender;
                EventData = eventData;
            }

            public void OnPointerMove(Model sender, OnPointerMoveEventData eventData)
            {
                Sender = sender;
                EventData = eventData;
            }

            public void OnPointerUp(Model sender, OnPointerUpEventData eventData)
            {
                Sender = sender;
                EventData = eventData;
            }
        }

        [Test, Description("Check to setting OnPointerXXXEvent in ControllerTypeManager")]
        public void DefaultSettingByOnPointerEventPasses()
        {
            var controllerPairs = new (System.Type sender, System.Type reciever)[]
            {
                (typeof(IOnPointerDownSender), typeof(IOnPointerDownReciever)),
                (typeof(IOnPointerMoveSender), typeof(IOnPointerMoveReciever)),
                (typeof(IOnPointerUpSender), typeof(IOnPointerUpReciever)),
                (typeof(IOnPointerClickSender), typeof(IOnPointerClickReciever)),
            };

            foreach(var pair in controllerPairs)
            {
                Assert.IsTrue(pair.sender.DoHasInterface<IControllerSender>());
                Assert.IsTrue(pair.reciever.DoHasInterface<IControllerReciever>());
                Assert.AreEqual(pair.sender, ControllerTypeManager.GetSenderType(pair.reciever), $"Don't match Controller pair... sender={pair.sender}, reciever={pair.reciever}");
                Assert.AreEqual(pair.reciever, ControllerTypeManager.GetRecieverType(pair.sender), $"Don't match Controller pair... sender={pair.sender}, reciever={pair.reciever}");
            }

            var onPointerRecievers = new OnPointerRecievers();

            {//OnPointerDownReciever
                var sender = new Model() { Name = "sender" };
                var downEventData = new OnPointerDownEventData();
                ControllerTypeManager.DoneRecieverExecuter(typeof(IOnPointerDownReciever), onPointerRecievers, sender, downEventData);
                Assert.AreSame(sender, onPointerRecievers.Sender);
                Assert.AreSame(downEventData, onPointerRecievers.EventData);
            }

            {//OnPointerMoveReciever
                var sender = new Model() { Name = "sender" };
                var eventData = new OnPointerMoveEventData();
                ControllerTypeManager.DoneRecieverExecuter(typeof(IOnPointerMoveReciever), onPointerRecievers, sender, eventData);
                Assert.AreSame(sender, onPointerRecievers.Sender);
                Assert.AreSame(eventData, onPointerRecievers.EventData);
            }

            {//OnPointerUpReciever
                var sender = new Model() { Name = "sender" };
                var eventData = new OnPointerUpEventData();
                ControllerTypeManager.DoneRecieverExecuter(typeof(IOnPointerUpReciever), onPointerRecievers, sender, eventData);
                Assert.AreSame(sender, onPointerRecievers.Sender);
                Assert.AreSame(eventData, onPointerRecievers.EventData);
            }

            {//OnPointerClickReciever
                var sender = new Model() { Name = "sender" };
                var eventData = new OnPointerClickEventData();
                ControllerTypeManager.DoneRecieverExecuter(typeof(IOnPointerClickReciever), onPointerRecievers, sender, eventData);
                Assert.AreSame(sender, onPointerRecievers.Sender);
                Assert.AreSame(eventData, onPointerRecievers.EventData);
            }
        }
    }
}
