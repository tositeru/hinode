// Copyright 2019~ tositeru
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
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// SubComponentManagerをラップしたMonoBehaviour
    ///
    /// 継承先のクラスが持つSubComponentの情報を閲覧することができるEditorWindowを提供してます。
    /// Hinode > Tools > SubComponent Summary からそのWindowを開けます。
    /// 
    /// <see cref="ISubComponent{T}"/>
    /// <see cref="SubComponentManager{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MonoBehaviourWithSubComponents<T> : MonoBehaviour
        , ISubComponent<T>
        where T : MonoBehaviour
    {
        SubComponentManager<T> _subComponents;

        protected SubComponentManager<T> SubComponentManager { get => _subComponents; }
        public T RootComponent { get; set; }

        public void BindCallbacks(object obj)
        {
            var labelObj = LabelObject.GetLabelObject(obj);
            if (labelObj == null) return;

            BindCallbackAttribute.BindToGameObject(this, labelObj.gameObject, Labels.MatchOp.Included, labelObj.AllLabels);
        }

        protected virtual void Awake()
        {
            Assert.IsTrue(this is T, $"{this.GetType()} is not {typeof(T)}...");

            _subComponents = new SubComponentManager<T>(this as T);
            _subComponents.Init();
        }

        protected virtual void Start()
        {
            _subComponents.UpdateUI();
        }

        protected virtual void OnDestroy()
        {
            _subComponents.Destroy();
        }

        #region ISubComponent
        public virtual void Init()
        {
            var selfScene = SceneExtensions.GetSceneEnumerable()
                .First(_s => _s.GetRootGameObjects().Any(_o => _o == gameObject));
            foreach (var obj in selfScene.GetGameObjectEnumerable())
            {
                BindCallbacks(obj);
            }
        }
        public virtual void Destroy() { }
        public virtual void UpdateUI() { }
        #endregion
    }
}
