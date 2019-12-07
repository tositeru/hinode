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
            DoTakeSnapshot = true;
            TakeOrValid(data, stackFrame, 0, validateSnapshot,
                "Failed to Take snapshot...");

            var snapshotFilepath = CreateSnapshotFilepath(stackFrame);
            FileAssert.Exists(snapshotFilepath);
            var savedSnapshot = AssetDatabase.LoadAssetAtPath<Snapshot>(snapshotFilepath);
            AssertionUtils.AssertEnumerable(AssetDatabase.GetLabels(savedSnapshot), new[] { "snapshot" }, "想定したラベルが付けられていません。");

            DoTakeSnapshot = false;
            Assert.DoesNotThrow(() => TakeOrValid(data, stackFrame, 0, validateSnapshot, "Failed to Take snapshot..."));

            ReserveDeleteAssets(Path.GetDirectoryName(snapshotFilepath));
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
            DoTakeSnapshot = true;
            var enumerator = TakeOrValidWithCaptureScreen(data, stackFrame, 0, validateSnapshot,
                "Failed to Take snapshot...");
            do { yield return enumerator.Current; } while (enumerator.MoveNext());

            var snapshotFilepath = CreateSnapshotFilepath(stackFrame);
            FileAssert.Exists(snapshotFilepath);
            var savedSnapshot = AssetDatabase.LoadAssetAtPath<Snapshot>(snapshotFilepath);
            AssertionUtils.AssertEnumerable(AssetDatabase.GetLabels(savedSnapshot), new[] { "snapshot" }, "想定したラベルが付けられていません。");

            DoTakeSnapshot = false;
            enumerator = TakeOrValidWithCaptureScreen(data, stackFrame, 0, validateSnapshot, "Failed to Take snapshot...");
            do { yield return enumerator.Current; } while (enumerator.MoveNext());

            var screenshotFilepath = CreateScreenshotFilepath(stackFrame, false);
            var screenshotFilepathAtTest = CreateScreenshotFilepath(stackFrame, true); 
            FileAssert.Exists(screenshotFilepath);
            FileAssert.Exists(screenshotFilepathAtTest);

            ReserveDeleteAssets(Path.GetDirectoryName(snapshotFilepath), screenshotFilepath, screenshotFilepathAtTest);
        }

        string CreateSnapshotFilepath(StackFrame stackFrame)
        {
            var method = stackFrame.GetMethod();
            var asm = method.DeclaringType.Assembly;
            var (className, methodName) = GetClassAndMethodName(stackFrame);

            var filepath = Path.Combine("Assets", "Snapshots", asm.GetName().Name, $"{method.DeclaringType.Namespace}_{className}", methodName + $"_{0}")
                .Replace('.', '_');
            filepath += ".asset";
            return filepath;
        }

        string CreateScreenshotFilepath(StackFrame stackFrame, bool isAtTest)
        {
            var method = stackFrame.GetMethod();
            var asm = method.DeclaringType.Assembly;

            var (className, methodName) = GetClassAndMethodName(stackFrame);

            var filepath = Path.Combine(
                "SnapshotScreenshots",
                asm.GetName().Name,
                $"{method.DeclaringType.Namespace}_{className}",
                methodName + $"_{0}{(isAtTest ? "_AtTest" : "")}")
                .Replace('.', '_');
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
