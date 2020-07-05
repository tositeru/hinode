﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="Hinode.Tests.Extensions.TestTransformExtensions"/>
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// <seealso cref="Hinode.Tests.Extensions.TestTransformExtensions.GetChildEnumerablePass()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IEnumerable<Transform> GetChildEnumerable(this Transform t)
        {
            return new ChildEnumerable(t);
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Extensions.TestTransformExtensions.GetHierarchyEnumerablePass()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IEnumerable<Transform> GetHierarchyEnumerable(this Transform t)
        {
            return new HierarchyEnumerable(t);
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Extensions.TestTransformExtensions.GetParentEnumerablePass()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IEnumerable<Transform> GetParentEnumerable(this Transform t)
        {
            return new ParentEnumerable(t);
        }

        /// <summary>
        /// <seealso cref="GetChildEnumerable(Transform)"/>
        /// </summary>
        public class ChildEnumerable : IEnumerable<Transform>, IEnumerable
        {
            Transform _target;
            public ChildEnumerable(Transform target)
            {
                _target = target;
            }

            public IEnumerator<Transform> GetEnumerator()
            {
                for(int i=0; i<_target.childCount; ++i)
                {
                    yield return _target.GetChild(i);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        /// <summary>
        /// 自身を含み、ルートオブジェクトとしたオブジェクト階層をたどるIEnumerable
        /// <seealso cref="GetHierarchyEnumerable(Transform)"/>
        /// </summary>
        public class HierarchyEnumerable : IEnumerable<Transform>, IEnumerable
        {
            Transform _target;
            public HierarchyEnumerable(Transform t)
            {
                Assert.IsNotNull(t);
                _target = t;
            }

            public IEnumerator<Transform> GetEnumerator()
            {
                var it = _target;
                while(it != null)
                {
                    yield return it;
                    it = GetNext(it);
                }
            }

            Transform GetNext(Transform now, int nextChildIndex=0)
            {
                if(nextChildIndex >= now.childCount)
                {
                    return now == _target
                        ? null
                        : GetNext(now.parent, now.GetSiblingIndex()+1);
                }
                if (now.childCount > 0)
                {
                    return now.GetChild(nextChildIndex);
                }
                if (now == _target)
                {
                    return null;
                }
                if (now.GetSiblingIndex() < now.parent.childCount-1)
                {
                    return now.parent.GetChild(now.GetSiblingIndex()+1);
                }
                return GetNext(now.parent, now.GetSiblingIndex()+1);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        /// <summary>
        /// <seealso cref="GetParentEnumerable(Transform)"/>
        /// </summary>
        class ParentEnumerable : IEnumerable<Transform>, IEnumerable
        {
            Transform _target;
            public ParentEnumerable(Transform target)
            {
                Assert.IsNotNull(target);
                _target = target;
            }

            public IEnumerator<Transform> GetEnumerator()
            {
                var p = _target.parent;
                while (p != null)
                {
                    yield return p;
                    p = p.parent;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }

    }
}
