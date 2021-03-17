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
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="MonoBehaviourWithSubComponents"/>
    /// <seealso cref="ISubComponent{T}"/>
    /// </summary>
    public abstract class GameObjectPoolBase<TRootSubComponent, TOwner, TInstance> : UnityComponentPool<TInstance>
        where TRootSubComponent : MonoBehaviourWithSubComponents<TRootSubComponent>
        where TOwner : class, ISubComponent<TRootSubComponent>
        where TInstance : Component
    {
        public TOwner Owner { get; private set; }
        public new PoolCreator Creator { get => base.Creator as PoolCreator; }

        public GameObjectPoolBase(TOwner owner, Transform parent, GameObject instanceTemplate)
            : base(new PoolCreator(owner, parent, instanceTemplate))
        {
            Owner = owner;
        }

        public virtual void DestroyNotTemplateInRoot()
        {
            foreach (var t in Creator.Parent.GetChildEnumerable()
                .Where(_c => _c.gameObject != Creator.Template.gameObject))
            {
                Object.Destroy(t.gameObject);
            }
        }

        public virtual void PushAllIcon()
        {
            foreach (var icon in Creator.Parent.GetChildEnumerable()
                .Select(_c => _c.GetComponent<TInstance>())
                .Where(_i => _i != null))
            {
                Push(icon);
            }
        }

        public virtual TInstance GetInstance()
        {
            var icon = PopOrCreate();
            icon.gameObject.SetActive(true);
            icon.transform.SetParent(Creator.Parent);
            return icon;
        }

        public class PoolCreator : IInstanceCreater
        {
            TOwner _owner;

            public TInstance Template { get; private set; }
            public Transform Parent { get; private set; }

            public TOwner Owner
            {
                get => _owner;
                private set
                {
                    if (_owner == value) return;
                    _owner = value;
                }
            }

            public PoolCreator(TOwner owner, Transform parent, GameObject instanceTemplate)
            {
                Parent = parent;
                Owner = owner;

                Template = instanceTemplate.GetOrAddComponent<TInstance>();
                Template.gameObject.SetActive(false);
                //note: 他のインスタンスと区別するためにParentから外している
                Template.transform.SetParent(null, false);
                Template.transform.localScale = Vector3.one;
            }

            #region ObjectPool<MassComponent>.IInstanceCreater interface
            public TInstance Create()
            {
                var inst = Object.Instantiate(Template, Parent);
                Owner.RootComponent.BindCallbacks(inst);
                return inst;
            }
            #endregion
        }
    }
}
