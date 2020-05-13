using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;
using System.Linq;
using System.IO;

namespace Hinode.Editors.UnityViewInstanceCreatorSettings
{
    public class EditorCommon
    {
        public enum PropType
        {
            Infos,
        }
        static readonly (PropType, string)[] props = {
                (PropType.Infos, "_infos"),
            };
        public static SerializedPropertyDictionary<PropType> CreatePropDict(SerializedObject SO)
        {
            return new SerializedPropertyDictionary<PropType>(SO, props);
        }
        public static SerializedPropertyDictionary<PropType> CreatePropDict(SerializedProperty parentProp)
        {
            return new SerializedPropertyDictionary<PropType>(parentProp, props);
        }

        public class EditorParam
        {
            List<InstanceInfoCommon.EditorParam> _instInfoParams = new List<InstanceInfoCommon.EditorParam>();
            PagenationArrayEditorGUILayout _infoArrayLayout;

            public SerializedPropertyDictionary<PropType> propDict;

            public PagenationArrayEditorGUILayout InfoArrayLayout
            {
                get => _infoArrayLayout != null
                    ? _infoArrayLayout
                    : _infoArrayLayout = new PagenationArrayEditorGUILayout(3, null, DrawInfoElement);
            }

            public EditorParam(SerializedObject SO)
            {
                propDict = CreatePropDict(SO);
                ResizeInfos(propDict[PropType.Infos].arraySize);
            }
            public EditorParam(SerializedProperty prop)
            {
                propDict = CreatePropDict(prop);
                ResizeInfos(propDict[PropType.Infos].arraySize);
            }

            public void ResizeInfos(int count)
            {
                if (_instInfoParams == null)
                {
                    _instInfoParams = new List<InstanceInfoCommon.EditorParam>(propDict[PropType.Infos].arraySize);
                }
                for (var i = 0; i < count; ++i)
                {
                    var e = propDict[PropType.Infos].GetArrayElementAtIndex(i);
                    _instInfoParams.Add(new InstanceInfoCommon.EditorParam(e));
                }
            }

            public void OnInspectorGUI()
            {
                InfoArrayLayout.OnInspectorGUI(propDict[PropType.Infos], new GUIContent(propDict[PropType.Infos].displayName));
            }

            void DrawInfoElement(int index, SerializedProperty element, GUIContent label)
            {
                if (_instInfoParams.Count <= index) return;
                var elemetnParam = _instInfoParams[index];
                elemetnParam.OnInspectorGUI(label);
            }
        }

        [CustomEditor(typeof(Hinode.UnityViewInstanceCreatorSettings))]
        public class Inspector : Editor
        {
            EditorParam _param;
            private void OnEnable()
            {
                _param = new EditorParam(serializedObject);
            }
            public override void OnInspectorGUI()
            {
                _param.OnInspectorGUI();
                if (serializedObject.ApplyModifiedProperties())
                {
                    _param.ResizeInfos(_param.propDict[PropType.Infos].arraySize);
                }
            }
        }
    }

    class InstanceInfoCommon
    {
        public enum PropType
        {
            Type,
            InstanceType,
            ParamBinderType,
            InstanceKey,
            BinderKey,
            AssetPath,
            AssetBundleName,
            AssetReference,
        }
        static readonly (PropType, string)[] props = {
                (PropType.Type, "_type"),
                (PropType.InstanceType, "_instanceType"),
                (PropType.ParamBinderType, "_paramBinderType"),
                (PropType.InstanceKey, "_instanceKey"),
                (PropType.BinderKey, "_binderKey"),
                (PropType.AssetPath, "_assetPath"),
                (PropType.AssetBundleName, "_assetBundleName"),
                (PropType.AssetReference, "_assetReference"),
            };
        public static SerializedPropertyDictionary<PropType> CreatePropDict(SerializedObject SO)
        {
            return new SerializedPropertyDictionary<PropType>(SO, props);
        }
        public static SerializedPropertyDictionary<PropType> CreatePropDict(SerializedProperty parentProp)
        {
            return new SerializedPropertyDictionary<PropType>(parentProp, props);
        }

        public class EditorParam
        {
            public SerializedPropertyDictionary<PropType> propDict;

            Hinode.UnityViewInstanceCreatorSettings.InstanceInfo Self { get; }
            bool Foldout { get; set; } = true;
            InstanceTypePopup CurrentInstanceTypePopup { get; } = new InstanceTypePopup();
            ParamBinderTypePopup CurrentParamBinderTypePopup { get; set; }
            InstanceTypeResourcesPopup CurrentInstanceTypeResourcesPopup { get; set; } = new InstanceTypeResourcesPopup(typeof(MonoBehaviourViewObject));

            public EditorParam(SerializedProperty prop)
            {
                propDict = CreatePropDict(prop);
                Self = prop.GetSelf() as Hinode.UnityViewInstanceCreatorSettings.InstanceInfo;
                CurrentParamBinderTypePopup = new ParamBinderTypePopup(Self);
                if (Self.InstanceTypeFullName == null || Self.InstanceTypeFullName == "")
                {
                    Self.InstanceTypeFullName = CurrentInstanceTypePopup.DisplayOptionList[0];
                }
                if (Self.ParamBinderTypeFullName == null || Self.ParamBinderTypeFullName == "")
                {
                    Self.ParamBinderTypeFullName = CurrentParamBinderTypePopup.DisplayOptionList[0];
                }
            }

