using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Hinode.Layouts.Editors
{
    [CustomEditor(typeof(LayoutTargetComponent))]
    public class LayoutTargetComponentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var it = serializedObject.GetIterator();
            it.NextVisible(true);
            while (it.NextVisible(false))
            {
                EditorGUILayout.PropertyField(it);
            }

            if(serializedObject.ApplyModifiedProperties())
            {
                var inst = target as LayoutTargetComponent;

                inst.AutoDetectUpdater();

                inst.LayoutTarget.ClearLayouts();
                foreach (var layout in inst.GetComponents<ILayoutComponent>())
                {
                    inst.LayoutTarget.AddLayout(layout.LayoutInstance);
                }

                foreach (var layout in inst.LayoutTarget.Layouts)
                {
                    layout.UpdateLayout();
                }

                inst.CopyToTransform();
            }

            if (GUILayout.Button("Copy From Transform"))
            {
                var inst = target as LayoutTargetComponent;
                inst.CopyToLayoutTarget();
            }
        }
    }
}
