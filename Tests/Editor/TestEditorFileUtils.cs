using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using System.IO;
using UnityEditor.PackageManager;

namespace Hinode.Tests.Editors
{
    /// <summary>
    /// <see cref="Hinode.Editors.EditorFileUtils"/>
    /// </summary>
    public class TestEditorFileUtils : TestBase
    {
        [Test]
        public void IsProjectAssetPathPasses()
        {
            Assert.IsTrue(Hinode.Editors.EditorFileUtils.IsProjectAssetPath("Assets/hoge.png"));
            Assert.IsTrue(Hinode.Editors.EditorFileUtils.IsProjectAssetPath(Path.Combine(Application.dataPath, "hoge.png")));
            Assert.IsTrue(Hinode.Editors.EditorFileUtils.IsProjectAssetPath(Path.GetFullPath("Assets/hoge.png")));

            Assert.IsFalse(Hinode.Editors.EditorFileUtils.IsProjectAssetPath("hoge.png"));
            Assert.IsFalse(Hinode.Editors.EditorFileUtils.IsProjectAssetPath(Path.Combine(Application.dataPath, "../hoge.png")));
            Assert.IsFalse(Hinode.Editors.EditorFileUtils.IsProjectAssetPath(Path.GetFullPath("hoge.png")));
        }

        [Test]
        public void IsPackageAssetPathPasses()
        {
            Assert.IsTrue(Hinode.Editors.EditorFileUtils.IsPackageAssetPath("Packages/hoge.png"));

            Assert.IsFalse(Hinode.Editors.EditorFileUtils.IsPackageAssetPath(Path.Combine(Application.dataPath, "..", "Packages", "hoge.png")));
            Assert.IsFalse(Hinode.Editors.EditorFileUtils.IsPackageAssetPath("hoge.png"));
            Assert.IsFalse(Hinode.Editors.EditorFileUtils.IsPackageAssetPath(Path.Combine(Application.dataPath, "hoge.png")));
            Assert.IsFalse(Hinode.Editors.EditorFileUtils.IsPackageAssetPath(Path.GetFullPath("Packages/hoge.png")));
            Assert.IsFalse(Hinode.Editors.EditorFileUtils.IsPackageAssetPath(Path.GetFullPath("hoge.png")));
        }

        [Test]
        public void IsExistProjectAssetPasses()
        {
            var testAsset = new TextAsset();
            AssetDatabase.CreateFolder("Assets", "__test");
            AssetDatabase.CreateAsset(testAsset, "Assets/__test/text.txt");
            var assetDirpath = "Assets/__test";
            var assetFilepath = Path.Combine(assetDirpath, "text.txt");
            ReserveDeleteAssets(assetDirpath);
            ReserveDeleteAssets(assetFilepath);

            Assert.IsTrue(Hinode.Editors.EditorFileUtils.IsExistAsset(assetFilepath));
            Assert.IsTrue(Hinode.Editors.EditorFileUtils.IsExistAsset(assetDirpath));

            Assert.IsFalse(Hinode.Editors.EditorFileUtils.IsExistAsset("Assets/hogehohgoe/file.txt"));
            Assert.IsFalse(Hinode.Editors.EditorFileUtils.IsExistAsset("Assets/hogehohgoe"));
        }

        [Test]
        public void IsExistPackageAssetPasses()
        {
            var testAsset = new TextAsset();
            var assetDirpath = PackageDefines.GetHinodeAssetPath("__test");
            var assetFilepath = Path.Combine(assetDirpath, "text.txt");
            AssetDatabase.CreateFolder(PackageDefines.PACKAGE_ASSET_ROOT_PATH, "__test");
            AssetDatabase.CreateAsset(testAsset, assetFilepath);
            ReserveDeleteAssets(assetDirpath);
            ReserveDeleteAssets(assetFilepath);

            Assert.IsTrue(Hinode.Editors.EditorFileUtils.IsExistAsset(assetFilepath));
            Assert.IsTrue(Hinode.Editors.EditorFileUtils.IsExistAsset(assetDirpath));
        }

        [Test]
        public void CreateDirectoryPasses()
        {
            //ディレクトリ名のケース
            var dirPath = GetAssetsPathForTest("A/B/C");
            Assert.DoesNotThrow(() => Hinode.Editors.EditorFileUtils.CreateDirectory(dirPath));
            DirectoryAssert.Exists(dirPath);

            //ファイル名を含むケース
            var dirPath2 = GetAssetsPathForTest("D/E/F/file.txt");
            Assert.DoesNotThrow(() => Hinode.Editors.EditorFileUtils.CreateDirectory(dirPath2));
            DirectoryAssert.Exists(Path.GetDirectoryName(dirPath2));

            // Assets/ディレクトリ配下以外のディレクトリを作成した時は例外を投げる
            Assert.Throws<UnityEngine.Assertions.AssertionException>(
                () => Hinode.Editors.EditorFileUtils.CreateDirectory("Hoge/Hoge"));
        }

        [Test]
        public void CreateDirectoryInPackagesPasses()
        {
            //ディレクトリ名のケース
            //var dirPath = GetAssetsPathForTest("A/B/C");
            var dirPath = Path.Combine(PackageDefines.PACKAGE_ASSET_ROOT_PATH, "__forTest");
            Assert.DoesNotThrow(() => Hinode.Editors.EditorFileUtils.CreateDirectory(dirPath));
            DirectoryAssert.Exists(dirPath);
            ReserveDeleteAssets(dirPath);

            //ファイル名を含むケース
            var dirPath2 = Path.Combine(PackageDefines.PACKAGE_ASSET_ROOT_PATH, "__forTest2", "child", "hoge.txt");
            Assert.DoesNotThrow(() => Hinode.Editors.EditorFileUtils.CreateDirectory(dirPath2));
            DirectoryAssert.Exists(Path.GetDirectoryName(dirPath2));
            ReserveDeleteAssets(dirPath2);
            ReserveDeleteAssets(PackageDefines.GetHinodeAssetPath("__forTest2"));

            // Packagesのディレクトリ配下以外のディレクトリを作成した時は例外を投げる
            Assert.Throws<UnityEngine.Assertions.AssertionException>(
                () => Hinode.Editors.EditorFileUtils.CreateDirectory("Hoge/Hoge"));
        }

        [Test]
        public void GetFullFilepathPasses()
        {
            Assert.AreEqual(
                $"{Application.dataPath}/Test.txt",
                Hinode.Editors.EditorFileUtils.GetFullFilepath("Assets/Test.txt"));

            var hinodePackageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(PackageDefines.GetHinodeAssetPath());
            Assert.AreEqual(
                Path.GetFullPath($"{hinodePackageInfo.resolvedPath}/Test.txt"),
                Hinode.Editors.EditorFileUtils.GetFullFilepath(PackageDefines.GetHinodeAssetPath("Test.txt")));

            Assert.IsNull(Hinode.Editors.EditorFileUtils.GetFullFilepath(null));
            Assert.IsNull(Hinode.Editors.EditorFileUtils.GetFullFilepath(""));
            Assert.IsNull(Hinode.Editors.EditorFileUtils.GetFullFilepath("  "));
        }
    }
}
