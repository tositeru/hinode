using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using System.IO;
using System.Linq;
using Hinode.Tests;

namespace Hinode.MVC.Tests.ViewInstanceCreator
{
    /// <summary>
    /// <seealso cref="UnityViewInstanceCreatorSettings"/>
    /// </summary>
    public class TestUnityViewInstanceCreatorSettings
    {
        readonly string TEST_ASSET_DIR_PATH = "MVC/Tests/Runtime/ViewInstanceCreator";

        [Test]
        public void InitializeInstanceInfoPasses()
        {
            var assetPath = PackageDefines.GetHinodeAssetPath(TEST_ASSET_DIR_PATH, "testViewObjPrefab.prefab");
            var testViewObjPrefab = AssetDatabase.LoadAssetAtPath<Transform>(assetPath);
            Assert.IsNotNull(testViewObjPrefab, $"テスト用のPrefabの読み込みに失敗しました... assetPath={assetPath}");

            {//最小限の設定の時の値確認
                var info = new UnityViewInstanceCreatorSettings.InstanceInfo()
                {
                    Type = UnityViewInstanceCreatorSettings.InfoType.Asset,
                    InstanceTypeFullName = testViewObjPrefab.GetComponent<IViewObject>().GetType().FullName,
                    ParamBinderTypeFullName = typeof(EmptyModelViewParamBinder).FullName,
                    AssetReference = testViewObjPrefab,
                };
                var instanceType = testViewObjPrefab.GetComponent<IViewObject>().GetType();
                var paramBinderType = typeof(EmptyModelViewParamBinder);
                Assert.AreEqual(UnityViewInstanceCreatorSettings.InfoType.Asset, info.Type);
                Assert.AreEqual(instanceType.FullName, info.InstanceTypeFullName);
                Assert.AreEqual(instanceType, info.InstanceType);
                Assert.AreEqual(instanceType.FullName, info.InstanceKey);
                Assert.AreEqual(instanceType.FullName, info.BinderKey);
                Assert.AreEqual(paramBinderType.FullName, info.ParamBinderTypeFullName);
                Assert.AreEqual(paramBinderType, info.ParamBinderType);
                Assert.AreEqual(testViewObjPrefab, info.AssetReference);
            }

            {//Keyの明示的な設定の時の値確認
                var instanceKey = "instKey";
                var binderKey = "binderKey";
                var assetBundlePath = Path.Combine(TEST_ASSET_DIR_PATH, "test_asset_bundle");
                var assetFilePath = Path.Combine(TEST_ASSET_DIR_PATH, "Resources");
                var info = new UnityViewInstanceCreatorSettings.InstanceInfo()
                {
                    InstanceKey = instanceKey,
                    BinderKey = binderKey,
                    AssetBundleName = assetBundlePath,
                    AssetPath = assetFilePath,

                    Type = UnityViewInstanceCreatorSettings.InfoType.Resources,
                    InstanceTypeFullName = testViewObjPrefab.GetComponent<IViewObject>().GetType().FullName,
                    ParamBinderTypeFullName = typeof(EmptyModelViewParamBinder).FullName,
                    AssetReference = testViewObjPrefab,
                };
                var instanceType = testViewObjPrefab.GetComponent<IViewObject>().GetType();
                var paramBinderType = typeof(EmptyModelViewParamBinder);
                Assert.AreEqual(UnityViewInstanceCreatorSettings.InfoType.Resources, info.Type);
                Assert.AreEqual(instanceType.FullName, info.InstanceTypeFullName);
                Assert.AreEqual(instanceType, info.InstanceType);
                Assert.AreEqual(instanceKey, info.InstanceKey);
                Assert.AreEqual(binderKey, info.BinderKey);
                Assert.AreEqual(paramBinderType.FullName, info.ParamBinderTypeFullName);
                Assert.AreEqual(paramBinderType, info.ParamBinderType);
                Assert.AreEqual(testViewObjPrefab, info.AssetReference);
                Assert.AreEqual(assetBundlePath, info.AssetBundleName);
                Assert.AreEqual(assetFilePath, info.AssetPath);
            }

        }

        [UnityTest]
        public IEnumerator SetToPasses()
        {
            yield return null;

            var assetPath = PackageDefines.GetHinodeAssetPath(TEST_ASSET_DIR_PATH, "testViewObjPrefab.prefab");
            var testViewObjPrefab = AssetDatabase.LoadAssetAtPath<ButtonViewObject>(assetPath);
            Assert.IsNotNull(testViewObjPrefab, $"テスト用のPrefabの読み込みに失敗しました... assetPath={assetPath}");

            var bundleFilepath = PackageDefines.GetHinodeAssetPath(TEST_ASSET_DIR_PATH, "test_view_instance_creator_bundle");
            var bundle = AssetBundle.LoadFromFile(bundleFilepath);
            Assert.IsNotNull(bundle, $"Failed to load AssetBundle({bundleFilepath}) For Test...");

            var settings = ScriptableObject.CreateInstance<UnityViewInstanceCreatorSettings>();
            settings
                .AddInfo(UnityViewInstanceCreatorSettings.InstanceInfo.CreateAsset<EmptyModelViewParamBinder>(testViewObjPrefab)
                    .SetInstanceKey("instA")
                    .SetBinderKey("binderA")
                )
                .AddInfo(UnityViewInstanceCreatorSettings.InstanceInfo.CreateAssetBundle<ButtonViewObject, EmptyModelViewParamBinder>(bundle.name, "assets/testviewobjprefab.prefab")
                    .SetInstanceKey("instB")
                    .SetBinderKey("binderB")
                )
            ;

            Debug.Log($"{bundle.GetAllAssetNames().Aggregate("", (_s, _c) => $"{_s}{_c};")}");

            settings.EntryAssetBundle(bundle); //

            var creator = new UnityViewInstanceCreator();
            settings.SetTo(creator);
            foreach(var info in settings.Infos)
            {
                var bindInfo = new ModelViewBinder.BindInfo("", info.InstanceKey, info.BinderKey);
                Assert.AreEqual(info.InstanceType, creator.GetViewObjType(bindInfo), $"instanceKey={info.InstanceKey}, binderKey={info.BinderKey}");
                Assert.AreEqual(info.ParamBinderType, creator.GetParamBinderType(bindInfo), $"instanceKey={info.InstanceKey}, binderKey={info.BinderKey}");
            }
            bundle.Unload(true);
        }
    }
}
