using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using UnityEngine.TestTools;
using UnityEditor.SceneManagement;
using Hinode.Editors;
using System.IO;

namespace Hinode.Tests.Editors.Tools
{
    public static class StepByStepTestRunnerDefines
    {
        public static readonly string LOGGER_SELECTOR = "StepByStepTestRunner";
    }

    public class StepByStepTestRunnerWindow : EditorWindow
    {
        [MenuItem("Hinode/Tools/StepByStep Test Runner")]
        public static void CreateOrOpen()
        {
            var window = CreateWindow<StepByStepTestRunnerWindow>("StepByStep Test Runner");
            window.Show();
        }

        Assembly[] _cachedAssembies = null;
        TypeInfo[] _cachedTestClassTypes = null;

        TypeInfo SelectedTestClassInfo
        {
            get; set;
        }
        MethodInfo SelectedUnityTestMethod { get; set; }
        //{
        //    get => SelectedTestClassInfo?.GetMethod(_selectedUnityTestMethodName)
        //        ?? null;
        //}
        public bool IsValid { get => SelectedTestClassInfo != null && SelectedUnityTestMethod != null; }


        Assembly[] CachedAssembies
        {
            get => _cachedAssembies != null
                ? _cachedAssembies
                : _cachedAssembies = System.AppDomain.CurrentDomain.GetAssemblies()
                    .Where(_asm => _asm.IsFullyTrusted)
                    .ToArray();
            set => _cachedAssembies = value ?? new Assembly[] { };
        }

        TypeInfo[] CachedTestClassInfos
        {
            get => _cachedTestClassTypes != null
                ? _cachedTestClassTypes
                : _cachedTestClassTypes = CachedAssembies
                    .SelectMany(_asm => _asm.DefinedTypes)
                    .Where(_t => _t.IsClass
                        && _t.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                            .Any(_m => _m.GetCustomAttribute<UnityTestAttribute>() != null)
                    )
                    .ToArray();
            set => _cachedTestClassTypes = value ?? new TypeInfo[] { };
        }

        [System.Serializable]
        class SaveData
        {
            public string TestClassTypeFulleName;
            public string MethodName;

            public static void Save(StepByStepTestRunnerWindow target)
            {
                if (!Directory.Exists("HinodeCaches"))
                {
                    Directory.CreateDirectory("HinodeCaches");
                }
                var saveData = new SaveData()
                {
                    TestClassTypeFulleName = target.SelectedTestClassInfo.FullName,
                    MethodName = target.SelectedUnityTestMethod.Name,
                };
                File.WriteAllText(Path.Combine("HinodeCaches", "StepByStepTestRunner.json"), JsonUtility.ToJson(saveData));
            }

            public static SaveData Load()
            {
                if (!Directory.Exists("HinodeCaches"))
                {
                    return new SaveData();
                }
                var json = File.ReadAllText(Path.Combine("HinodeCaches", "StepByStepTestRunner.json"));
                return JsonUtility.FromJson<SaveData>(json);
            }
        }

        void StartTest()
        {
            SaveData.Save(this);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = $"__{SelectedTestClassInfo.FullName}#{SelectedUnityTestMethod.Name}";
            RunTestStepByStep.SetupScene(scene, SelectedUnityTestMethod);
            EditorApplication.isPlaying = true;
        }


        void ReloadAssembies()
        {
            Logger.Log(Logger.Priority.Debug, () => "Loading Assemblies", StepByStepTestRunnerDefines.LOGGER_SELECTOR);

            CachedAssembies = System.AppDomain.CurrentDomain.GetAssemblies()
                    .Where(_asm => _asm.IsFullyTrusted)
                    .ToArray();
            CachedTestClassInfos = CachedAssembies
                    .SelectMany(_asm => _asm.DefinedTypes)
                    .Where(_t => _t.IsClass
                        && _t.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                            .Any(_m => _m.GetCustomAttribute<UnityTestAttribute>() != null)
                    )
                    .ToArray();

            var saveData = SaveData.Load();
            TestClassPopup = new TestClassPopupGUILayout(this);
            TestClassPopup.SelectedElement = saveData.TestClassTypeFulleName;

            UnityTestMethodPopup = new UnityTestMethodPopupGUILayout(TestClassPopup.SelectedTestClassType);
            UnityTestMethodPopup.SelectedElement = saveData.MethodName;

            if(SelectedTestClassInfo == null)
            {
                SelectedTestClassInfo = TestClassPopup.SelectedTestClassType;
            }
            else
            {
                TestClassPopup.SelectedTestClassType = SelectedTestClassInfo;
                SelectedTestClassInfo = TestClassPopup.SelectedTestClassType;
            }

            if (SelectedUnityTestMethod == null)
            {
                SelectedUnityTestMethod = UnityTestMethodPopup.SelectedUnityTestMethodInfo;
            }
            else
            {
                UnityTestMethodPopup.SelectedUnityTestMethodInfo = SelectedUnityTestMethod;
                SelectedUnityTestMethod = UnityTestMethodPopup.SelectedUnityTestMethodInfo;
            }
            Logger.Log(Logger.Priority.Debug, () => "Finish to Load Assemblies", StepByStepTestRunnerDefines.LOGGER_SELECTOR);
        }

