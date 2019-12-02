using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Hinode.Editors
{
    [CustomEditor(typeof(InputRecord))]
    public class InputRecordEditor : Editor
    {
        enum Prop
        {
            ScreenSize,
            Frames,
            Test,
        }

        SerializedPropertyDictionary<Prop> props;
        PaginationArrayGUIAttributePropertyDrawer frameDrawer = new PaginationArrayGUIAttributePropertyDrawer(15, GetFrameElementLabel);

        private void OnEnable()
        {
            props = new SerializedPropertyDictionary<Prop>(serializedObject,
                (Prop.ScreenSize, "_screenSize"),
                (Prop.Frames, "_frames"));
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(props[Prop.ScreenSize]);

            var label = new GUIContent("Frames");
            frameDrawer.OnGUI(this.GetNextRect(label), props[Prop.Frames], label);
            serializedObject.ApplyModifiedProperties();
        }

        static GUIContent GetFrameElementLabel(int i, SerializedProperty prop)
        {
            var frameNo = prop.FindPropertyRelative("_frameNo").intValue;

            bool isFirst = false;
            if(i > 1)
            {
                var rootFrames = prop.serializedObject.FindProperty("_frames");
                var prevProp = rootFrames.GetArrayElementAtIndex(i-1);
                var prevFrameNo = prevProp.FindPropertyRelative("_frameNo").intValue;
                isFirst = (frameNo - prevFrameNo) > 1;
            }

            return new GUIContent($"{frameNo} Frame {(isFirst ? "<- New" : "")}");
        }
    }
}
