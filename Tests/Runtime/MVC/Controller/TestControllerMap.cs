using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Hinode.Tests.MVC.Controller
{
    /// <summary>
    /// <seealso cref="ControllerMap"/>
    /// <seealso cref="IControllerSenderGroup"/>
    /// <seealso cref="IControllerSender"/>
    /// <seealso cref="IControllerSender"/>
    /// <seealso cref="ControllerSenderGroupObject{InstanceType}"/>
    /// <seealso cref="ControllerTypeManager"/>
    /// </summary>
    public class TestControllerMap
    {
        [Test]
        public void BasicUsagePasses()
        {
            #region Initial Enviroment
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(TestView), new TestView.ParamBinder()));
            var allBinder = new ModelViewBinder("*", null,
                new ModelViewBinder.BindInfo(typeof(TestView)));
            var binderMap = new ModelViewBinderMap(viewInstanceCreator,
                allBinder);
            var binderInstanceMap = binderMap.CreateBinderInstaceMap();

            var model = new TestModel() { Name = "model" };
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
            var useEvents2 = new Dictionary<string, System.Type>()
            {
                { TestSenderInstance.KEYWORD_ON_TEST2, typeof(ITest2Sender) },
            };
            var test2Events = new DefaultControllerSenderGroup<TestSenderInstance>(useEvents2);
            var controllerMap = new ControllerMap(testEvents, test2Events);
            {//Test Constructor
                var errorMessage = "";
                AssertionUtils.AssertEnumerableByUnordered(new IControllerSenderGroup[] {
                    testEvents, test2Events,
                }, controllerMap.SenderGroups, errorMessage);
            }
            {
                Assert.IsTrue(controllerMap.ContainsSenderKeyword(TestSenderInstance.KEYWORD_ON_TEST));
                Assert.IsTrue(controllerMap.ContainsSenderKeyword(TestSenderInstance.KEYWORD_ON_TEST2));
                Assert.IsTrue(controllerMap.ContainsSenderKeyword("__empty"));
            }
            {//Test CreateController (at onTest)
                // Test onTest
                var controllerInstance = controllerMap.CreateController(TestSenderInstance.KEYWORD_ON_TEST, null, model, binderInstanceMap);

                Assert.AreEqual(typeof(TestSenderInstance), controllerInstance.GetType());
                Assert.AreSame(model, controllerInstance.Target);
                Assert.AreSame(binderInstanceMap, controllerInstance.UseBinderInstanceMap);

                Assert.IsTrue(controllerInstance.DoEnableSender<ITestSender>());
                Assert.IsFalse(controllerInstance.DoEnableSender<IEmptySender>(), "ControllerMap#CreateControllerで生成したIControllerSenderInstanceは生成時に指定したKeywordと対応しているSender型のみを有効化してください。");
                AssertionUtils.AssertEnumerableByUnordered(new System.Type[] { typeof(ITestSender) }, controllerInstance.EnabledSenders, "");
            }

            {//Test CreateController (at onTest2)
                model.Value = -1;
                model.TestSender = null;
                model.Value2 = -1;
                model.Test2Sender = null;
                // Test onTest2
                var controllerInstance = controllerMap.CreateController(TestSenderInstance.KEYWORD_ON_TEST2, null, model, binderInstanceMap);

                Assert.AreEqual(typeof(TestSenderInstance), controllerInstance.GetType());
                Assert.AreSame(model, controllerInstance.Target);
                Assert.AreSame(binderInstanceMap, controllerInstance.UseBinderInstanceMap);
                Assert.IsTrue(controllerInstance.DoEnableSender<ITest2Sender>());
                AssertionUtils.AssertEnumerableByUnordered(useEvents2.Values, controllerInstance.EnabledSenders, "");
            }
        }

        interface ITestFookSender : IControllerSender
        { }

        interface ITestFookReciever : IControllerReciever
        {
            void Fook(Model sender, string eventData);
        }

        class FookModel : Model, ITestFookReciever
        {
            public Model FookSender { get; set; }
            public string FookEventData { get; set; }
            public void Fook(Model sender, string eventData)
            {
                FookSender = sender;
                FookEventData = eventData;
            }
        }

        [Test]
        public void AutoBindPasses()
        {
            #region Initial Enviroment
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(TestView), new TestView.ParamBinder()));
            var viewID = "viewID";
            var allBinder = new ModelViewBinder("*", null,
                new ModelViewBinder.BindInfo(viewID, typeof(TestView))
                    .AddControllerInfo(new ModelViewBinder.ControllerInfo(
                            TestSenderInstance.KEYWORD_ON_TEST,
                            new RecieverSelector(RecieverSelector.ModelRelationShip.Self, "", "")
                        )
                    )
                );
            var orangeBinder = new ModelViewBinder("orange", null,
                new ModelViewBinder.BindInfo(viewID, typeof(TestView))
                    .AddControllerInfo(new ModelViewBinder.ControllerInfo(
                            TestSenderInstance.KEYWORD_ON_TEST,
                            new RecieverSelector(RecieverSelector.ModelRelationShip.Self, "", "")
                        )
                    ).AddControllerInfo(new ModelViewBinder.ControllerInfo(
                            TestSenderInstance.KEYWORD_ON_TEST2,
                            new RecieverSelector(RecieverSelector.ModelRelationShip.Self, "", ""),
                            new RecieverSelector(RecieverSelector.ModelRelationShip.Self, "", "") {
                                FookingRecieverType = typeof(ITestFookReciever),
                                FookEventData = "FookParam1"
                            }
                        )
                    )
                );
            var binderMap = new ModelViewBinderMap(viewInstanceCreator, allBinder, orangeBinder);
            Assert.IsNull(binderMap.UseControllerMap);
            TestSenderInstance.SetControllerTypeSettings();

            ControllerTypeManager.EntryRecieverExecuter<ITestFookReciever, string>((reciever, sender, eventData) => {
                reciever.Fook(sender, eventData);
            });

            var useEvents = new Dictionary<string, System.Type>()
            {
                { TestSenderInstance.KEYWORD_ON_TEST, typeof(ITestSender) },
            };
            var testEvents = new DefaultControllerSenderGroup<TestSenderInstance>(useEvents);
            var useOrangeEvents = new Dictionary<string, System.Type>()
            {
                { TestSenderInstance.KEYWORD_ON_TEST2, typeof(ITest2Sender) },
            };
            var orangeEvents = new DefaultControllerSenderGroup<TestSenderInstance>(useOrangeEvents);
            var controllerMap = new ControllerMap(testEvents, orangeEvents);
            binderMap.UseControllerMap = controllerMap;
            #endregion

            var binderInstanceMap = binderMap.CreateBinderInstaceMap();
            Assert.AreSame(binderMap.UseControllerMap, binderInstanceMap.UseControllerMap);

            var root = new TestModel() { Name = "root" };
            var apple = new TestModel() { Name = "apple", Parent = root };
            var orange = new FookModel() { Name = "orange", Parent = root };
            binderInstanceMap.RootModel = root;

            {//Check AutoBind Settings
                //Test
                //  - ModelViewBinderInstance#GetControllerSenders
                var rootBindInst = binderInstanceMap.BindInstances[root];
                var rootViewControllerSenders = rootBindInst.GetControllerSenders(rootBindInst.QueryViews(viewID).First());
                AssertionUtils.AssertEnumerableByUnordered(new System.Type[] {
                    typeof(TestSenderInstance),
                }, rootViewControllerSenders.Select(_s => _s.GetType()), "想定したものになっていません。");

                var appleBindInst = binderInstanceMap.BindInstances[apple];
                var appleViewControllerSenders = appleBindInst.GetControllerSenders(appleBindInst.QueryViews(viewID).First());
                AssertionUtils.AssertEnumerableByUnordered(new System.Type[] {
                    typeof(TestSenderInstance),
                }, appleViewControllerSenders.Select(_s => _s.GetType()), "想定したものになっていません。");

                var orangeBindInst = binderInstanceMap.BindInstances[orange];
                var orangeViewControllerSenders = orangeBindInst.GetControllerSenders(orangeBindInst.QueryViews(viewID).First());
                AssertionUtils.AssertEnumerableByUnordered(new System.Type[] {
                    typeof(TestSenderInstance),
                    typeof(TestSenderInstance),
                }, orangeViewControllerSenders.Select(_s => _s.GetType()), "想定したものになっていません。");
            }

            {//Default Test
                var rootBindInst = binderInstanceMap.BindInstances[root];
                var rootViewObj = rootBindInst.QueryViews(viewID).First();
                var senderInstance = rootBindInst.GetControllerSenders(rootViewObj).First() as TestSenderInstance;

                senderInstance.Send(100);

                Assert.AreSame(root, root.TestSender);
                Assert.AreEqual(100, root.Value);

                // Not Entry Sender in ControllerMap
                root.Test2Sender = null;
                root.Value2 = 0;
                senderInstance.Send2(-100);

                Assert.AreNotSame(root, root.Test2Sender);
                Assert.AreNotEqual(-100, root.Value2);
            }

            {//Fook Test
                var orangeBindInst = binderInstanceMap.BindInstances[orange];
                var orangeViewObj = orangeBindInst.ViewObjects.OfType<TestView>().First();
                var senderInstance = orangeBindInst
                    .GetControllerSenders(orangeViewObj)
                    .Where(_s => _s.ContainsSelector<ITest2Sender>())
                    .OfType<TestSenderInstance>()
                    .First();

                Assert.IsTrue(senderInstance.DoEnableSender<ITest2Sender>());
                senderInstance.Send2(222); // <- Fook ITestFookReciever

                var bindInfo = orangeBinder.BindInfos.First(_i => _i.ID == viewID);
                var controllerInfo = bindInfo.Controllers[TestSenderInstance.KEYWORD_ON_TEST2];
                var fookSelector = controllerInfo.RecieverSelectors.First(_r => _r.IsFooking);
                Assert.AreSame(orange, orange.FookSender);
                Assert.AreEqual(fookSelector.FookEventData, orange.FookEventData);
            }
        }
    }
}
