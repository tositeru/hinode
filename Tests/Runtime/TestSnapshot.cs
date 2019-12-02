using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Editors
{
    [Description("スクリーンショットを取るためにRuntimeに置いているがEditor内のみでの使用を想定している")]
    public class TestSnapshot : TestBase
    {
        [SetUp]
        public void Init()
        {
            PackagePath = PackageDefines.PACKAGE_ASSET_ROOT_PATH;
        }

        [System.Serializable]
        class SnapshotData
        {
            public int value1;
            public string value2;
            
            public bool AreSame(SnapshotData other)
            {
                return value1 == other.value1 &&
                    value2 == other.value2;
            }
        }


        // A Test behaves as an ordinary method
        [Test]
        public void TakeSnapshotPasses()
        {
            var data = new SnapshotData
            {
                value1 = 100,
                value2 = "Test",
            };
            var stackFrame = new StackFrame();
            System.Func<SnapshotData, SnapshotData, bool> validateSnapshot = (correct, got) => correct.AreSame(got);

            //Snapshotの作成のテスト
            DoTakeSnapshot = true;
            var snapshot = TakeOrValid(data, stackFrame, 0, validateSnapshot,
                "Failed to Take snapshot...");
            Assert.AreSame(snapshot, LastSnapshot);

            var snapshotFilepath = CreateSnapshotFilepath(stackFrame);
            Assert.AreEqual(snapshot.GetAssetPath(), snapshotFilepath);

            Assert.IsTrue(Hinode.Editors.EditorFileUtils.IsExistAsset(snapshotFilepath), $"don't exist snapshot file... filepath='{snapshotFilepath}'");
            var savedSnapshot = AssetDatabase.LoadAssetAtPath<Snapshot>(snapshotFilepath);
            AssertionUtils.AssertEnumerable(new[] { "snapshot" }, AssetDatabase.GetLabels(savedSnapshot), "想定したラベルが付けられていません。");

            //Snapshotによる検証のテスト
            DoTakeSnapshot = false;
            Assert.DoesNotThrow(() => {
                var snap = TakeOrValid(data, stackFrame, 0, validateSnapshot, "Failed to Take snapshot...");
                Assert.AreSame(snap, LastSnapshot);
            });
            ReserveDeleteAssets(snapshotFilepath);
        }

        [UnityTest]
        public IEnumerator CaptureScreenshotPasses()
        {
            var data = new SnapshotData
            {
                value1 = 100,
                value2 = "Test",
            };
            var stackFrame = new StackFrame();

            System.Func<SnapshotData, SnapshotData, bool> validateSnapshot = (correct, got) => correct.AreSame(got);

            //Snapshotの作成のテスト
            DoTakeSnapshot = true;
            var enumerator = TakeOrValidWithCaptureScreen(data, stackFrame, 0, validateSnapshot,
                "Failed to Take snapshot...");
            while (enumerator.MoveNext()) { yield return enumerator.Current; }
            Assert.IsNotNull(LastSnapshot);

            var snapshotFilepath = CreateSnapshotFilepath(stackFrame);

            Assert.IsTrue(Hinode.Editors.EditorFileUtils.IsExistAsset(snapshotFilepath));
            var savedSnapshot = AssetDatabase.LoadAssetAtPath<Snapshot>(snapshotFilepath);
            Assert.AreEqual(savedSnapshot, LastSnapshot);
            AssertionUtils.AssertEnumerable(new[] { "snapshot" }, AssetDatabase.GetLabels(savedSnapshot), "想定したラベルが付けられていません。");

            //Snapshotによる検証のテスト
            DoTakeSnapshot = false;
            enumerator = TakeOrValidWithCaptureScreen(data, stackFrame, 0, validateSnapshot, "Failed to Take snapshot...");
            while (enumerator.MoveNext()) { yield return enumerator.Current; }
            Assert.IsNotNull(LastSnapshot);
            var snapshotForTest = LastSnapshot;
            Assert.IsTrue(Hinode.Editors.EditorFileUtils.IsExistAsset(snapshotForTest.ScreenshotFilepath), $"検証用のスクリーンショットはAssetとして保存するようにしてください。assetPath={snapshotForTest.ScreenshotFilepath}");
            Assert.AreEqual(Path.GetDirectoryName(savedSnapshot.GetAssetPath()), Path.GetDirectoryName(snapshotForTest.ScreenshotFilepath), $"検証用のスクリーンショットはSnapshotと同じディレクトリに保存するようにしてください");

            Assert.IsFalse(Hinode.Editors.EditorFileUtils.IsExistAsset(snapshotForTest.ScreenshotFilepathAtTest), $"テスト用のスクリーンショットはAssetとしては保存しないでください. filepath={snapshotForTest.ScreenshotFilepathAtTest}");
            FileAssert.Exists(snapshotForTest.ScreenshotFilepathAtTest, $"テスト用のスクリーンショットはAssetとしては保存しないでください. filepath={snapshotForTest.ScreenshotFilepathAtTest}");
            ReserveDeleteAssets(snapshotForTest.ScreenshotFilepath, snapshotForTest.ScreenshotFilepathAtTest, Path.GetDirectoryName(snapshotFilepath));
        }

        string CreateSnapshotFilepath(StackFrame stackFrame)
        {
            var method = stackFrame.GetMethod();
            var asm = method.DeclaringType.Assembly;
            var (className, methodName) = GetClassAndMethodName(stackFrame);

            var filepath = Path.Combine(
                PackagePath,
                "Snapshots",
                "asm_" + asm.GetName().Name.Replace('.', '_'),
                $"{method.DeclaringType.Namespace}_{className}".Replace('.', '_')
                , methodName + $"_{0}");
            filepath += ".asset";
            return filepath;
        }

        string CreateScreenshotFilepath(StackFrame stackFrame, bool isAtTest)
        {
            var method = stackFrame.GetMethod();
            var asm = method.DeclaringType.Assembly;

            var (className, methodName) = GetClassAndMethodName(stackFrame);

            var filepath = Path.Combine(
                (PackagePath != "") ? PackagePath : "",
                "SnapshotScreenshots",
                asm.GetName().Name.Replace('.', '_'),
                $"{method.DeclaringType.Namespace}_{className}".Replace('.', '_'),
                methodName + $"_{0}{(isAtTest ? "_AtTest" : "")}");
            filepath += ".png";
            return filepath;
        }

        readonly static Regex REGEX_CLASS_NAME = new Regex(@"<(.+)>d");
        (string className, string methodName) GetClassAndMethodName(StackFrame stackFrame)
        {
            var method = stackFrame.GetMethod();
            
            var className = method.DeclaringType.Name;
            var methodName = method.Name;
            if (REGEX_CLASS_NAME.IsMatch(className))
            {
                var match = REGEX_CLASS_NAME.Match(className);
                className = method.DeclaringType.DeclaringType.Name;
                methodName = match.Groups[1].Value;
            }
            return (className, methodName);
        }
    }
}
