using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Reflection;

namespace Hinode.Editors
{
    public class SubComponentSummaryWindow : EditorWindow
    {
        [MenuItem("Hinode/Tools/SubComponent Summary")]
        public static void Open()
        {
            var window = EditorWindow.CreateWindow<SubComponentSummaryWindow>();
            window.Show();
        }

        RootPopup _rootPopup;
        MethodLabelPopup _methodLabelPopup;
        MonoBehaviour _currentRoot;

        bool FoldoutLabelFilters { get; set; } = true;
        bool FoldoutControllerLabelFilters { get; set; } = true;
        bool FoldoutBindCallbacks { get; set; } = true;
        bool FoldOutSubComponents { get; set; } = true;
        bool FoldOutSubComponentsHowTo { get; set; } = false;

        Vector2 ScrollPosSubComponents { get; set; }
        string FilterLabel { get => _methodLabelPopup.SelectedLabel; }
        bool EnableFilterLabel { get => !string.IsNullOrEmpty(FilterLabel); }

        GameObject CheckPrefab { get; set; } = null;

        IEnumerable<LabelObject> LabelObjects
        {
            get
            {
                IEnumerable<LabelObject> labelObjs;
                if (CheckPrefab == null)
                {
                    var selfScene = SceneExtensions.GetSceneEnumerable()
                        .First(_s => _s.GetRootGameObjects().Any(_o => _o == _currentRoot.gameObject));
                    labelObjs = selfScene.GetGameObjectEnumerable()
                        .Select(_o => _o.GetComponent<LabelObject>())
                        .Where(_l => _l != null);
                }
                else
                {
                    labelObjs = CheckPrefab.GetComponentsInChildren<LabelObject>();
                }
                return labelObjs;
            }
        }

        bool MatchLabelFilter(LabelsAttribute labels)
        {
            if (!EnableFilterLabel) return true;

            return labels.Contains(FilterLabel);
        }

        void OnEnable()
        {
            titleContent = new GUIContent("SubComponent Summary");
            _rootPopup = new RootPopup(_rootPopup);
            _currentRoot = _rootPopup.SelectedMonoBehaviour;
            _methodLabelPopup = new MethodLabelPopup(_currentRoot);
        }

        private void OnFocus()
        {
            titleContent = new GUIContent("SubComponent Summary");
            _rootPopup = new RootPopup(_rootPopup);
            _currentRoot = _rootPopup.SelectedMonoBehaviour;
            _methodLabelPopup = new MethodLabelPopup(_currentRoot);
        }

