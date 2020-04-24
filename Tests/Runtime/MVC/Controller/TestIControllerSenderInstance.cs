using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC.Controller
{
    /// <summary>
    /// <seealso cref="IControllerSenderInstance"/>
    /// </summary>
    public class TestIControllerSenderInstance
    {
        [Test]
        public void BasicUsageOfDefaultControllerSenderGroupPasses()
        {
            #region Initial Enviroment
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(TestView), new TestView.ParamBinder()));
            var allBinder = new ModelViewBinder("*", null,
                new ModelViewBinder.BindInfo(typeof(TestView)));
            var binderMap = new ModelViewBinderMap(viewInstanceCreator,
                allBinder);
            var binderInstanceMap = binderMap.CreateBinderInstaceMap();

            var model = new TestModel();
            binderInstanceMap.RootModel = model;
            #endregion

            TestSenderInstance.SetControllerTypeSettings();

            var useEvents = new Dictionary<string, System.Type>()
            {
                { TestSenderInstance.KEYWORD_ON_TEST, typeof(ITestSender) },
                { "__empty", typeof(IEmptySender) },
            };
            var testEvents = new DefaultControllerSenderGroup<TestSenderInstance>(useEvents);

            {//Test HasEventKeyword
                Assert.IsTrue(testEvents.ContainsSenderKeyword(TestSenderInstance.KEYWORD_ON_TEST));
                Assert.IsFalse(testEvents.ContainsSenderKeyword("invalid"));
            }

            {//Test GetEventType
                Assert.AreEqual(typeof(ITestSender), testEvents.GetSenderType(TestSenderInstance.KEYWORD_ON_TEST));
                Assert.Throws<KeyNotFoundException>(() => testEvents.GetSenderType("invalid"));
            }
            {
                Assert.IsTrue(testEvents.ContainsSender<ITestSender>());
                Assert.IsTrue(testEvents.ContainsSender<IEmptySender>());
                Assert.IsFalse(testEvents.ContainsSender<ITest2Sender>());
            }

            {//test DefaultControllerSenderGroup#CreateInstance
                var viewObj = binderInstanceMap[model].ViewObjects.First();
                var controllerInstance = testEvents.CreateInstance(viewObj, model, binderInstanceMap);
                Assert.AreEqual(typeof(TestSenderInstance), controllerInstance.GetType());
                Assert.AreSame(model, controllerInstance.Target);
                Assert.AreSame(viewObj, controllerInstance.TargetViewObj);
                Assert.AreSame(binderInstanceMap, controllerInstance.UseBinderInstanceMap);
                Assert.AreSame(testEvents, controllerInstance.UseSenderGroup);
                Assert.IsFalse(controllerInstance.DoEnableSender<ITestSender>());
                Assert.IsFalse(controllerInstance.DoEnableSender<IEmptySender>());
            }
        }

        [Test]
        public void BasicUsageOfIControllerSenderInstancePasses()
        {
            #region Initial Enviroment
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(TestView), new TestView.ParamBinder()));
            var allBinder = new ModelViewBinder("*", null,
                new ModelViewBinder.BindInfo(typeof(TestView)));
            var binderMap = new ModelViewBinderMap(viewInstanceCreator,
                allBinder);
            var binderInstanceMap = binderMap.CreateBinderInstaceMap();

            var model = new TestModel();
            binderInstanceMap.RootModel = model;
            #endregion

            ControllerTypeManager.EntryPair<ITestSender, ITestReciever>();
            ControllerTypeManager.EntryPair<ITest2Sender, ITest2Reciever>();

            ControllerTypeManager.EntryRecieverExecuter<ITestReciever, int>((reciever, sender, eventData) => {
                reciever.Test(sender, eventData);
            });
            ControllerTypeManager.EntryRecieverExecuter<ITest2Reciever, int>((reciever, sender, eventData) => {
                reciever.Test2(sender, eventData);
            });

            var useEvents = new Dictionary<string, System.Type>()
            {
                { TestSenderInstance.KEYWORD_ON_TEST, typeof(ITestSender) },
                { "__empty", typeof(IEmptySender) },
            };
            var testEvents = new DefaultControllerSenderGroup<TestSenderInstance>(useEvents);

            {//Basic Usage Of IControllerSender and IControllerSenderInstance
                //IControllerSenderInstance All Method
                // - AddSelector
                // - ContainsSelector
                // - SelectorCount
                // - EnableSender
                // - DoEnableSender
                // - DisableSender
                // - ClearSelectorList
                // - ClearSelectorDict
                // - Send
                var controllerInstance = testEvents.CreateInstance(null, model, binderInstanceMap);
                var testControllerInst = controllerInstance as TestSenderInstance;
                {
                    //Test IControllerSenderInstance
                    // - AddSelector
                    // - ContainsSelector
                    // - SelectorCount
                    // - EnableSender
                    // - DoEnableSender
                    // - Send
                    controllerInstance.AddSelector<ITestSender>(new RecieverSelector(ModelRelationShip.Self, "", ""));
                    Assert.IsTrue(controllerInstance.ContainsSelector<ITestSender>());
                    Assert.AreEqual(1, controllerInstance.SelectorCount<ITestSender>());

                    controllerInstance.EnableSender<ITestSender>();
                    Assert.IsTrue(controllerInstance.DoEnableSender<ITestSender>());
                    (controllerInstance as TestSenderInstance).Send(100);

                    var errorMessage = $"{typeof(ITestSender)}のsenderとrecieverのバインドができていません。";
                    Assert.AreSame(model, model.TestSender);
                    Assert.AreEqual(100, model.Value, errorMessage);
                }

                {//Test IControllerSenderInstance#DisableSender
                    //Test IControllerSenderInstance
                    // - DisableSender
                    // - ContainsSelector
                    // - SelectorCount
                    // - DoEnableSender
                    // - Send
                    controllerInstance.DisableSender<ITestSender>();
                    Assert.IsFalse(controllerInstance.DoEnableSender<ITestSender>());
                    Assert.IsTrue(controllerInstance.ContainsSelector<ITestSender>());
                    Assert.AreEqual(1, controllerInstance.SelectorCount<ITestSender>());

                    model.TestSender = null;
                    model.Value = 0;
                    (controllerInstance as TestSenderInstance).Send(100);

                    var errorMessage = $"IControllerSenderInstance#DisableSenderの後は{typeof(ITestSender)}のsenderとrecieverのバインドしないようにしてください。";
                    Assert.AreNotSame(model, model.TestSender);
                    Assert.AreNotEqual(100, model.Value, errorMessage);
                }

                {//Test IControllerSenderInstance#ClearSelector
                    //Test IControllerSenderInstance
                    // - ClearSelectorList
                    // - ContainsSelector
                    // - SelectorCount
                    // - DoEnableSender
                    // - Send
                    controllerInstance.EnableSender<ITestSender>();

                    controllerInstance.ClearSelectorList<ITestSender>();
                    Assert.IsTrue(controllerInstance.DoEnableSender<ITestSender>());
                    Assert.IsFalse(controllerInstance.ContainsSelector<ITestSender>());
                    Assert.AreEqual(0, controllerInstance.SelectorCount<ITestSender>());

                    model.TestSender = null;
                    model.Value = 0;
                    (controllerInstance as TestSenderInstance).Send(100);

                    var errorMessage = $"Selectorがない時は{typeof(ITestSender)}のsenderとrecieverのバインドしないようにしてください。";
                    Assert.AreNotSame(model, model.TestSender);
                    Assert.AreNotEqual(100, model.Value, errorMessage);
                }

                {
                    //Test IControllerSenderInstance
                    // - ClearSelectorDict
                    controllerInstance.ClearSelectorList<ITestSender>();
                    controllerInstance.ClearSelectorList<IEmptySender>();
                    controllerInstance.AddSelector<ITestSender>(new RecieverSelector(ModelRelationShip.Self, "", ""));
                    controllerInstance.AddSelector<IEmptySender>(new RecieverSelector(ModelRelationShip.Self, "", ""));
                    Assert.IsTrue(controllerInstance.ContainsSelector<ITestSender>());
                    Assert.IsTrue(controllerInstance.ContainsSelector<IEmptySender>());
                    Assert.AreEqual(1, controllerInstance.SelectorCount<ITestSender>());
                    Assert.AreEqual(1, controllerInstance.SelectorCount<IEmptySender>());

                    controllerInstance.ClearSelectorDict();

                    Assert.IsFalse(controllerInstance.ContainsSelector<ITestSender>());
                    Assert.IsFalse(controllerInstance.ContainsSelector<IEmptySender>());
                    Assert.AreEqual(0, controllerInstance.SelectorCount<ITestSender>());
                    Assert.AreEqual(0, controllerInstance.SelectorCount<IEmptySender>());

                }
            }
        }

        [Test]
        public void NotEntryIControllerSenderFailed()
        {
            #region Initial Enviroment
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(TestView), new TestView.ParamBinder()));
            var allBinder = new ModelViewBinder("*", null,
                new ModelViewBinder.BindInfo(typeof(TestView)));
            var binderMap = new ModelViewBinderMap(viewInstanceCreator,
                allBinder);
            var binderInstanceMap = binderMap.CreateBinderInstaceMap();

            var model = new TestModel();
            binderInstanceMap.RootModel = model;
            #endregion

            TestSenderInstance.SetControllerTypeSettings();

            var useEvents = new Dictionary<string, System.Type>()
            {
                { TestSenderInstance.KEYWORD_ON_TEST, typeof(ITestSender) },
                { "__empty", typeof(IEmptySender) },
            };
            var testEvents = new DefaultControllerSenderGroup<TestSenderInstance>(useEvents);
            var controllerInstance = testEvents.CreateInstance(null, model, binderInstanceMap);
            var testControllerInst = controllerInstance as TestSenderInstance;

            //Test IControllerSenderInstance
            // - AddSelector
            // - ContainsSelector
            // - SelectorCount
            // - EnableSender
            // - DoEnableSender
            // - DisableSender
            // - Send
            Assert.IsFalse(controllerInstance.ContainsSelector<ITest2Sender>());
            Assert.AreEqual(0, controllerInstance.SelectorCount<ITest2Sender>());
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
                controllerInstance.AddSelector(typeof(ITest2Sender), new RecieverSelector(ModelRelationShip.Self, "", "")));
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => controllerInstance.DoEnableSender<ITest2Sender>());
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => controllerInstance.EnableSender<ITest2Sender>());
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => controllerInstance.DisableSender<ITest2Sender>());

            testControllerInst.Send2(-100);
            var errorMessage = $"{typeof(ITest2Sender)}はControllerSenderGroupに登録されていないのでsenderとrecieverのバインドを行わないでください。";
            Assert.IsNull(model.Test2Sender);
            Assert.AreNotEqual(-100, model.Value2, errorMessage);
        }
    }
}
