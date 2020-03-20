using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    public static partial class EditorMonoBehaviourExtensions
    {
        /// <summary>
        /// InspectorではなくGameView上で処理を実行したい時に使用してください
        ///
        /// ex) Screen#widthをInspectorではなくGameViewのものを使いたい時
        /// </summary>
        /// <param name="target"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static Coroutine DoneInGameView(this MonoBehaviour target, System.Action predicate)
        {
            return target.StartCoroutine(DoneInGameViewEnumerator(predicate));
        }

        static IEnumerator DoneInGameViewEnumerator(System.Action predicate)
        {
            yield return null;
            predicate();
        }
    }

}
