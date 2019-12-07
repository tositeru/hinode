using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

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

        public string Created { get => _created; }
        public string GetAssetPath(int no)
        {
            return Path.Combine("Assets", "Snapshots", _assemblyName.Replace('.', '_'), _testClassName.Replace('.', '_'), $"{_testMethodName}_{no}.asset");
        }
        public string GetScreenshotFilepath(int no, string ext=".png")
        {
            return GetScreenshotFilepath($"{_testMethodName}_{no}", no, ext);
        }
        public string GetScreenshotFilepathAtTest(int no, string ext = ".png")
        {
            return GetScreenshotFilepath($"{_testMethodName}_{no}_AtTest", no, ext);
        }
        string GetScreenshotFilepath(string filename, int no, string ext = ".png")
        {
            return Path.Combine("SnapshotScreenshots", _assemblyName.Replace('.', '_'), _testClassName.Replace('.', '_'), $"{filename}{ext}");
        }

        public T GetSnapshot<T>()
        {
            return JsonUtility.FromJson<T>(_snapshotJson);
        }

        readonly static Regex REGEX_CLASS_NAME = new Regex(@"<(.+)>d");
        public static Snapshot Create(string snapshotJson, System.Diagnostics.StackFrame testStackFrame)
        {
            var inst = ScriptableObject.CreateInstance<Snapshot>();

            inst._created = System.DateTime.Now.ToString();
            inst._snapshotJson = snapshotJson;
            var methodInfo = testStackFrame.GetMethod();
            inst._assemblyName = methodInfo.DeclaringType.Assembly.GetName().Name;

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
            return inst;
        }

        public static Snapshot Create<T>(T snapshot, System.Diagnostics.StackFrame testStackFrame)
        {
            return Create(JsonUtility.ToJson(snapshot), testStackFrame);
        }
    }
}
