using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Hinode.MVC;

namespace Hinode.Editors.MVC
{
    public class MVCViewer : EditorWindow
    {
        [MenuItem("Hinode/Tools/MVC Viewer")]
        public static void Open()
        {
            var window = CreateWindow<MVCViewer>("Hinode MVC Viewer");
            window.Show();
        }
        GUIContent _modelHomeLabel = new GUIContent("Model Homes");

        IModelHome _selectedHome;

        Vector2 _rootScrollPos;
        private void OnGUI()
		{
            if(EditorApplication.isPlaying)
            {
                DrawModelHome();
            }
            else
            {
                EditorGUILayout.LabelField("This Tool only run Play Mode!");
            }
		}

        void DrawModelHome()
        {
            var homes = IModelHome.GetAllHomes();

            var (selectedHome, selectIndex) = homes.Zip(Enumerable.Range(0, homes.Length), (_h, _i) => (home: _h, index: _i))
                .FirstOrDefault(_t => _t.home == _selectedHome);
            var homeNames = homes
                .Select(_h => GetModelDisplayName(_h.RootModel))
                .ToArray();
            var newSelected = EditorGUILayout.Popup(_modelHomeLabel, selectIndex, homeNames);
            if (newSelected != selectIndex)
            {
                _selectedHome = homes[newSelected];
            }

            if (_selectedHome == null)
            {
                if(!homes.Any())
                {
                    return;
                }
                _selectedHome = homes.First();
            }

            using (var scrollScope = new EditorGUILayout.ScrollViewScope(_rootScrollPos))
            {
                _rootScrollPos = scrollScope.scrollPosition;
                using (var indentScope = new EditorGUI.IndentLevelScope())
                {
                    if (_selectedHome.RootModel == null)
                    {
                        EditorGUILayout.LabelField($"RootModel is none");
                        return;
                    }

                    EditorGUILayout.LabelField(_selectedHome.RootModel.GetPath());

                    DrawModel(_selectedHome.RootModel);
                }
            }
        }

        GUIContent _modelParentLabel = new GUIContent("Parent");
        GUIContent _modelParentSelectButtonLabel = new GUIContent("Select");
        GUIContent _modelChildrenPopupLabel = new GUIContent("Children");
        GUIContent _modelChildSelectButtonLabel = new GUIContent("Select");
        void DrawModel(Model model)
        {
            if(model == null)
            {
                EditorGUILayout.LabelField($"(null)...");
                return;
            }

            //Parent
            using (var HScope = new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(_modelParentLabel);
                EditorGUILayout.LabelField(GetModelDisplayName(model.Parent));
                if (GUILayout.Button(_modelParentSelectButtonLabel))
                {
                    // TODO select Parent
                }
            }

            //Children
            using (var HScope = new EditorGUILayout.HorizontalScope())
            {
                var childNames = model.Children
                    .Zip(Enumerable.Range(0, model.ChildCount), (_c, _i) => (child: _c, index: _i))
                    .Select(_t => $"({_t.index}:{GetModelDisplayName(_t.child)}")
                    .ToArray();
                var selectedChildIndex = 0;
                selectedChildIndex = EditorGUILayout.Popup(_modelChildrenPopupLabel, selectedChildIndex, childNames);
                if (GUILayout.Button(_modelChildSelectButtonLabel))
                {
                    // TODO select child
                }
            }

            //Self Parameters
        }

        string GetModelDisplayName(Model model)
            => model != null
                ? $"{model.GetType().FullName}: {model.Name}"
                : "(null)";
    }

}
