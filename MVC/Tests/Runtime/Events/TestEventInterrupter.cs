using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Tests;

namespace Hinode.MVC.Tests.Events
{
    /// <summary>
    /// <seealso cref="EventInterrupter"/>
    /// </summary>
    public class TestEventInterrupter
    {
        enum TestEventName
        {
            Test,
        }
        interface ITestEventHandler : IEventHandler
        {
            void Test(Model sender, int value);
        }

        class TestEventDispatcher : IEventDispatcher
        {
            public int SendData { get; set; }

            #region IEventDispatcher
            public override bool DoEnabled { get; set; } = true;

            public override IEventDispatcherHelper CreateEventDispatcherHelpObject(Model model, IViewObject viewObject)
                => null;

            public override bool IsCreatableControllerObject(Model model, IViewObject viewObject)
                => false;

            protected override EventInfoManager CreateEventInfoManager()
                => new EventInfoManager(
                    EventInfoManager.CreateInfo<ITestEventHandler>(TestEventName.Test)
                );

            protected override object GetEventData(Model model, IViewObject viewObject, ControllerInfo controllerInfo)
                => SendData;

            protected override void UpdateImpl(ModelViewBinderInstanceMap binderInstanceMap)
            { }
            #endregion
        }

        class TestModel : Model
            , ITestEventHandler
        {
            public Model SenderModel { get; private set; }
            public int RecievedData { get; private set; }

            public void Reset()
            {
                SenderModel = null;
                RecievedData = 0;
            }
            public void Test(Model sender, int eventData)
            {
                SenderModel = sender;
                RecievedData = eventData;
            }
        }

        [Test]
        public void BasicPasses()
        {
            var modelID = "modelID";
            var interruptModelName = "Interrupt";
            Model senderModel = null;
            IViewObject senderViewObject = null;
            int senderEventData = 0;
            System.Type recieverEventType = null;
            ControllerInfo senderControllerInfo = null;
            var eventInterrupter = new EventInterrupter();

            var binderMap = new ModelViewBinderMap(null,
                new ModelViewBinder("*", null))
            {
                UseEventInterrupter = eventInterrupter
            };
            var binderInstanceMap = binderMap.CreateBinderInstaceMap();

            eventInterrupter.Add(new EventDispatchQuery(modelID, ""),
                (_binderInstanceMap, _interruptedData) =>
                {
                    senderModel = _interruptedData.SenderModel;
                    senderViewObject = _interruptedData.SenderViewObj;
                    recieverEventType = _interruptedData.EventType;
                    senderEventData = (int)_interruptedData.SendEventData;
                    senderControllerInfo = _interruptedData.SenderControllerInfo;
                    var interruptModel = new Model() { Name = interruptModelName };
                    return (interruptModel, false);
                }
            );

            //run Test
            var interruptedData = EventInterruptedData.Create<ITestEventHandler>(
                new Model() { Name = modelID }
                , new EmptyViewObject()
                {
                    UseBindInfo = new ModelViewBinder.BindInfo(typeof(EmptyViewObject))
                }
                , 100
                , new ControllerInfo(TestEventName.Test));

            Assert.IsTrue(eventInterrupter.DoMatch(interruptedData));
            var doSendImmediate = eventInterrupter.Interrupt(binderInstanceMap, interruptedData);
            Assert.IsFalse(doSendImmediate, $"設定したコールバックが返すdoSendImmediateと異なります");

            Debug.Log($"Success to EventInterrupter#Interrupt!!");

            {//設定したPredicateが実行されているか確認
                Assert.AreSame(interruptedData.SenderModel, senderModel);
                Assert.AreSame(interruptedData.SenderViewObj, senderViewObject);
                Assert.AreEqual(interruptedData.EventType, recieverEventType);
                Assert.AreEqual(interruptedData.SendEventData, senderEventData);
                Assert.AreSame(interruptedData.SenderControllerInfo, senderControllerInfo);
            }
            Debug.Log($"Success to Check EventInterrupter#Interrupt Callback!!");

            {//自動的にModelをModelViewBinderInstanceMapに登録しているか確認
                Assert.IsTrue(binderInstanceMap.BindInstances.Any(_b => _b.Value.Model.Name == interruptModelName));
                var interruptBinderInstace = binderInstanceMap.BindInstances.Values.First(_b => _b.Model.Name == interruptModelName);

                Assert.IsTrue(interruptBinderInstace.HasEventInterruptedData);
                var holdInterruptedData = interruptBinderInstace.HoldedEventInterruptedData;
                Assert.AreSame(interruptedData.SenderModel, holdInterruptedData.SenderModel);
                Assert.AreSame(interruptedData.SenderViewObj, holdInterruptedData.SenderViewObj);
                Assert.AreEqual(interruptedData.EventType, holdInterruptedData.EventType);
                Assert.AreEqual(interruptedData.SendEventData, holdInterruptedData.SendEventData);
                Assert.AreSame(interruptedData.SenderControllerInfo, holdInterruptedData.SenderControllerInfo);
            }

            Debug.Log($"Success to Add Created Model in Interrupt Callback to BinderInstanceMap!!");
        }

