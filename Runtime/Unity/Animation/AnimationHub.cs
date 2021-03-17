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
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;
using UnityEngine.Events;

namespace Hinode
{
    /// <summary>
    /// Not Test Code
    /// </summary>
    [DisallowMultipleComponent()]
    [RequireComponent(typeof(AnimationSequence))]
    public class AnimationHub : MonoBehaviour
    {
        public delegate void OnStateMachiBehaviourDelegate(AnimationHubBehaviour behaviour, AnimationHubBehaviour.State state, Animator animator, AnimatorStateInfo stateInfo, int layerIndex);

#pragma warning disable CS0649
        [SerializeField] List<AnimationPack> _animationPacks = new List<AnimationPack>();
#pragma warning restore CS0649

        SmartDelegate<OnStateMachiBehaviourDelegate> _onStateMachiBehaviourDelegate = new SmartDelegate<OnStateMachiBehaviourDelegate>();

        public List<AnimationPack> AnimationPacks { get => _animationPacks; }
        public AnimationSequence Sequence { get => GetComponent<AnimationSequence>(); }

        public NotInvokableDelegate<OnStateMachiBehaviourDelegate> OnStateMachiBehaviour { get => _onStateMachiBehaviourDelegate; }

        public bool ContainsKey(string key)
        {
            return AnimationPacks.Any(_p => _p.key == key);
        }

        public bool FirePack(string key)
        {
            bool isFire = false;
            foreach (var pack in AnimationPacks
                .Where(_p => _p.key == key))
            {
                pack.Fire();
                isFire = true;
            }
            return isFire;
        }

        public void DestroyGameObject(GameObject target)
        {
            Destroy(target);
        }

        internal void OnStateMachineBehaviour(AnimationHubBehaviour behaviour, AnimationHubBehaviour.State state, Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Sequence.Unlock(state, animator, stateInfo, layerIndex);

            _onStateMachiBehaviourDelegate.SafeDynamicInvoke(behaviour, state, animator, stateInfo, layerIndex, () => "Fail in OnStateMachineBehaviour...");
        }

#if UNITY_EDITOR
        //private void OnValidate()
        //{
        //    if (Application.isPlaying) return;

        //    foreach (var p in animationPacks)
        //    {
        //        p.Validate();
        //    }
        //}
#endif

        [System.Serializable]
        public class AnimationPack
        {
#pragma warning disable CS0649
            [SerializeField] string _key;
            [SerializeField] List<AnimatorInfo> _infos = new List<AnimatorInfo>();
#pragma warning restore CS0649

            public string key { get => _key; set => _key = value; }
            public List<AnimatorInfo> infos { get => _infos; }

            public void Validate()
            {
                foreach (var i in infos)
                {
                    i.Validate(key);
                }
            }

            public void Fire()
            {
                foreach (var i in infos)
                {
                    i.Fire();
                }
            }
        }

        [System.Serializable]
        public class AnimatorInfo
        {
            public enum ValueKind
            {
                Integer,
                Float,
                Bool,
                Trigger,
                Activate,
                Signal
            }

#pragma warning disable CS0649
            [SerializeField] Animator _animator;
            [SerializeField] ValueKind _valueKind;
            [SerializeField] string _valueName;
            [SerializeField] int _intValue;
            [SerializeField] float _floatValue;
            [SerializeField] bool _boolValue;
            [SerializeField] bool _isActivate;

            [SerializeField] UnityEvent _signal;

#pragma warning restore CS0649

            public Animator animator { get => _animator; set => _animator = value; }
            public ValueKind valueKind { get => _valueKind; set => _valueKind = value; }
            public string valueName { get => _valueName; set => _valueName = value; }
            public int intValue { get => _intValue; set => _intValue = value; }
            public float floatValue { get => _floatValue; set => _floatValue = value; }
            public bool boolValue { get => _boolValue; set => _boolValue = value; }
            public bool IsActivate { get => _isActivate; set => _isActivate = value; }

            public void Validate(string name)
            {
                if (valueKind == ValueKind.Signal)
                {

                }
                else
                {
                    if (animator == null)
                    {
                        Debug.LogError($"{name}: 'animator' is not null... kind={valueKind}, valueName={valueName}");
                    }
                    else if (valueKind == ValueKind.Activate)
                    {

                    }
                    else if (!animator.parameters.Any(_p => _p.name == valueName))
                    {
                        Debug.LogError($"{name}: '{animator.name}.{valueName}' do not exist in animator's parameters... kind={valueKind}, valueName={valueName}");
                    }
                }
            }

            public void Fire()
            {
                if (valueKind == ValueKind.Signal)
                {
                    _signal.Invoke();
                }
                else if (valueKind == ValueKind.Activate)
                {
                    animator.gameObject.SetActive(boolValue);
                }
                else
                {
                    Assert.IsNotNull(animator, "animator is not null...");

                    if (IsActivate)
                    {
                        animator.gameObject.SetActive(true);
                    }

                    switch (valueKind)
                    {
                        case ValueKind.Integer:
                            animator.SetInteger(valueName, intValue);
                            break;
                        case ValueKind.Float:
                            animator.SetFloat(valueName, floatValue);
                            break;
                        case ValueKind.Bool:
                            animator.SetBool(valueName, boolValue);
                            break;
                        case ValueKind.Trigger:
                            animator.SetTrigger(valueName);
                            break;
                    }
                }
            }
        }
    }
}
