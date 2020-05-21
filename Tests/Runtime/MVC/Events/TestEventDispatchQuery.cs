using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC.Events
{
    public class TestEventDispatchQuery
    {
        enum TestEventName
        {
            test,
            test2,
        }

        enum TestDispatchStateName
        {
            test,
            testSecond,
        }
        interface IOnTestReciever : IEventHandler { }
        interface IOnTest2Reciever : IEventHandler { }

        [Test]
        public void BasicPasses()
        {
            var viewID = "viewID";
            var model = new Model() { Name = "model1", LogicalID = new ModelIDList("lg") };
            var viewObj = new EmptyViewObject()
            {
                UseBindInfo = new ModelViewBinder.BindInfo(viewID, typeof(EmptyViewObject))
            };

            {
                var query = new EventDispatchQuery("#lg", "");
                Assert.IsTrue(query.DoEnableEventType<IOnTestReciever>());
                Assert.IsTrue(query.DoEnableEventType<IOnTest2Reciever>());

                Assert.Throws<UnityEngine.Assertions.AssertionException>(() => query.DoMatch<IOnTestReciever>(null, null));
                Assert.IsTrue(query.DoMatch<IOnTestReciever>(model, null));
                Assert.IsTrue(query.DoMatch<IOnTestReciever>(model, viewObj));

                Assert.Throws<UnityEngine.Assertions.AssertionException>(() => query.DoMatch<IOnTest2Reciever>(null, null));
                Assert.IsTrue(query.DoMatch<IOnTest2Reciever>(model, null));
                Assert.IsTrue(query.DoMatch<IOnTest2Reciever>(model, viewObj));
            }

            {
                var query = new EventDispatchQuery("#lg", viewID);
                Assert.IsTrue(query.DoEnableEventType<IOnTestReciever>());
                Assert.IsTrue(query.DoEnableEventType<IOnTest2Reciever>());

                Assert.Throws<UnityEngine.Assertions.AssertionException>(() => query.DoMatch<IOnTestReciever>(null, null));
                Assert.IsFalse(query.DoMatch<IOnTestReciever>(model, null));
                Assert.IsTrue(query.DoMatch<IOnTestReciever>(model, viewObj));

                Assert.Throws<UnityEngine.Assertions.AssertionException>(() => query.DoMatch<IOnTest2Reciever>(null, null));
                Assert.IsFalse(query.DoMatch<IOnTest2Reciever>(model, null));
                Assert.IsTrue(query.DoMatch<IOnTest2Reciever>(model, viewObj));
            }

            {
                var query = new EventDispatchQuery("#lg", "")
                    .AddIncludedEventType<IOnTestReciever>();

                Assert.IsTrue(query.DoEnableEventType<IOnTestReciever>());
                Assert.IsFalse(query.DoEnableEventType<IOnTest2Reciever>());

                Assert.Throws<UnityEngine.Assertions.AssertionException>(() => query.DoMatch<IOnTestReciever>(null, null));
                Assert.IsTrue(query.DoMatch<IOnTestReciever>(model, null));
                Assert.IsTrue(query.DoMatch<IOnTestReciever>(model, viewObj));

                Assert.Throws<UnityEngine.Assertions.AssertionException>(() => query.DoMatch<IOnTest2Reciever>(null, null));
                Assert.IsFalse(query.DoMatch<IOnTest2Reciever>(model, null));
                Assert.IsFalse(query.DoMatch<IOnTest2Reciever>(model, viewObj));
            }

        }
    }
}
