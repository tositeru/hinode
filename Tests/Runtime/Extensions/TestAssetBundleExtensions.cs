using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;

namespace Hinode.Tests.Extensions
{
    /// <summary>
	/// <seealso cref="AssetBundleExtensions"/>
	/// </summary>
    public class TestAssetBundleExtensions
    {
        AssetBundle LoadTestBundle()
		{
            var filepath = PackageDefines.GetHinodeAssetPath("Tests/Runtime/Extensions/testassetbundle");
            var bundle = AssetBundle.LoadFromFile(filepath);
            Assert.IsNotNull(bundle, $"Failed to load Test AssetBundle... filepath={filepath}");
            return bundle;
        }
        [UnityTest]
        public IEnumerator LoadGameObjectComponentPasses()
        {
            yield return null;

            var gameObject = new GameObject();
            gameObject.AddComponent<BoxCollider>();

            var bundle = LoadTestBundle();
            Assert.IsNotNull(bundle.LoadGameObjectComponent<BoxCollider>("boxcollider"));
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                bundle.LoadGameObjectComponent<RectTransform>("boxcollider");
            });
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                bundle.LoadGameObjectComponent<RectTransform>("__notGameObject");
            });

        }
    }
}
