using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Hinode.Layouts.Editors
{
    [CustomEditor(typeof(LayoutTargetComponent))]
    public class LayoutTargetComponentEditor : Editor
    {
        [System.Flags]
        enum OpMode : uint
        {
            Anchor = 0x1 << 1,
            SizeAndOffset = 0x1 << 2,
            Both = 0xffffffff,
            Invisible = 0,
        }

        enum AnchorPresetType
        {
            Low,
            Middle,
            High,
            Expand,
            Free,
        }

        OpMode CurrentOpMode { get; set; } = OpMode.Anchor;
        Dictionary<string, OpMode> PropertyOpModeDict { get; } = new Dictionary<string, OpMode>()
        {
            { "_prevParentSize", OpMode.Invisible },
            { "_localPos", OpMode.Both },
            { "_localSize", OpMode.SizeAndOffset },
            { "_anchorMin", OpMode.Anchor },
            { "_anchorMax", OpMode.Anchor },
            { "_offset", OpMode.SizeAndOffset },
            { "_pivot", OpMode.Both },
        };
        OpModePopup opModePopUp = new OpModePopup();

        class OpModePopup : Hinode.Editors.PopupEditorGUILayout
        {
            public OpMode SelectedOpMode
            {
                get
                {
                    switch (SelectedIndex)
                    {
                        case 0: return OpMode.Anchor;
                        case 1: return OpMode.SizeAndOffset;
                        default:
                            throw new System.NotImplementedException();
                    }
                }
            }

            protected override string[] CreateDisplayOptionList()
            {
                return new string[]{ "Anchor", "Size And Offset"};
            }
        }

        bool FollowRectTransform { get; set; } = true;
        bool _foldoutAnchorTypePreset=true;
        bool _foldoutAnchorOffset = true;

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

            LayoutManagerEditor.UpdateLayoutHierachy(inst);
        }

        public override void OnInspectorGUI()
        {
            var inst = target as LayoutTargetComponent;
            if(inst.transform is RectTransform)
            {
                FollowRectTransform = EditorGUILayout.Toggle("Follow RectTransform?", FollowRectTransform);
                if(FollowRectTransform)
                {
                    inst.AutoDetectUpdater();
                    inst.CopyToLayoutTarget();
                }
            }

            if(opModePopUp.Draw(new GUIContent("OpMode")))
            {
                CurrentOpMode = opModePopUp.SelectedOpMode;
            }

            var propSP = serializedObject.FindProperty("_target");
            var enterChildProps = true;
            while (propSP.NextVisible(enterChildProps))
            {
                if(0 != (PropertyOpModeDict[propSP.name] & CurrentOpMode))
                {
                    EditorGUILayout.PropertyField(propSP);
                }
                enterChildProps = false;
            }

            if (0 != (CurrentOpMode & OpMode.Anchor))
            {

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

                _foldoutAnchorOffset = EditorGUILayout.Foldout(_foldoutAnchorOffset, "Anchor Offset Min/Max");
                if (_foldoutAnchorOffset)
                {
                    using (var s = new EditorGUI.IndentLevelScope())
                    {
                        var targetSP = serializedObject.FindProperty("_target");
                        var anchorMinSP = targetSP.FindPropertyRelative("_anchorMin");
                        var anchorMaxSP = targetSP.FindPropertyRelative("_anchorMax");
                        var localSizeSP = targetSP.FindPropertyRelative("_localSize");
                        var offsetSP = targetSP.FindPropertyRelative("_offset");

                        var other = new LayoutTargetObject();
                        other.SetParent(GetParentLayoutTarget(inst));
                        other.Pivot = inst.LayoutTarget.Pivot;
                        other.SetAnchor(inst.LayoutTarget.AnchorMin, inst.LayoutTarget.AnchorMax);
                        other.UpdateLocalSize(inst.LayoutTarget.LocalSize, inst.LayoutTarget.Offset);

                        var (offsetMin, offsetMax) = other.AnchorOffsetMinMax();
                        var newOffsetMin = EditorGUILayout.Vector3Field("Offset Min", offsetMin);
                        var newOffsetMax = EditorGUILayout.Vector3Field("Offset Max", offsetMax);

                        if (!offsetMin.AreNearlyEqual(newOffsetMin)
                            || !offsetMax.AreNearlyEqual(newOffsetMax))
                        {
                            other.SetAnchorOffset(newOffsetMin, newOffsetMax);

                            anchorMinSP.vector3Value = other.AnchorMin;
                            anchorMaxSP.vector3Value = other.AnchorMax;
                            localSizeSP.vector3Value = other.LocalSize;
                            offsetSP.vector3Value = other.Offset;

                            other.Dispose();
                        }
                    }
                }
            }

            if(serializedObject.ApplyModifiedProperties())
            {
                LayoutManagerEditor.UpdateLayoutHierachy(inst);
            }

            if (GUILayout.Button("Copy From Transform"))
            {
                inst.CopyToLayoutTarget();
            }
        }

        public static void UpdateSelf(LayoutTargetComponent t)
        {
            t.LayoutTarget.IsAutoUpdate = false;
            t.AutoDetectUpdater();
            t.UpdateLayoutTargetHierachy();
            t.LayoutTarget.FollowParent();

            t.LayoutTarget.ClearLayouts();
            foreach (var layout in t.GetComponents<ILayoutComponent>())
            {
                t.LayoutTarget.AddLayout(layout.LayoutInstance);
            }

            foreach (var layout in t.LayoutTarget.Layouts)
            {
                layout.UpdateLayout();
            }

            t.CopyToTransform();
        }

        static LayoutTargetObject GetParentLayoutTarget(LayoutTargetComponent layoutTargetComponent)
        {
            if (layoutTargetComponent.LayoutTarget.Parent == null)
            {
                var parent = layoutTargetComponent.transform.parent;
                if (parent.TryGetComponent<LayoutTargetComponent>(out var parentLayoutTarget))
                {
                    return parentLayoutTarget.LayoutTarget;
                }
                else if (parent.transform is RectTransform)
                {
                    var parentR = parent.transform as RectTransform;
                    return LayoutTargetComponent.RectTransformUpdater.Create(parentR) as LayoutTargetObject;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return layoutTargetComponent.LayoutTarget.Parent as LayoutTargetObject;
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
