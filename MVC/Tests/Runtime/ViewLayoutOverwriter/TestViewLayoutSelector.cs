using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.MVC.Tests.LayoutOverwriter
{
    public class TestViewLayoutSelector
    {
        class HaveChildViewObject : EmptyViewObject
        {
            public static readonly string CHILD_ID = "child";
            public EmptyViewObject ChildViewObj { get; } = new EmptyViewObject();
            public override object QueryChild(string childID)
                => childID == CHILD_ID
                ? ChildViewObj
                : null;
        }

        [Test]
        public void BasicUsagePasses()
        {
            var query = ".style";
            var selector = new ViewLayoutSelector(query, "");
            Assert.AreEqual(query, selector.Query);
            Assert.IsTrue(selector.ViewID.IsEmpty);

            var model = new Model() { Name = "Model", StylingID = new ModelIDList(query) };
            var otherModel = new Model() { Name = "OtherModel", StylingID = new ModelIDList(".style2") };
            var testData = new List<(bool result, Model, IViewObject)>()
            {
                (true, model, null),
                (true, model, new EmptyViewObject() { UseModel = model, UseBindInfo = new ModelViewBinder.BindInfo("view1", typeof(EmptyViewObject))}),
                (true, model, new HaveChildViewObject() { UseModel = model, UseBindInfo = new ModelViewBinder.BindInfo("view1", typeof(HaveChildViewObject))}),
                (true, model, new HaveChildViewObject() { UseModel = model, UseBindInfo = new ModelViewBinder.BindInfo("view2", typeof(HaveChildViewObject))}),

                (false, otherModel, null),
                (false, otherModel, new EmptyViewObject() { UseModel = otherModel, UseBindInfo = new ModelViewBinder.BindInfo("view1", typeof(EmptyViewObject))}),
                (false, otherModel, new HaveChildViewObject() { UseModel = otherModel, UseBindInfo = new ModelViewBinder.BindInfo("view1", typeof(HaveChildViewObject))}),
                (false, otherModel, new HaveChildViewObject() { UseModel = otherModel, UseBindInfo = new ModelViewBinder.BindInfo("view2", typeof(HaveChildViewObject))}),
            };
            foreach(var (result, m, v) in testData)
            {
                Assert.AreEqual(result, selector.DoMatch(m, v), $"Failed test... selector={selector} result={result}, model={m}, viewObj={v}");
            }
        }

        [Test, Description("Version Specify ViewID")]
        public void BasicUsageSpecifiedViewIDPasses()
        {
            var query = ".style";
            var viewID = "view1";
            var selector = new ViewLayoutSelector(query, viewID);

            var model = new Model() { Name = "Model", StylingID = new ModelIDList(query) };
            var otherModel = new Model() { Name = "OtherModel", StylingID = new ModelIDList(".style2") };
            var testData = new List<(bool result, Model, IViewObject)>()
            {
                (false, model, null),
                (true, model, new EmptyViewObject() { UseModel = model, UseBindInfo = new ModelViewBinder.BindInfo(viewID, typeof(EmptyViewObject))}),
                (false, model, new EmptyViewObject() { UseModel = model, UseBindInfo = new ModelViewBinder.BindInfo("otherViewID", typeof(EmptyViewObject))}),
                (true, model, new HaveChildViewObject() { UseModel = model, UseBindInfo = new ModelViewBinder.BindInfo(viewID, typeof(HaveChildViewObject))}),
                (false, model, new HaveChildViewObject() { UseModel = model, UseBindInfo = new ModelViewBinder.BindInfo("view2", typeof(HaveChildViewObject))}),

                (false, otherModel, null),
                (false, otherModel, new EmptyViewObject() { UseModel = otherModel, UseBindInfo = new ModelViewBinder.BindInfo(viewID, typeof(EmptyViewObject))}),
                (false, otherModel, new EmptyViewObject() { UseModel = otherModel, UseBindInfo = new ModelViewBinder.BindInfo("otherViewID", typeof(EmptyViewObject))}),
                (false, otherModel, new HaveChildViewObject() { UseModel = otherModel, UseBindInfo = new ModelViewBinder.BindInfo(viewID, typeof(HaveChildViewObject))}),
                (false, otherModel, new HaveChildViewObject() { UseModel = otherModel, UseBindInfo = new ModelViewBinder.BindInfo("view2", typeof(HaveChildViewObject))}),
            };
            foreach (var (result, m, v) in testData)
            {
                Assert.AreEqual(result, selector.DoMatch(m, v), $"Failed test... selector={selector} result={result}, model={m}, viewObj={v}");
            }
        }

        [Test]
        public void DoMatchThrowExceptionFail()
        {
            var query = ".style";
            var selector = new ViewLayoutSelector(query, "view1");

            var model = new Model() { Name = "Model", StylingID = new ModelIDList(query) };
            var otherModel = new Model() { Name = "otherModel", StylingID = model.StylingID };
            var testData = new List<(Model, IViewObject)>()
            {
                (null, null),
                (model, new EmptyViewObject() { UseModel = null, UseBindInfo = null }),
                (model, new EmptyViewObject() { UseModel = model, UseBindInfo = null }),
                (model, new EmptyViewObject() { UseModel = null, UseBindInfo = new ModelViewBinder.BindInfo("view1", typeof(EmptyViewObject)) }),
                (model, new EmptyViewObject() { UseModel = otherModel, UseBindInfo = new ModelViewBinder.BindInfo("view1", typeof(EmptyViewObject)) }),
            };
            foreach (var (m, v) in testData)
            {
                Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                    selector.DoMatch(m, v);
                }, $"Failed test... selecotr={selector}, model={m}, viewObj={v}");
            }
        }
    }
}
