using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Hinode.Tests.Components
{
    /// <summary>
    /// <seealso cref="SingletonMonoBehaviour{T}"/>
    /// </summary>
    public class TestSingletonMonoBehaviour : TestBase
    {
        class BasicUsagePassesComponent : SingletonMonoBehaviour<BasicUsagePassesComponent>
        {
            protected override string DefaultInstanceName => "BasicUsagePassesComponent";

            protected override void OnAwaked()
            {
            }

            protected override void OnDestroyed(bool isInstance)
            {
            }
        }

        [UnityTest]
        public IEnumerator BasicUsagePasses()
        {
            yield return null;
            var scene = SceneManager.GetActiveScene();
            var instObjEnumerable = scene.GetGameObjectEnumerable().Where(_o => _o.TryGetComponent<BasicUsagePassesComponent>(out var _));
            Assert.IsFalse(instObjEnumerable.Any());
            Assert.IsFalse(BasicUsagePassesComponent.DoExistInstance);

            var inst = BasicUsagePassesComponent.Instance;
            var instObj = inst.gameObject;
            Assert.IsTrue(BasicUsagePassesComponent.DoExistInstance);
            Assert.AreEqual(1, instObjEnumerable.Count());

            //二つ以上生成されないか確認
            Assert.AreSame(inst, BasicUsagePassesComponent.Instance);
            Assert.AreEqual(1, instObjEnumerable.Count());
            Assert.IsTrue(BasicUsagePassesComponent.DoExistInstance);

            //直接生成した時に自動的に削除されるか確認
            var obj = new GameObject("", typeof(BasicUsagePassesComponent));
            yield return null; //1フレーム遅れで削除されるので、待つ
            Assert.IsTrue(BasicUsagePassesComponent.DoExistInstance);
            Assert.AreEqual(1, instObjEnumerable.Count());
            Assert.AreSame(inst, BasicUsagePassesComponent.Instance);
            Assert.AreSame(instObj, instObjEnumerable.First());
            Assert.IsFalse(obj.TryGetComponent<BasicUsagePassesComponent>(out var _));

            // シーンが切り替わった時は削除される
            SetDontDestroyTestRunner();
            var newScene = SceneManager.CreateScene("_____", new CreateSceneParameters(LocalPhysicsMode.None));
            var sceneAsync = SceneManager.UnloadSceneAsync(scene);
            yield return null;

            Assert.IsFalse(BasicUsagePassesComponent.DoExistInstance);
            instObjEnumerable = newScene.GetGameObjectEnumerable().Where(_o => _o.TryGetComponent<BasicUsagePassesComponent>(out var _));
            Assert.IsFalse(instObjEnumerable.Any());
        }
    }
}
