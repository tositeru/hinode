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

    public class TestArray2DExtensions
    {
        const int ORDER_AS_ENUMERABLE = 0;
        const int ORDER_COPY = ORDER_AS_ENUMERABLE + 100;

        /// <summary>
        /// <seealso cref="Array2DExtensions.AsEnumerable{T}(T[,])"/>
        /// </summary>
        [Test, Order(ORDER_AS_ENUMERABLE)]
        public void AsEnumerable_Pass()
        {
            int[,] list = new int[,] {
                { 0, 1, 2, },
                { 3, 4, 5, },
            };

            AssertionUtils.AssertEnumerable(
                Enumerable.Range(0, 6)
                , list.AsEnumerable()
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="Array2DExtensions.AsEnumerableWithIndex{T}(T[,]){T}(T[,])"/>
        /// </summary>
        [Test, Order(ORDER_AS_ENUMERABLE)]
        public void AsEnumerableWithIndex_Pass()
        {
            int[,] list = new int[,] {
                { 0, 1, 2, },
                { 3, 4, 5, },
            };

            //Assert.AreEqual(1, list[0, 1]);
            //Assert.AreEqual(3, list.GetLength(0));

            AssertionUtils.AssertEnumerable(
                Enumerable.Range(0, 6)
                    .Select(_i => (_i, _i % 3, _i / 3))
                , list.AsEnumerableWithIndex()
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="Array2DExtensions.Copy{T}(T[,])"/>
        /// </summary>
        [Test, Order(ORDER_COPY)]
        public void Copy_Passes()
        {
            int[,] list = new int[,] {
                { 0, 1, 2, },
                { 3, 4, 5, },
            };

            var copy = list.Copy();

            AssertionUtils.AssertEnumerable(
                list.AsEnumerableWithIndex()
                , copy.AsEnumerableWithIndex()
                , ""
            );
        }
    }
}
