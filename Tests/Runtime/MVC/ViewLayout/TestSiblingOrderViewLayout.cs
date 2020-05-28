using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC.ViewLayout
{
    public class TestSiblingOrderViewLayout
    {
        class SiblingOrderModel : Model
            , ISiblingOrder
        {
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
    }
}
