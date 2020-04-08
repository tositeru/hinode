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
        // A Test behaves as an ordinary method
        [Test]
        public void GetEnumerablePasses()
        {
            System.Array arr = Enumerable.Range(0, 5).ToArray();

            AssertionUtils.AssertEnumerable(arr.GetEnumerable().Select(_o => (int)_o), Enumerable.Range(0, 5), "");
        }
    }
}
