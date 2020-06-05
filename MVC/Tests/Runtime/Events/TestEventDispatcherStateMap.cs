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
    /// <seealso cref="EventDispatchStateMap"/>
    /// </summary>
    public class TestEventDispatcherStateMap
    {
        enum TestEventName
        {
            test,
        }

        enum TestDispatchStateName
        {
            test,
            testSecond,
        }
        interface IOnTestReciever : IEventHandler { }

        // A Test behaves as an ordinary method
        [Test]
        public void BasicPasses()
        {
            var viewID = "viewID";
            #region
            var viewCreator = new DefaultViewInstanceCreator(
                (typeof(EmptyViewObject), new EmptyModelViewParamBinder())
            );
            var testBinder = new ModelViewBinder("*", null,
                    new ModelViewBinder.BindInfo(typeof(EmptyViewObject))
                        .AddControllerInfo(new ControllerInfo(TestEventName.test, new EventHandlerSelector(ModelRelationShip.Self, "", ""))),
                    new ModelViewBinder.BindInfo(viewID, typeof(EmptyViewObject))
                        .AddControllerInfo(new ControllerInfo(TestEventName.test, new EventHandlerSelector(ModelRelationShip.Self, "", "")))
                );
            var binderMap = new ModelViewBinderMap(viewCreator,
                    testBinder
                );
            var binderInstanceMap = binderMap.CreateBinderInstaceMap();
            #endregion

            var root = new Model { Name = "root", LogicalID = new ModelIDList("test") };
            var child = new Model { Name = "root", LogicalID = new ModelIDList("test2") };
            child.Parent = root;
            binderInstanceMap.RootModel = root;
            var eventDispatcherStateMap = new EventDispatchStateMap()
                .AddState(TestDispatchStateName.test, new EventDispatchQuery("#test", ""))
                .AddState(TestDispatchStateName.test, new EventDispatchQuery("#test2", "viewID"))
                .AddState(TestDispatchStateName.testSecond, new EventDispatchQuery("#test", "viewID"))
                .AddState(TestDispatchStateName.testSecond, new EventDispatchQuery("#test2", ""))
                ;

            {//DoMatch
                {//root
                    var rootBinderInstance = binderInstanceMap.BindInstances[root];
                    var defaultViewObj = rootBinderInstance.QueryViews(ModelViewBinder.BindInfo.ToID(typeof(EmptyViewObject))).First();
                    var viewObjWithViewID = rootBinderInstance.QueryViews(viewID).First();
                    Assert.IsTrue(eventDispatcherStateMap.DoMatch<IOnTestReciever>(TestDispatchStateName.test, root, null));
                    Assert.IsTrue(rootBinderInstance.ViewObjects
                        .All(_v => eventDispatcherStateMap.DoMatch<IOnTestReciever>(TestDispatchStateName.test, root, _v)));

                    Assert.IsFalse(eventDispatcherStateMap.DoMatch<IOnTestReciever>(TestDispatchStateName.testSecond, root, null));
                    Assert.IsFalse(eventDispatcherStateMap.DoMatch<IOnTestReciever>(TestDispatchStateName.testSecond, root, defaultViewObj));
                    Assert.IsTrue(eventDispatcherStateMap.DoMatch<IOnTestReciever>(TestDispatchStateName.testSecond, root, viewObjWithViewID));

                    var errorMessage = "";
                    AssertionUtils.AssertEnumerableByUnordered(
                        new string[] { TestDispatchStateName.test.ToString() },
                        eventDispatcherStateMap.MatchStates<IOnTestReciever>(root, null),
                        errorMessage);

                    AssertionUtils.AssertEnumerableByUnordered(
                        new string[] { TestDispatchStateName.test.ToString() },
                        eventDispatcherStateMap.MatchStates<IOnTestReciever>(root, defaultViewObj),
                        errorMessage);

                    AssertionUtils.AssertEnumerableByUnordered(
                        new string[] { TestDispatchStateName.test.ToString(), TestDispatchStateName.testSecond.ToString() },
                        eventDispatcherStateMap.MatchStates<IOnTestReciever>(root, viewObjWithViewID),
                        errorMessage);
                }

                {//child
                    var childBinderInstace = binderInstanceMap.BindInstances[child];
                    var defaultViewObj = childBinderInstace.QueryViews(ModelViewBinder.BindInfo.ToID(typeof(EmptyViewObject))).First();
                    var viewObjWithViewID = childBinderInstace.QueryViews(viewID).First();
                    Assert.IsFalse(eventDispatcherStateMap.DoMatch<IOnTestReciever>(TestDispatchStateName.test, child, null));
                    Assert.IsFalse(eventDispatcherStateMap.DoMatch<IOnTestReciever>(TestDispatchStateName.test, child, defaultViewObj));
                    Assert.IsTrue(eventDispatcherStateMap.DoMatch<IOnTestReciever>(TestDispatchStateName.test, child, viewObjWithViewID));

                    Assert.IsTrue(eventDispatcherStateMap.DoMatch<IOnTestReciever>(TestDispatchStateName.testSecond, child, null));
                    Assert.IsTrue(eventDispatcherStateMap.DoMatch<IOnTestReciever>(TestDispatchStateName.testSecond, child, defaultViewObj));
                    Assert.IsTrue(eventDispatcherStateMap.DoMatch<IOnTestReciever>(TestDispatchStateName.testSecond, child, viewObjWithViewID));

                    var errorMessage = "";
                    AssertionUtils.AssertEnumerableByUnordered(
                        new string[] { TestDispatchStateName.testSecond.ToString() },
                        eventDispatcherStateMap.MatchStates<IOnTestReciever>(child, null),
                        errorMessage);

                    AssertionUtils.AssertEnumerableByUnordered(
                        new string[] { TestDispatchStateName.testSecond.ToString() },
                        eventDispatcherStateMap.MatchStates<IOnTestReciever>(child, defaultViewObj),
                        errorMessage);

                    AssertionUtils.AssertEnumerableByUnordered(
                        new string[] { TestDispatchStateName.test.ToString(), TestDispatchStateName.testSecond.ToString() },
                        eventDispatcherStateMap.MatchStates<IOnTestReciever>(child, viewObjWithViewID),
                        errorMessage);
                }
            }
        }

        interface LayerTestEventHandler : IEventHandler
        {
            void LayerTest(Model sender, int eventData);
        }

        [Test]
        public void LayerPasses()
        {
            var layerName = "#LayerName";
            var layer2Name = "#LayerName2";
            var DisableID = "#Disable";
            var LayerID = "#Layer";
            var Layer2ID = "#Layer2";
            var eventDispatcherStateMap = new EventDispatchStateMap()
                .AddState(EventDispatchStateName.disable, new EventDispatchQuery(DisableID, "")
                    .AddIncludedEventType<LayerTestEventHandler>())
                .AddState(layerName, EventDispatchStateName.disable, new EventDispatchQuery($"{LayerID}", "")
                    .AddIncludedEventType<LayerTestEventHandler>())
                .AddState(layer2Name, EventDispatchStateName.disable, new EventDispatchQuery($"{Layer2ID}", "")
                    .AddIncludedEventType<LayerTestEventHandler>());

            {
                var switchingModel = new Model() { Name = "switch1" };
                eventDispatcherStateMap
                    .AddSwitchingModel(switchingModel);
                Assert.IsTrue(eventDispatcherStateMap.DoMatch<LayerTestEventHandler>(
                    EventDispatchStateName.disable
                    , new Model().AddLogicalID(DisableID)
                    , null));

                Assert.IsFalse(eventDispatcherStateMap.DoMatch<LayerTestEventHandler>(
                    EventDispatchStateName.disable
                    , new Model().AddLogicalID(LayerID)
                    , null));

                Assert.IsFalse(eventDispatcherStateMap.DoMatch<LayerTestEventHandler>(
                    EventDispatchStateName.disable
                    , new Model().AddLogicalID(Layer2ID)
                    , null));

                eventDispatcherStateMap.RemoveSwitchingModel(switchingModel);
            }
            Debug.Log($"Success to None Enabled Switching Model");
            {
                var switchingModel = new Model() { Name = "switch1" }
                    .AddLogicalID(layerName);
                eventDispatcherStateMap
                    .AddSwitchingModel(switchingModel);

                Assert.IsTrue(eventDispatcherStateMap.DoMatch<LayerTestEventHandler>(
                    EventDispatchStateName.disable
                    , new Model().AddLogicalID(DisableID)
                    , null));

                Assert.IsTrue(eventDispatcherStateMap.DoMatch<LayerTestEventHandler>(
                    EventDispatchStateName.disable
                    , new Model().AddLogicalID(LayerID)
                    , null));

                Assert.IsFalse(eventDispatcherStateMap.DoMatch<LayerTestEventHandler>(
                    EventDispatchStateName.disable
                    , new Model().AddLogicalID(Layer2ID)
                    , null));

                eventDispatcherStateMap.RemoveSwitchingModel(switchingModel);
            }
            Debug.Log($"Success to Enabled Switching Model(switchingModel)");
            {
                var switchingModel = new Model() { Name = "switch1" }
                    .AddLogicalID(layerName);
                var switchingModel2 = new Model() { Name = "switch2" }
                    .AddLogicalID(layer2Name);
                eventDispatcherStateMap
                    .AddSwitchingModel(switchingModel)
                    .AddSwitchingModel(switchingModel2);

                Assert.IsTrue(eventDispatcherStateMap.DoMatch<LayerTestEventHandler>(
                    EventDispatchStateName.disable
                    , new Model().AddLogicalID(DisableID)
                    , null));

                Assert.IsTrue(eventDispatcherStateMap.DoMatch<LayerTestEventHandler>(
                    EventDispatchStateName.disable
                    , new Model().AddLogicalID(LayerID)
                    , null));

                Assert.IsTrue(eventDispatcherStateMap.DoMatch<LayerTestEventHandler>(
                    EventDispatchStateName.disable
                    , new Model().AddLogicalID(Layer2ID)
                    , null));

                eventDispatcherStateMap
                    .RemoveSwitchingModel(switchingModel)
                    .RemoveSwitchingModel(switchingModel2);
            }
            Debug.Log($"Success to Enabled Multiple Switching Model(switchingModel, switchingModel2)");
        }

        [Test]
        public void DisableDefaultLayerPasses()
        {
            var DisableID = "#Disable";
            var eventDispatcherStateMap = new EventDispatchStateMap()
                .AddState(EventDispatchStateName.disable, new EventDispatchQuery(DisableID, "")
                    .AddIncludedEventType<LayerTestEventHandler>());

            var switchingModel = new Model() { Name = "switch1" }
                .AddLogicalID(EventDispatchStateMap.DISABLE_DEFAULT_LAYER_LOGICAL_ID);
            eventDispatcherStateMap
                .AddSwitchingModel(switchingModel);
            Assert.IsFalse(eventDispatcherStateMap.DoMatch<LayerTestEventHandler>(
                EventDispatchStateName.disable
                , new Model().AddLogicalID(DisableID)
                , null));
        }

        [Test, Description("ModelViewBinderInstaceMapにModelを追加した時に自動的にSwitchingModelに登録するかテスト")]
        public void AutoAddAndRemoveSwitchingModelPasses()
        {
            var binderMap = new ModelViewBinderMap()
            {
                ViewInstanceCreator = new DefaultViewInstanceCreator((typeof(EmptyViewObject), new EmptyModelViewParamBinder())),
                UseEventDispatchStateMap = new EventDispatchStateMap(),
            };
            binderMap.AddBinder(new ModelViewBinder("*", null)
                .AddBindInfo(new ModelViewBinder.BindInfo(typeof(EmptyViewObject))));

            var binderInstacenMap = binderMap.CreateBinderInstaceMap();
            var model = new Model().AddLogicalID(EventDispatchStateMap.AUTO_ADDED_SWITCHING_MODEL_LOGICAL_ID);
            binderInstacenMap.Add(
                model,
                false);

            Assert.IsTrue(binderMap.UseEventDispatchStateMap.SwitchingModels.Any(_m => _m == model));
            Debug.Log($"Success to Auto Add in Switching Models");

            binderInstacenMap.Remove(model);
            Assert.IsFalse(binderMap.UseEventDispatchStateMap.SwitchingModels.Any(_m => _m == model));
            Debug.Log($"Success to Remove Switching Models");
        }

    }
}
