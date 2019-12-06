using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using NUnit.Framework;
using UnityEngine.SceneManagement;

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

        public T GetSnapshot<T>()
        {
            return JsonUtility.FromJson<T>(_snapshotJson);
        }

        public static Snapshot Create(string snapshotJson, System.Diagnostics.StackFrame testStackFrame)
        {
            var inst = ScriptableObject.CreateInstance<Snapshot>();

            inst._created = System.DateTime.Now.ToString();
            inst._snapshotJson = snapshotJson;
            var methodInfo = testStackFrame.GetMethod();
            inst._assemblyName = methodInfo.DeclaringType.Assembly.GetName().Name;
            inst._testClassName = methodInfo.DeclaringType.FullName;
            inst._testMethodName = methodInfo.Name;
            return inst;
        }

        public static Snapshot Create<T>(T snapshot, System.Diagnostics.StackFrame testStackFrame)
        {
            return Create(JsonUtility.ToJson(snapshot), testStackFrame);
        }
    }
}
