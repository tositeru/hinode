using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Hinode.Tests.Editors
{
    [CustomEditor(typeof(Snapshot))]
    public class SnapshotEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.LabelField("拡張も作ってね");
        }
    }
}
