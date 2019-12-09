using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Hinode.Editors;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace Hinode.Tests.Editors
{
    [CustomEditor(typeof(Snapshot))]
    public class SnapshotEditor : Editor
    {
        enum Prop
        {
            Created,
            SnapshotJson,
            AssemblyName,
            ClassName,
            MethodName,
            InnerNumber,
            ScreenshotFilepath,
            ScreenshotFilepathAtTest,
        }

        SerializedPropertyDictionary<Prop> _props;
        private void OnEnable()
        {
            _props = new SerializedPropertyDictionary<Prop>(serializedObject,
                (Prop.Created, "_created"),
                (Prop.SnapshotJson, "_snapshotJson"),
                (Prop.AssemblyName, "_assemblyName"),
                (Prop.ClassName, "_testClassName"),
                (Prop.MethodName, "_testMethodName"),
                (Prop.InnerNumber, "_innerNumber"),
                (Prop.ScreenshotFilepath, "_screenshotFilepath"),
                (Prop.ScreenshotFilepathAtTest, "_screenshotFilepathAtTest")
                );
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField(new GUIContent("Created"), new GUIContent(_props[Prop.Created].stringValue));

            EditorGUILayout.TextField(new GUIContent("Assembly"), _props[Prop.AssemblyName].stringValue);
            EditorGUILayout.TextField(new GUIContent("Class"), _props[Prop.ClassName].stringValue);
            EditorGUILayout.TextField(new GUIContent("Method"), _props[Prop.MethodName].stringValue);
            EditorGUILayout.IntField(new GUIContent("Inner Number"), _props[Prop.InnerNumber].intValue);

            EditorGUILayout.PropertyField(_props[Prop.ScreenshotFilepath], new GUIContent("Screenshot"));
            EditorGUILayout.PropertyField(_props[Prop.ScreenshotFilepathAtTest], new GUIContent("ScreenshotAtTest"));

            if (!EditorApplication.isPlaying && GUILayout.Button("Run Test by Step by Step"))
            {
                StartTest(target as Snapshot);
            }
        }

        void StartTest(Snapshot snapshot)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = $"__{snapshot.name}";
            RunTestStepByStep.SetupScene(scene, snapshot);
            EditorApplication.isPlaying = true;
        }
    }
}
