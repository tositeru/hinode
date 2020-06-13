using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp.Extensions
{
    /// <summary>
    /// <seealso cref="ArrayExtentions"/>
    /// </summary>
    public class TestArrayExtentions
    {
        /// <summary>
        /// <seealso cref="ArrayExtentions.GetEnumerable(System.Array)"/>
        /// </summary>
        [Test]
        public void GetEnumerablePasses()
        {
            System.Array arr = Enumerable.Range(0, 5).ToArray();

            AssertionUtils.AssertEnumerable(Enumerable.Range(0, 5), arr.GetEnumerable().Select(_o => (int)_o), "");
        }

        /// <summary>
        /// <seealso cref="ArrayExtentions.GetEnumerable{T}(System.Array)"/>
        /// </summary>
        [Test]
        public void GetEnumerableWithTypePasses()
        {
            System.Array arr = Enumerable.Range(0, 5).ToArray();

            AssertionUtils.AssertEnumerable(Enumerable.Range(0, 5), arr.GetEnumerable<int>().Select(_o => (int)_o), "");
        }
    }
}
