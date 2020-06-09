using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Tests;

namespace Hinode.MVC.Tests.ViewLayout
{
    public class TestSiblingOrderViewLayout
    {
        [SetUp]
        public void SetUp()
        {
            Logger.PriorityLevel = Logger.Priority.Debug;
        }

        class SiblingOrderModel : Model
            , ISiblingOrder
        {
            public SiblingOrderModel(uint order = ISiblingOrderConst.INVALID_ORDER)
            {
                SiblingOrder = order;
            }

            #region ISiblingOrder interface
            public uint SiblingOrder { get; set; }
            #endregion
        }

        [Test]
        public void IsHighSiblingOrderPasses()
        {
            var notSiblingOrderModel = new Model() { Name = "notSiblingOrder" };
            var siblingOrderModel = new SiblingOrderModel() { Name = "SiblingOrder", SiblingOrder = 100 };
            var highestSiblingModelView = new EmptyViewObject()
            {
                UseModel = siblingOrderModel,
                UseBindInfo = new ModelViewBinder.BindInfo("highest", typeof(EmptyViewObject))
                        .AddViewLayoutValue(BasicViewLayoutName.siblingOrder, 200)
            };
            var siblingModelView= new EmptyViewObject()
            {
                UseModel = siblingOrderModel,
                UseBindInfo = new ModelViewBinder.BindInfo("siblingModelView", typeof(EmptyViewObject))
                        .AddViewLayoutValue(BasicViewLayoutName.siblingOrder, 100)
            };
            var siblingModel = new EmptyViewObject()
            {
                UseModel = siblingOrderModel,
                UseBindInfo = new ModelViewBinder.BindInfo("siblingModel", typeof(EmptyViewObject))
            };
            var siblingView = new EmptyViewObject()
            {
                UseModel = notSiblingOrderModel,
                UseBindInfo = new ModelViewBinder.BindInfo("siblingView", typeof(EmptyViewObject))
                        .AddViewLayoutValue(BasicViewLayoutName.siblingOrder, 100)
            };
            var notSiblingView = new EmptyViewObject()
            {
                UseModel = notSiblingOrderModel,
                UseBindInfo = new ModelViewBinder.BindInfo("notSibling", typeof(EmptyViewObject))
            };
            var list = new IViewObject[]
            {
                siblingView,
                notSiblingView,
                highestSiblingModelView,
                siblingModelView,
                siblingModel
            };
            AssertionUtils.AssertEnumerable(
                new IViewObject[] {
                    highestSiblingModelView,
                    siblingModelView,
                    siblingModel,
                    siblingView,
                    notSiblingView,
                }, list.OrderBy(_v => _v, new SiblingOrderViewObjectCompare()),
                $"想定された並び順になっていません");
        }

        [UnityTest]
        public IEnumerator InsertPasses()
        {
            yield return null;

            var parent = new GameObject("parent");
            var binder = new ModelViewBinder.BindInfo(typeof(MonoBehaviourViewObject));

            var child1 = MonoBehaviourViewObject.Create("child1");
            var child2 = MonoBehaviourViewObject.Create("child2");
            var child3 = MonoBehaviourViewObject.Create("child3");

            child1.Bind(new SiblingOrderModel(100), binder, null);
            child2.Bind(new SiblingOrderModel(200), binder, null);
            child3.Bind(new SiblingOrderModel(300), binder, null);

            SiblingOrderViewLayoutAccessor.Insert(parent.transform, child1);
            SiblingOrderViewLayoutAccessor.Insert(parent.transform, child2);
            SiblingOrderViewLayoutAccessor.Insert(parent.transform, child3);

            AssertionUtils.AssertEnumerable(
                new Transform[] {
                    child3.transform, child2.transform, child1.transform
                }, parent.transform.GetChildEnumerable(), "");


            var child4 = MonoBehaviourViewObject.Create("child4");
            child4.Bind(new SiblingOrderModel(150), binder, null);
            SiblingOrderViewLayoutAccessor.Insert(parent.transform, child4);

            AssertionUtils.AssertEnumerable(
                new Transform[] {
                    child3.transform, child2.transform, child4.transform, child1.transform
                }, parent.transform.GetChildEnumerable(), "");
        }

        [UnityTest]
        public IEnumerator InsertToSameIndexPasses()
        {
            yield return null;

            var parent = new GameObject("parent");
            var binder = new ModelViewBinder.BindInfo(typeof(MonoBehaviourViewObject));

            var child1 = MonoBehaviourViewObject.Create("child1");
            var child2 = MonoBehaviourViewObject.Create("child2");
            var child3 = MonoBehaviourViewObject.Create("child3");

            child1.Bind(new SiblingOrderModel(100), binder, null);
            child2.Bind(new SiblingOrderModel(200), binder, null);
            child3.Bind(new SiblingOrderModel(300), binder, null);

            SiblingOrderViewLayoutAccessor.Insert(parent.transform, child1);
            SiblingOrderViewLayoutAccessor.Insert(parent.transform, child2);
            SiblingOrderViewLayoutAccessor.Insert(parent.transform, child3);

            SiblingOrderViewLayoutAccessor.Insert(parent.transform, child1);
            AssertionUtils.AssertEnumerable(
                new Transform[] {
                    child3.transform, child2.transform, child1.transform
                }, parent.transform.GetChildEnumerable(), "");

            SiblingOrderViewLayoutAccessor.Insert(parent.transform, child2);
            AssertionUtils.AssertEnumerable(
                new Transform[] {
                    child3.transform, child2.transform, child1.transform
                }, parent.transform.GetChildEnumerable(), "");

            SiblingOrderViewLayoutAccessor.Insert(parent.transform, child3);
            AssertionUtils.AssertEnumerable(
                new Transform[] {
                    child3.transform, child2.transform, child1.transform
                }, parent.transform.GetChildEnumerable(), "");

        }
    }
}
