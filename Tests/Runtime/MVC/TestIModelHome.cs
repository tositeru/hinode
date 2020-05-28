using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC
{
    /// <summary>
    /// <seealso cref="IModelHome"/>
    /// </summary>
    public class TestIModelHome : TestBase
    {
        class TestModelHome : IModelHome
        {
            ModelViewBinderMap _binderMap;
            ModelViewBinderInstanceMap _binderInstanceMap;
            public override ModelViewBinderMap BinderMap { get => _binderMap; }
            public override ModelViewBinderInstanceMap BinderInstanceMap { get => _binderInstanceMap; }

            class ViewObj : EmptyViewObject
            {
                public class ParamBinder : IModelViewParamBinder
                {
                    public void Update(Model model, IViewObject viewObj)
                    {
                    }
                }
            }

            private void Awake()
            {
                var allBinder = new ModelViewBinder("*", null,
                    new ModelViewBinder.BindInfo(typeof(ViewObj))
                );
                var viewInstanceCreator = new DefaultViewInstanceCreator(
                    (typeof(ViewObj), new ViewObj.ParamBinder())
                );
                _binderMap = new ModelViewBinderMap(viewInstanceCreator, allBinder);
                _binderInstanceMap = _binderMap.CreateBinderInstaceMap();
            }

            public void Set(ModelViewBinderMap binderMap, ModelViewBinderInstanceMap instanceMap)
            {
                _binderMap = binderMap;
                _binderInstanceMap = instanceMap;
            }
        }

        [UnityTest]
        public IEnumerator RootModelPasses()
        {
            yield return null;
            var rootModelHome = (new GameObject("home1", typeof(TestModelHome))).GetComponent<TestModelHome>();
            var root = new Model() { Name = "root" };

            rootModelHome.BinderInstanceMap.RootModel = root;

            Assert.AreSame(rootModelHome.RootModel, rootModelHome.BinderInstanceMap.RootModel);

            rootModelHome.Set(null, null);
            Assert.IsNull(rootModelHome.RootModel);

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
            {
                rootModelHome.RootModel = root; // <- throw exception
            }, "BinderInstanceMapがNullの時にIModelHome#RootModelを設定した場合は例外を投げるようにしてください。");
        }

        [UnityTest]
        public IEnumerator GetJoinHomesPasses()
        {
            yield return null;
            var rootModelHome = (new GameObject("home1", typeof(TestModelHome))).GetComponent<TestModelHome>();
            var orangeModelHome = (new GameObject("orangeHome1", typeof(TestModelHome))).GetComponent<TestModelHome>();
            var bananaModelHome = (new GameObject("bananaHome1", typeof(TestModelHome))).GetComponent<TestModelHome>();

            var root = new Model() { Name = "root" };
            var apple = new Model() { Name = "root" };
            var orange = new Model() { Name = "root" };
            var grape = new Model() { Name = "root" };
            var banana = new Model() { Name = "banana" };
            var empty = new Model() { Name = "empty" };
            apple.Parent = root;
            orange.Parent = root;
            grape.Parent = orange;

            rootModelHome.RootModel = root;
            orangeModelHome.RootModel = orange;
            bananaModelHome.RootModel = banana;

            var errorMessage = "想定した結果ではありません。";
            AssertionUtils.AssertEnumerable(new IModelHome[] { rootModelHome }, IModelHome.GetJoinHomes(root), errorMessage);
            AssertionUtils.AssertEnumerable(new IModelHome[] { rootModelHome }, IModelHome.GetJoinHomes(apple), errorMessage);
            AssertionUtils.AssertEnumerable(new IModelHome[] { orangeModelHome, rootModelHome }, IModelHome.GetJoinHomes(orange), errorMessage);
            AssertionUtils.AssertEnumerable(new IModelHome[] { orangeModelHome, rootModelHome }, IModelHome.GetJoinHomes(grape), errorMessage);
            AssertionUtils.AssertEnumerable(new IModelHome[] { }, IModelHome.GetJoinHomes(empty), errorMessage);
        }

        [UnityTest]
        public IEnumerator GetNearestHomePasses()
        {
            yield return null;
            var rootModelHome = (new GameObject("home1", typeof(TestModelHome))).GetComponent<TestModelHome>();
            var orangeModelHome = (new GameObject("orangeHome1", typeof(TestModelHome))).GetComponent<TestModelHome>();
            var bananaModelHome = (new GameObject("bananaHome1", typeof(TestModelHome))).GetComponent<TestModelHome>();

            var root = new Model() { Name = "root" };
            var apple = new Model() { Name = "root" };
            var orange = new Model() { Name = "root" };
            var grape = new Model() { Name = "root" };
            var banana = new Model() { Name = "banana" };
            var empty = new Model() { Name = "empty" };
            apple.Parent = root;
            orange.Parent = root;
            grape.Parent = orange;

            rootModelHome.RootModel = root;
            orangeModelHome.RootModel = orange;
            bananaModelHome.RootModel = banana;

            //var errorMessage = "想定した結果ではありません。";
            Assert.AreSame(rootModelHome, IModelHome.GetNearestHome(root));
            Assert.AreSame(rootModelHome, IModelHome.GetNearestHome(apple));
            Assert.AreSame(orangeModelHome, IModelHome.GetNearestHome(orange));
            Assert.AreSame(orangeModelHome, IModelHome.GetNearestHome(grape));
            Assert.AreSame(bananaModelHome, IModelHome.GetNearestHome(banana));
            Assert.IsNull(IModelHome.GetNearestHome(empty));
        }

    }
}
