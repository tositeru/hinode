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
using System.Text.RegularExpressions;

namespace Hinode.Tests
{
    /// <summary>
    /// Test Runner用のクラスのベースクラス
    /// 
    /// </summary>
    public abstract class TestBase
    {
        /// <summary>
        /// テスト対象のパッケージのパス
        ///
        /// 何も設定されていない時は現在のProjectのパスを表します。
        /// Snapshotの保存先になどに影響を与えます。
        /// </summary>
        protected string PackagePath { get; set; }

        #region Snapshot
        Snapshot _lastSnapshot;
        SnapshotSettings _snapshotSetting;

        /// <summary>
        /// 最後に作成した/使用したSnapshotを返します。
        ///
        /// TakeOrValidWithCaptureScreen()で作成したSnapshotを取得したい場合はこちらを使用してください
        /// </summary>
        protected Snapshot LastSnapshot
        {
            get => _lastSnapshot;
            private set => _lastSnapshot = value;
        }

        /// <summary>
        /// スナップショットを作成するかどうかを表すフラグ。
        /// 
        /// ProjectSettingsのSnapshotから変更できます。
        /// Snapshotのテスト以外で値を変更しないでください
        /// </summary>
        public bool DoTakeSnapshot
        {
            get => _snapshotSetting.DoTakeSnapshot;
            set => _snapshotSetting.DoTakeSnapshot = value;
        }

        [SetUp]
        public void ReadSnapshotSetting()
        {
            _snapshotSetting = SnapshotSettings.CreateOrGet();
        }

        /// <summary>
        /// 現在のDoTakeSnapshotの状態からスナップショットを取るか検証するかどちらかを実行する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="snapshot"></param>
        /// <param name="testStackFrame"></param>
        /// <param name="snapshotNo"></param>
        /// <param name="validateFunc"></param>
        /// <param name="message"></param>
        protected Snapshot TakeOrValid<T>(T snapshot, System.Diagnostics.StackFrame testStackFrame, int snapshotNo, System.Func<T, T, bool> validateFunc, string message)
        {
            var newSnapshot = Snapshot.Create(snapshot, testStackFrame, snapshotNo, PackagePath);
            var assetPath = newSnapshot.GetAssetPath();
            if (DoTakeSnapshot)
            {
                var assetDirPath = Path.GetDirectoryName(assetPath);
                EditorFileUtils.CreateDirectory(assetDirPath);
                //Debug.Log($"debug -- create snapshot: {assetPath}");
                AssetDatabase.CreateAsset(newSnapshot, assetPath);
                AssetDatabase.SetLabels(newSnapshot, new string[] { "snapshot" });
            }
            else
            {
                var recoredSnapshot = AssetDatabase.LoadAssetAtPath<Snapshot>(assetPath);
                Assert.IsNotNull(recoredSnapshot, $"Don't exist Snapshot... paht=>{assetPath}");
                Assert.IsTrue(validateFunc(recoredSnapshot.GetSnapshot<T>(), snapshot), $"Failed to validate snapshot... : {message}");
            }
            LastSnapshot = newSnapshot;
            return newSnapshot;
        }

        readonly string FOR_SCREENSHOT_LABEL = "__for_screenshot";
        protected IEnumerator TakeOrValidWithCaptureScreen<T>(T snapshot, System.Diagnostics.StackFrame testStackFrame, int snapshotNo, System.Func<T, T, bool> validateFunc, string message)
        {
            var newSnapshot = TakeOrValid(snapshot, testStackFrame, snapshotNo, validateFunc, message);

            var cameraObj = new GameObject("__ScreenshotCamera");
            var camera = cameraObj.AddComponent<Camera>();
            camera.targetTexture = new RenderTexture(512, 512, 1);

            foreach(var canvas in Object.FindObjectsOfType<Canvas>()
                .Where(_c => _c.renderMode == RenderMode.ScreenSpaceOverlay))
            {
                var label = canvas.gameObject.AddComponent<LabelObj>();
                label.Add(FOR_SCREENSHOT_LABEL);
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
            }

            yield return new WaitForEndOfFrame();
            var holdActiveRT = RenderTexture.active;
            RenderTexture.active = camera.targetTexture;
            var captureTex = new Texture2D(RenderTexture.active.width, RenderTexture.active.height);
            captureTex.ReadPixels(new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height), 0, 0);
            captureTex.Apply();
            RenderTexture.active = holdActiveRT;

