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

namespace Hinode.Tests
{
    /// <summary>
    /// スナップショットテストの情報を保存するクラス
    /// </summary>
    public class Snapshot : ScriptableObject
    {
        [SerializeField] string _created;
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
        public string GetAssetPath()
        {
            return Path.Combine("Assets", "Snapshots", _assemblyName.Replace('.', '_'), _testClassName.Replace('.', '_'), $"{_testMethodName}_{_innerNumber}.asset");
        }

        public string ScreenshotFilepath { get => _screenshotFilepath; }
        public string ScreenshotFilepathAtTest { get => _screenshotFilepathAtTest; }

        string GetScreenshotFilepath()
        {
            return CreateScreenshotFilepath($"{_testMethodName}_{_innerNumber}.png");
        }
        string GetScreenshotFilepathAtTest()
        {
            return CreateScreenshotFilepath($"{_testMethodName}_{_innerNumber}_AtTest.png");
        }
        string CreateScreenshotFilepath(string filename)
        {
            return Path.Combine("SnapshotScreenshots", _assemblyName.Replace('.', '_'), _testClassName.Replace('.', '_'), $"{filename}");
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
        public static Snapshot Create(string snapshotJson, System.Diagnostics.StackFrame testStackFrame, int no)
        {
            var inst = ScriptableObject.CreateInstance<Snapshot>();

            inst._created = System.DateTime.Now.ToString();
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

        public static Snapshot Create<T>(T snapshot, System.Diagnostics.StackFrame testStackFrame, int no)
        {
            return Create(JsonUtility.ToJson(snapshot), testStackFrame, no);
        }

        public Snapshot Copy()
        {
            var SO = new SerializedObject(this);
            var it = SO.GetIterator();
            var destSO = new SerializedObject(CreateInstance<Snapshot>());
            it.Next(true);
            for(; it.Next(false); )
            {
                destSO.CopyFromSerializedProperty(it);
            }
            destSO.ApplyModifiedProperties();
            var copy = destSO.targetObject as Snapshot;
            copy.name = $"copy_{this.name}";
            return copy;
        }
    }
}
