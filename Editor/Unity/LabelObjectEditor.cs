using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;

namespace Hinode.Editors
{
    [CustomEditor(typeof(LabelObject))]
    public class LabelObjectEditor : Editor
    {
        static HashSet<System.Type> _labelListClassies = new HashSet<System.Type>();
        [InitializeOnLoadMethod()]
        static void InitLabelListClassies()
        {
            var labelListClassies = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(_asm => _asm.DefinedTypes.Where(_t => null !=  _t.GetCustomAttribute<LabelObject.LabelListClassAttribute>()));

            foreach(var type in labelListClassies)
            {
                _labelListClassies.Add(type);
            }
        }

        LabelListClassPopup _labelListClassPopup;
        LabelListPopup _labelListPopup;

        private void OnEnable()
        {
            _labelListClassPopup = new LabelListClassPopup(_labelListClassies);

            if(_labelListClassies.Any())
            {
                _labelListPopup = new LabelListPopup(_labelListClassies.First());
            }
            else
            {
                _labelListPopup = null;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(_labelListClassPopup.LabelListTypes.Any())
            {
                if(_labelListClassPopup.Draw(new GUIContent("LabelClass")))
                {
                    _labelListPopup = new LabelListPopup(_labelListClassPopup.SelectedType);
                }

                if(_labelListPopup != null)
                {
                    using (var h = new EditorGUILayout.HorizontalScope())
                    {
                        _labelListPopup.Draw(new GUIContent("labels"));

                        if(GUILayout.Button("Add"))
                        {
                            var label = _labelListPopup.SelectedLabelValue;

                            var initialLabelProp = serializedObject.FindProperty("_constLabels");
                            if(!initialLabelProp.GetArrayElementEnumerable().Any(_p => _p.prop.stringValue == label))
                            {
                                var index = initialLabelProp.arraySize;
                                initialLabelProp.InsertArrayElementAtIndex(index);
                                initialLabelProp.GetArrayElementAtIndex(index).stringValue = label;

                                serializedObject.ApplyModifiedProperties();
                            }
                        }
                    }
                }
            }
        }

        class LabelListClassPopup : PopupEditorGUILayout
        {
            public System.Type[] LabelListTypes { get; }
            public LabelListClassPopup(HashSet<System.Type> types)
            {
                LabelListTypes = types.ToArray();
            }

            public System.Type SelectedType
            {
                get => LabelListTypes[SelectedIndex];
            }

            protected override string[] CreateDisplayOptionList()
            {
                return LabelListTypes
                    .Select(_t => $"{_t.FullName}")
                    .ToArray();
                throw new System.NotImplementedException();
            }
        }

        class LabelListPopup : PopupEditorGUILayout
        {
            public System.Type LabelListClassType { get; }
            public LabelListPopup(System.Type type)
            {
                LabelListClassType = type;
            }

            public string SelectedLabelValue
            {
                get => LabelFieldInfos[SelectedIndex].GetValue(null) as string;
            }

            FieldInfo[] _labelFieldInfos;
            public FieldInfo[] LabelFieldInfos
            {
                get
                {
                    if(_labelFieldInfos == null)
                    {
                        _labelFieldInfos = LabelListClassType.GetFields(BindingFlags.Public | BindingFlags.Static)
                            .Where(_f => _f.IsLiteral || _f.IsInitOnly)
                            .ToArray();
                    }
                    return _labelFieldInfos;
                }
            }

            protected override string[] CreateDisplayOptionList()
            {
                return LabelFieldInfos
                    .Select(_f => $"{_f.Name} = \"{_f.GetValue(null)}\"")
                    .ToArray();
            }
        }
    }
}