        public void OnGUI()
        {
            FoldOutSubComponentsHowTo = EditorGUILayout.Foldout(FoldOutSubComponentsHowTo, "How To");
            if (FoldOutSubComponentsHowTo)
            {
                EditorGUILayout.TextArea(SUB_COMPONENTS_HOW_TO, GUILayout.MinHeight(150));
            }

            using (var _h1 = new EditorGUILayout.HorizontalScope())
            {
                if (_rootPopup.Draw(new GUIContent("Root MonoBehaviour")))
                {
                    _currentRoot = _rootPopup.SelectedMonoBehaviour;
                    if (_currentRoot != null)
                    {
                        _methodLabelPopup = new MethodLabelPopup(_currentRoot);
                    }
                }
                EditorGUILayout.ObjectField(_currentRoot, typeof(MonoBehaviour), true, GUILayout.MaxWidth(150));
            }

            if (_currentRoot == null) return;

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            using (var scroll = new EditorGUILayout.ScrollViewScope(ScrollPosSubComponents))
            {
                ScrollPosSubComponents = scroll.scrollPosition;

                //var labelFilterInfo = _currentRoot.GetType().GetProperty("ControllerLabelFilters");
                //var labelFilters = labelFilterInfo.GetValue(_currentRoot) as ControllerLabelFilter[];

                FoldoutLabelFilters = EditorGUILayout.Foldout(FoldoutLabelFilters, $"LabelObjects in {(CheckPrefab != null ? "Prefab" : "Scene")}");
                if(FoldoutLabelFilters)
                {
                    using (var indent = new EditorGUI.IndentLevelScope())
                    {
                        CheckPrefab = EditorGUILayout.ObjectField(new GUIContent("Check Prefab Root"), CheckPrefab, typeof(GameObject), false)
                            as GameObject;

                        foreach (var label in LabelObjects)
                        {
                            using (var h = new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
                            {
                                EditorGUILayout.ObjectField(label, label.GetType(), true, GUILayout.MaxWidth(150));

                                var txt = label.Labels.Concat(label.ConstLabels).Aggregate("", (_s, _c) => _s + _c + " ");
                                EditorGUILayout.LabelField($"{txt}");

                                var matchingMethods = _currentRoot.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
                                    .Select(_m => (methodInfo: _m, labels: _m.GetCustomAttributes<BindCallbackAttribute>()))
                                    .Where(_t => _t.labels.Any())
                                    .Where(_t => _t.labels.Any(_l =>
                                        _l.DoMatch(Labels.MatchOp.Included, label.AllLabels)
                                        && _l.EnableBind(_t.methodInfo, label.gameObject)
                                    ));

                                //EditorGUILayout.LabelField($"Bind Methods({matchFilter.ComponentType?.Name ?? "<none>"})");
                                EditorGUILayout.Popup(new GUIContent("Bind Methods")
                                    , 0
                                    , matchingMethods
                                        .Select(_t => _currentRoot.GetType().Name + "#" + _t.methodInfo.ToString())
                                        .ToArray());

                                //var matchFilter = labelFilters.FirstOrDefault(_f => _f.DoMatch(label, out var _));
                                ////EditorGUILayout.Toggle("DoMatchFilter?", matchFilter != null);

                                //if(matchFilter != null)
                                //{
                                //    var controllerMethods = _currentRoot.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                //        .Select(_m => (method: _m, label: _m.GetCustomAttribute<MethodLabelAttribute>()))
                                //        .Where(_t => _t.label != null)
                                //        .Where(_t => matchFilter.MethodLabels.Count == _t.label.Labels.Count
                                //            && matchFilter.MethodLabels.All(_l => _t.label.Contains(_l)));

                                //    EditorGUILayout.LabelField($"Bind Methods({matchFilter.ComponentType?.Name ?? "<none>"})");
                                //    EditorGUILayout.Popup(0, controllerMethods.Select(_t => _currentRoot.GetType().Name + "#" +  _t.method.Name).ToArray());
                                //}
                            }
                        }
                    }
                }

                EditorGUILayout.Space();

                DrawBindCallbacks();

                //FoldoutControllerLabelFilters = EditorGUILayout.Foldout(FoldoutControllerLabelFilters, "Controller Label Filters");
                //if (FoldoutControllerLabelFilters)
                //{
                //    using (var indent = new EditorGUI.IndentLevelScope())
                //    {
                //        foreach(var filter in labelFilters)
                //        {
                //            EditorGUILayout.LabelField($"'{filter.Description}'");
                //            using (var indent2 = new EditorGUI.IndentLevelScope())
                //            {
                //                using (var h = new EditorGUILayout.HorizontalScope())
                //                {
                //                    using (var v = new EditorGUILayout.VerticalScope())
                //                    {
                //                        var txt = filter.ComponentType?.Name ?? "<empty type>";
                //                        EditorGUILayout.LabelField("Component=>" + txt, GUILayout.MaxWidth(150), GUILayout.ExpandWidth(false));
                //                        using (var i = new EditorGUI.IndentLevelScope())
                //                        {
                //                            foreach (var l in GetLabelObjectsEnumerable(CheckPrefab)
                //                                .Where(_l => filter.DoMatch(_l, out var _)))
                //                            {
                //                                EditorGUILayout.ObjectField(l, typeof(LabelObject), true, GUILayout.MaxWidth(125));
                //                            }
                //                        }
                //                    }

                //                    float itemHeight = 15;
                //                    using (var v = new EditorGUILayout.VerticalScope())
                //                    {
                //                        EditorGUILayout.LabelField("Labels");
                //                        using (var i = new EditorGUI.IndentLevelScope())
                //                        {
                //                            foreach (var l in filter.Labels)
                //                            {
                //                                EditorGUILayout.LabelField("+ " + l, GUILayout.Height(itemHeight));
                //                            }
                //                        }
                //                    }
                //                    using (var v = new EditorGUILayout.VerticalScope())
                //                    {
                //                        EditorGUILayout.LabelField("MethodLabels");
                //                        using (var i = new EditorGUI.IndentLevelScope())
                //                        {
                //                            foreach (var l in filter.MethodLabels)
                //                            {
                //                                EditorGUILayout.LabelField("+ " + l, GUILayout.Height(itemHeight));
                //                            }
                //                        }
                //                    }

                //                    using (var v = new EditorGUILayout.VerticalScope())
                //                    {
                //                        EditorGUILayout.LabelField("May Bind Methods");
                //                        using (var i = new EditorGUI.IndentLevelScope())
                //                        {
                //                            var controllerMethods = _currentRoot.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                //                                .Select(_m => (method: _m, label: _m.GetCustomAttribute<MethodLabelAttribute>()))
                //                                .Where(_t => _t.label != null)
                //                                .Where(_t => filter.MethodLabels.Count == _t.label.Labels.Count
                //                                    && filter.MethodLabels.All(_l => _t.label.Contains(_l)));

                //                            EditorGUILayout.Popup(0, controllerMethods
                //                                .Select(_t => _currentRoot.GetType().Name + "#" + _t.method.Name).ToArray());
                //                        }
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}

                EditorGUILayout.Space();
                FoldOutSubComponents = EditorGUILayout.Foldout(FoldOutSubComponents, "SubComponents");
                if (FoldOutSubComponents)
                {
                    using (var indent = new EditorGUI.IndentLevelScope())
                    {
                        _methodLabelPopup.Draw(new GUIContent("Method Label Filter"));
                        foreach (var subCom in GetSubComponentsInRoot(_currentRoot.GetType()))
                        {
                            DrawSubComponent(subCom.type, subCom.fieldName);
                        }
                    }
                }
            }
        }

