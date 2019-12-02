using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.IO;

namespace Hinode.Tests.Editors
{
    static class SnapshotSettingsIMGUIRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var provider = new SettingsProvider("Project/Hinode Snapshot Test", SettingsScope.Project)
            {
                label = "Hinode Snapshot Test",
                keywords = new HashSet<string>(new[] { "Test", "Snapshot", "Hinode" }),
                guiHandler = (searchContext) =>
                {
                    var settings = SnapshotSettings.CreateOrGet();
                    var SO = new SerializedObject(settings);
                    EditorGUILayout.PropertyField(SO.FindProperty("_doTakeSnapshot"), new GUIContent("Do Take Snapshot"));
                    if(SO.ApplyModifiedPropertiesWithoutUndo())
                    {
                        SnapshotSettings.Save(SO.targetObject as SnapshotSettings);
                    }
                }
            };
            return provider;
        }
    }
}
