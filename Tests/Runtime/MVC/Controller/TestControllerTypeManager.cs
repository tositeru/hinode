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
            : __legacy.IOnPointerDownReciever
            , __legacy.IOnPointerUpReciever
            , __legacy.IOnPointerClickReciever
            , __legacy.IOnPointerBeginDragReciever
            , __legacy.IOnPointerDragReciever
            , __legacy.IOnPointerEndDragReciever
            , __legacy.IOnPointerDropReciever
            , __legacy.IOnPointerEnterReciever
            , __legacy.IOnPointerExitReciever
        {
            public Model Sender { get; private set; }
            public object EventData { get; private set; }
            public void OnPointerClick(Model sender, __legacy.OnPointerClickEventData eventData)
            {
                Sender = sender;
                EventData = eventData;
            }

            public void OnPointerDown(Model sender, __legacy.OnPointerDownEventData eventData)
            {
                Sender = sender;
                EventData = eventData;
            }

            public void OnPointerUp(Model sender, __legacy.OnPointerUpEventData eventData)
            {
                Sender = sender;
                EventData = eventData;
            }

            public void OnPointerEnter(Model sender, __legacy.OnPointerEnterEventData eventData)
            {
                Sender = sender;
                EventData = eventData;
            }

            public void OnPointerExit(Model sender, __legacy.OnPointerExitEventData eventData)
            {
                Sender = sender;
                EventData = eventData;
            }

            public void OnPointerBeginDrag(Model sender, __legacy.OnPointerBeginDragEventData eventData)
            {
                Sender = sender;
                EventData = eventData;
            }

            public void OnPointerDrag(Model sender, __legacy.OnPointerDragEventData eventData)
            {
                Sender = sender;
                EventData = eventData;
            }

            public void OnPointerEndDrag(Model sender, __legacy.OnPointerEndDragEventData eventData)
            {
                Sender = sender;
                EventData = eventData;
            }

            public void OnPointerDrop(Model sender, __legacy.OnPointerDropEventData eventData)
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
                (typeof(__legacy.IOnPointerDownSender), typeof(__legacy.IOnPointerDownReciever)),
                (typeof(__legacy.IOnPointerUpSender), typeof(__legacy.IOnPointerUpReciever)),
                (typeof(__legacy.IOnPointerClickSender), typeof(__legacy.IOnPointerClickReciever)),
                (typeof(__legacy.IOnPointerEnterSender), typeof(__legacy.IOnPointerEnterReciever)),
                (typeof(__legacy.IOnPointerExitSender), typeof(__legacy.IOnPointerExitReciever)),
                (typeof(__legacy.IOnPointerBeginDragSender), typeof(__legacy.IOnPointerBeginDragReciever)),
                (typeof(__legacy.IOnPointerDragSender), typeof(__legacy.IOnPointerDragReciever)),
                (typeof(__legacy.IOnPointerEndDragSender), typeof(__legacy.IOnPointerEndDragReciever)),
                (typeof(__legacy.IOnPointerDropSender), typeof(__legacy.IOnPointerDropReciever)),
            };

            foreach(var pair in controllerPairs)
            {
                Assert.IsTrue(pair.sender.HasInterface<IControllerSender>());
                Assert.IsTrue(pair.reciever.HasInterface<IControllerReciever>());
                Assert.AreEqual(pair.sender, ControllerTypeManager.GetSenderType(pair.reciever), $"Don't match Controller pair... sender={pair.sender}, reciever={pair.reciever}");
                Assert.AreEqual(pair.reciever, ControllerTypeManager.GetRecieverType(pair.sender), $"Don't match Controller pair... sender={pair.sender}, reciever={pair.reciever}");
            }

            var onPointerRecievers = new OnPointerRecievers();

            {//OnPointerDownReciever
                var sender = new Model() { Name = "sender" };
                var downEventData = new __legacy.OnPointerDownEventData();
                ControllerTypeManager.DoneRecieverExecuter(typeof(__legacy.IOnPointerDownReciever), onPointerRecievers, sender, downEventData);
                Assert.AreSame(sender, onPointerRecievers.Sender);
                Assert.AreSame(downEventData, onPointerRecievers.EventData);
            }

            {//OnPointerUpReciever
                var sender = new Model() { Name = "sender" };
                var eventData = new __legacy.OnPointerUpEventData();
                ControllerTypeManager.DoneRecieverExecuter(typeof(__legacy.IOnPointerUpReciever), onPointerRecievers, sender, eventData);
                Assert.AreSame(sender, onPointerRecievers.Sender);
                Assert.AreSame(eventData, onPointerRecievers.EventData);
            }

            {//OnPointerClickReciever
                var sender = new Model() { Name = "sender" };
                var eventData = new __legacy.OnPointerClickEventData();
                ControllerTypeManager.DoneRecieverExecuter(typeof(__legacy.IOnPointerClickReciever), onPointerRecievers, sender, eventData);
                Assert.AreSame(sender, onPointerRecievers.Sender);
                Assert.AreSame(eventData, onPointerRecievers.EventData);
            }

            {//OnPointerEnterReciever
                var sender = new Model() { Name = "sender" };
                var eventData = new __legacy.OnPointerEnterEventData();
                ControllerTypeManager.DoneRecieverExecuter(typeof(__legacy.IOnPointerEnterReciever), onPointerRecievers, sender, eventData);
                Assert.AreSame(sender, onPointerRecievers.Sender);
                Assert.AreSame(eventData, onPointerRecievers.EventData);
            }

            {//OnPointerExitReciever
                var sender = new Model() { Name = "sender" };
                var eventData = new __legacy.OnPointerExitEventData();
                ControllerTypeManager.DoneRecieverExecuter(typeof(__legacy.IOnPointerExitReciever), onPointerRecievers, sender, eventData);
                Assert.AreSame(sender, onPointerRecievers.Sender);
                Assert.AreSame(eventData, onPointerRecievers.EventData);
            }

            {//OnPointerBeginDragReciever
                var sender = new Model() { Name = "sender" };
                var eventData = new __legacy.OnPointerBeginDragEventData();
                ControllerTypeManager.DoneRecieverExecuter(typeof(__legacy.IOnPointerBeginDragReciever), onPointerRecievers, sender, eventData);
                Assert.AreSame(sender, onPointerRecievers.Sender);
                Assert.AreSame(eventData, onPointerRecievers.EventData);
            }

            {//OnPointerDragReciever
                var sender = new Model() { Name = "sender" };
                var eventData = new __legacy.OnPointerDragEventData();
                ControllerTypeManager.DoneRecieverExecuter(typeof(__legacy.IOnPointerDragReciever), onPointerRecievers, sender, eventData);
                Assert.AreSame(sender, onPointerRecievers.Sender);
                Assert.AreSame(eventData, onPointerRecievers.EventData);
            }

            {//OnPointerEndDragReciever
                var sender = new Model() { Name = "sender" };
                var eventData = new __legacy.OnPointerEndDragEventData();
                ControllerTypeManager.DoneRecieverExecuter(typeof(__legacy.IOnPointerEndDragReciever), onPointerRecievers, sender, eventData);
                Assert.AreSame(sender, onPointerRecievers.Sender);
                Assert.AreSame(eventData, onPointerRecievers.EventData);
            }

            {//OnPointerDropReciever
                var sender = new Model() { Name = "sender" };
                var eventData = new __legacy.OnPointerDropEventData();
                ControllerTypeManager.DoneRecieverExecuter(typeof(__legacy.IOnPointerDropReciever), onPointerRecievers, sender, eventData);
                Assert.AreSame(sender, onPointerRecievers.Sender);
                Assert.AreSame(eventData, onPointerRecievers.EventData);
            }

        }
    }
}
