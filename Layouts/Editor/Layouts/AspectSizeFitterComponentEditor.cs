using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;
using System.Reflection;

namespace Hinode.Layouts
{
    /// <summary>
	/// <seealso cref="AspectSizeFitterComponent"/>
	/// </summary>
    [CustomEditor(typeof(AspectSizeFitterComponent))]
    public class AspectSizeFitterComponentEditor : Editor
    {
        LayoutTargetComponent Parent
        { get; set; }

        protected void OnEnable()
        {
            var inst = target as AspectSizeFitterComponent;
            inst.Target.AutoDetectUpdater();
            inst.LayoutInstance.Target = inst.Target.LayoutTarget;
            Parent = inst.Parent;
            Parent.AutoDetectUpdater();

            Assert.IsNotNull(inst.Target);
            Assert.IsNotNull(inst.Target.LayoutTarget);
        }

        public override void OnInspectorGUI()
        {
            var inst = target as AspectSizeFitterComponent;
            {//Model側にデータを設定する
                inst.Target.AutoDetectUpdater();
                inst.LayoutInstance.Target = inst.Target.LayoutTarget;
                Parent = inst.Parent;
                Parent.AutoDetectUpdater();
            }

            var isValid = inst.LayoutInstance.Validate();
            if(!isValid)
            {
                var reasons = "";
                if (inst.Target.LayoutTarget == null) reasons += $"Target is Null...";
                if (inst.Target.LayoutTarget.Parent == null) reasons += $"Parent is Null...";
                EditorGUILayout.HelpBox($"not be Valid. Skip Layout Caluculation. " + reasons, MessageType.Warning);
            }

            var it = serializedObject.GetIterator();
            it.NextVisible(true);
            while(it.NextVisible(false))
            {
                EditorGUILayout.PropertyField(it, true);
            }

            if(serializedObject.ApplyModifiedProperties())
            {
                if(isValid)
                {
                    //Parent.CopyToLayoutTarget();
                    //inst.Target.CopyToLayoutTarget();
                    inst.LayoutInstance.ForceUpdateLayout();
                    inst.Target.CopyToTransform();
                    Parent.CopyToTransform();
                }
            }
        }
    }
}