            newSnapshot.SaveScreenshot(captureTex, !DoTakeSnapshot);

            foreach (var pair in Object.FindObjectsOfType<LabelObj>()
                .Where(_l => _l.Contains(FOR_SCREENSHOT_LABEL))
                .Select(_l => (l: _l, c: _l.GetComponent<Canvas>())))
            {
                pair.c.renderMode = RenderMode.ScreenSpaceOverlay;
                pair.c.worldCamera = null;
                Object.Destroy(pair.l);
            }

            Object.Destroy(cameraObj);
            LastSnapshot = newSnapshot;
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

        /// <summary>
        /// 指定したファイルパスをテスト終了時に削除するよう登録します
        ///
        /// ファイル名を指定した時はファイルのみを、フォルダー名を指定した時は指定したフォルダーのみ削除します。
        /// 削除したファイルのフォルダーや、親フォルダーは削除されませんので、削除したい時は別途指定してください。
        /// </summary>
        /// <param name="paths"></param>
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
            //var projectAssetPathRegex = new Regex($"^{Application.dataPath}");
            //var packagesAssetPathRegex = new Regex($"^Packages/");
            foreach (var p in _deleteAssets)
            {
                //var fullpath = Path.GetFullPath(p);
                Debug.Log($"delete test filepath {p}");
                if(EditorFileUtils.IsProjectAssetPath(p)//projectAssetPathRegex.IsMatch(fullpath)
                    || EditorFileUtils.IsPackageAssetPath(p))// packagesAssetPathRegex.IsMatch(p))
                {
                    AssetDatabase.DeleteAsset(p);
                }
                else if(File.Exists(p))
                {
                    File.Delete(p);
                }
                else if(Directory.Exists(p))
                {
                    Directory.Delete(p);
                }
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
            var scene = SceneManager.GetActiveScene();
            var objEnumerable = scene.GetRootGameObjects().AsEnumerable();

            var PlaymodeTestsControllerType = GetUnityPlaymodeTestRunnerControllerType();
            if (PlaymodeTestsControllerType != null)
            {
                objEnumerable = objEnumerable.Where(_o => !_o.TryGetComponent(PlaymodeTestsControllerType, out var _));
            }

            foreach (var root in objEnumerable)
            {
                Object.DestroyImmediate(root);
            }
        }

        /// <summary>
        /// UnityのTestRunnerオブジェクトをDontDestroyOnLoadにする
        /// テスト中にシーンを切り替える際に使用してください
        /// </summary>
        protected void SetDontDestroyTestRunner()
        {
            var scene = SceneManager.GetActiveScene();
            var objEnumerable = scene.GetRootGameObjects().AsEnumerable();

            var PlaymodeTestsControllerType = GetUnityPlaymodeTestRunnerControllerType();
            if (PlaymodeTestsControllerType != null)
            {
                objEnumerable = objEnumerable.Where(_o => _o.TryGetComponent(PlaymodeTestsControllerType, out var _));
            }

            foreach (var root in objEnumerable)
            {
                Object.DontDestroyOnLoad(root);
            }
        }

        /// <summary>
        /// UnityEngine.TestTools.TestRunner.PlaymodeTestsControllerのvisibilityがinternalだったのでリフレクション経由でその型を取得するための関数
        /// </summary>
        /// <returns></returns>
        System.Type GetUnityPlaymodeTestRunnerControllerType()
        {
            var asm = System.AppDomain.CurrentDomain.GetAssemblies().First(_asm => _asm.GetName().Name == "UnityEngine.TestRunner");
            var PlaymodeTestsControllerType = asm.GetTypes().Where(_t => _t.IsClass).First(_t => _t.Name == "PlaymodeTestsController");
            return PlaymodeTestsControllerType;
        }
    }
}
