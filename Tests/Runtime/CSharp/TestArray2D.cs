using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp
{
    /// <summary>
    ///
    /// Test Case
    /// ## Constructor
    /// - Constructor()
    /// - Constructor(width, height)
    /// - Constructor(width, height, params T[] values)
    /// - Constructor(width, height, IEnumerable<T> values)
    /// ## IsInRange()
    /// ## this[x, y]
    /// - getter
    /// - setter
    /// ## GetEnumerable | GetEnumerator
    /// - GetEnumerator()
    /// - GetEnumerableWithIndex()
    /// - GetEnumerableWithIndexXY()
    /// ## Resize
    /// - Resize(int width, int height)
    /// - Resize(width, height, offsetX, offsetY)
    /// - Resize(int width, int height, params T[] values)
    /// - Resize(int width, int height, IEnumerable<T> values)
    /// ## Shift
    /// - Shift(int x, int y)
    /// ## Copy
    /// - Copy()
    /// <seealso cref="Array2D{T}"/>
    /// </summary>
    public class TestArray2D
    {
        const int ORDER_CONSTRUCTOR = 0;
        const int ORDER_IS_IN_RANGE = ORDER_CONSTRUCTOR + 100;
        const int ORDER_INDEXER = ORDER_CONSTRUCTOR + 100;
        const int ORDER_GET_ENUMERABLE = ORDER_CONSTRUCTOR + 100;
        const int ORDER_RESIZE = ORDER_CONSTRUCTOR + 100;
        const int ORDER_SHIFT = ORDER_CONSTRUCTOR + 100;
        const int ORDER_COPY = ORDER_CONSTRUCTOR + 100;

        #region Constructor
        /// <summary>
        /// <seealso cref="Array2D{T}()"/>
        /// </summary>
        [Test, Order(ORDER_CONSTRUCTOR), Description("Constructor()")]
        public void Constructor_Passes()
        {
            var array2D = new Array2D<int>();

            Assert.AreEqual(0, array2D.Count);
            Assert.AreEqual(0, array2D.Width);
            Assert.AreEqual(0, array2D.Height);
        }

        /// <summary>
        /// <seealso cref="Array2D{T}(int width, int height)"/>
        /// </summary>
        [Test, Order(ORDER_CONSTRUCTOR), Description("Constructor(width, height)")]
        public void Constructor_WithSize_Passes()
        {
            var w = 2;
            var h = 4;
            var array2D = new Array2D<int>(w, h);

            Assert.AreEqual(w*h, array2D.Count);
            Assert.AreEqual(w, array2D.Width);
            Assert.AreEqual(h, array2D.Height);
        }

        /// <summary>
        /// <seealso cref="Array2D{T}(int width, int height, params T[] values)"/>
        /// </summary>
        [Test, Order(ORDER_GET_ENUMERABLE + 10), Description("Constructor(width, height, params T[] values)")]
        public void Constructor_WithSizeAndValues_Passes()
        {
            var w = 2;
            var h = 2;
            var array2D = new Array2D<int>(w, h, 10, 20, 30, 40, 50);

            Assert.AreEqual(w * h, array2D.Count);
            Assert.AreEqual(w, array2D.Width);
            Assert.AreEqual(h, array2D.Height);

            AssertionUtils.AssertEnumerable(
                new (int value, int x, int y)[] {
                    (10, 0, 0),
                    (20, 1, 0),
                    (30, 0, 1),
                    (40, 1, 1),
                }
                , array2D.GetEnumerableWithIndexXY()
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="Array2D{T}(int width, int height, IEnumerable{T} values)"/>
        /// </summary>
        [Test, Order(ORDER_GET_ENUMERABLE + 10), Description("Constructor(width, height, IEnumerable<T> values)")]
        public void Constructor_WithSizeAndValues2_Passes()
        {
            var list = new List<int>()
            {
                10, 20, 30, 40, 50
            };
            var w = 2;
            var h = 2;
            var array2D = new Array2D<int>(w, h, list);

            Assert.AreEqual(w * h, array2D.Count);
            Assert.AreEqual(w, array2D.Width);
            Assert.AreEqual(h, array2D.Height);

            AssertionUtils.AssertEnumerable(
                new (int value, int x, int y)[] {
                    (10, 0, 0),
                    (20, 1, 0),
                    (30, 0, 1),
                    (40, 1, 1),
                }
                , array2D.GetEnumerableWithIndexXY()
                , ""
            );
        }
        #endregion

        #region IsInRange()
        /// <summary>
        /// <seealso cref="Array2D{T}.IsInRange(int , int)"/>
        /// </summary>
        [Test, Order(ORDER_IS_IN_RANGE), Description("IsInRange")]
        public void IsInRange_Passes()
        {
            var array2D = new Array2D<int>(2, 2, 10, 20, 30, 40);

            Assert.IsTrue(array2D.IsInRange(0, 0));
            Assert.IsTrue(array2D.IsInRange(1, 0));
            Assert.IsTrue(array2D.IsInRange(0, 1));
            Assert.IsTrue(array2D.IsInRange(1, 1));

            Assert.IsFalse(array2D.IsInRange(-1, 0));
            Assert.IsFalse(array2D.IsInRange(-1, 1));
            Assert.IsFalse(array2D.IsInRange(2, 0));
            Assert.IsFalse(array2D.IsInRange(2, 1));

            Assert.IsFalse(array2D.IsInRange(0, -1));
            Assert.IsFalse(array2D.IsInRange(1, -1));
            Assert.IsFalse(array2D.IsInRange(0, 2));
            Assert.IsFalse(array2D.IsInRange(1, 2));
        }
        #endregion

        #region
        /// <summary>
        /// <seealso cref="Array2D{T}.indexer[int , int]"/>
        /// </summary>
        [Test, Order(ORDER_INDEXER), Description("this[x, y] getter")]
        public void Indexer_Getter_Passes()
        {
            var array2D = new Array2D<int>(2, 2, 10, 20, 30, 40);

            Assert.AreEqual(10, array2D[0, 0]);
            Assert.AreEqual(20, array2D[1, 0]);
            Assert.AreEqual(30, array2D[0, 1]);
            Assert.AreEqual(40, array2D[1, 1]);

            Assert.Throws<System.IndexOutOfRangeException>(() => { var n = array2D[-1, 0]; });
            Assert.Throws<System.IndexOutOfRangeException>(() => { var n = array2D[-1, 1]; });
            Assert.Throws<System.IndexOutOfRangeException>(() => { var n = array2D[2, 0]; });
            Assert.Throws<System.IndexOutOfRangeException>(() => { var n = array2D[2, 1]; });

            Assert.Throws<System.IndexOutOfRangeException>(() => { var n = array2D[0, -1]; });
            Assert.Throws<System.IndexOutOfRangeException>(() => { var n = array2D[1, -1]; });
            Assert.Throws<System.IndexOutOfRangeException>(() => { var n = array2D[0, 2]; });
            Assert.Throws<System.IndexOutOfRangeException>(() => { var n = array2D[1, 2]; });
        }

        /// <summary>
        /// <seealso cref="Array2D{T}.indexer[int , int]"/>
        /// </summary>
        [Test, Order(ORDER_INDEXER), Description("this[x, y] setter")]
        public void Indexer_Setter_Passes()
        {
            var array2D = new Array2D<int>(2, 2, 10, 20, 30, 40);

            array2D[0, 0] = 100;
            Assert.AreEqual(100, array2D[0, 0]);

            Assert.Throws<System.IndexOutOfRangeException>(() => array2D[-1, 0] = 111);
        }
        #endregion

        #region GetEnumerable
        /// <summary>
        /// <seealso cref="Array2D{T}.GetEnumerator()"/>
        /// </summary>
        [Test, Order(ORDER_GET_ENUMERABLE), Description("GetEnumerator()")]
        public void GetEnumerator_Passes()
        {
            var array2D = new Array2D<int>(2, 2);
            var data = new int[] { 10, 20, 30, 40 };
            array2D[0, 0] = data[0];
            array2D[1, 0] = data[1];
            array2D[0, 1] = data[2];
            array2D[1, 1] = data[3];

            var e = array2D.GetEnumerator();
            var index = 0;
            while(e.MoveNext())
            {
                Assert.AreEqual(data[index], e.Current, $"Don't equal index={index}...");
                index++;
            }
        }

        /// <summary>
        /// <seealso cref="Array2D{T}.GetEnumerableWithIndex()"/>
        /// </summary>
        [Test, Order(ORDER_GET_ENUMERABLE), Description("GetEnumerableWithIndex()")]
        public void GetEnumerableWithIndex_Passes()
        {
            var array2D = new Array2D<int>(2, 2);
            var data = new int[] { 10, 20, 30, 40 };
            array2D[0, 0] = data[0];
            array2D[1, 0] = data[1];
            array2D[0, 1] = data[2];
            array2D[1, 1] = data[3];

            AssertionUtils.AssertEnumerable(
                new (int value, int index)[]
                {
                    (data[0], 0),
                    (data[1], 1),
                    (data[2], 2),
                    (data[3], 3),
                }
                , array2D.GetEnumerableWithIndex()
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="Array2D{T}.GetEnumerableWithIndexXY()"/>
        /// </summary>
        [Test, Order(ORDER_GET_ENUMERABLE), Description("GetEnumerableWithIndexXY()")]
        public void GetEnumerableWithIndexXY_Passes()
        {
            var array2D = new Array2D<int>(2, 2);
            var data = new int[] { 10, 20, 30, 40 };
            array2D[0, 0] = data[0];
            array2D[1, 0] = data[1];
            array2D[0, 1] = data[2];
            array2D[1, 1] = data[3];

            AssertionUtils.AssertEnumerable(
                new (int value, int x, int y)[]
                {
                    (data[0], 0, 0),
                    (data[1], 1, 0),
                    (data[2], 0, 1),
                    (data[3], 1, 1),
                }
                , array2D.GetEnumerableWithIndexXY()
                , ""
            );
        }
        #endregion

        #region Resize
        /// <summary>
        /// <seealso cref="Array2D{T}.Resize(int x, int y)"/>
        /// </summary>
        [Test, Order(ORDER_RESIZE), Description("Resize(width, height)")]
        public void Resize_Passes()
        {
            var array2D = new Array2D<int>();

            var w = 2;
            var h = 2;
            array2D.Resize(w, h);
            Assert.AreEqual(w * h, array2D.Count);
            Assert.AreEqual(w, array2D.Width);
            Assert.AreEqual(h, array2D.Height);
        }

        /// <summary>
        /// <seealso cref="Array2D{T}.Resize(int x, int y, int offsetX, int offsetY)"/>
        /// </summary>
        [Test, Order(ORDER_SHIFT + 100), Description("Resize(width, height, offsetX, offsetY)")]
        public void Resize_AndShift_Passes()
        {
            var w = 3;
            var h = 3;
            var array2D = new Array2D<int>(w, h
                , 1, 2, 3
                , 2, 4, 6
                , 3, 6, 9);

            w = 5;
            h = 5;
            array2D.Resize(w, h, 1, 1);

            Assert.AreEqual(w * h, array2D.Count);
            Assert.AreEqual(w, array2D.Width);
            Assert.AreEqual(h, array2D.Height);

            AssertionUtils.AssertEnumerable(
                new int[] {
                    0, 0, 0, 0, 0,
                    0, 1, 2, 3, 0,
                    0, 2, 4, 6, 0,
                    0, 3, 6, 9, 0,
                    0, 0, 0, 0, 0,
                }
                , array2D
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="Array2D{T}.Resize(int x, int y, params T[] values)"/>
        /// </summary>
        [Test, Order(ORDER_RESIZE), Description("Resize(int width, int height, params T[] values)")]
        public void Resize_WithValues_Passes()
        {
            var w = 2;
            var h = 2;
            var array2D = new Array2D<int>(w, h, 10, 20, 30, 40, 50);

            Assert.AreEqual(w * h, array2D.Count);
            Assert.AreEqual(w, array2D.Width);
            Assert.AreEqual(h, array2D.Height);

            AssertionUtils.AssertEnumerable(
                new (int value, int x, int y)[] {
                    (10, 0, 0),
                    (20, 1, 0),
                    (30, 0, 1),
                    (40, 1, 1),
                }
                , array2D.GetEnumerableWithIndexXY()
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="Array2D{T}.Resize(int x, int y, IEnumerable{T} values)"/>
        /// </summary>
        [Test, Order(ORDER_RESIZE), Description("Resize(int width, int height, IEnumerable<T> values)")]
        public void Resize_WithValues2_Passes()
        {
            var data = new List<int>() { 10, 20, 30, 40, 50 };
            var w = 2;
            var h = 2;
            var array2D = new Array2D<int>(w, h, data);

            Assert.AreEqual(w * h, array2D.Count);
            Assert.AreEqual(w, array2D.Width);
            Assert.AreEqual(h, array2D.Height);

            AssertionUtils.AssertEnumerable(
                new (int value, int x, int y)[] {
                    (data[0], 0, 0),
                    (data[1], 1, 0),
                    (data[2], 0, 1),
                    (data[3], 1, 1),
                }
                , array2D.GetEnumerableWithIndexXY()
                , ""
            );
        }
        #endregion

        #region Shift
        /// - Shift(int x, int y)
        /// <summary>
        /// <seealso cref="Array2D{T}.Resize(int x, int y, IEnumerable{T} values)"/>
        /// </summary>
        [Test, Order(ORDER_SHIFT), Description("Shift(int x, int y)")]
        public void Shift_Passes()
        {
            {//Shift(+, 0)
                var w = 3;
                var h = 3;
                var array2D = new Array2D<int>(w, h
                    , 1, 2, 3
                    , 2, 4, 6
                    , 3, 6, 9);

                array2D.Shift(1, 0);
                AssertionUtils.AssertEnumerable(
                    new int[]
                    {
                        0, 1, 2,
                        0, 2, 4,
                        0, 3, 6,
                    }
                    , array2D
                    , ""
                );
            }
            Logger.Log(Logger.Priority.High, () => "Success to Shift(+, 0)!!");

            {//Shift(-, 0)
                var w = 3;
                var h = 3;
                var array2D = new Array2D<int>(w, h
                    , 1, 2, 3
                    , 2, 4, 6
                    , 3, 6, 9);

                array2D.Shift(-1, 0);
                AssertionUtils.AssertEnumerable(
                    new int[]
                    {
                        2, 3, 0,
                        4, 6, 0,
                        6, 9, 0
                    }
                    , array2D
                    , ""
                );
            }
            Logger.Log(Logger.Priority.High, () => "Success to Shift(-, 0)!!");

            {//Shift(0, +)
                var w = 3;
                var h = 3;
                var array2D = new Array2D<int>(w, h
                    , 1, 2, 3
                    , 2, 4, 6
                    , 3, 6, 9);

                array2D.Shift(0, 1);
                AssertionUtils.AssertEnumerable(
                    new int[]
                    {
                        0, 0, 0,
                        1, 2, 3,
                        2, 4, 6,
                    }
                    , array2D
                    , ""
                );
            }
            Logger.Log(Logger.Priority.High, () => "Success to Shift(0, +)!!");

            {//Shift(0, -)
                var w = 3;
                var h = 3;
                var array2D = new Array2D<int>(w, h
                    , 1, 2, 3
                    , 2, 4, 6
                    , 3, 6, 9);

                array2D.Shift(0, -1);
                AssertionUtils.AssertEnumerable(
                    new int[]
                    {
                        2, 4, 6,
                        3, 6, 9,
                        0, 0, 0,
                    }
                    , array2D
                    , ""
                );
            }
            Logger.Log(Logger.Priority.High, () => "Success to Shift(0, -)!!");

            {//Shift(0, 0)
                var w = 3;
                var h = 3;
                var array2D = new Array2D<int>(w, h
                    , 1, 2, 3
                    , 2, 4, 6
                    , 3, 6, 9);

                array2D.Shift(0, 0);
                AssertionUtils.AssertEnumerable(
                    new int[]
                    {
                        1, 2, 3,
                        2, 4, 6,
                        3, 6, 9,
                    }
                    , array2D
                    , ""
                );
            }
            Logger.Log(Logger.Priority.High, () => "Success to Shift(0, 0)!!");

            {//Shift(2, -2)
                var w = 3;
                var h = 3;
                var array2D = new Array2D<int>(w, h
                    , 1, 2, 3
                    , 2, 4, 6
                    , 3, 6, 9);

                array2D.Shift(2, -2);
                AssertionUtils.AssertEnumerable(
                    new int[]
                    {
                        0, 0, 3,
                        0, 0, 0,
                        0, 0, 0,
                    }
                    , array2D
                    , ""
                );
            }
            Logger.Log(Logger.Priority.High, () => "Success to Shift(+2, -2)!!");

        }
        #endregion

        #region Copy
        /// <summary>
        /// <seealso cref="Array2D{T}.Copy()"/>
        /// </summary>
        [Test, Order(ORDER_COPY), Description("Copy()")]
        public void Copy_Passes()
        {
            var w = 2;
            var h = 2;
            var array2D = new Array2D<int>(w, h, 10, 20, 30, 40);

            var copy = array2D.Copy();
            Assert.AreEqual(copy.Width, array2D.Width);
            Assert.AreEqual(copy.Height, array2D.Height);

            AssertionUtils.AssertEnumerable(
                array2D
                , copy
                , ""
            );
        }
        #endregion
    }
}
