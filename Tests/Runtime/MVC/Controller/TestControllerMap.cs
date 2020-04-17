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
    /// <seealso cref="IControllerReciever"/>
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

            {//Test CreateController (at onTest)
                // Test onTest
                var controllerInstance = controllerMap.CreateController(TestSenderInstance.KEYWORD_ON_TEST, model, binderInstanceMap);

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
                var controllerInstance = controllerMap.CreateController(TestSenderInstance.KEYWORD_ON_TEST2, model, binderInstanceMap);

                Assert.AreEqual(typeof(TestSenderInstance), controllerInstance.GetType());
                Assert.AreSame(model, controllerInstance.Target);
                Assert.AreSame(binderInstanceMap, controllerInstance.UseBinderInstanceMap);
                Assert.IsTrue(controllerInstance.DoEnableSender<ITest2Sender>());
                AssertionUtils.AssertEnumerableByUnordered(useEvents2.Values, controllerInstance.EnabledSenders, "");
            }
        }

        //[Test]
        //public void AutoBindPasses()
        //{
        //    #region Initial Enviroment
        //    var viewInstanceCreator = new DefaultViewInstanceCreator(
        //        (typeof(TestView), new TestView.ParamBinder()));
        //    var allBinder = new ModelViewBinder("*", null,
        //        new ModelViewBinder.BindInfo(typeof(TestView))
        //        {
        //            Controllers = {
        //                (TestSenderInstance.KEYWORD_ON_TEST, new RecieverInfo() {
        //                    Selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Self, "", ""),
        //                    Type=RecieverInfo.RecieveType.Pass,
        //                    RecieverType = typeof(ITestReciever)
        //                }),
        //                (TestSenderInstance.KEYWORD_ON_TEST2, new RecieverInfo() {
        //                    Selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Self, "", ""),
        //                    Type=RecieverInfo.RecieveType.Fook,
        //                    RecieverType=typeof(TestFookReciever),
        //                    FookParam=new object[] { "FookParam1" }
        //                }),
        //            }
        //        });
        //    var binderMap = new ModelViewBinderMap(viewInstanceCreator,
        //        allBinder);
        //    Assert.IsNull(binderMap.ControllerMap);

            //ControllerTypeManager.EntryPair<ITestSender, ITestReciever>();
            //ControllerTypeManager.EntryPair<ITest2Sender, ITest2Reciever>();

            //ControllerTypeManager.EntryRecieverExecuter<ITestReciever, int>((reciever, sender, eventData) => {
            //    reciever.Test(sender, eventData);
            //});
            //ControllerTypeManager.EntryRecieverExecuter<ITest2Reciever, int>((reciever, sender, eventData) => {
            //    reciever.Test2(sender, eventData);
            //});

        //    var useEvents = new (string keyword, System.Type)[]
        //    {
        //        (TestSenderInstance.KEYWORD_ON_TEST, typeof(ITestSender))
        //    };
        //    var testEvents = new DefaultControllerSenderGroup<TestSenderInstance>(useEvents);
        //    var controllerMap = new ControllerMap(testEvents);
        //    binderMap.ControllerMap = controllerMap;
        //    #endregion

        //    var binderInstanceMap = binderMap.CreateBinderInstaceMap();
        //    Assert.AreSame(controllerMap, binderInstanceMap.ControllerMap);

        //    var root = new TestModel() { Name = "root" };
        //    var apple = new TestModel() { Name = "apple", Parent = root };
        //    var orange = new Model() { Name = "orange", Parent = root };
        //    binderInstanceMap.RootModel = root;

        //    {
        //        var rootBindInst = binderInstanceMap.BindInstances[root];
        //        Assert.IsNotNull(rootBindInst.UseControllerSenderInstance);
        //        Assert.AreEqual(typeof(TestSenderInstance), rootBindInst.UseControllerSenderInstance);

        //        var appleBindInst = binderInstanceMap.BindInstances[apple];
        //        Assert.IsNotNull(appleBindInst.UseControllerSenderInstance);
        //        Assert.AreEqual(typeof(TestSenderInstance), appleBindInst.UseControllerSenderInstance);

        //        var orangeBindInst = binderInstanceMap.BindInstances[orange];
        //        Assert.IsNull(orangeBindInst, "");
        //    }
        //}
    }
}
