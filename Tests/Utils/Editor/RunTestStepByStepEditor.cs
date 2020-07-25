using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Hinode.Tests.Editors
{
    [CustomEditor(typeof(RunTestStepByStep))]
    public class RunTestStepByStepEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var settings = TestSettings.CreateOrGet();
            var doTakeSnapshot = EditorGUILayout.Toggle("Do Take Snapshot", settings.DoTakeSnapshot);

            if (EditorApplication.isPlaying)
            {
                var inst = target as RunTestStepByStep;

                if(!inst.IsComplete && GUILayout.Button("Go Next Step"))
                {
                    inst.GoNext();
                }

                if(GUILayout.Button("Reset"))
                {
                    inst.Reset();
                }
            }
            else
            {
                if (doTakeSnapshot != settings.DoTakeSnapshot)
                {
                    settings.DoTakeSnapshot = doTakeSnapshot;
                    TestSettings.Save(settings);
                }

                if (GUILayout.Button("Play Test"))
                {
                    bool isPlaying = true;
                    if(doTakeSnapshot)
                    {
                        isPlaying = EditorUtility.DisplayDialog(
                            "Confirm to overwrite the snapshot for verification",
                            "Do you run the test as it may overwrite the snapshot for verification?", "Run", "Cancel");
                    }

                    EditorApplication.isPlaying = isPlaying;
                }
            }
        }
    }
}
