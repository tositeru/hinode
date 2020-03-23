using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Hinode.Editors
{
    [CustomEditor(typeof(InputRecorder))]
    public class InputRecorderEditor : Editor
    {
        enum Props
        {
            Target,
        }
        SerializedPropertyDictionary<Props> props;
        private void OnEnable()
        {
            props = new SerializedPropertyDictionary<Props>(serializedObject,
                (Props.Target, "_target"));
        }

        public override void OnInspectorGUI()
		{
            EditorGUILayout.PropertyField(props[Props.Target]);
            serializedObject.ApplyModifiedProperties();

            if (!Application.isPlaying)
            {
                EditorGUILayout.LabelField("入力データの記録/再生は実行中のみ可能です");
                return;
            }

            var inst = target as InputRecorder;
            EditorGUILayout.LabelField($"Current State => {inst.CurrentState}");
            using (var scope = new EditorGUILayout.HorizontalScope())
            {
                switch (inst.CurrentState)
                {
                    case InputRecorder.State.Recording:
                        if (GUILayout.Button("Finish Record"))
                        {
                            inst.DoneInGameView(() => {
                                inst.StopRecord();
                                inst.SaveToTarget();
                                EditorUtility.SetDirty(inst.Target);
                            });
                        }
                        break;
                    case InputRecorder.State.Replaying:
                        if (GUILayout.Button("Stop Replay"))
                        {
                            inst.DoneInGameView(() => {
                                inst.StopReplay();
                            });
                        }
                        if (GUILayout.Button("PauseReplay"))
                        {
                            inst.DoneInGameView(() => {
                                inst.PauseReplay();
                            });
                        }
                        break;
                    case InputRecorder.State.PauseingReplay:
                        if (GUILayout.Button("Stop Replay"))
                        {
                            inst.DoneInGameView(() => {
                                inst.StopReplay();
                            });
                        }
                        if (GUILayout.Button("Restart Replay"))
                        {
                            inst.DoneInGameView(() => {
                                inst.StartReplay();
                            });
                        }
                        break;
                    default:
                        if (GUILayout.Button("Start Record"))
                        {
                            inst.DoneInGameView(() => {
                                inst.StartRecord();
                            });
                        }
                        bool enableReplay = inst.Target != null && inst.Target.FrameCount > 0;
                        if (enableReplay && GUILayout.Button("Start Replay"))
                        {
                            inst.DoneInGameView(() => {
                                inst.StartReplay();
                            });
                        }
                        break;
                }
            }
		}
    }
}