        [Test]
        public void DontMatchBinderFailed()
        {
            var modelID = "modelID";
            var interruptModelName = "Interrupt";
            Model senderModel = null;
            IViewObject senderViewObject = null;
            int senderEventData = 0;
            System.Type recieverEventType = null;
            ControllerInfo senderControllerInfo = null;
            var eventInterrupter = new EventInterrupter();

            var binderMap = new ModelViewBinderMap()
            {
                UseEventInterrupter = eventInterrupter
            };
            var binderInstanceMap = binderMap.CreateBinderInstaceMap();

            eventInterrupter.Add(new EventDispatchQuery(modelID, ""),
                (_binderInstanceMap, _interruptedData) =>
                {
                    senderModel = _interruptedData.SenderModel;
                    senderViewObject = _interruptedData.SenderViewObj;
                    recieverEventType = _interruptedData.EventType;
                    senderEventData = (int)_interruptedData.SendEventData;
                    senderControllerInfo = _interruptedData.SenderControllerInfo;
                    var interruptModel = new Model() { Name = interruptModelName };
                    return (interruptModel, true);
                }
            );

            //run Test
            var interruptedData = EventInterruptedData.Create<ITestEventHandler>(
                new Model() { Name = modelID }
                , new EmptyViewObject()
                {
                    UseBindInfo = new ModelViewBinder.BindInfo(typeof(EmptyViewObject))
                }
                , 100
                , new ControllerInfo(TestEventName.Test));
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
            {
                eventInterrupter.Interrupt(binderInstanceMap, interruptedData);
            }, "OnEventInterruptCallbackでModelを返した時、ModelViewBinderInstanceMapへ登録が失敗した時は例外を投げるようにしてください");
        }

        // A Test behaves as an ordinary method
        [Test, Description("ModelViewBinderMapとの連帯が取れているかのテスト")]
        public void UsageModelViewBinderMapPasses()
        {
            #region Construct Enviroment
            EventHandlerTypeManager.Instance.EntryEventHandlerExecuter<ITestEventHandler, int>((reciever, sender, eventData) =>
            {
                reciever.Test(sender, eventData);
            });
            var eventDispatcher = new TestEventDispatcher()
            {
                SendData = 111,
            };

            var modelID = "Model";
            var interruptTargetID = "Target";
            var interruptModelName = "Interrupt";

            var interruptedTargetQuery = new EventDispatchQuery(modelID, "");
            var viewBinder = new ModelViewBinder(modelID, null
                , new ModelViewBinder.BindInfo(typeof(EmptyViewObject))
                    .AddControllerInfo(new ControllerInfo(TestEventName.Test, new EventHandlerSelector(ModelRelationShip.Self, "", ""))));
            var interruptBinder = new ModelViewBinder(interruptModelName, null
                , new ModelViewBinder.BindInfo(typeof(EmptyViewObject))
                    .AddControllerInfo(new ControllerInfo(TestEventName.Test)
                        .SetInterrupt(true))
            );
            var viewCreator = new DefaultViewInstanceCreator(
                (typeof(EmptyViewObject), new EmptyModelViewParamBinder())
            );
            var binderMap = new ModelViewBinderMap(viewCreator
                , viewBinder
                , interruptBinder)
            {
                UseEventDispatcherMap = new EventDispatcherMap(
                    eventDispatcher
                ),
                UseEventDispatchStateMap = new EventDispatchStateMap()
                    .AddState(EventDispatchStateName.interrupt, interruptedTargetQuery),
            };
            var binderInstanceMap = binderMap.CreateBinderInstaceMap();
            var root = new TestModel() { Name = modelID };

            binderInstanceMap.RootModel = root;
            #endregion

            var eventInterrupter = new EventInterrupter();
            eventInterrupter.Add(interruptedTargetQuery,
                (_binderInstanceMap, interruptedData) =>
                {
                    var interruptModel = new Model() { Name = interruptModelName };
                    return (interruptModel, true);
                }
            );

            binderMap.UseEventInterrupter = eventInterrupter;

            Assert.IsNotNull(binderInstanceMap.UseEventDispatchStateMap);
            Assert.IsNotNull(binderInstanceMap.UseEventInterrupter);
            Assert.IsTrue(binderInstanceMap.UseEventDispatchStateMap.DoMatch<ITestEventHandler>(EventDispatchStateName.interrupt, root, null), $"イベントの割り込みテストに使用するモデル({root})が割り込み対象になっていません。EventDispatchStateMapの設定を見直してください");

            eventDispatcher.SendTo(binderInstanceMap);

            Assert.AreSame(root, root.SenderModel);
            Assert.AreEqual(eventDispatcher.SendData, root.RecievedData);

            Debug.Log($"Success to Send First Event!!");

            {//割り込み処理が実行されているか確認
                //
                var errorMessage = "イベントが送信される際に処理の割り込みが実行されていません。";
                Assert.IsTrue(binderInstanceMap.BindInstances.Any(_b => _b.Value.Model.Name == interruptModelName), errorMessage);
                var interruptBinderInstance = binderInstanceMap.BindInstances.Values.First(_b => _b.Model.Name == interruptModelName);

                Assert.IsTrue(interruptBinderInstance.HasEventInterruptedData);
                var interruptedData = interruptBinderInstance.HoldedEventInterruptedData;
                Assert.AreSame(root, interruptedData.SenderModel);
                Assert.AreEqual(eventDispatcher.SendData, interruptedData.SendEventData);
            }
            Debug.Log($"Success to Interrupt Event of RootModel!!");

            {//割り込みが有効化されたControllerInfoが割り込みデータを元に送信できているか確認
                {//ControllerInfo#IsInterruptModeが正しく動作しているか確認するため、rootのBinderInstanceを削除する
                    root.Name = "hoge";
                    Assert.IsFalse(binderInstanceMap.BindInstances.ContainsKey(root));
                }

                root.Reset();
                eventDispatcher.SendTo(binderInstanceMap);

                Assert.AreSame(root, root.SenderModel);
                Assert.AreEqual(eventDispatcher.SendData, root.RecievedData);
            }
            Debug.Log($"Success to Send Interrupted Event!!");
        }

    }
}
