using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.TestTools.TestRunner;
using Hinode.Editors;

namespace Hinode.Tests
{
    /// <summary>
    /// Test Runner用のクラスのベースクラス
    /// 
    /// </summary>
    public abstract class TestBase
    {
        #region Snapshot
        SnapshotSettings _snapshotSetting;
        /// <summary>
        /// Snapshotのテスト以外で値を変更しないでください
        /// </summary>
        public bool DoTakeSnapshot
        {
            get => _snapshotSetting.DoTakeSnapshot;
            set => _snapshotSetting.DoTakeSnapshot = value;
        }
        [SetUp]
        public void RecoredSnapshotSetting()
        {
            _snapshotSetting = SnapshotSettings.CreateOrGet();
        }

        /// <summary>
        /// 現在のDoTAkeSnapshotの状態からスナップショットを取るか検証するかどちらかを実行する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="snapshot"></param>
        /// <param name="testStackFrame"></param>
        /// <param name="snapshotNo"></param>
        /// <param name="validateFunc"></param>
        /// <param name="message"></param>
        protected void TakeOrValid<T>(T snapshot, System.Diagnostics.StackFrame testStackFrame, int snapshotNo, System.Func<T, T, bool> validateFunc, string message)
        {
            var newSnapshot = Snapshot.Create(snapshot, testStackFrame);
            var assetPath = newSnapshot.GetAssetPath(snapshotNo);
            if (_snapshotSetting.DoTakeSnapshot)
            {
                var assetDirPath = Path.GetDirectoryName(assetPath);
                EditorFileUtils.CreateDirectory(assetDirPath);
                AssetDatabase.CreateAsset(newSnapshot, assetPath);
                AssetDatabase.SetLabels(newSnapshot, new string[] { "snapshot" });
            }
            else
            {
                var recoredSnapshot = AssetDatabase.LoadAssetAtPath<Snapshot>(assetPath);
                Assert.IsNotNull(recoredSnapshot, $"Don't exist Snapshot... paht=>{assetPath}");
                Assert.IsTrue(validateFunc(recoredSnapshot.GetSnapshot<T>(), snapshot), $"Failed to validate snapshot... : {message}");
            }
        }

        protected IEnumerator TakeOrValidWithCaptureScreen<T>(T snapshot, System.Diagnostics.StackFrame testStackFrame, int snapshotNo, System.Func<T, T, bool> validateFunc, string message)
        {
            TakeOrValid(snapshot, testStackFrame, snapshotNo, validateFunc, message);
            throw new System.NotImplementedException("Capture Screen");
        }

        #endregion

        #region テスト用のファイル・ディレクトリ
        readonly string TEST_DIRPATH = "Assets/__TEST__TEST__TEST__";
        /// <summary>
        /// テスト用のAssetsディレクトリ内のファイルパスを生成する
        /// この関数を通じて作成したファイル・ディレクトリはテスト終了時に自動的に削除されます。
        /// </summary>
        /// <param name="filepathInTestDir"></param>
        /// <returns></returns>
        protected string GetAssetsPathForTest(string filepathInTestDir)
        {
            return Path.Combine(TEST_DIRPATH, filepathInTestDir);
        }

        HashSet<string> _deleteAssets = new HashSet<string>();
        protected void ReserveDeleteAssets(params string[] paths)
        {
            foreach(var p in paths)
            {
                _deleteAssets.Add(p);
            }
        }

        /// <summary>
        /// テスト用のファイル・ディレクトリを削除する
        /// </summary>
        [TearDown]
        public void RemoveTestFileAndDirectory()
        {
            if(AssetDatabase.IsValidFolder(TEST_DIRPATH))
            {
                AssetDatabase.DeleteAsset(TEST_DIRPATH);
                //Directory.Delete(TEST_DIRPATH, true);
            }
            foreach(var p in _deleteAssets)
            {
                AssetDatabase.DeleteAsset(p);
            }
            _deleteAssets.Clear();
        }

        #endregion

        /// <summary>
        /// テスト終了時にシーンに存在するオブジェクトを削除する
        /// </summary>
        [TearDown]
        public virtual void CleanUpScene()
        {
            var PlaymodeTestsControllerType = GetUnityTestRunnerController();

            var scene = SceneManager.GetActiveScene();
            foreach(var root in scene.GetRootGameObjects()
                .Where(_o => !_o.TryGetComponent(PlaymodeTestsControllerType, out var _)))
            {
                Object.DestroyImmediate(root);
            }
        }

        /// <summary>
        /// UnityEngine.TestTools.TestRunner.PlaymodeTestsControllerのvisibilityがinternalだったのでリフレクション経由でその型を取得するための関数
        /// </summary>
        /// <returns></returns>
        System.Type GetUnityTestRunnerController()
        {
            var asm = System.AppDomain.CurrentDomain.GetAssemblies().First(_asm => _asm.GetName().Name == "UnityEngine.TestRunner");
            var PlaymodeTestsControllerType = asm.GetTypes().Where(_t => _t.IsClass).First(_t => _t.Name == "PlaymodeTestsController");
            return PlaymodeTestsControllerType;
        }
    }
}