            public void OnInspectorGUI(GUIContent label = null)
            {
                Foldout = EditorGUILayout.Foldout(Foldout, label ?? new GUIContent("Instance Info"));
                if (!Foldout) return;

                using (var indent = new EditorGUI.IndentLevelScope(1))
                {
                    EditorGUILayout.PropertyField(propDict[PropType.Type]);

                    if (CurrentInstanceTypePopup.Draw(propDict[PropType.InstanceType]))
                    {
                        propDict[PropType.AssetReference].objectReferenceValue = null;
                    }

                    if (CurrentParamBinderTypePopup.CurrentInstanceType == null
                        || CurrentParamBinderTypePopup.CurrentInstanceType.FullName != Self.InstanceTypeFullName)
                    {
                        if (Self.InstanceTypeFullName != null && Self.InstanceTypeFullName != "")
                        {
                            CurrentParamBinderTypePopup = new ParamBinderTypePopup(Self);
                        }
                    }
                    CurrentParamBinderTypePopup.Draw(propDict[PropType.ParamBinderType]);
                    EditorGUILayout.PropertyField(propDict[PropType.InstanceKey]);
                    EditorGUILayout.PropertyField(propDict[PropType.BinderKey]);

                    switch ((Hinode.UnityViewInstanceCreatorSettings.InfoType)propDict[PropType.Type].enumValueIndex)
                    {
                        case Hinode.UnityViewInstanceCreatorSettings.InfoType.Asset:
                            DrawAsset();
                            break;
                        case Hinode.UnityViewInstanceCreatorSettings.InfoType.Resources:
                            DrawResources();
                            break;
                        case Hinode.UnityViewInstanceCreatorSettings.InfoType.AssetBundle:
                            DrawAssetBundle();
                            break;
                        default:
                            throw new System.NotImplementedException();
                    }
                }
            }

            void DrawAsset()
            {
                if (Self.InstanceTypeFullName != null && Self.InstanceTypeFullName != "")
                {
                    var newAssetRef = EditorGUILayout.ObjectField(new GUIContent(propDict[PropType.AssetReference].displayName), propDict[PropType.AssetReference].objectReferenceValue, Self.InstanceType, false);

                    if (newAssetRef != propDict[PropType.AssetReference].objectReferenceValue)
                    {
                        propDict[PropType.AssetReference].objectReferenceValue = newAssetRef;
                    }
                }
            }

            void DrawResources()
            {
                if (CurrentInstanceTypeResourcesPopup.CurrentType.FullName != Self.InstanceTypeFullName)
                {
                    CurrentInstanceTypeResourcesPopup = new InstanceTypeResourcesPopup(Self.InstanceType);
                }
                CurrentInstanceTypeResourcesPopup.Draw(propDict[PropType.AssetPath]);
            }

            void DrawAssetBundle()
            {
                EditorGUILayout.PropertyField(propDict[PropType.AssetBundleName]);
                EditorGUILayout.PropertyField(propDict[PropType.AssetPath]);
            }

            class InstanceTypePopup : PopupEditorGUILayout
            {
                protected override string[] CreateDisplayOptionList()
                {
                    var instanceTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(_asm => _asm.ExportedTypes)
                        .Where(_t => _t.HasInterface<IViewObject>() && _t.IsSubclassOf(typeof(MonoBehaviour)))
                        .Select(_t => _t.FullName);
                    return instanceTypes.ToArray();
                }
            }

            class ParamBinderTypePopup : PopupEditorGUILayout
            {
                Hinode.UnityViewInstanceCreatorSettings.InstanceInfo Info { get; set; }
                public System.Type CurrentInstanceType { get; }

                public ParamBinderTypePopup(Hinode.UnityViewInstanceCreatorSettings.InstanceInfo info)
                {
                    Info = info;
                    CurrentInstanceType = (Info.InstanceTypeFullName == null || Info.InstanceTypeFullName == "")
                        ? null
                        : Info.InstanceType;
                }

                protected override string[] CreateDisplayOptionList()
                {
                    var instanceTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(_asm => _asm.ExportedTypes)
                        .Where(_t => _t.HasInterface<IModelViewParamBinder>())
                        .Select(_t => _t.FullName);
                    if (CurrentInstanceType == null)
                    {
                        return instanceTypes.ToArray();
                    }
                    else
                    {
                        var availableParamBinderTypes = Info.InstanceType.GetCustomAttributes(false)
                            .OfType<AvailableModelViewParamBinderAttribute>();
                        if (availableParamBinderTypes.Any())
                        {
                            return availableParamBinderTypes.SelectMany(_a => _a.AvailableParamBinders)
                                .Select(_t => _t.FullName)
                                .ToArray();
                        }
                        else
                        {
                            return instanceTypes.ToArray();
                        }
                    }
                }
            }

            class InstanceTypeResourcesPopup : PopupEditorGUILayout
            {
                public System.Type CurrentType { get; set; }

                public InstanceTypeResourcesPopup(System.Type type)
                {
                    CurrentType = type;
                }

                protected override string[] CreateDisplayOptionList()
                {
                    return Resources.FindObjectsOfTypeAll(CurrentType)
                        .Select(_r => AssetDatabase.GetAssetPath(_r))
                        .Where(_assetPath => _assetPath.Split(Path.DirectorySeparatorChar).Any(_p => _p == "Resources"))
                        .Select(_assetPath =>
                        {
                            var pathSplit = _assetPath.Split(Path.DirectorySeparatorChar);
                            var pathElements = pathSplit.Zip(Enumerable.Range(0, pathSplit.Length), (_p, _i) => (p: _p, index: _i));
                            var resourcesPath = pathElements
                                .FirstOrDefault(_t => _t.p == "Resources")
                            ;
                            var resourceAssetPath = pathElements.Where(_t => _t.index > resourcesPath.index)
                                .Select(_t => _t.p)
                                .Aggregate((_s, _c) => _s + Path.DirectorySeparatorChar + _c);
                            return resourceAssetPath;
                        }).ToArray();
                }
            }

        }
    }

}
