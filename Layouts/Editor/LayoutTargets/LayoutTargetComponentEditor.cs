using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Hinode.Layouts.Editors
{
    [CustomEditor(typeof(LayoutTargetComponent))]
    public class LayoutTargetComponentEditor : Editor
    {
        enum AnchorPresetType
        {
            Low,
            Middle,
            High,
            Expand,
            Free,
        }

        bool _foldoutAnchorTypePreset=true;
        bool _foldoutAnchorExpandFlag = true;

        class AnchorPresetInfo
        {
            public int ElementIndex { get; }
            public string Name { get; set; }
            public AnchorPresetType Type { get; set; }

            public AnchorPresetInfo(string name, AnchorPresetType type)
            {
                Name = name;
                Type = type;

                ElementIndex = name == "x" ? 0
                    : name == "y" ? 1
                        : 2;
            }

            public static AnchorPresetType ToAnchorePresetType(float anchorMin, float anchorMax)
            {
                if (anchorMin == 0f && anchorMax == 0f) return AnchorPresetType.Low;
                if (anchorMin == 0.5f && anchorMax == 0.5f) return AnchorPresetType.Middle;
                if (anchorMin == 1f && anchorMax == 1f) return AnchorPresetType.High;
                if (anchorMin == 0f && anchorMax == 1f) return AnchorPresetType.Expand;
                return AnchorPresetType.Free;
            }
        }

        AnchorPresetInfo[] _anchorPresetTypes = new AnchorPresetInfo[]
        {
            new AnchorPresetInfo("x", AnchorPresetType.Middle),
            new AnchorPresetInfo("y", AnchorPresetType.Middle),
            new AnchorPresetInfo("z", AnchorPresetType.Middle),
        };

        private void OnEnable()
        {
            var inst = (target as LayoutTargetComponent);

            for(var i=0; i<3; ++i)
            {
                _anchorPresetTypes[i].Type = AnchorPresetInfo.ToAnchorePresetType(inst.LayoutTarget.AnchorMin[i], inst.LayoutTarget.AnchorMax[i]);
            }
        }

        public override void OnInspectorGUI()
        {
            var it = serializedObject.GetIterator();
            it.NextVisible(true);
            while (it.NextVisible(false))
            {
                EditorGUILayout.PropertyField(it);
            }

            _foldoutAnchorTypePreset = EditorGUILayout.Foldout(_foldoutAnchorTypePreset, "Anchor Min/Max Preset");
            if (_foldoutAnchorTypePreset)
            {
                using (var indentScope = new EditorGUI.IndentLevelScope())
                {
                    foreach (var presetType in _anchorPresetTypes)
                    {
                        using (var scope = new EditorGUILayout.HorizontalScope())
                        {
                            var targetSP = serializedObject.FindProperty("_target");
                            var anchorMinSP = targetSP.FindPropertyRelative("_anchorMin");
                            var anchorMaxSP = targetSP.FindPropertyRelative("_anchorMax");
                            var localSizeSP = targetSP.FindPropertyRelative("_localSize");

                            var newAnchorType = (AnchorPresetType)EditorGUILayout.EnumPopup(presetType.Name, presetType.Type);
                            if (presetType.Type != newAnchorType)
                            {
                                presetType.Type = newAnchorType;

                                var elementMinSP = anchorMinSP.FindPropertyRelative(presetType.Name);
                                var elementMaxSP = anchorMaxSP.FindPropertyRelative(presetType.Name);
                                switch (presetType.Type)
                                {
                                    case AnchorPresetType.Low:
                                        elementMinSP.floatValue = 0f;
                                        elementMaxSP.floatValue = 0f;
                                        break;
                                    case AnchorPresetType.Middle:
                                        elementMinSP.floatValue = 0.5f;
                                        elementMaxSP.floatValue = 0.5f;
                                        break;
                                    case AnchorPresetType.High:
                                        elementMinSP.floatValue = 1f;
                                        elementMaxSP.floatValue = 1f;
                                        break;
                                    case AnchorPresetType.Expand:
                                        elementMinSP.floatValue = 0f;
                                        elementMaxSP.floatValue = 1f;
                                        break;
                                }
                            }

                            var anchorAreaSize = GetLayoutSize(this.target as LayoutTargetComponent);
                            var doExpand = MathUtils.AreNearlyEqual(anchorAreaSize[presetType.ElementIndex], localSizeSP.vector3Value[presetType.ElementIndex], LayoutDefines.NUMBER_PRECISION);
                            var newDoExpand = EditorGUILayout.Toggle("doExpand?", doExpand);
                            if (newDoExpand != doExpand && newDoExpand)
                            {
                                var layoutSize = GetLayoutSize(this.target as LayoutTargetComponent);
                                var tmp = localSizeSP.vector3Value;
                                tmp[presetType.ElementIndex] = layoutSize[presetType.ElementIndex];
                                localSizeSP.vector3Value = tmp;
                            }
                        }
                    }
                }
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
                //TODO 他のLayoutTargetComponentの再計算を行う
            }

            if (GUILayout.Button("Copy From Transform"))
            {
                var inst = target as LayoutTargetComponent;
                inst.CopyToLayoutTarget();
            }
        }

        static Vector3 GetAnchorAreaSize(LayoutTargetComponent layoutTargetComponent)
        {
            if(layoutTargetComponent.LayoutTarget.Parent == null)
            {
                var parent = layoutTargetComponent.transform.parent;
                if(parent.TryGetComponent<LayoutTargetComponent>(out var parentLayoutTarget))
                {
                    layoutTargetComponent.LayoutTarget.SetParent(parentLayoutTarget.LayoutTarget);
                    return layoutTargetComponent.LayoutTarget.AnchorAreaSize();
                }
                else if(parent.transform is RectTransform)
                {
                    var parentR = parent.transform as RectTransform;
                    var self = layoutTargetComponent.LayoutTarget;
                    return parentR.rect.size.Mul(self.AnchorMax - self.AnchorMin);
                }
                else
                {
                    return Vector3.zero;
                }
            }
            else
            {
                return layoutTargetComponent.LayoutTarget.AnchorAreaSize();
            }
        }

        static Vector3 GetLayoutSize(LayoutTargetComponent layoutTargetComponent)
        {
            var self = layoutTargetComponent.LayoutTarget;
            var localSize = Vector3.zero;
            if (layoutTargetComponent.LayoutTarget.Parent == null)
            {
                var parent = layoutTargetComponent.transform.parent;
                if(parent == null)
                {

                }
                else if (parent.TryGetComponent<LayoutTargetComponent>(out var parentLayoutTarget))
                {
                    localSize = parentLayoutTarget.LayoutTarget.LocalSize;
                }
                else if (parent.transform is RectTransform)
                {
                    var parentR = parent.transform as RectTransform;
                    localSize = parentR.rect.size;
                }
            }
            else
            {
                localSize = layoutTargetComponent.LayoutTarget.Parent.LocalSize;
            }

            if (self.LayoutInfo.LayoutSize.x >= 0) localSize.x = Mathf.Min(localSize.x, self.LayoutInfo.LayoutSize.x);
            if (self.LayoutInfo.LayoutSize.y >= 0) localSize.y = Mathf.Min(localSize.y, self.LayoutInfo.LayoutSize.y);
            if (self.LayoutInfo.LayoutSize.z >= 0) localSize.z = Mathf.Min(localSize.z, self.LayoutInfo.LayoutSize.z);
            return localSize;
        }
    }
}
