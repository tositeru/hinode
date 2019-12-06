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
    public class SnapshotSettings : ScriptableObject
    {
        [SerializeField] bool _doTakeSnapshot;

        public bool DoTakeSnapshot { get => _doTakeSnapshot; set => _doTakeSnapshot = value; }

        static readonly string _assetPath = "ProjectSettings/HinodeSnapshotSettings.asset";
        public static SnapshotSettings CreateOrGet()
        {
            SnapshotSettings settings = ScriptableObject.CreateInstance<SnapshotSettings>();
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

        public static void Save(SnapshotSettings settings)
        {
            File.WriteAllText(_assetPath, EditorJsonUtility.ToJson(settings));
        }
    }
}
