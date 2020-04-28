#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Linq;
using UnityEngine.TestTools;
using Hinode.Editors;

namespace Hinode.Tests
{
    /// <summary>
    /// スナップショットテストの情報を保存するクラス
    /// </summary>
    public class Snapshot : ScriptableObject
    {
        [SerializeField] string _created;
        /// <summary>
        /// スナップショットが所属しているUnityのパッケージの名前。
        /// 指定されていなければ、Projectに所属していると扱います。
        /// </summary>
        [SerializeField] string _packageName;
        [SerializeField] string _snapshotJson;

        [SerializeField] string _assemblyName;
        [SerializeField] string _testClassName;
        [SerializeField] string _testMethodName;
        /// <summary>
        /// 同じテスト内での番号
        /// </summary>
        [SerializeField] int _innerNumber;

        [SerializeField, TextureFilepath] string _screenshotFilepath;
        [SerializeField, TextureFilepath] string _screenshotFilepathAtTest;

        public string Created { get => _created; }
        /// <summary>
        /// プロジェクトのアセットかどうか?
        ///
        /// プロジェクトのアセットかパッケージのアセットかでファイルの保存先が異なりますので注意してください
        /// </summary>
        public bool IsProjectAsset { get => _packageName == null || _packageName == ""; }

        /// <summary>
        /// 自身のアセットパスを返す
        /// </summary>
        /// <returns></returns>
        public string GetAssetPath()
        {
            var useRootPath = IsProjectAsset ? "Assets" : _packageName;
            return Path.Combine(
                useRootPath,
                "Snapshots",
                "asm_"+_assemblyName.Replace('.', '_'),
                _testClassName.Replace('.', '_'),
                $"{_testMethodName}_{_innerNumber}.asset");
        }

        public string ScreenshotFilepath { get => _screenshotFilepath; }
        public string ScreenshotFilepathAtTest { get => _screenshotFilepathAtTest; }

