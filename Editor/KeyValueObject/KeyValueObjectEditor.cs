using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using UnityEngine.Assertions;

namespace Hinode.Editors
{
    public abstract class IKeyValueObjectPropertyDrawer<T> : PropertyDrawer
    {
        protected float WIDTH_OFFSET = 5f;
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var group = new VisualElement();
            group.Bind(property.serializedObject);
            group.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);

            var labelField = new Label(property.displayName);
            labelField.style.flexGrow = new StyleFloat(0.5f);
            group.Add(labelField);

            //var keyProp = property.FindPropertyRelative("_key");
            var keyField = new TextField()
            {
                bindingPath = property.propertyPath + "._key"
            };
            keyField.style.flexGrow = new StyleFloat(1);
            group.Add(keyField);

            //var valueField = new PropertyField(property.FindPropertyRelative("_value"));
            //valueField.label = " ";
            var valueField = CreateValueElement(property);
            if (valueField is IBindable)
            {
                (valueField as IBindable).bindingPath = $"{property.propertyPath}._value";
            }
            valueField.style.flexGrow = new StyleFloat(1);
            group.Add(valueField);
            return group;
        }

        protected abstract VisualElement CreateValueElement(SerializedProperty property);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (var propScope = new EditorGUI.PropertyScope(position, label, property))
            {
                var labelPos = position;
                if(label.text != "")
                {
                    labelPos.width = labelPos.width / 3;
                    EditorGUI.LabelField(labelPos, propScope.content);
                }
                else
                {
                    labelPos.width = 0;
                }

                var propWidth = position.width - labelPos.width;
                var keyProp = property.FindPropertyRelative("_key");
                var keyPos = labelPos;
                keyPos.x += keyPos.width;
                keyPos.width = propWidth / 3f;
                using (var scope = new EditorGUI.ChangeCheckScope())
                {
                    var p = keyPos;
                    p.width -= WIDTH_OFFSET;
                    var newKey = EditorGUI.TextField(p, keyProp.stringValue);
                    if (scope.changed)
                    {
                        keyProp.stringValue = newKey;
                    }
                }

                var sliderPos = keyPos;
                sliderPos.x += keyPos.width;
                sliderPos.width = propWidth - keyPos.width;
                OnGUIValue(sliderPos, property, label);
            }
        }

        protected abstract void OnGUIValue(Rect position, SerializedProperty property, GUIContent label);
    }

    [CustomPropertyDrawer(typeof(KeyIntObject))]
    public class KeyIntObjectPropertyDrawer : IKeyValueObjectPropertyDrawer<int>
    {
        protected override VisualElement CreateValueElement(SerializedProperty property)
        {
            var attributes = property.GetFieldAttributes();
            if (attributes.Any(_a => _a is RangeIntAttribute))
            {
                var range = attributes.First(_a => _a is RangeIntAttribute) as RangeIntAttribute;
                var root = new VisualElement();
                //root.style.justifyContent = new StyleEnum<Justify>(Justify.SpaceBetween);
                root.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
                var slider = new SliderInt(range.Min, range.Max)
                {
                    bindingPath = $"{property.propertyPath}._value"
                };
                slider.style.marginLeft = new StyleLength(5);
                slider.style.marginRight = new StyleLength(5);
                slider.style.flexGrow = new StyleFloat(3);
                root.Add(slider);
                var intField = new IntegerField()
                {
                    bindingPath = $"{property.propertyPath}._value"
                };
                //intField.style.paddingLeft = new StyleLength(5);
                intField.style.marginRight = new StyleLength(5);
                intField.style.maxWidth = new StyleLength(150);
                intField.style.minWidth = new StyleLength(50);
                EventCallback<ChangeEvent<int>> action = (e) => {
                    var valueProp = property.FindPropertyRelative("_value");
                    Debug.Log($"prop={valueProp.intValue}, value=>{e.newValue}; range=>({range.Min}:{range.Max})");
                    if (range.IsInRange(e.newValue))
                        return;
                    var field = e.target as IntegerField;
                    field.value = range.Clamp(e.newValue);
                };
                intField.RegisterValueChangedCallback(action);

                //intField.style.flexGrow = new StyleFloat(1);
                root.Add(intField);
                return root;
            }
            else if(attributes.Any(_a => _a is MinAttribute))
            {
                var min = attributes.First(_a => _a is MinAttribute) as MinAttribute;
                var intField = new IntegerField();
                intField.RegisterValueChangedCallback(_e => {
                    if((int)min.min > _e.newValue)
                    {
                        var valueProp = property.FindPropertyRelative("_value");
                        valueProp.intValue = (int)min.min;
                    }
                });
                return intField;
            }
            else
            {
                var intField = new IntegerField();
                return intField;
            }
        }

        protected override void OnGUIValue(Rect position, SerializedProperty property, GUIContent label)
        {
            var valueProp = property.FindPropertyRelative("_value");
            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                int newValue;
                var attr = property.GetFieldAttributes<RangeIntAttribute>().FirstOrDefault();
                if (attr != null)
                {
                    var r = attr as RangeIntAttribute;
                    newValue = EditorGUI.IntSlider(position, valueProp.intValue, r.Min, r.Max);
                }
                else
                {
                    newValue = EditorGUI.IntField(position, valueProp.intValue);
                }

                if (scope.changed)
                {
                    valueProp.intValue = newValue;
                }
            }
        }
    }

    [CustomPropertyDrawer(typeof(KeyFloatObject))]
    [CustomPropertyDrawer(typeof(KeyDoubleObject))]
    public class KeyNumberObjectPropertyDrawer : IKeyValueObjectPropertyDrawer<float>
    {
        protected override VisualElement CreateValueElement(SerializedProperty property)
        {
            return new Label("Not Implement");
        }

        protected override void OnGUIValue(Rect position, SerializedProperty property, GUIContent label)
        {
            var valueProp = property.FindPropertyRelative("_value");
            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                float newValue;
                var attr = property.GetFieldAttributes<RangeNumberAttribute>().FirstOrDefault();
                if (attr != null)
                {
                    newValue = EditorGUI.Slider(position, valueProp.floatValue, attr.Min, attr.Max);
                }
                else
                {
                    newValue = EditorGUI.FloatField(position, valueProp.floatValue);
                }

                if (scope.changed)
                {
                    valueProp.floatValue = newValue;
                }
            }
        }
    }

    [CustomPropertyDrawer(typeof(KeyBoolObject))]
    public class KeyBoolObjectPropertyDrawer : IKeyValueObjectPropertyDrawer<bool>
    {
        protected override VisualElement CreateValueElement(SerializedProperty property)
        {
            return new Label("Not Implement");
        }

        protected override void OnGUIValue(Rect position, SerializedProperty property, GUIContent label)
        {
            var valueProp = property.FindPropertyRelative("_value");
            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                bool newValue = EditorGUI.Toggle(position, valueProp.boolValue);
                if (scope.changed)
                {
                    valueProp.boolValue = newValue;
                }
            }
        }
    }

    [CustomPropertyDrawer(typeof(KeyStringObject))]
    public class KeyStringObjectPropertyDrawer : IKeyValueObjectPropertyDrawer<string>
    {
        protected override VisualElement CreateValueElement(SerializedProperty property)
        {
            return new Label("Not Implement");
        }

        protected override void OnGUIValue(Rect position, SerializedProperty property, GUIContent label)
        {
            var valueProp = property.FindPropertyRelative("_value");
            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                //TODO TextArea対応
                var newValue = EditorGUI.TextField(position, valueProp.stringValue);
                if (scope.changed)
                {
                    valueProp.stringValue = newValue;
                }
            }
        }
    }

    [CustomPropertyDrawer(typeof(KeyEnumObject))]
    public class KeyEnumObjectPropertyDrawer : IKeyValueObjectPropertyDrawer<System.Enum>
    {
        protected override VisualElement CreateValueElement(SerializedProperty property)
        {
            return new Label("Not Implement");
        }

        protected override void OnGUIValue(Rect position, SerializedProperty property, GUIContent label)
        {
            var inst = property.GetSelf() as KeyEnumObject;
            if (!inst.IsValid) return;

            if(inst.IsFlags)
            {
                DrawFlagsEnum(inst, position, property, label);
            }
            else
            {
                DrawStandardEnum(inst, position, property, label);
            }
        }

        void DrawFlagsEnum(KeyEnumObject instance, Rect position, SerializedProperty property, GUIContent label)
        {
            var valueProp = property.FindPropertyRelative("_value");
            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                if(instance.IsValid)
                {
                    var e = EditorGUI.EnumFlagsField(position, instance.Value);
                    if (scope.changed)
                    {
                        //Debug.Log($"{e.ToString()} value={(int)((object)e)}");
                        var v = (int)((object)e);
                        valueProp.intValue = instance.IsValidValue(v) ? v : 0;
                    }
                }
                else
                {
                    Debug.LogWarning($"Invalid Enum Value... This Property is set 0... name={property.displayName}, value={instance.EnumIndex}, enumType={instance.CurrentType.FullName}");
                    valueProp.intValue = 0;
                }
            }
        }

        void DrawStandardEnum(KeyEnumObject instance, Rect position, SerializedProperty property, GUIContent label)
        {
            var valueProp = property.FindPropertyRelative("_value");
            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                var names = instance.CurrentType.GetEnumNames().ToList();
                var index = valueProp.intValue;
                if (!instance.IsValid)
                {
                    index = names.Count;
                    names.Add("!! Invalid Value !!");
                }
                var newValue = EditorGUI.Popup(position, index, names.ToArray());
                if (scope.changed)
                {
                    valueProp.intValue = newValue;
                }
            }
        }
    }

    [CustomPropertyDrawer(typeof(KeyObjectRefObject), true)]
    public class KeyObjectRefObjectPropertyDrawer : IKeyValueObjectPropertyDrawer<Object>
    {
        protected override VisualElement CreateValueElement(SerializedProperty property)
        {
            return new Label("Not Implement");
        }

        protected override void OnGUIValue(Rect position, SerializedProperty property, GUIContent label)
        {
            var valueProp = property.FindPropertyRelative("_value");
            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                var inst = property.GetSelf() as KeyObjectRefObject;
                var newValue = EditorGUI.ObjectField(position, valueProp.objectReferenceValue, inst.CurrentType, true);
                if (scope.changed)
                {
                    valueProp.objectReferenceValue = newValue;
                }
            }
        }
    }

}
