using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;

namespace Hinode
{
    public class KeyValueObjectDebugUIElementWindow : EditorWindow
    {
        [MenuItem("Hinode/Tools/Debug/KeyValueObject GUI Viewer(UI Element)")]
        public static void Open()
        {
            var window = CreateWindow<KeyValueObjectDebugUIElementWindow>("KeyValueObject GUI Viewer(ver UI Element)");
            window.Show();
        }

        enum TestEnum
        {
            Apple, Orange, Grape, Banana
        }
        [System.Flags]
        enum TestFlagEnum
        {
            A=0x1,
            B=0x1<<1,
            C=0x1<<2,
            D=0x1<<3,
        }

        [SerializeField] KeyIntObject _keyIntValue;
        [SerializeField, RangeInt(0, 100)] KeyIntObject _keyRangeIntValue;
        [SerializeField, Min(-10)] KeyIntObject _keyMinIntValue;

        [SerializeField] KeyFloatObject _keyFloatValue;
        [SerializeField, RangeNumber(0, 100)] KeyFloatObject _keyRangeFloatValue;
        [SerializeField, Min(-10)] KeyFloatObject _keyMinFloatValue;

        [SerializeField] KeyDoubleObject _keyDoubleValue;
        [SerializeField, RangeNumber(0, 100)] KeyDoubleObject _keyRangeDoubleValue;
        [SerializeField, Min(-10)] KeyDoubleObject _keyMinDoubleValue;

        [SerializeField] KeyBoolObject _keyBoolValue;
        [SerializeField] KeyStringObject _keyStringValue;
        [SerializeField, UsedEnum(typeof(TestEnum))] KeyEnumObject _keyEnumValue;
        [SerializeField, UsedEnum(typeof(TestFlagEnum))] KeyEnumObject _keyFlagEnumValue;
        [SerializeField, UsedUnityObject(typeof(GameObject))] KeyObjectRefObject _keyObjRefValue;

        private void OnEnable()
        {
            var SO = new SerializedObject(this);
            var root = rootVisualElement;
            root.Add(new Label("KeyValueObject Custom Editor Viewer(ver UIElement)"));
            foreach(var field in this.GetType()
                .GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly)
                .Where(_f => _f.CustomAttributes.Any(_a => _a.AttributeType.Equals(typeof(SerializeField)))))
            {
                root.Add(new PropertyField(SO.FindProperty(field.Name)));
            }
            root.Bind(SO);
        }
    }
}