        public void SaveScreenshot(Texture2D screenshot, bool isAtTest)
        {
            if(isAtTest)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ScreenshotFilepathAtTest));
                File.WriteAllBytes(ScreenshotFilepathAtTest, screenshot.EncodeToPNG());
            }
            else
            {
                EditorFileUtils.CreateDirectory(ScreenshotFilepath);
                File.WriteAllBytes(ScreenshotFilepath, screenshot.EncodeToPNG());
                AssetDatabase.ImportAsset(ScreenshotFilepath, ImportAssetOptions.Default);
                var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(ScreenshotFilepath);
                AssetDatabase.SetLabels(asset, new string[] { "snapshot" });
            }
        }

        string GetScreenshotFilepath()
        {
            return CreateScreenshotFilepath($"{_testMethodName}_{_innerNumber}.png", false);
        }
        string GetScreenshotFilepathAtTest()
        {
            return CreateScreenshotFilepath($"{_testMethodName}_{_innerNumber}_AtTest.png", true);
        }
        string CreateScreenshotFilepath(string filename, bool isAtTest)
        {
            if(isAtTest)
            {
                return Path.Combine(
                    "SnapshotScreenshots",
                    IsProjectAsset ? "Project" : _packageName.Replace('.', '_'),
                    _assemblyName.Replace('.', '_'),
                    _testClassName.Replace('.', '_'),
                    $"{filename}");
            }
            else
            {
                return Path.Combine(
                    Path.GetDirectoryName(GetAssetPath()),
                    filename);
            }
        }

        public T GetSnapshot<T>()
        {
            return JsonUtility.FromJson<T>(_snapshotJson);
        }

        public Assembly GetTestAssmebly()
        {
            return System.AppDomain.CurrentDomain.GetAssemblies()
                .First(_asm => _asm.GetName().Name == _assemblyName);
        }

        public class TestData
        {
            readonly object _inst;
            readonly TypeInfo _classInfo;
            readonly MethodInfo _methodInfo;
            readonly List<MethodInfo> _setupMethodInfos;
            readonly List<MethodInfo> _tearDownMethodInfos;

            public object Instance { get => _inst; }
            public TypeInfo ClassInfo { get => _classInfo; }
            public MethodInfo MethodInfo { get => _methodInfo; }
            public List<MethodInfo> SetupMethodInfos { get => _setupMethodInfos; }
            public List<MethodInfo> TearDownMethodInfos { get => _tearDownMethodInfos; }

            public TestData(object inst, TypeInfo classInfo, MethodInfo methodInfo, List<MethodInfo> setupMethodInfos, List<MethodInfo> tearDownMethodInfos)
            {
                _inst = inst;
                _classInfo = classInfo;
                _methodInfo = methodInfo;
                _setupMethodInfos = setupMethodInfos;
                _tearDownMethodInfos = tearDownMethodInfos;
            }

            public IEnumerator ExecuteMethod()
            {
                return (IEnumerator)_methodInfo.Invoke(Instance, new object[] { });
            }

            public void ExecuteSetUpMethods()
            {
                foreach(var info in _setupMethodInfos)
                {
                    info.Invoke(Instance, new object[] { });
                }
            }

            public void ExecuteTearDownMethods()
            {
                foreach (var info in _tearDownMethodInfos)
                {
                    info.Invoke(Instance, new object[] { });
                }
            }
        }

        public TestData GetTestData()
        {
            var asm = GetTestAssmebly();
            var classInfo = asm.DefinedTypes.First(_t => _t.FullName == _testClassName);
            var testMethodInfo = classInfo.GetMethod(_testMethodName);
            Assert.IsNotNull(testMethodInfo.GetCustomAttribute<UnityTestAttribute>(), "テスト対象のメソッドが存在しません。");

            var cstor = classInfo.GetConstructor(new System.Type[] { });
            var instance = cstor.Invoke(null);

            var setupMethods = classInfo.GetRuntimeMethods()
                .Where(_m => _m.GetCustomAttributes<SetUpAttribute>().Any());
            var tearDownMethods = classInfo.GetRuntimeMethods()
                .Where(_m => _m.GetCustomAttributes<TearDownAttribute>().Any());

            return new TestData(instance, classInfo, testMethodInfo, setupMethods.ToList(), tearDownMethods.ToList());
        }

        readonly static Regex REGEX_CLASS_NAME = new Regex(@"<(.+)>d");
        public static Snapshot Create(string snapshotJson, System.Diagnostics.StackFrame testStackFrame, int no, string packageName)
        {
            var inst = ScriptableObject.CreateInstance<Snapshot>();

            inst._created = System.DateTime.Now.ToString();
            inst._packageName = packageName;
            inst._snapshotJson = snapshotJson;
            var methodInfo = testStackFrame.GetMethod();
            inst._assemblyName = methodInfo.DeclaringType.Assembly.GetName().Name;
            inst._innerNumber = no;

            string className;
            string methodName;
            if(REGEX_CLASS_NAME.IsMatch(methodInfo.DeclaringType.Name))
            {
                var match = REGEX_CLASS_NAME.Match(methodInfo.DeclaringType.Name);
                className = methodInfo.DeclaringType.DeclaringType.Name;
                methodName = match.Groups[1].Value;
            }
            else
            {
                className = methodInfo.DeclaringType.Name;
                methodName = methodInfo.Name;
            }
            inst._testClassName = methodInfo.DeclaringType.Namespace + "." + className;
            inst._testMethodName = methodName;

            inst._screenshotFilepath = inst.GetScreenshotFilepath();
            inst._screenshotFilepathAtTest = inst.GetScreenshotFilepathAtTest();
            return inst;
        }

        public static Snapshot Create<T>(T snapshot, System.Diagnostics.StackFrame testStackFrame, int no, string packageName)
        {
            return Create(JsonUtility.ToJson(snapshot), testStackFrame, no, packageName);
        }

        public Snapshot Copy()
        {
            var copy = SerializedObjectExtensions.Copy(this, CreateInstance<Snapshot>());
            copy.name = $"copy_{this.name}";
            return copy;
        }
    }
}
#endif