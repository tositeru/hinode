// Copyright 2019 ~ tositeru
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Hinode.SceneUtils
{
    public enum SceneKind
    {
        Primary,
        Sub,
        Daemon,
    }

    public enum SceneCondition
    {
        Loading,
        Loaded,
        Unloading,
        Unloaded,
    }

    public class SceneManagerModel
    {
        public static void ExitApplication()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        Dictionary<string, SceneInfo> _scenes = new Dictionary<string, SceneInfo>();
        public IReadOnlyDictionary<string, SceneInfo> Scenes { get => _scenes; }

        public SceneInfo PrimarySceneInfo { get => _scenes.Values.FirstOrDefault(_s => _s.Tags.Contains(SceneInfo.TAG_PRIMARY)); }

        public SceneInfo Find(string scenePath)
        {
            return _scenes.ContainsKey(scenePath)
                ? _scenes[scenePath]
                : null;
        }

        public (SceneInfo, SceneInfo prevPrimary) ChangePrimaryScene(SceneAddParameters addParameters, bool doUnloadPrevPrimary=true)
        {
            var prevPrimary = PrimarySceneInfo;
            if (addParameters.IsSingleModeLoading)
            {
                foreach (var info in Scenes.Values)
                {
                    info.Unload();
                }
                _scenes.Clear();
            }
            else if (prevPrimary != null)
            {
                if (prevPrimary.ScenePath == addParameters.ScenePath)
                {
                    return (prevPrimary, prevPrimary);
                }
                else if(doUnloadPrevPrimary)
                {
                    prevPrimary.Unload();
                }
            }

            addParameters.Kind = SceneKind.Primary;
            var t = InnerAddScene(addParameters);
            return (t.sceneInfo, prevPrimary);
        }

        public (SceneInfo sceneInfo, bool isAdd) AddSubScene(SceneAddParameters addParameters)
        {
            addParameters.Kind = SceneKind.Sub;
            return InnerAddScene(addParameters);
        }

        public (SceneInfo sceneInfo, bool isAdd) AddDaemonScene(SceneAddParameters addParameters)
        {
            addParameters.Kind = SceneKind.Daemon;
            return InnerAddScene(addParameters);
        }

        (SceneInfo sceneInfo, bool isAdd) InnerAddScene(SceneAddParameters addParameters)
        {
            var sceneInfo = Find(addParameters.ScenePath);
            if (sceneInfo != null)
            {
                sceneInfo.Kind = addParameters.Kind;
                return (sceneInfo, false);
            }

            var info = new SceneInfo(this, addParameters);
            _scenes.Add(addParameters.ScenePath, info);
            return (info, true);
        }

        internal void RemoveSceneInfo(SceneInfo sceneInfo)
        {
            if (!_scenes.ContainsKey(sceneInfo.ScenePath))
                return;

            _scenes.Remove(sceneInfo.ScenePath);
        }

        public List<(Scene, AsyncOperation)> UnloadUnmanagedScene()
        {
            var asyncList = new List<(Scene, AsyncOperation)>();
            for (var i = 0; i < SceneManager.sceneCount; ++i)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (Scenes.ContainsKey(scene.path)) continue;

                var async = SceneManager.UnloadSceneAsync(scene);
                //Debug.Log($"test : {scene.path} isValid = {scene.IsValid()}, {async != null}");
                asyncList.Add((scene, async));
            }
            return asyncList;
        }
    }

    public class SceneAddParameters
    {
        string _scenePath = "";

        public bool IsAlreadyLoaded { get; private set; } = false;
        public string ScenePath
        {
            get => _scenePath;
            set
            {
                if (_scenePath == value) return;
                _scenePath = value;

                var scene = SceneManager.GetSceneByPath(_scenePath);
                IsAlreadyLoaded = scene.path == _scenePath;
            }
        }
        public SceneKind Kind { get; set; }
        public HashSet<string> Tags { get; } = new HashSet<string>();
        public object AppendParameters { get; set; }

        /// <summary>
        /// This property enable only Kind == SceneKind.Primary!
        /// </summary>
        public bool IsSingleModeLoading { get; set; } = false;

        public bool IsAutoLoad { get; set; } = true;
    }

    public interface IReadOnlySceneInfo
    {
        string ScenePath { get; }
        Scene Scene { get; }
        SceneKind Kind { get; }
        SceneCondition Condition { get; }
        IReadOnlyHashSetHelper<string> ReadOnlyTags { get; }

        bool ContainAsyncOP { get; }
        bool AllowSceneActivation { get; }
    }

    public delegate void OnChangedConditionDelegate(IReadOnlySceneInfo sceneInfo, SceneCondition prev);
    public delegate void OnChangedKindDelegate(IReadOnlySceneInfo sceneInfo, SceneKind prev);

    public class SceneInfo : IReadOnlySceneInfo
    {
        public static readonly string TAG_PRIMARY = "@PRIMARY";

        SmartDelegate<OnChangedConditionDelegate> _onChangedCondition = new SmartDelegate<OnChangedConditionDelegate>();
        SmartDelegate<OnChangedKindDelegate> _onChangedKind = new SmartDelegate<OnChangedKindDelegate>();

        HashSetHelper<string> _tags = new HashSetHelper<string>();
        SceneKind _kind = SceneKind.Primary;
        AsyncOperation _asyncOP;

        public SceneManagerModel Manager { get; }
        public string ScenePath { get; }
        public Scene Scene { get; private set; }
        public SceneKind Kind
        {
            get => _kind;
            set
            {
                if (_kind == value) return;
                var prev = _kind;
                _kind = value;
                if (value == SceneKind.Primary)
                {
                    _tags.Add(TAG_PRIMARY);
                }
                else
                {
                    _tags.Remove(TAG_PRIMARY);
                }

                _onChangedKind.SafeDynamicInvoke(this, prev, () => "Fail in Kind_set...");
            }
        }

        public bool IsSingleModeLoading { get; }
        public SceneCondition Condition { get; private set; }
        public IReadOnlyHashSetHelper<string> ReadOnlyTags { get => _tags; }
        public HashSetHelper<string> Tags { get => _tags; }
        public bool ContainAsyncOP { get => _asyncOP != null; }

        public object AppendParameters { get; set; }
        /// <summary>
        /// Unityの仕様上の注意点として、AsyncOperation#allowSceneActivationをfalseにすると、
        /// AsyncOperation対象となるSceneより後に読み込んだシーンがある際、AsyncOperation#allowSceneActivationの値に関係なく
        /// そのアクティブ化も待つようになりますので、極力アクティブ化したい順番にシーンを読み込むようにしてください。
        /// </summary>
        /// <param name="isActivation"></param>
        public bool AllowSceneActivation
        {
            get => _asyncOP?.allowSceneActivation ?? false;
            set
            {
                if (_asyncOP == null) return;
                _asyncOP.allowSceneActivation = value;
            }
        }

        public NotInvokableDelegate<OnChangedConditionDelegate> OnChangedCondition { get => _onChangedCondition; }
        public NotInvokableDelegate<OnChangedKindDelegate> OnChangedKind { get => _onChangedKind; }

        internal SceneInfo(SceneManagerModel manager, SceneAddParameters addParameters)
        {
            Manager = manager;
            ScenePath = addParameters.ScenePath;
            Kind = addParameters.Kind;
            Condition = SceneCondition.Unloaded;
            AppendParameters = addParameters.AppendParameters;

            if (addParameters.Kind == SceneKind.Primary)
            {
                Tags.Add(TAG_PRIMARY);
                IsSingleModeLoading = addParameters.IsSingleModeLoading;
            }

            if(addParameters.IsAlreadyLoaded)
            {
                Scene = SceneManager.GetSceneByPath(addParameters.ScenePath);
                if(Scene.isLoaded)
                {
                    Condition = SceneCondition.Loaded;
                }
            }

            if(addParameters.IsAutoLoad)
            {
                Load();
            }
        }

        #region Load/Unload
        public void Load()
        {
            if (Condition == SceneCondition.Loading || Condition == SceneCondition.Loaded) return;

            var prevCondition = Condition;
            Condition = SceneCondition.Loading;

            var loadMode = IsSingleModeLoading ? LoadSceneMode.Single : LoadSceneMode.Additive;
            _asyncOP = SceneManager.LoadSceneAsync(ScenePath, loadMode);
            _asyncOP.allowSceneActivation = true;
            _asyncOP.completed += AsyncOP_completed;

            _onChangedCondition.SafeDynamicInvoke(this, prevCondition, () => "Fail in SceneInfo#Load...");
        }

        public void Unload()
        {
            if (Condition == SceneCondition.Unloading || Condition == SceneCondition.Unloaded) return;
            if (!Scene.IsValid())
            {
                return;
            }

            switch (Condition)
            {
                case SceneCondition.Unloading: return;
                case SceneCondition.Loading:
                    _asyncOP.allowSceneActivation = false;
                    break;
            }

            var prevCondition = Condition;
            Condition = SceneCondition.Unloading;
            _asyncOP = SceneManager.UnloadSceneAsync(Scene);
            _asyncOP.allowSceneActivation = true;
            _asyncOP.completed += AsyncOP_completed;

            _onChangedCondition.SafeDynamicInvoke(this, prevCondition, () => "Fail in SceneInfo#UnLoad...");
        }

        private void AsyncOP_completed(AsyncOperation obj)
        {
            var prevCondition = Condition;
            switch (Condition)
            {
                case SceneCondition.Loading:
                    Condition = SceneCondition.Loaded;
                    Scene = SceneManager.GetSceneByPath(ScenePath);
                    _onChangedCondition.SafeDynamicInvoke(this, prevCondition, () => "Fail in SceneInfo#Load...");
                    break;
                case SceneCondition.Unloading:
                    Manager.RemoveSceneInfo(this);
                    Condition = SceneCondition.Unloaded;
                    Scene = default;
                    _onChangedCondition.SafeDynamicInvoke(this, prevCondition, () => "Fail in SceneInfo#Load...");
                    break;
            }
            _asyncOP = null;
        }
        #endregion
    }

}
