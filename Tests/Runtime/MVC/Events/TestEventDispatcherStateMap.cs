using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC.Controller
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
                    var defaultViewObj = rootBinderInstance.QueryViews(typeof(EmptyViewObject).FullName).First();
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
                    var defaultViewObj = childBinderInstace.QueryViews(typeof(EmptyViewObject).FullName).First();
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
    }
}
