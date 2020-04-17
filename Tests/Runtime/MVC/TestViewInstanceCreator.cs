using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC
{
    /// <summary>
    /// <seealso cref="ModelViewBinder.ViewInstanceCreator"/>
    /// </summary>
    public class TestViewInstanceCreator
    {
        class ViewObj : IViewObject
        {
            public int Value { get; }
            public ViewObj(int value)
            {
                Value = value;
            }

            public Model UseModel { get; set; }
            public ModelViewBinder.BindInfo UseBindInfo { get; set; }

            public void Bind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap)
            {
                throw new System.NotImplementedException();
            }

            public void Unbind()
            {
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

        class TestCreator : ModelViewBinder.IViewInstanceCreator
        {
            protected override IViewObject CreateViewObjImpl(string instanceKey)
            {
                switch(instanceKey)
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

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                var bindInfo = new ModelViewBinder.BindInfo("B", "__invalid", "b");
                var nullObj = creator.CreateViewObj(bindInfo);
            });
        }

        [Test]
        public void GetParamBinderPasses()
        {
            var creator = new TestCreator();

            var viewObjAbindInfo = new ModelViewBinder.BindInfo("A", "A", "a");
            var paramBinderA = creator.GetParamBinderObj(viewObjAbindInfo);
            Assert.IsTrue(paramBinderA is ViewObj.ParamBinder);
            Assert.AreEqual(1, (paramBinderA as ViewObj.ParamBinder).Value);

            var viewObjBbindInfo = new ModelViewBinder.BindInfo("A", "A", "b");
            var paramBinderB = creator.GetParamBinderObj(viewObjBbindInfo);
            Assert.IsTrue(paramBinderB is ViewObj.ParamBinder);
            Assert.AreEqual(-1, (paramBinderB as ViewObj.ParamBinder).Value);

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                var bindInfo = new ModelViewBinder.BindInfo("B", "B", "abc");
                var nullParamBinder = creator.GetParamBinderObj(bindInfo);
            });
        }

    }
}
