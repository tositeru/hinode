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
    /// <seealso cref="IControllerSender"/>
    /// </summary>
    public class TestControllerTypeManager
    {
        class OnPointerRecievers
            : IOnPointerDownReciever
            , IOnPointerUpReciever
            , IOnPointerClickReciever
            , IOnPointerBeginDragReciever
            , IOnPointerDragReciever
            , IOnPointerEndDragReciever
            , IOnPointerDropReciever
            , IOnPointerEnterReciever
            , IOnPointerExitReciever
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

            public void OnPointerUp(Model sender, OnPointerUpEventData eventData)
            {
                Sender = sender;
                EventData = eventData;
            }

            public void OnPointerEnter(Model sender, OnPointerEnterEventData eventData)
            {
                Sender = sender;
                EventData = eventData;
            }

            public void OnPointerExit(Model sender, OnPointerExitEventData eventData)
            {
                Sender = sender;
                EventData = eventData;
            }

            public void OnPointerBeginDrag(Model sender, OnPointerBeginDragEventData eventData)
            {
                Sender = sender;
                EventData = eventData;
            }

            public void OnPointerDrag(Model sender, OnPointerDragEventData eventData)
            {
                Sender = sender;
                EventData = eventData;
            }

            public void OnPointerEndDrag(Model sender, OnPointerEndDragEventData eventData)
            {
                Sender = sender;
                EventData = eventData;
            }

            public void OnPointerDrop(Model sender, OnPointerDropEventData eventData)
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
                (typeof(IOnPointerUpSender), typeof(IOnPointerUpReciever)),
                (typeof(IOnPointerClickSender), typeof(IOnPointerClickReciever)),
                (typeof(IOnPointerEnterSender), typeof(IOnPointerEnterReciever)),
                (typeof(IOnPointerExitSender), typeof(IOnPointerExitReciever)),
                (typeof(IOnPointerBeginDragSender), typeof(IOnPointerBeginDragReciever)),
                (typeof(IOnPointerDragSender), typeof(IOnPointerDragReciever)),
                (typeof(IOnPointerEndDragSender), typeof(IOnPointerEndDragReciever)),
                (typeof(IOnPointerDropSender), typeof(IOnPointerDropReciever)),
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

            {//OnPointerEnterReciever
                var sender = new Model() { Name = "sender" };
                var eventData = new OnPointerEnterEventData();
                ControllerTypeManager.DoneRecieverExecuter(typeof(IOnPointerEnterReciever), onPointerRecievers, sender, eventData);
                Assert.AreSame(sender, onPointerRecievers.Sender);
                Assert.AreSame(eventData, onPointerRecievers.EventData);
            }

            {//OnPointerExitReciever
                var sender = new Model() { Name = "sender" };
                var eventData = new OnPointerExitEventData();
                ControllerTypeManager.DoneRecieverExecuter(typeof(IOnPointerExitReciever), onPointerRecievers, sender, eventData);
                Assert.AreSame(sender, onPointerRecievers.Sender);
                Assert.AreSame(eventData, onPointerRecievers.EventData);
            }

            {//OnPointerBeginDragReciever
                var sender = new Model() { Name = "sender" };
                var eventData = new OnPointerBeginDragEventData();
                ControllerTypeManager.DoneRecieverExecuter(typeof(IOnPointerBeginDragReciever), onPointerRecievers, sender, eventData);
                Assert.AreSame(sender, onPointerRecievers.Sender);
                Assert.AreSame(eventData, onPointerRecievers.EventData);
            }

            {//OnPointerDragReciever
                var sender = new Model() { Name = "sender" };
                var eventData = new OnPointerDragEventData();
                ControllerTypeManager.DoneRecieverExecuter(typeof(IOnPointerDragReciever), onPointerRecievers, sender, eventData);
                Assert.AreSame(sender, onPointerRecievers.Sender);
                Assert.AreSame(eventData, onPointerRecievers.EventData);
            }

            {//OnPointerEndDragReciever
                var sender = new Model() { Name = "sender" };
                var eventData = new OnPointerEndDragEventData();
                ControllerTypeManager.DoneRecieverExecuter(typeof(IOnPointerEndDragReciever), onPointerRecievers, sender, eventData);
                Assert.AreSame(sender, onPointerRecievers.Sender);
                Assert.AreSame(eventData, onPointerRecievers.EventData);
            }

            {//OnPointerDropReciever
                var sender = new Model() { Name = "sender" };
                var eventData = new OnPointerDropEventData();
                ControllerTypeManager.DoneRecieverExecuter(typeof(IOnPointerDropReciever), onPointerRecievers, sender, eventData);
                Assert.AreSame(sender, onPointerRecievers.Sender);
                Assert.AreSame(eventData, onPointerRecievers.EventData);
            }

        }
    }
}
