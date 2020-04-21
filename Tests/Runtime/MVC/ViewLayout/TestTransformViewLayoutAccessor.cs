using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC.ViewLayout
{
    /// <summary>
    /// <seealso cref="TransformViewLayoutAccessor"/>
    /// </summary>
    public class TestTransformViewLayoutAccessor : TestBase
    {
        [Test]
        public void CheckClassDefinePasses()
        {
            Assert.IsTrue(typeof(TransformViewLayoutAccessor).DoHasInterface<IViewObject>());
            Assert.IsTrue(typeof(TransformViewLayoutAccessor).DoHasInterface<ITransformParentViewLayout>());
            Assert.IsTrue(typeof(TransformViewLayoutAccessor).DoHasInterface<ITransformPosViewLayout>());
            Assert.IsTrue(typeof(TransformViewLayoutAccessor).DoHasInterface<ITransformRotateViewLayout>());
            Assert.IsTrue(typeof(TransformViewLayoutAccessor).DoHasInterface<ITransformLocalPosViewLayout>());
            Assert.IsTrue(typeof(TransformViewLayoutAccessor).DoHasInterface<ITransformLocalRotateViewLayout>());
            Assert.IsTrue(typeof(TransformViewLayoutAccessor).DoHasInterface<ITransformLocalScaleViewLayout>());
        }

        [Test]
        public void CheckViewLayouterPasses()
        {
            var viewLayouter = new ViewLayouter();
            TransformViewLayoutAccessor.AddKeywordsAndAutoCreator(viewLayouter);
            var keywords = new Dictionary<string, IViewLayoutAccessor>() {
                { "parent", new TransformParentViewLayoutAccessor() },
                { "pos", new TransformPosViewLayoutAccessor()},
                { "rotate", new TransformRotateViewLayoutAccessor()},
                { "localPos", new TransformLocalPosViewLayoutAccessor()},
                { "localRotate", new TransformLocalRotateViewLayoutAccessor()},
                { "localScale", new TransformLocalScaleViewLayoutAccessor()},
            };
            foreach (var (keyword, accessor) in keywords.Select(_t => (_t.Key, _t.Value)))
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
            var creator = new TransformViewLayoutAccessor.AutoCreator();

            AssertionUtils.AssertEnumerableByUnordered(new System.Type[]{
                typeof(ITransformParentViewLayout),
                typeof(ITransformPosViewLayout),
                typeof(ITransformRotateViewLayout),
                typeof(ITransformLocalPosViewLayout),
                typeof(ITransformLocalRotateViewLayout),
                typeof(ITransformLocalScaleViewLayout),
            }, creator.GetSupportedIViewLayouts(), "Please inherit ITransformXXXViewLayout interface...");

            {
                var obj = new GameObject("layout",
                    typeof(RectTransform),
                    typeof(ViewObj));
                var viewObj = obj.GetComponent<ViewObj>();
                var rectTransformLayoutAccessor = creator.Create(viewObj);
                Assert.IsNotNull(rectTransformLayoutAccessor);
                Assert.IsTrue(viewObj.TryGetComponent<TransformViewLayoutAccessor>(out var getAccessor));

                var inst2 = creator.Create(viewObj);
                Assert.AreSame(getAccessor, inst2, "既にComponentが追加されていたらそれを返すようにし、一つ以上追加しないようにする。");
                Assert.AreEqual(1, viewObj.GetComponents<TransformViewLayoutAccessor>().Count());
            }

            {
                var noneMonoBehaviourViewObj = new NoneMonoBehaviourViewObj();
                Assert.Throws<UnityEngine.Assertions.AssertionException>(() => creator.Create(noneMonoBehaviourViewObj));
            }
        }

        class TestComponent : MonoBehaviour, IViewObject
        {
            public Model UseModel { get; set; }
            public ModelViewBinder.BindInfo UseBindInfo { get; set; }
            public ModelViewBinderInstance UseBinderInstance { get; set; }

            public void Bind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap)
            {
            }

            public void Unbind()
            {
            }
        }

        class ViewInstanceCreator : IViewInstanceCreator
        {
            protected override IViewObject CreateViewObjImpl(string instanceKey)
            {
                if (typeof(TestComponent).FullName == instanceKey)
                {
                    var obj = new GameObject(instanceKey);
                    return obj.AddComponent<TestComponent>();
                }
                else
                {
                    throw new System.NotImplementedException($"対応しているIViewObjectが見つかりません。instanceKey={instanceKey}");
                }
            }

            static readonly Dictionary<string, IModelViewParamBinder> _paramBinderDict = new Dictionary<string, IModelViewParamBinder>()
            {
                { typeof(TestComponent).FullName, new EmptyModelViewParamBinder() },
            };
            protected override IModelViewParamBinder GetParamBinderImpl(string binderKey)
            {
                if (_paramBinderDict.ContainsKey(binderKey))
                {
                    return _paramBinderDict[binderKey];
                }
                else
                {
                    throw new System.NotImplementedException($"対応しているBindInfoが見つかりません。instanceKey={binderKey}");
                }
            }
        }

        [UnityTest, Description("TransformParentViewLayoutAccessorに対してModelViewSelectorを値に指定した時のテスト")]
        public IEnumerator ParentLayoutAccessorPasses()
        {
            yield return null;
            #region Construct Enviroment
            var viewID = "testComponent";
            var binderMap = new ModelViewBinderMap(new ViewInstanceCreator(),
                new ModelViewBinder("root", null,
                    new ModelViewBinder.BindInfo(viewID, typeof(TestComponent))
                ),
                new ModelViewBinder("child", null,
                    new ModelViewBinder.BindInfo(viewID, typeof(TestComponent))
                        .AddViewLayout("parent", new ModelViewSelector(ModelViewSelector.ModelRelationShip.Parent, "", viewID))
                )
            );
            binderMap.UseViewLayouter = new ViewLayouter();

            TransformViewLayoutAccessor.AddKeywordsAndAutoCreator(binderMap.UseViewLayouter);
            var binderInstanceMap = binderMap.CreateBinderInstaceMap();

            var root = new Model() { Name = "root" };
            var parent = new Model() { Name = "root", Parent = root };
            var child = new Model() { Name = "child", Parent = parent };
            #endregion

            binderInstanceMap.RootModel = root; // <- Here test point!!

            {//Basic Usage
                var parentBindInstance = binderInstanceMap.BindInstances[parent];
                var parentViewObj = parentBindInstance.ViewObjects[0] as TestComponent;

                var childBindInstance = binderInstanceMap.BindInstances[child];
                var childViewObj = childBindInstance.ViewObjects[0] as TestComponent;
                Assert.AreSame(parentViewObj.transform, childViewObj.transform.parent);
            }

            {//自身を親に設定した時
                var childBindInstance = binderInstanceMap.BindInstances[child];
                var childViewObj = childBindInstance.ViewObjects[0] as TestComponent;
                var childAutoViewObj = childBindInstance.AutoLayoutViewObjects[childViewObj].First() as TransformViewLayoutAccessor;

                var selfSelector = new ModelViewSelector(ModelViewSelector.ModelRelationShip.Self, "", viewID);
                binderMap.UseViewLayouter.Set("parent", selfSelector, childAutoViewObj);

                Assert.AreSame(null, childViewObj.transform.parent);
            }

            {//複数ある時
                var rootBindInstance = binderInstanceMap.BindInstances[root];
                var rootViewObj = rootBindInstance.ViewObjects[0] as TestComponent;

                var parentBindInstance = binderInstanceMap.BindInstances[root];
                var parentViewObj = parentBindInstance.ViewObjects[0] as TestComponent;

                var childBindInstance = binderInstanceMap.BindInstances[child];
                var childViewObj = childBindInstance.ViewObjects[0] as TestComponent;
                var childAutoViewObj = childBindInstance.AutoLayoutViewObjects[childViewObj].First() as TransformViewLayoutAccessor;

                //Set Default value
                childViewObj.transform.SetParent(null);

                var selector = new ModelViewSelector(ModelViewSelector.ModelRelationShip.Parent, "*", viewID);
                binderMap.UseViewLayouter.Set("parent", selector, childAutoViewObj);

                var enumerable = selector.Query<TestComponent>(child, binderInstanceMap);
                Debug.Log($"{enumerable.Count()} parent={childViewObj.transform.parent != null}");
                Assert.IsTrue(selector.Query<TestComponent>(child, binderInstanceMap)
                    .Any(_c => _c.transform == childViewObj.transform.parent));
            }

            {//一致しなかった時
                var rootBindInstance = binderInstanceMap.BindInstances[root];
                var rootViewObj = rootBindInstance.ViewObjects[0] as TestComponent;

                var childBindInstance = binderInstanceMap.BindInstances[child];
                var childViewObj = childBindInstance.ViewObjects[0] as TestComponent;
                var childAutoViewObj = childBindInstance.AutoLayoutViewObjects[childViewObj].First() as TransformViewLayoutAccessor;

                //Set Default value
                childViewObj.transform.SetParent(rootViewObj.transform);

                var selfSelector = new ModelViewSelector(ModelViewSelector.ModelRelationShip.Child, "", viewID);
                binderMap.UseViewLayouter.Set("parent", selfSelector, childAutoViewObj);

                Assert.AreSame(null, childViewObj.transform.parent);
            }
        }
    }
}
