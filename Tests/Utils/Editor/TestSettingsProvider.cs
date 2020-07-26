using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.IO;

namespace Hinode.Tests.Editors
{
    static class TestSettingsIMGUIRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var provider = new SettingsProvider("Project/Hinode Test Settings", SettingsScope.Project)
            {
                label = "Hinode Test Settings",
                keywords = new HashSet<string>(new[] { "Test", "Hinode" }),
                guiHandler = (searchContext) =>
                {
                    var settings = TestSettings.CreateOrGet();
                    var SO = new SerializedObject(settings);
                    EditorGUILayout.PropertyField(SO.FindProperty("_doTakeSnapshot"), new GUIContent("Do Take Snapshot"));
                    EditorGUILayout.PropertyField(SO.FindProperty("_enableABTest"), new GUIContent("Enable A/B Test"));
                    EditorGUILayout.PropertyField(SO.FindProperty("_defaultABTestLoopCount"), new GUIContent("Default A/B Test Loop Count"));

                    if (SO.ApplyModifiedPropertiesWithoutUndo())
                    {
                        TestSettings.Save(SO.targetObject as TestSettings);
                    }
                }
            };
            return provider;
        }
    }
}
