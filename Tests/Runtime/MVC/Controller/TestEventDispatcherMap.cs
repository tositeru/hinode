using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC.Controller
{
    /// <summary>
    /// <seealso cref="EventDispatcherMap"/>
    /// </summary>
    public class TestEventDispatcherMap
    {
        enum TestEventName
        {
            Apple,
            Orange
        }

        interface IAppleEventSender : IControllerSender
        { }
        interface IAppleEventReciever : IControllerReciever
        {
            void OnApple(Model sender, int value);
        }

        class AppleModel : Model, IAppleEventReciever
        {
            public Model Sender { get; set; }
            public int Value { get; set; }
            public void OnApple(Model sender, int value)
            {
                Sender = sender;
                Value = value;
            }
        }

        interface IOrangeEventSender : IControllerSender
        { }
        interface IOrangeEventReciever : IControllerReciever
        {
            void OnOrange(Model sender, float value);
        }

        class OrangeModel : Model, IOrangeEventReciever
        {
            public Model Sender { get; set; }
            public float Value { get; set; }
            public void OnOrange(Model sender, float value)
            {
                Sender = sender;
                Value = value;
            }
        }

        class ControllerObj : IControllerObject
        {
            public TestEventName EventName { get; }

            public ControllerObj(TestEventName eventName) { EventName = eventName; }

            #region IControllerObject
            public void Destroy()
            { }
            #endregion
        }

        class AppleEventDispatcher : IEventDispatcher
        {
            public int SendValue { get; set; }

            #region IEventDispatcher interface
            public override bool DoEnabled { get; set; } = true;

            public override IControllerObject CreateControllerObject(Model model, IViewObject viewObject)
            {
                return new ControllerObj(TestEventName.Apple);
            }

            public override bool IsCreatableControllerObject(Model model, IViewObject viewObject)
            {
                return model.Name == "Apple";
            }

            protected override EventInfoManager CreateEventInfoManager()
                => new EventInfoManager(
                    EventInfoManager.CreateInfo<IAppleEventSender, IAppleEventReciever>(TestEventName.Apple)
                );

            protected override object GetEventData(Model model, IViewObject viewObject, ControllerInfo controllerInfo)
            {
                switch ((TestEventName)System.Enum.Parse(typeof(TestEventName), controllerInfo.Keyword))
                {
                    case TestEventName.Apple: return SendValue;
                }
                throw new System.NotImplementedException();
            }

            protected override void UpdateImpl(ModelViewBinderInstanceMap binderInstanceMap)
            {
                SendValue += 100;
            }
            #endregion
        }

        class OrangeEventDispatcher : IEventDispatcher
        {
            public float SendValue { get; set; }

            #region IEventDispatcher interface
            public override bool DoEnabled { get; set; } = true;

            public override IControllerObject CreateControllerObject(Model model, IViewObject viewObject)
            {
                return new ControllerObj(TestEventName.Orange);
            }

            public override bool IsCreatableControllerObject(Model model, IViewObject viewObject)
            {
                return model.Name == "Orange";
            }

            protected override EventInfoManager CreateEventInfoManager()
                => new EventInfoManager(
                    EventInfoManager.CreateInfo<IOrangeEventSender, IOrangeEventReciever>(TestEventName.Orange)
                );

            protected override object GetEventData(Model model, IViewObject viewObject, ControllerInfo controllerInfo)
            {
                switch ((TestEventName)System.Enum.Parse(typeof(TestEventName), controllerInfo.Keyword))
                {
                    case TestEventName.Orange: return SendValue;
                }
                throw new System.NotImplementedException();
            }

            protected override void UpdateImpl(ModelViewBinderInstanceMap binderInstanceMap)
            {
                SendValue += 0.5f;
            }
            #endregion
        }

        [SetUp]
        public void Setup()
        {
            ControllerTypeManager.EntryRecieverExecuter<IAppleEventReciever, int>(
                (reciever, sender, eventData) => (reciever as IAppleEventReciever).OnApple(sender, eventData));
            ControllerTypeManager.EntryRecieverExecuter<IOrangeEventReciever, float>(
                (reciever, sender, eventData) => (reciever as IOrangeEventReciever).OnOrange(sender, eventData));
        }

        [Test, Description("IControllerObjectの作成関係のテスト")]
        public void IsCreatableAndCreateControllerObjectsPasses()
        {
            var eventDispatcherMap = new EventDispatcherMap(
                new AppleEventDispatcher(),
                new OrangeEventDispatcher()
            );

            var apple = new Model() { Name = "Apple" };
            var orange = new Model() { Name = "Orange" };
            var grape = new Model() { Name = "Grape" };

            var appleControllerInfo = new ControllerInfo(TestEventName.Apple.ToString());
            var orangeControllerInfo = new ControllerInfo(TestEventName.Orange.ToString());
            var unknownControllerInfo = new ControllerInfo("???");
            {// case EventDispatcherMap#IsCreatableControllerObjects() == true
                var testData = new (Model model, IViewObject viewObj, ControllerInfo[] controllerInfos, TestEventName createResultEventName)[]
                {
                    (apple, null, new ControllerInfo[] { appleControllerInfo }, TestEventName.Apple),
                    (orange, null, new ControllerInfo[] { orangeControllerInfo }, TestEventName.Orange),
                    (apple, null, new ControllerInfo[] { appleControllerInfo, unknownControllerInfo, orangeControllerInfo }, TestEventName.Apple),
                    (orange, null, new ControllerInfo[] { appleControllerInfo, unknownControllerInfo, orangeControllerInfo }, TestEventName.Orange),
                };

                foreach(var (model, viewObj, controllerInfos, resultEventName) in testData)
                {
                    var controllerInfoKeywords = controllerInfos.Select(_c => _c.Keyword).Aggregate("", (_s, _c) => _s + _c + ";");
                    var identity = $"model={model}, viewObj={viewObj}, controllerInfos={controllerInfoKeywords}";
                    var errorMessage = $"Failed... {identity}";
                    Assert.IsTrue(eventDispatcherMap.IsCreatableControllerObjects(model, viewObj, controllerInfos), errorMessage);
                    Assert.DoesNotThrow(() => {
                        var objs = eventDispatcherMap.CreateControllerObjects(model, viewObj, controllerInfos);
                        var errorMessageAtCreate = $"Failed to create controller objects... {identity}";
                        Assert.IsNotNull(objs, errorMessageAtCreate);
                        Assert.AreEqual(1, objs.Count, errorMessageAtCreate);
                        Assert.AreEqual(resultEventName, objs.OfType<ControllerObj>().First().EventName, errorMessageAtCreate);
                    }, errorMessage);
                }
            }

            {// case EventDispatcherMap#IsCreatableControllerObjects() == false
                var testData = new (
                    Model model,
                    IViewObject viewObj,
                    ControllerInfo[] controllerInfos)[]
                {
                    (grape, null, new ControllerInfo[] { unknownControllerInfo }),
                    (grape, null, new ControllerInfo[] { appleControllerInfo }),
                    (grape, null, new ControllerInfo[] { orangeControllerInfo }),
                    (apple, null, new ControllerInfo[] { unknownControllerInfo, orangeControllerInfo }),
                    (orange, null, new ControllerInfo[] { appleControllerInfo, unknownControllerInfo }),
                };

                foreach (var (model, viewObj, controllerInfos) in testData)
                {
                    var controllerInfoKeywords = controllerInfos.Select(_c => _c.Keyword).Aggregate("", (_s, _c) => _s + _c + ";");
                    var identity = $"model={model}, viewObj={viewObj}, controllerInfos={controllerInfoKeywords}";
                    var errorMessage = $"Failed... {identity}";
                    Assert.IsFalse(eventDispatcherMap.IsCreatableControllerObjects(model, viewObj, controllerInfos), errorMessage);
                    Assert.IsNull(eventDispatcherMap.CreateControllerObjects(model, viewObj, controllerInfos), $"Not return Null... {identity}");
                }
            }
        }

        abstract class TestEmptyEventDispatcher : IEventDispatcher { }

        [Test, Description("設定されているIEventDispatcherの取得と存在確認のテスト")]
        public void ContainsAndGetPassese()
        {
            var appleEvents = new AppleEventDispatcher();
            var orangeEvents = new OrangeEventDispatcher();
            var eventDispatcherMap = new EventDispatcherMap(
                appleEvents,
                orangeEvents
            );
            Assert.IsTrue(eventDispatcherMap.Contains<AppleEventDispatcher>());
            Assert.IsTrue(eventDispatcherMap.Contains<OrangeEventDispatcher>());
            Assert.IsFalse(eventDispatcherMap.Contains<IEventDispatcher>());
            Assert.IsFalse(eventDispatcherMap.Contains<TestEmptyEventDispatcher>());

            Assert.AreSame(appleEvents, eventDispatcherMap.Get<AppleEventDispatcher>());
            Assert.AreSame(orangeEvents, eventDispatcherMap.Get<OrangeEventDispatcher>());

            Assert.Throws<System.InvalidOperationException>(() => {
                eventDispatcherMap.Get<IEventDispatcher>();
            });
            Assert.Throws<System.InvalidOperationException>(() => {
                eventDispatcherMap.Get<TestEmptyEventDispatcher>();
            });
        }

        [Test]
        public void UpdateAndSendToPasses()
        {
            var selfSelector = new RecieverSelector(ModelRelationShip.Self, "", "");
            var appleBinder = new ModelViewBinder("Apple", null)
                .AddEnabledModelType<AppleModel>()
                .AddControllerInfo(new ControllerInfo(TestEventName.Apple, selfSelector));

            var orangeBinder = new ModelViewBinder("Orange", null)
                .AddEnabledModelType<OrangeModel>()
                .AddControllerInfo(new ControllerInfo(TestEventName.Orange, selfSelector));

            var binderMap = new ModelViewBinderMap(
                new DefaultViewInstanceCreator(
                    (typeof(EmptyViewObject), new EmptyModelViewParamBinder())
                )
                , appleBinder
                , orangeBinder
            );

            var appleDispatcher = new AppleEventDispatcher();
            var orangeDispatcher = new OrangeEventDispatcher();

            var eventDispatcherMap = new EventDispatcherMap(
                appleDispatcher,
                orangeDispatcher
            );
            binderMap.UseEventDispatcherMap = eventDispatcherMap;
            var binderInstanceMap = binderMap.CreateBinderInstaceMap();

            var root = new Model() { Name = "Root" };
            var apple = new AppleModel() { Name = "Apple" };
            var orange = new OrangeModel() { Name = "Orange" };

            apple.Parent = root;
            orange.Parent = root;
            binderInstanceMap.RootModel = root;

            {
                appleDispatcher.SendValue = 0;
                orangeDispatcher.SendValue = 0;
                apple.Sender = null; apple.Value = -1;
                orange.Sender = null; orange.Value = -1f;

                eventDispatcherMap.Update(binderInstanceMap);
                eventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(apple, apple.Sender);
                Assert.AreEqual(appleDispatcher.SendValue, apple.Value);

                Assert.AreSame(orange, orange.Sender);
                Assert.AreEqual(orangeDispatcher.SendValue, orange.Value);
            }

            {//ModelViewBinderInstanceMap#RootModelのモデル階層からSend対象のModelがなくなった時のテスト
                orange.Parent = null;

                appleDispatcher.SendValue = 0;
                orangeDispatcher.SendValue = 0;
                apple.Sender = null; apple.Value = -1;
                orange.Sender = null; orange.Value = -1f;

                eventDispatcherMap.Update(binderInstanceMap);
                eventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(apple, apple.Sender);
                Assert.AreEqual(appleDispatcher.SendValue, apple.Value);

                Assert.AreSame(null, orange.Sender);
                Assert.AreEqual(-1f, orange.Value);
            }

            {//ModelViewBinderInstanceMap#RootModelのモデル階層に新しいSend対象のModelが追加された時のテスト
                orange.Parent = root;

                appleDispatcher.SendValue = 0;
                orangeDispatcher.SendValue = 2;
                apple.Sender = null; apple.Value = -1;
                orange.Sender = null; orange.Value = -1f;

                eventDispatcherMap.Update(binderInstanceMap);
                eventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(apple, apple.Sender);
                Assert.AreEqual(appleDispatcher.SendValue, apple.Value);

                Assert.AreSame(orange, orange.Sender);
                Assert.AreEqual(orangeDispatcher.SendValue, orange.Value);
            }

        }
    }
}