        static readonly GUIContent LABEL_BIND_CALLBACKS = new GUIContent("BindCallbacks");
        void DrawBindCallbacks()
        {
            FoldoutBindCallbacks = EditorGUILayout.Foldout(FoldoutBindCallbacks, LABEL_BIND_CALLBACKS);
            if (!FoldoutBindCallbacks) return;

            var rootType = _currentRoot.GetType();
            using (var rootIndent = new EditorGUI.IndentLevelScope())
            {
                foreach(var (methodInfo, attrs) in BindCallbackAttribute.GetMethodInfoAndAttrEnumerable(rootType))
                {
                    EditorGUILayout.LabelField(methodInfo.ToString());
                    //Show BindCallbacksAttr
                    using (var indent = new EditorGUI.IndentLevelScope())
                    {
                        //Draw Memo
                        foreach (var memo in methodInfo.GetCustomAttributes<MemoAttribute>()
                            .Where(_a => _a.GetType().Equals(typeof(MemoAttribute)))
                            )
                        {
                            EditorGUILayout.TextField("memo", memo.Memo);
                        }

                        foreach (var attr in attrs)
                        {
                            using (var H = new EditorGUILayout.HorizontalScope())
                            {
                                //Draw May Bind Targets
                                using (var V = new EditorGUILayout.VerticalScope())
                                {
                                    EditorGUILayout.LabelField("May Bind Targets");
                                    using (var ___ = new EditorGUI.IndentLevelScope())
                                    {
                                        foreach (var obj in LabelObjects
                                            .Where(_l => attr.LabelHashSet.DoMatch(Labels.MatchOp.Included, _l.AllLabels))
                                            .Where(_o => attr.EnableBind(methodInfo, _o.gameObject))
                                        )
                                        {
                                            EditorGUILayout.ObjectField(obj, attr.CallbackBaseType, true);
                                        }
                                    }
                                }
                                //Draw Labels
                                using (var V = new EditorGUILayout.VerticalScope())
                                {
                                    EditorGUILayout.LabelField("Labels");
                                    using (var ___ = new EditorGUI.IndentLevelScope())
                                    {
                                        foreach (var l in attr.LabelHashSet)
                                        {
                                            EditorGUILayout.LabelField("+" + l);
                                        }
                                    }
                                }
                                //Draw Kind
                                using (var V = new EditorGUILayout.VerticalScope())
                                {
                                    EditorGUILayout.EnumPopup("Kind", attr.CurrentKind);

                                    using (var ___ = new EditorGUI.IndentLevelScope())
                                    {
                                        using (var ____ = new EditorGUILayout.HorizontalScope())
                                        {
                                            EditorGUILayout.Toggle(attr.IsValid, GUILayout.MaxWidth(50));
                                            switch (attr.CurrentKind)
                                            {
                                                case BindCallbackAttribute.Kind.TypeAndCallback:
                                                    EditorGUILayout.LabelField(attr.CallbackBaseType.ToString());
                                                    EditorGUILayout.LabelField(attr.CallbackName);
                                                    break;
                                                case BindCallbackAttribute.Kind.Binder:
                                                    EditorGUILayout.LabelField(attr.Binder.GetType().ToString());
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }

                            //Draw Memo
                            using (var ____ = new EditorGUI.IndentLevelScope())
                            {
                                EditorGUILayout.TextField("memo", attr.Memo);
                            }

                        }// foreach(attr)
                    }
                }
            }
        }

        void DrawSubComponent(System.Type subComType, string fieldName)
        {
            EditorGUILayout.LabelField($"{subComType.Name} : {fieldName}");
            using (var indent = new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.LabelField("MethodLabels");
                using (var methodLabelIndent = new EditorGUI.IndentLevelScope())
                {
                    var methodsWithLabel = GetMethodLabelsEnumerable(subComType)
                        .SelectMany(_t => _t.labels.Select(_l => (methodInfo: _t.methodInfo, label: _l)))
                        .Where(_t => MatchLabelFilter(_t.label));
                    foreach (var (method, label) in methodsWithLabel)
                    {
                        var labels = label.Labels.Select(_l => $"\"{_l}\"")
                            .Aggregate("", (_s, _c) => _s + _c + " ");
                        EditorGUILayout.LabelField($"{method.Name}() -> {labels}");
                    }
                }
            }
        }

        IEnumerable<LabelObject> GetLabelObjectsEnumerable(GameObject CheckPrefab)
        {
            if (CheckPrefab == null)
            {
                var selfScene = SceneExtensions.GetSceneEnumerable()
                    .First(_s => _s.GetRootGameObjects().Any(_o => _o == _currentRoot.gameObject));
                return selfScene.GetGameObjectEnumerable()
                    .Select(_o => _o.GetComponent<LabelObject>())
                    .Where(_l => _l != null);
            }
            else
            {
                return CheckPrefab.GetComponentsInChildren<LabelObject>();
            }
        }

        static IEnumerable<(MethodInfo methodInfo, IEnumerable<LabelsAttribute> labels)> GetMethodLabelsEnumerable(System.Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Select(_m => (
                    method: _m,
                    labels: _m.GetCustomAttributes<LabelsAttribute>()
                        .Where(_a => _a.GetType().Equals(typeof(LabelsAttribute)))
                    )
                );
        }

        static IEnumerable<(System.Type type, string fieldName)> GetSubComponentsInRoot(System.Type rootType)
        {
            return new SubComponentEnumerable(rootType);
        }

        class SubComponentEnumerable : IEnumerable<(System.Type type, string filedName)>, IEnumerable
        {
            System.Type _target;
            public SubComponentEnumerable(System.Type target)
            {
                _target = target;
            }

            public IEnumerator<(System.Type type, string filedName)> GetEnumerator()
            {
                if (_target == null) yield break;

                if (_target.EqualGenericTypeDefinition(typeof(ISubComponent<>)))
                {//self Draw
                    yield return (_target, "__Root");
                }

                var subComponents = _target.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .Where(_f => _f.FieldType.EqualGenericTypeDefinition(typeof(ISubComponent<>)));

                foreach (var subCom in subComponents)
                {
                    yield return (subCom.FieldType, subCom.Name);
                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }


        class RootPopup : PopupEditorGUILayout
        {
            (Scene scene, MonoBehaviour com)[] _optionList;

            public RootPopup()
            {}

            public RootPopup(RootPopup other)
            {
                SelectedIndex = other?.SelectedIndex ?? 0;
                SelectedIndex = Mathf.Clamp(SelectedIndex, 0, OptionList.Length-1);
            }

            public MonoBehaviour SelectedMonoBehaviour
            {
                get => SelectedIndex == 0
                    ? null
                    : OptionList[SelectedIndex].com;
            }

            public (Scene scene, MonoBehaviour com)[] OptionList
            {
                get
                {
                    if(_optionList == null)
                    {
                        _optionList = new (Scene, MonoBehaviour)[] { (default, null) }
                            .Concat(
                                SceneExtensions.GetSceneEnumerable()
                                .SelectMany(_s => _s.GetGameObjectEnumerable()
                                    .SelectMany(_g => _g.GetComponents<MonoBehaviour>())
                                    .Where(_c => _c.GetType().EqualGenericTypeDefinition(typeof(MonoBehaviourWithSubComponents<>)))
                                    .Select(_com => (scene: _s, com: _com)))
                            )
                            .ToArray();
                    }
                    return _optionList;
                }
            }
            protected override string[] CreateDisplayOptionList()
            {
                return OptionList
                    .Select(_c => {
                        return _c.com != null
                            ? $"{_c.scene.name}:{_c.com.name}:{_c.com.GetType().Name}"
                            : $"(none)";
                    })
                    .ToArray();
            }
        }

        class MethodLabelPopup : PopupEditorGUILayout
        {
            public System.Type RootType { get; private set; }
            public string SelectedLabel
            {
                get => SelectedIndex == 0 ? "" : SelectedElement;
            }

            public MethodLabelPopup(MonoBehaviour root)
            {
                RootType = root != null ? root.GetType() : null;
            }

            protected override string[] CreateDisplayOptionList()
            {
                return new string[] { "(none)" }
                    .Concat(
                        GetSubComponentsInRoot(RootType)
                        .SelectMany(_t => GetMethodLabelsEnumerable(_t.type)
                            .SelectMany(_tt => _tt.labels.SelectMany(_l => _l.Labels))
                        )
                    )
                    .ToArray();
            }
        }

        const string SUB_COMPONENTS_HOW_TO =
@"How to
ここでは選択したMonoBehaviourWithSubComponent<T>が持つSubComponentを確認することができます。
確認できる項目は以下のものになります。
- 関連するLabelObjects:
- Controller Label Filters
- SubComponents

## 関連するLabelObjects:
  選択中のMonoBehaviourWithSubComponent<T>が所属するSceneにあるLabelObject Componentを確認できます。
  or Check Prefab Root　を指定した時はそのPrefabにあるLabelObject Componentが確認できます。

  確認できる項目は以下のものです。
  - Initial Labels
  - バインドされる可能性がある選択中のMonoBehaviourWithSubComponent<T>のメソッド

  メソッドのバインドの自動化は実装されていないため、Script上から設定してください。
  その際は、MonoBehaviourWithSubComponent<T>#CreateControllerLabelFilters()とControllerLabelFilter classを利用してください。

## Controller Label Filters
  選択中のMonoBehaviourWithSubComponent<T>に設定されているControllerLabelFilterを確認できます。
  
  以下の情報を確認できます。
  - LabelObjectを持つGameObject中で使用するComponent + このフィルターと一致するシーン/Prefabの中に存在するLabelObject
  - Label
  - MethodLabel
  - 選択中のMonoBehaviourWithSubComponent<T>のメンバ関数の内、このフィルターと一致する関数の一覧

## SubComponents
  選択中のMonoBehaviourWithSubComponent<T>にあるSubComponentの情報を表示します。
  以下情報を確認できます。
  - メンバ関数に指定されているMethodLabelAttributeの一覧。

";
    }
}
