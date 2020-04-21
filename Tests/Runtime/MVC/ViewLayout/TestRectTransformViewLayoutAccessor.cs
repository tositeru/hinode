using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC.ViewLayout
{
    /// <summary>
	/// <seealso cref="TestRectTransformViewLayoutAccessor"/>
	/// </summary>
    public class TestRectTransformViewLayoutAccessor : TestBase
    {
        [Test]
        public void CheckClassDefinePasses()
        {
            Assert.IsTrue(typeof(RectTransformViewLayoutAccessor).DoHasInterface<IViewObject>());
            Assert.IsTrue(typeof(RectTransformViewLayoutAccessor).DoHasInterface<IRectTransformAnchorMinViewLayout>());
            Assert.IsTrue(typeof(RectTransformViewLayoutAccessor).DoHasInterface<IRectTransformAnchorMaxViewLayout>());
            Assert.IsTrue(typeof(RectTransformViewLayoutAccessor).DoHasInterface<IRectTransformAnchorXViewLayout>());
            Assert.IsTrue(typeof(RectTransformViewLayoutAccessor).DoHasInterface<IRectTransformAnchorYViewLayout>());
            Assert.IsTrue(typeof(RectTransformViewLayoutAccessor).DoHasInterface<IRectTransformPivotViewLayout>());
            Assert.IsTrue(typeof(RectTransformViewLayoutAccessor).DoHasInterface<IRectTransformSizeViewLayout>());
        }

        [Test]
        public void CheckViewLayouterPasses()
        {
            var viewLayouter = new ViewLayouter();
            RectTransformViewLayoutAccessor.AddKeywordsAndAutoCreator(viewLayouter);
            var keywords = new Dictionary<string, IViewLayoutAccessor>() {
                { "anchorX", new RectTransformAnchorXViewLayoutAccessor() },
                { "anchorY", new RectTransformAnchorYViewLayoutAccessor()},
                { "anchorMin", new RectTransformAnchorMinViewLayoutAccessor()},
                { "anchorMax", new RectTransformAnchorMaxViewLayoutAccessor()},
                { "pivot", new RectTransformPivotViewLayoutAccessor()},
                { "size", new RectTransformSizeViewLayoutAccessor()},
            };
            foreach(var (keyword, accessor) in keywords.Select(_t => (_t.Key, _t.Value)))
            {
                Assert.IsTrue(viewLayouter.ContainsKeyword(keyword), $"Don't exist {keyword}...");
                Assert.AreSame(accessor.GetType(), viewLayouter.Accessors[keyword].GetType(), $"cur={accessor.GetType()}, got={viewLayouter.Accessors[keyword].GetType()}");
                Assert.IsTrue(viewLayouter.ContainAutoViewObjectCreator(keyword), $"Don't exist autoLayoutCreator... keyword={keyword}");
            }
            Assert.IsTrue(viewLayouter.ContainAutoViewObjectCreator(keywords.Keys));
        }

        class ViewObj : MonoBehaviour, IViewObject
        {
            public Model UseModel { get; set; }
            public ModelViewBinder.BindInfo UseBindInfo { get; set; }
            public ModelViewBinderInstance UseBinderInstance { get; set; }

            public void Bind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap)
            { }

            public void Unbind()
            { }
        }

        class NoneMonoBehaviourViewObj : IViewObject
        {
            public Model UseModel { get; set; }
            public ModelViewBinder.BindInfo UseBindInfo { get; set; }
            public ModelViewBinderInstance UseBinderInstance { get; set; }

            public void Bind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap)
            { }

            public void Unbind()
            { }
        }

        [UnityTest]
        public IEnumerator AutoLayoutViewCreatorPasses()
        {
            yield return null;
            var creator = new RectTransformViewLayoutAccessor.AutoCreator();

            AssertionUtils.AssertEnumerableByUnordered(new System.Type[]{
                typeof(IRectTransformAnchorMinViewLayout),
                typeof(IRectTransformAnchorMaxViewLayout),
                typeof(IRectTransformAnchorXViewLayout),
                typeof(IRectTransformAnchorYViewLayout),
                typeof(IRectTransformPivotViewLayout),
                typeof(IRectTransformSizeViewLayout),
            }, creator.GetSupportedIViewLayouts(), "Please inherit IRectTransformXXXViewLayout interface...");

            {
                var obj = new GameObject("layout",
                    typeof(RectTransform),
                    typeof(ViewObj));
                var viewObj = obj.GetComponent<ViewObj>();
                var rectTransformLayoutAccessor = creator.Create(viewObj);
                Assert.IsNotNull(rectTransformLayoutAccessor);
                Assert.IsTrue(viewObj.TryGetComponent<RectTransformViewLayoutAccessor>(out var getAccessor));

                var inst2 = creator.Create(viewObj);
                Assert.AreSame(getAccessor, inst2, "既にComponentが追加されていたらそれを返すようにし、一つ以上追加しないようにする。");
                Assert.AreEqual(1, viewObj.GetComponents<RectTransformViewLayoutAccessor>().Count());
            }

            {
                var noneMonoBehaviourViewObj = new NoneMonoBehaviourViewObj();
                Assert.Throws<UnityEngine.Assertions.AssertionException>(() => creator.Create(noneMonoBehaviourViewObj));
            }
        }
    }
}
