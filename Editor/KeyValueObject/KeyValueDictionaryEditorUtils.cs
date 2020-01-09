﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.Assertions;

namespace Hinode.Editors
{
    /// <summary>
    /// KeyValueDictionaryのカスタムEditorとPropertyDrawerで処理を共通化するためのクラス
    /// 型を指定する必要がある時はKeyValueDictionaryWithTypeNameEditorUtilsクラスを使用してください
    /// <seealso cref="KeyValueDictionaryWithTypeNameEditorUtils{TDictionary, TKeyValue, T, TCacheType}"/>
    /// </summary>
    internal class KeyValueDictionaryEditorUtils<TDictionary, TKeyValue, T>
        where TDictionary : IKeyValueDictionary<TKeyValue, T>
        where TKeyValue : IKeyValueObject<T>
    {
        protected SerializedTarget Target
        {
            get; private set;
        } = new SerializedTarget();

        bool _foldout;
        int _page;
        readonly int ELEMENT_PER_PAGE = 15;

        public void Draw(SerializedObject target, Rect position, GUIContent label) => Draw(target, SerializedTarget.Type.SerializedObject, position, label);
        public void Draw(SerializedProperty target, Rect position, GUIContent label) => Draw(target, SerializedTarget.Type.SerializedProperty, position, label);

        void Draw(object target, SerializedTarget.Type type, Rect position, GUIContent label)
        {
            Target.Set(type, target);

            DrawImpl(position, label);
        }

        #region Virtual Methods
        protected virtual void DrawImpl(Rect position, GUIContent label)
        {
            var rowHeight = GUI.skin.font.lineHeight;
            rowHeight += GUI.skin.label.margin.vertical + GUI.skin.label.padding.vertical;
            var pos = new GUILayoutPosition(position, rowHeight);

            if(Target.CurrentType == SerializedTarget.Type.SerializedProperty)
            {
                using (var scope = new EditorGUI.ChangeCheckScope())
                {
                    var foldoutPos = pos.GetSplitPos(3f, 0);
                    _foldout = EditorGUI.Foldout(foldoutPos, _foldout, label);

                    var objFieldPos = pos.GetSplitPos(3f, 1, 2);
                    var newObj = EditorGUI.ObjectField(objFieldPos, Target.ObjectReference, typeof(TDictionary), false);
                    if (scope.changed)
                    {
                        Target.ObjectReference = newObj;
                    }
                }
                if (!_foldout || Target.ObjectReference == null) return;
            }
            else
            {
                EditorGUI.LabelField(pos.Pos, label);
            }

            var assetInstance = Dictionary;
            if (assetInstance == null)
            {
                pos.IncrementRow();
                var labelPos = pos.Pos;
                EditorGUI.LabelField(labelPos, "Don't Found Reference...");
                return;
            }

            var SO = new SerializedObject(assetInstance);
            var (doApply, nextPos) = OnDrawElementsBefore(pos, SO, rowHeight);
            pos = nextPos;
            if (doApply)
            {
                return;
            }

            using (var indent1 = new EditorGUI.IndentLevelScope())
            {
                var indentPos = pos.Indent(true);
                DrawElements(indentPos, SO, rowHeight);
            }
        }

        protected virtual (bool doApply, GUILayoutPosition nextPos) OnDrawElementsBefore(GUILayoutPosition position, SerializedObject SO, float rowHeight)
        {
            return (false, position);
        }
        protected virtual void OnApplyBefore(SerializedObject SO, SerializedProperty valuesProp) { }
        protected virtual void OnApplyAfter(TDictionary instance) { }

        #endregion

        protected TDictionary Dictionary
        {
            get
            {
                switch (Target.CurrentType)
                {
                    case SerializedTarget.Type.SerializedObject:
                        return Target.SerializedObject.targetObject as TDictionary;
                    case SerializedTarget.Type.SerializedProperty:
                        return Target.SerializedProperty.GetAssetInstance<TDictionary>();
                    default:
                        throw new System.NotImplementedException();
                }
            }
        }

        /// <summary>
        /// 同名のキーがあるなら別名をつけるようにする
        /// </summary>
        /// <param name="valuesProp"></param>
        void Reflesh(SerializedProperty valuesProp)
        {
            foreach (var (e, index) in valuesProp.GetArrayElementEnumerable())
            {
                var keyProp = e.FindPropertyRelative("_key");
                var sameNames = valuesProp.GetArrayElementEnumerable()
                    .Where(_p => _p.index != index)
                    .Where(_p => _p.prop.FindPropertyRelative("_key").stringValue == keyProp.stringValue);

                var suffixNumber = 1;
                foreach (var sameNameProp in sameNames)
                {
                    var rename = $"{keyProp.stringValue}_{suffixNumber}";
                    while (valuesProp.GetArrayElementEnumerable()
                        .Any(_p => _p.prop.FindPropertyRelative("_key").stringValue == rename))
                    {
                        suffixNumber++;
                        rename = $"{keyProp.stringValue}_{suffixNumber}";
                    }
                    var targetKeyProp = sameNameProp.prop.FindPropertyRelative("_key");
                    targetKeyProp.stringValue = rename;
                    suffixNumber++;
                }
            }
        }

        /// <summary>
        /// 要素の配列を表示する
        /// </summary>
        /// <param name="position"></param>
        /// <param name="SO"></param>
        /// <param name="rowHeight"></param>
        void DrawElements(GUILayoutPosition position, SerializedObject SO, float rowHeight)
        {
            var valuesProp = SO.FindProperty("_values");
            if (valuesProp == null) { return; }

            bool doChanged = false;
            int newArraySize = -1;
            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                position.IncrementRow();
                newArraySize = EditorGUI.IntField(position.Pos, "Size", valuesProp.arraySize);
                if (scope.changed)
                {
                    newArraySize = Mathf.Min(100, newArraySize);
                    doChanged |= true;
                }
            }

            var nameHash = new HashSet<string>();
            var endIndex = Mathf.Min(valuesProp.arraySize, (_page + 1) * ELEMENT_PER_PAGE);
            for (var i = _page * ELEMENT_PER_PAGE; i < endIndex; ++i)
            {
                var element = valuesProp.GetArrayElementAtIndex(i);
                using (var scope = new EditorGUI.ChangeCheckScope())
                {
                    position.IncrementRow();

                    CustomGUIContent label = new CustomGUIContent();
                    if (Target.CurrentType == SerializedTarget.Type.SerializedProperty)
                    {
                        var usedTypeAttr = Target.SerializedProperty.GetFieldAttributes<UsedTypeAttribute>().FirstOrDefault();
                        if(usedTypeAttr != null)
                        {
                            label.Parameter = usedTypeAttr;
                        }
                    }
                    EditorGUI.PropertyField(position.Pos, element, label);
                    if (scope.changed)
                    {
                        doChanged |= scope.changed;
                    }
                    var keyProp = element.FindPropertyRelative("_key");
                    var doContains = nameHash.Contains(keyProp.stringValue);
                    doChanged |= doContains;
                    if (!doContains)
                    {
                        nameHash.Add(keyProp.stringValue);
                    }
                }
            }
            if (doChanged)
            {
                valuesProp.arraySize = newArraySize;
                Reflesh(valuesProp);
                OnApplyBefore(SO, valuesProp);
                if (SO.ApplyModifiedProperties())
                {
                    var instance = SO.targetObject as TDictionary;
                    instance.Refresh();
                    OnApplyAfter(instance);
                }
            }

            //Show Pagination
            if (valuesProp.arraySize >= ELEMENT_PER_PAGE)
            {
                position.IncrementRow();

                var maxPage = (valuesProp.arraySize / ELEMENT_PER_PAGE);
                maxPage += (valuesProp.arraySize % ELEMENT_PER_PAGE) == 0 ? 0 : 1;
                _page = EditorGUI.IntSlider(position.Pos, $"Pagination {_page + 1}/{maxPage}", _page + 1, 1, maxPage);
                _page = Mathf.Clamp(_page - 1, 0, maxPage);
            }
        }
    }

    /// <summary>
    /// KeyValueDictionaryのカスタムEditorとPropertyDrawerで処理を共通化するためのクラス
    /// こちらは型の指定する項目が追加されています。
    /// <seealso cref="KeyValueDictionaryEditorUtils{TDictionary, TKeyValue, T}"/>
    /// </summary>
    internal class KeyValueDictionaryWithTypeNameEditorUtils<TDictionary, TKeyValue, T, TCacheType>
        : KeyValueDictionaryEditorUtils<TDictionary, TKeyValue, T>
        where TDictionary : IKeyValueDictionaryWithTypeName<TKeyValue, T>, IHasTypeName
        where TKeyValue : IKeyValueObject<T>
    {
        protected readonly int layoutPosDivideCount = 3;

        /// <summary>
        /// 処理負荷が高かったのでキャッシュしている
        /// 使用する時はGetOrCreateTypeCahce関数を経由して使用するようにしてください。
        /// </summary>
        private TypeListCache<TCacheType> _typeCache;

        protected TypeListCache<TCacheType> GetOrCreateTypeCahce(SerializedObject SO, out bool isNew)
        {
            isNew = false;
            var instance = SO.targetObject as TDictionary;
            if (_typeCache == null || !instance.IsValidCurrentType)
            {
                var asmName = instance.IsValidCurrentType
                    ? instance.HasType.Assembly.GetName().Name
                    : "Assembly-CSharp";
                var curTypeName = instance.HasType?.FullName ?? "";
                _typeCache = new TypeListCache<TCacheType>(asmName, curTypeName);
                isNew = true;
            }
            return _typeCache;
        }

        /// <summary>
        /// Targetと引数SOが参照しているものは同じだが、場合によってTargetがポインタ型の場合があるためプロパティを変更したい場合はSOの方を使用してください。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="SO"></param>
        /// <param name="rowHeight"></param>
        /// <returns></returns>
        protected override (bool doApply, GUILayoutPosition nextPos) OnDrawElementsBefore(GUILayoutPosition position, SerializedObject SO, float rowHeight)
        {
            position.IncrementRow();
            var labelPos = position.GetSplitPos(layoutPosDivideCount, 0);
            RectOffset offset = new RectOffset((int)(labelPos.width * -0.1f), 0, 0, 0);
            labelPos = offset.Add(labelPos);
            EditorGUI.LabelField(labelPos, "Used Type");

            bool doApply = false;
            bool enableUpdateType = true;
            //UsedUnityObjectAttributeが指定されていたら型は固定し、そうでなければ選択できるようにする
            var instance = SO.targetObject as TDictionary;
            if (Target.CurrentType == SerializedTarget.Type.SerializedProperty
                && UsedTypeAttribute.SetTypeFromFieldInfo(Target.SerializedProperty.GetFieldInfo(), instance))
            {
                var typeNameProp = SO.FindProperty("_typeName");
                Assert.IsNotNull(typeNameProp, $"{Target.SerializedProperty.propertyPath}");
                if(typeNameProp.stringValue != instance.HasType.FullName)
                {
                    typeNameProp.stringValue = instance.HasType.FullName;
                    var typeCache = GetOrCreateTypeCahce(SO, out bool _);
                    typeCache.CurrentType = instance.HasType;
                    doApply = true;
                }
                enableUpdateType = false;
            }

            doApply |= DrawTypePopUp(SO, position, enableUpdateType);

            if (doApply)
            {
                var typeNameProp = SO.FindProperty("_typeName");
                if (typeNameProp.stringValue != _typeCache.CurrentType.FullName)
                {
                    typeNameProp.stringValue = _typeCache.CurrentType.FullName;
                    SO.FindProperty("_values").ClearArray();
                    SO.ApplyModifiedProperties();
                }

                var inst = SO.targetObject as TDictionary;
                inst.Refresh();
            }

            return (doApply, position);
        }

        /// <summary>
        /// Targetと引数SOが参照しているものは同じですが、場合によってTargetがポインタ型の場合があるためプロパティを変更したい場合はSOの方を使用してください。
        /// </summary>
        /// <param name="SO"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        bool DrawTypePopUp(SerializedObject SO, GUILayoutPosition position, bool enableUpdate)
        {
            bool doApply = false;
            var typeCache = GetOrCreateTypeCahce(SO, out var isNew);
            if(isNew)
            {
                doApply = true;
            }

            var newAsmIndex = EditorGUI.Popup(position.GetSplitPos(layoutPosDivideCount, 1), typeCache.AssemblyIndex, typeCache.AssemblyNameList);
            var newTypeIndex = EditorGUI.Popup(position.GetSplitPos(layoutPosDivideCount, 2), typeCache.TypeIndex, typeCache.TypeNameList);

            doApply |= newAsmIndex != typeCache.AssemblyIndex;
            doApply |= newTypeIndex != typeCache.TypeIndex;
            if (doApply && enableUpdate)
            {
                typeCache.AssemblyIndex = newAsmIndex;
                typeCache.TypeIndex = newTypeIndex;
            }
            return doApply;
        }

        /// <summary>
        /// Targetと引数SOが参照しているものは同じですが、場合によってTargetがポインタ型の場合があるためプロパティを変更したい場合はSOの方を使用してください。
        /// </summary>
        /// <param name="SO"></param>
        /// <param name="valuesProp"></param>
        protected override void OnApplyBefore(SerializedObject SO, SerializedProperty valuesProp)
        {
            //不正な状態にある値がないか検索する
            var typeNameProp = SO.FindProperty("_typeName");
            foreach (var (elementProp, index) in valuesProp.GetArrayElementEnumerable())
            {
                var elementTypeNameProp = elementProp.FindPropertyRelative("_typeName");
                if (elementTypeNameProp.stringValue == typeNameProp.stringValue) continue;
                elementTypeNameProp.stringValue = typeNameProp.stringValue;
            }
        }
    }
}
