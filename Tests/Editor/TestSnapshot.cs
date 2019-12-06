using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Editors
{
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

            var method = stackFrame.GetMethod();
            var asm = method.DeclaringType.Assembly;
            var snapshotFilepath = Path.Combine("Assets", "Snapshots", asm.GetName().Name, method.DeclaringType.FullName, method.Name + $"_{0}")
                .Replace('.', '_');
            snapshotFilepath += ".asset";
            FileAssert.Exists(snapshotFilepath);
            var savedSnapshot = AssetDatabase.LoadAssetAtPath<Snapshot>(snapshotFilepath);
            AssertionUtils.AssertEnumerable(AssetDatabase.GetLabels(savedSnapshot), new[] { "snapshot" }, "想定したラベルが付けられていません。");

            DoTakeSnapshot = false;
            Assert.DoesNotThrow(() => TakeOrValid(data, stackFrame, 0, validateSnapshot, "Failed to Take snapshot..."));

            ReserveDeleteAssets(Path.GetDirectoryName(snapshotFilepath));
        }
    }
}
