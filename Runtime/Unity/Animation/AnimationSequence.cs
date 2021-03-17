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
using UnityEngine.Events;

namespace Hinode
{
    /// <summary>
    /// Not Test Code
    /// </summary>
    public class AnimationSequence : MonoBehaviour
    {
        public Step[] animationSteps = new Step[] { };
        public int animationStep = 0;
        bool _doLocking = false;
        UnlockInfo _useUnlockInfo;

        public bool doLocking { get => _doLocking; }

        AnimationHub animationHub { get => GetComponent<AnimationHub>(); }

        public void NextStepAnimation()
        {
            if (doLocking) return;

            if (animationStep < animationSteps.Length)
            {
                var step = animationSteps[animationStep];
                if (!animationHub.FirePack(step.key))
                {
                    Debug.LogError($"'{step.key}' is not exist in AnimationHub({animationHub.name})...");
                }
                else
                {
                    animationStep++;

                    _doLocking = step.isLock;
                    _useUnlockInfo = step.unlockInfo;
                }
            }
        }

        internal bool Unlock(AnimationHubBehaviour.State state, Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!doLocking) return false;

            var doUnlock = _useUnlockInfo.DoUnlock(state, animator, stateInfo, layerIndex);
            if (doUnlock)
            {
                _doLocking = false;

                _useUnlockInfo.OnUnlocked?.Invoke();
                _useUnlockInfo = null;
            }

            return doUnlock;
        }

        [System.Serializable]
        public class Step
        {
            [SerializeField] string _key;
            [SerializeField] bool _isLock;
            [SerializeField] UnlockInfo _unlockInfo;

            public string key { get => _key; set => _key = value; }
            public bool isLock { get => _isLock; set => _isLock = value; }
            public UnlockInfo unlockInfo { get => _unlockInfo; set => _unlockInfo = value; }
        }

        [System.Serializable]
        public class UnlockInfo
        {
            [SerializeField] Animator _animator;
            [SerializeField] string _stateName;
            [SerializeField] string _layerName;
            [SerializeField] AnimationHubBehaviour.State _state;
            [SerializeField] UnityEvent _onUnlocked;

            public Animator animator { get => _animator; set => _animator = value; }
            public string stateName { get => _stateName; set => _stateName = value; }
            public string layerName { get => _layerName; set => _layerName = value; }
            public AnimationHubBehaviour.State state { get => _state; set => _state = value; }
            public UnityEvent OnUnlocked { get => _onUnlocked; }

            public bool DoUnlock(AnimationHubBehaviour.State state, Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
            {
                var isUnlock = this.state == state
                    && this.animator == animator
                    && layerIndex == animator.GetLayerIndex(layerName)
                    && stateInfo.shortNameHash == Animator.StringToHash(stateName)
                ;
                return isUnlock;
            }
        }
    }
}