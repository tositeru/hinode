#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Hinode.Tests
{
    /// <summary>
    /// Snapshotの設定
    /// </summary>
    public class TestSettings : ScriptableObject
    {
        [SerializeField] bool _doTakeSnapshot;
        [SerializeField] bool _enableABTest = false;
        [SerializeField] int _defaultABTestLoopCount = 10000;

        public bool DoTakeSnapshot { get => _doTakeSnapshot; set => _doTakeSnapshot = value; }
        public bool EnableABTest { get => _enableABTest; }
        public int DefaultABTestLoopCount { get => _defaultABTestLoopCount; }

        public System.Random GetRandomForABTest()
        {
            var seed = System.DateTime.Now.Second + System.DateTime.Now.Millisecond + System.DateTime.Now.Minute;
            return new System.Random(seed);
        }

        static readonly string _assetPath = "ProjectSettings/HinodeTestSettings.asset";
        public static TestSettings CreateOrGet()
        {
            var settings = ScriptableObject.CreateInstance<TestSettings>();
            if (File.Exists(_assetPath))
            {
                var t = File.ReadAllText(_assetPath);
                EditorJsonUtility.FromJsonOverwrite(t, settings);
            }
            else
            {
                Save(settings);
            }
            return settings;
        }

        public static void Save(TestSettings settings)
        {
            File.WriteAllText(_assetPath, EditorJsonUtility.ToJson(settings));
        }
    }
}
#endif