﻿using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Tests;

namespace Hinode.MVC.Tests
{
    /// <summary>
    /// <seealso cref="ModelViewBinder.ViewInstanceCreator"/>
    /// </summary>
    public class TestViewInstanceCreator
    {
        class ViewObj : EmptyViewObject
        {
            public int Value { get; }
            public ViewObj(int value)
            {
                Value = value;
            }

            public class ParamBinder : IModelViewParamBinder
            {
                public int Value { get; }
                public ParamBinder(int value) { Value = value; }

                public void Update(Model model, IViewObject viewObj)
                {
                }
            }
        }

        class TestCreator : IViewInstanceCreator
        {
            protected override System.Type GetViewObjTypeImpl(string instanceKey)
            {
                var viewObj = CreateViewObjImpl(instanceKey);
                return viewObj.GetType();
            }

            protected override IViewObject CreateViewObjImpl(string instanceKey)
            {
                switch (instanceKey)
                {
                    case "A":
                        return new ViewObj(1);
                    case "B":
                        return new ViewObj(-1);
                    default:
                        return null;
                }
            }

            protected override IModelViewParamBinder GetParamBinderImpl(string binderKey)
            {
                switch (binderKey)
                {
                    case "a":
                        return new ViewObj.ParamBinder(1);
                    case "b":
                        return new ViewObj.ParamBinder(-1);
                    default:
                        return null;
                }
            }
        }

        [Test]
        public void CreateViewObjPasses()
        {
            var creator = new TestCreator();

            var viewObjAbindInfo = new ModelViewBinder.BindInfo("A", "A", "a");
            var viewObjA = creator.CreateViewObj(viewObjAbindInfo);
            Assert.IsTrue(viewObjA is ViewObj);
            Assert.AreEqual(1, (viewObjA as ViewObj).Value);
            Assert.AreSame(viewObjAbindInfo, viewObjA.UseBindInfo);

            var viewObjBbindInfo = new ModelViewBinder.BindInfo("B", "B", "b");
            var viewObjB = creator.CreateViewObj(viewObjBbindInfo);
            Assert.IsTrue(viewObjB is ViewObj);
            Assert.AreEqual(-1, (viewObjB as ViewObj).Value);
            Assert.AreSame(viewObjBbindInfo, viewObjB.UseBindInfo);

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
            {
                var bindInfo = new ModelViewBinder.BindInfo("B", "__invalid", "b");
                var nullObj = creator.CreateViewObj(bindInfo);
            });
        }

        [Test]
        public void GetParamBinderPasses()
        {
            var creator = new TestCreator();

            var viewObjAbindInfo = new ModelViewBinder.BindInfo("A", "A", "a");
            var paramBinderA = creator.GetParamBinder(viewObjAbindInfo);
            Assert.IsTrue(paramBinderA is ViewObj.ParamBinder);
            Assert.AreEqual(1, (paramBinderA as ViewObj.ParamBinder).Value);

            var viewObjBbindInfo = new ModelViewBinder.BindInfo("A", "A", "b");
            var paramBinderB = creator.GetParamBinder(viewObjBbindInfo);
            Assert.IsTrue(paramBinderB is ViewObj.ParamBinder);
            Assert.AreEqual(-1, (paramBinderB as ViewObj.ParamBinder).Value);

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
            {
                var bindInfo = new ModelViewBinder.BindInfo("B", "B", "abc");
                var nullParamBinder = creator.GetParamBinder(bindInfo);
            });
        }

        [Test]
        public void UseParamBinderPasses()
        {
            var creator = new TestCreator();

            var bindInfo = new ModelViewBinder.BindInfo("A", "A", "a")
                .SetUseParamBinder(new ViewObj.ParamBinder(100));
            var paramBinder = creator.GetParamBinder(bindInfo);
            Assert.AreSame(bindInfo.UseParamBinder, paramBinder);
        }
    }
}
