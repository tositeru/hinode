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

namespace Hinode
{
    /// <summary>
    /// Not Test Code
    /// </summary>
    public class AnimationHubBehaviour : StateMachineBehaviour
    {
        public enum State
        {
            Enter,
            Exit
        }
        IEnumerable<AnimationHub> GetHubs(Animator animator) { return new ParentAnimationHubEnumerable(animator); }

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            foreach (var hub in GetHubs(animator))
            {
                hub.OnStateMachineBehaviour(this, State.Enter, animator, stateInfo, layerIndex);
            }
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //}

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            foreach (var hub in GetHubs(animator))
            {
                hub.OnStateMachineBehaviour(this, State.Exit, animator, stateInfo, layerIndex);
            }
        }

        // OnStateMove is called right after Animator.OnAnimatorMove()
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that processes and affects root motion
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK()
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that sets up animation IK (inverse kinematics)
        //}

        class ParentAnimationHubEnumerable : IEnumerable<AnimationHub>, IEnumerable
        {
            Animator _target;
            public ParentAnimationHubEnumerable(Animator target)
            {
                _target = target;
            }

            public IEnumerator<AnimationHub> GetEnumerator()
            {
                Transform t = _target.transform;
                while (t != null)
                {
                    var hub = t.GetComponent<AnimationHub>();
                    if (hub != null) yield return hub;

                    t = t.parent;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }

    }
}