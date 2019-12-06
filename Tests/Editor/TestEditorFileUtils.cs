using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using System.IO;

namespace Hinode.Tests.Editors
{
    public class TestEditorFileUtils : TestBase
    {
        // A Test behaves as an ordinary method
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
    }
}