        #region EditorWindow
        GUIContent _selectedTestClassLabel = new GUIContent("Test Class Name");
        GUIContent _selectedUnityTestMethodLabel = new GUIContent("UnityTest Method Name");

        TestClassPopupGUILayout _testClassPopup;
        UnityTestMethodPopupGUILayout _unityTestMethodPopup;

        TestClassPopupGUILayout TestClassPopup
        {
            get
            {
                return _testClassPopup;
            }
            set => _testClassPopup = value;
        }
        UnityTestMethodPopupGUILayout UnityTestMethodPopup
        {
            get
            {
                return _unityTestMethodPopup;
            }
            set => _unityTestMethodPopup = value;
        }

        private void OnEnable()
        {
            ReloadAssembies();
        }

        private void OnDisable()
        {
            SaveData.Save(this);
        }

        private void OnGUI()
        {
            if (TestClassPopup.Draw(_selectedTestClassLabel))
            {
                SelectedTestClassInfo = TestClassPopup.SelectedTestClassType;
                UnityTestMethodPopup = new UnityTestMethodPopupGUILayout(SelectedTestClassInfo);
            }
            if (UnityTestMethodPopup.Draw(_selectedUnityTestMethodLabel))
            {
                SelectedUnityTestMethod = UnityTestMethodPopup.SelectedUnityTestMethodInfo;
            }

            if (IsValid)
            {
                if (GUILayout.Button("Start Test"))
                {
                    StartTest();
                }
            }
            else
            {
                EditorGUILayout.LabelField("Not Start Test...");
            }

            using (var scope = new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Reload Assemblies?");
                if(GUILayout.Button("Reload"))
                {
                    ReloadAssembies();
                }
            }
        }
        #endregion

        #region For Layout 
        class TestClassPopupGUILayout : PopupEditorGUILayout
        {
            StepByStepTestRunnerWindow Target { get; }

            public TypeInfo SelectedTestClassType
            {
                get => Target.CachedTestClassInfos[SelectedIndex];
                set
                {
                    var index = System.Array.IndexOf(Target.CachedTestClassInfos, value);
                    SelectedIndex = index;
                }
            }

            public TestClassPopupGUILayout(StepByStepTestRunnerWindow target)
            {
                Target = target;
            }

            #region PopupEditorGUILayout
            protected override string[] CreateDisplayOptionList()
            {
                var classTypes = Target.CachedTestClassInfos
                    .Select(_t => _t.FullName)
                    .ToArray();
                return classTypes.Length > 0 ? classTypes : new string[] { "(empty)" };
            }
            #endregion
        }

        class UnityTestMethodPopupGUILayout : PopupEditorGUILayout
        {
            public TypeInfo TestClassType { get; }

            public MethodInfo SelectedUnityTestMethodInfo
            {
                get => TestClassType?.GetMethod(SelectedElement, BindingFlags.Instance | BindingFlags.Public)
                    ?? null;
                set
                {
                    var index = System.Array.IndexOf(DisplayOptionList, value?.Name ?? "");
                    SelectedIndex = index;
                }
            }

            public UnityTestMethodPopupGUILayout(TypeInfo testClassType)
            {
                TestClassType = testClassType;
            }

            #region PopupEditorGUILayout
            protected override string[] CreateDisplayOptionList()
            {
                if (TestClassType == null) return new string[] { "(empty)" };

                var methodNames = TestClassType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(_methodInfo => _methodInfo.GetCustomAttribute<UnityTestAttribute>() != null)
                    .Select(_methodInfo => _methodInfo.Name)
                    .ToArray();
                if (methodNames.Length <= 0)
                    methodNames = new string[] { "(empty)" };
                return methodNames;
            }
            #endregion
        }
        #endregion
    }
}
