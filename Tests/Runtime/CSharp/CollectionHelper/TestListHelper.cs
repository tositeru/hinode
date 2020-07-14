using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Hinode.Tests.CSharp.CollectionHelper
{
    /// <summary>
	/// <seealso cref="ListHelper{T}"/>
	/// </summary>
    public class TestListHelper
    {
        #region ListHelper.Add

        /// <summary>
        /// <seealso cref="ListHelper.Items"/>
        /// <seealso cref="ListHelper.Add(T)"/>
        /// </summary>
        [Test]
        public void AddPasses()
        {
            var helper = new ListHelper<int>();

            var value = 100;
            helper.Add(value);
            Assert.IsTrue(helper.Contains(value));
            Assert.AreEqual(1, helper.Count);

            var value2 = 101;
            helper.Add(value2);
            Assert.IsTrue(helper.Contains(value2));
            Assert.AreEqual(2, helper.Count);
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.Add(T)"/>
		/// <seealso cref="ListHelper{T}.OnAdded"/>
		/// </summary>
        [Test]
        public void OnAddedInAddPasses()
        {
            var helper = new ListHelper<int>();
            var counter = 0;
            var recievedValue = 0;
            var recievedIndex = -1;
            helper.OnAdded.Add((v, index) => { recievedValue = v; recievedIndex = index;  counter++; });


            var value = 100;
            helper.Add(value);
            Assert.AreEqual(value, recievedValue);
            Assert.AreEqual(0, recievedIndex);
            Assert.AreEqual(1, counter);
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.Add(T)"/>
        /// <seealso cref="ListHelper{T}.OnAdded"/>
        /// </summary>
        [Test]
        public void OnAddedInAddWhenOccurExceptionPasses()
        {
            var helper = new ListHelper<int>();
            helper.OnAdded.Add((v, index) => {
                throw new System.Exception();
            });

            Assert.DoesNotThrow(() => {
                int value = 100;
                helper.Add(value);

                Assert.IsTrue(helper.Contains(value));
                Assert.AreEqual(1, helper.Count);
            });
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
        /// <seealso cref="ListHelper{T}.Add(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInAddPasses()
        {
            var helper = new ListHelper<int>();
            var callCounter = 0;
            var recievedData = (self: (IReadOnlyListHelper<int>)null, count: (int)0);
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
                recievedData.self = _helper;
                recievedData.count = _count;
            });

            helper.Add(1);
            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
        /// <seealso cref="ListHelper{T}.Add(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInAddWhenOccurExceptionPasses()
        {
            var helper = new ListHelper<int>();

            helper.OnChangedCount.Add((_helper, _count) => {
                throw new System.Exception();
            });

            helper.Add(1);
            AssertionUtils.AssertEnumerable(
                new int[] { 1 },
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
        /// <seealso cref="ListHelper{T}.Add(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInAddWhenOccurOnAddedExceptionPasses()
        {
            var helper = new ListHelper<int>();
            helper.OnAdded.Add((_, __) => throw new System.Exception());

            var callCounter = 0;
            var recievedData = (self: (IReadOnlyListHelper<int>)null, count: (int)0);
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
                recievedData.self = _helper;
                recievedData.count = _count;
            });

            helper.Add(1);
            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }
        #endregion

        #region ListHelper.Remove

        /// <summary>
        /// <seealso cref="ListHelper.Items"/>
        /// <seealso cref="ListHelper.Remove(T)"/>
        /// </summary>
        [Test]
        public void RemovePasses()
        {
            var helper = new ListHelper<int>();

            var testData = new int[] {
                100, 200
            };
            foreach (var d in testData)
            {
                helper.Add(d);
            }

            helper.Remove(testData[0]);
            AssertionUtils.AssertEnumerableByUnordered(
                new int[] { testData[1] }
                , helper.Items
                , ""
            );

            helper.Remove(testData[1]);
            AssertionUtils.AssertEnumerableByUnordered(
                new int[] { }
                , helper.Items
                , ""
            );
        }

        /// <summary>
		/// <seealso cref="ListHelper.Items"/>
		/// <seealso cref="ListHelper.Remove(T)"/>
		/// </summary>
        [Test]
        public void RemoveThatNotContainsPasses()
        {
            var helper = new ListHelper<int>();

            var testData = new int[] {
                100, 200
            };
            foreach (var d in testData)
            {
                helper.Add(d);
            }
            helper.Remove(testData[0]);

            Assert.DoesNotThrow(() => {
                helper.Remove(testData[0]);
                Assert.IsFalse(helper.Contains(testData[0]));
            });

            Assert.DoesNotThrow(() => {
                helper.Remove(-123);
                Assert.IsFalse(helper.Contains(-123));
            });
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.Remove(T)"/>
        /// <seealso cref="ListHelper{T}.OnRemoved"/>
        /// </summary>
        [Test]
        public void OnRemovedInRemovePasses()
        {
            var helper = new ListHelper<int>();
            var counter = 0;
            var recievedValue = 0;
            var recievedIndex = -1;
            helper.OnRemoved.Add((v, index) => { counter++; recievedValue = v; recievedIndex = index; });

            var testData = new int[] {
                100, 200
            };
            helper.Add(testData);

            Assert.AreEqual(0, counter);
            helper.Remove(testData[1]);
            Assert.AreEqual(1, counter);
            Assert.AreEqual(testData[1], recievedValue);
            Assert.AreEqual(1, recievedIndex);

            counter = 0;
            recievedValue = 0;
            recievedIndex = -1;
            helper.Remove(testData[0]);
            Assert.AreEqual(1, counter);
            Assert.AreEqual(testData[0], recievedValue);
            Assert.AreEqual(0, recievedIndex);
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.Remove(T)"/>
        /// <seealso cref="ListHelper{T}.OnRemoved"/>
        /// </summary>
        [Test]
        public void OnRemovedInRemoveWhenNotContainsPasses()
        {
            var helper = new ListHelper<int>();
            var counter = 0;
            var recievedValue = 0;
            var recievedIndex = -1;
            helper.OnRemoved.Add((v, index) => { counter++; recievedValue = v; recievedIndex = index; });

            Assert.AreEqual(0, counter);
            helper.Remove(123);
            Assert.AreEqual(0, counter);
            Assert.AreEqual(0, recievedValue);
            Assert.AreEqual(-1, recievedIndex);
        }

        /// <summary>
        /// <seealso cref="ListHelper.Remove(T)"/>
        /// <seealso cref="ListHelper{T}.OnRemoved"/>
        /// </summary>
        [Test]
        public void OnRemovedInRemoveWhenOccurExceptionPasses()
        {
            var helper = new ListHelper<int>();
            helper.OnRemoved.Add((v, index) => {
                throw new System.Exception();
            });

            var value = 100;
            helper.Add(value);
            Assert.DoesNotThrow(() => {
                helper.Remove(value);
            });
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
        /// <seealso cref="ListHelper{T}.Remove(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemovePasses()
        {
            var helper = new ListHelper<int>();
            var value = 1;
            helper.Add(value);

            var callCounter = 0;
            var recievedData = (self: (IReadOnlyListHelper<int>)null, count: (int)0);
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
                recievedData.self = _helper;
                recievedData.count = _count;
            });

            helper.Remove(value);
            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
        /// <seealso cref="ListHelper{T}.Remove(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveNotRemovePasses()
        {
            var helper = new ListHelper<int>();

            var callCounter = 0;
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
            });

            helper.Remove(1);
            Assert.AreEqual(0, callCounter);
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
        /// <seealso cref="ListHelper{T}.Remove(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveWhenOccurExceptionPasses()
        {
            var helper = new ListHelper<int>();
            var value = 100;
            helper.Add(value);

            helper.OnChangedCount.Add((_helper, _count) => {
                throw new System.Exception();
            });

            helper.Remove(value);
            AssertionUtils.AssertEnumerable(
                new int[] { },
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
        /// <seealso cref="ListHelper{T}.Remove(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveWhenOccurOnRemovedExceptionPasses()
        {
            var helper = new ListHelper<int>();
            var value = 100;
            helper.Add(value);
            helper.OnRemoved.Add((_, __) => throw new System.Exception());

            var callCounter = 0;
            var recievedData = (self: (IReadOnlyListHelper<int>)null, count: (int)0);
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
                recievedData.self = _helper;
                recievedData.count = _count;
            });

            helper.Remove(value);
            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }
        #endregion

        #region ListHelper.RemoveAt

        /// <summary>
        /// <seealso cref="ListHelper.Items"/>
        /// <seealso cref="ListHelper.RemoveAt(int index)"/>
        /// </summary>
        [Test]
        public void RemoveAtPasses()
        {
            var helper = new ListHelper<int>();

            var testData = new int[] {
                100, 200
            };
            helper.Add(testData);

            helper.RemoveAt(0);
            AssertionUtils.AssertEnumerableByUnordered(
                new int[] { testData[1] }
                , helper.Items
                , ""
            );
        }

        /// <summary>
		/// <seealso cref="ListHelper.Items"/>
		/// <seealso cref="ListHelper.RemoveAt(int)"/>
		/// </summary>
        [Test]
        public void RemoveAtThatOutOfRangePasses()
        {
            var helper = new ListHelper<int>();

            var testData = new int[] {
                100, 200
            };
            foreach (var d in testData)
            {
                helper.Add(d);
            }
            helper.RemoveAt(0);

            Assert.DoesNotThrow(() => {
                helper.RemoveAt(helper.Count);
            });

            Assert.DoesNotThrow(() => {
                helper.RemoveAt(-1);
            });
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.RemoveAt(int)"/>
        /// <seealso cref="ListHelper{T}.OnRemoved"/>
        /// </summary>
        [Test]
        public void OnRemovedInRemoveAtPasses()
        {
            var helper = new ListHelper<int>();
            var recievedList = new List<(int value, int index)>();
            helper.OnRemoved.Add((v, index) => { recievedList.Add((v, index)); });

            var testData = new int[] {
                100, 200
            };
            helper.Add(testData);

            helper.RemoveAt(1);
            AssertionUtils.AssertEnumerable(
                new (int value, int index)[]
                {
                    (200, 1)
                },
                recievedList,
                ""
            );

            recievedList.Clear();
            helper.RemoveAt(0);
            AssertionUtils.AssertEnumerable(
                new (int value, int index)[]
                {
                    (100, 0)
                },
                recievedList,
                ""
            );
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.RemoveAt(T)"/>
        /// <seealso cref="ListHelper{T}.OnRemoved"/>
        /// </summary>
        [Test]
        public void OnRemovedInRemoveAtWhenOutOfRangePasses()
        {
            var helper = new ListHelper<int>();
            var recievedList = new List<(int value, int index)>();
            helper.OnRemoved.Add((v, index) => { recievedList.Add((v, index)); });

            helper.RemoveAt(-1);
            Assert.IsFalse(recievedList.Any());

            helper.RemoveAt(helper.Count);
            Assert.IsFalse(recievedList.Any());
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
        /// <seealso cref="ListHelper{T}.RemoveAt(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveAtPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1, 2, 3);

            var callCounter = 0;
            var recievedData = (self: (IReadOnlyListHelper<int>)null, count: (int)0);
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
                recievedData.self = _helper;
                recievedData.count = _count;
            });

            helper.RemoveAt(0);
            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
        /// <seealso cref="ListHelper{T}.RemoveAt(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveAtNotRemovePasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1, 2, 3);
            var callCounter = 0;
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
            });

            helper.RemoveAt(-1);
            Assert.AreEqual(0, callCounter);

            helper.RemoveAt(helper.Count);
            Assert.AreEqual(0, callCounter);
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
        /// <seealso cref="ListHelper{T}.RemoveAt(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveAtWhenOccurExceptionPasses()
        {
            var helper = new ListHelper<int>();
            var value = 100;
            helper.Add(value);

            helper.OnChangedCount.Add((_helper, _count) => {
                throw new System.Exception();
            });

            helper.RemoveAt(0);
            AssertionUtils.AssertEnumerable(
                new int[] { },
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
        /// <seealso cref="ListHelper{T}.RemoveAt(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveAtWhenOccurOnRemovedExceptionPasses()
        {
            var helper = new ListHelper<int>();
            var value = 100;
            helper.Add(value);
            helper.OnRemoved.Add((_, __) => throw new System.Exception());

            var callCounter = 0;
            var recievedData = (self: (IReadOnlyListHelper<int>)null, count: (int)0);
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
                recievedData.self = _helper;
                recievedData.count = _count;
            });

            helper.RemoveAt(0);
            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }
        #endregion

        #region ListHelper.Clear

        /// <summary>
        /// <seealso cref="ListHelper{T}.Clear()"/>
        /// <seealso cref="ListHelper{T}.OnRemoved"/>
        /// </summary>
        [Test]
        public void OnRemovedInClearPasses()
        {
            var helper = new ListHelper<int>();
            var recievedList = new List<(int value, int index)>();
            helper.OnRemoved.Add((v, index) => { recievedList.Add((v, index)); });

            var testData = new int[] {
                100, 200
            };
            helper.Add(testData);

            helper.Clear();
            AssertionUtils.AssertEnumerable(
                Enumerable.Range(0, testData.Length)
                    .Zip(testData, (i, v) => (v, i))
                    .Reverse(),
                recievedList,
                "");
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.Clear()"/>
        /// <seealso cref="ListHelper{T}.OnRemoved"/>
        /// </summary>
        [Test]
        public void OnRemovedInClearWhenEmptyPasses()
        {
            var helper = new ListHelper<int>();
            var recievedList = new List<(int value, int index)>();
            helper.OnRemoved.Add((v, index) => { recievedList.Add((v, index)); });

            Assert.IsFalse(recievedList.Any());
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.Clear()"/>
		/// </summary>
        [Test]
        public void ClearPasses()
        {
            var helper = new ListHelper<int>();

            var testData = new int[] {
                0, 1, 2, 3, 4
            };
            foreach (var d in testData)
            {
                helper.Add(d);
            }

            helper.Clear();
            Assert.AreEqual(0, helper.Count);
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.Clear()"/>
		/// </summary>
        [Test]
        public void ClearWhenEmptyPasses()
        {
            var helper = new ListHelper<int>();

            Assert.DoesNotThrow(() => {
                helper.Clear();
            });
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.Clear()"/>
        /// <seealso cref="ListHelper{T}.OnCleared"/>
        /// </summary>
        [Test]
        public void OnClearedInClearPasses()
        {
            var helper = new ListHelper<int>();
            var counter = 0;
            helper.OnCleared.Add(() => counter++);

            var testData = new int[] {
                0, 1, 2, 3, 4
            };
            foreach (var d in testData)
            {
                helper.Add(d);
            }

            helper.Clear();
            Assert.AreEqual(1, counter);
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.Clear()"/>
        /// <seealso cref="ListHelper{T}.OnCleared"/>
        /// </summary>
        [Test]
        public void OnClearedInClearWhenEmptyPasses()
        {
            var helper = new ListHelper<int>();
            var counter = 0;
            helper.OnCleared.Add(() => counter++);

            helper.Clear();

            Assert.AreEqual(0, counter);
        }

        /// <summary>
        /// <seealso cref="ListHelper.Clear()"/>
        /// <seealso cref="ListHelper{T}.OnRemoved"/>
        /// <seealso cref="ListHelper{T}.OnCleared"/>
        /// </summary>
        [Test]
        public void ClearWhenOccurExceptionPasses()
        {
            var helper = new ListHelper<int>();
            helper.OnCleared.Add(() => throw new System.Exception());

            var testData = new int[] {
                0, 1, 2, 3, 4
            };
            foreach (var d in testData)
            {
                helper.Add(d);
            }

            Assert.DoesNotThrow(() => {
                helper.Clear();
                Assert.AreEqual(0, helper.Count);
            });
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
        /// <seealso cref="ListHelper{T}.Clear(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInClearPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1, 2, 3);

            var callCounter = 0;
            var recievedData = (self: (IReadOnlyListHelper<int>)null, count: (int)0);
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
                recievedData.self = _helper;
                recievedData.count = _count;
            });

            helper.Clear();
            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
        /// <seealso cref="ListHelper{T}.Clear(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInClearNotRemovePasses()
        {
            var helper = new ListHelper<int>();

            var callCounter = 0;
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
            });

            helper.Clear();
            Assert.AreEqual(0, callCounter);
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
        /// <seealso cref="ListHelper{T}.Clear(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInClearWhenOccurExceptionPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1, 2, 3);

            helper.OnChangedCount.Add((_helper, _count) => {
                throw new System.Exception();
            });

            helper.Clear();
            AssertionUtils.AssertEnumerable(
                new int[] { },
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
        /// <seealso cref="ListHelper{T}.Clear(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInClearWhenOccurOnRemovedExceptionPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1, 2, 3);
            helper.OnRemoved.Add((_, __) => throw new System.Exception());

            var callCounter = 0;
            var recievedData = (self: (IReadOnlyListHelper<int>)null, count: (int)0);
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
                recievedData.self = _helper;
                recievedData.count = _count;
            });

            helper.Clear();
            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }
        #endregion

        #region ListHelper.Contains

        /// <summary>
        /// <seealso cref="ListHelper.Items"/>
        /// <seealso cref="ListHelper.Contains(T)"/>
        /// </summary>
        [Test]
        public void ContainsPasses()
        {
            var helper = new ListHelper<int>();

            var value = 100;
            helper.Add(value);
            Assert.IsTrue(helper.Contains(value));

            helper.Remove(value);
            Assert.IsFalse(helper.Contains(value));

            helper.Add(value);
            Assert.IsTrue(helper.Contains(value));
        }

        /// <summary>
		/// <seealso cref="ListHelper.Items"/>
        /// <seealso cref="ListHelper.Contains(T)"/>
        /// </summary>
        [Test]
        public void ContainsWhenNotContainsPasses()
        {
            var helper = new ListHelper<int>();
            Assert.IsFalse(helper.Contains(100));
        }

        #endregion

        #region ListHelper.IsValidIndex

        /// <summary>
        /// <seealso cref="ListHelper{T}.IsValidIndex(int)"/>
        /// </summary>
        [Test]
        public void IsValidIndexPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(10, 20, 30);

            Assert.IsTrue(helper.IsValidIndex(0));
            Assert.IsTrue(helper.IsValidIndex(1));
            Assert.IsTrue(helper.IsValidIndex(2));

            Assert.IsFalse(helper.IsValidIndex(-1));
            Assert.IsFalse(helper.IsValidIndex(helper.Count));
        }

        #endregion

        #region ListHelper.Count

        /// <summary>
        /// <seealso cref="ListHelper{T}.Count"/>
        /// </summary>
        [Test]
        public void CountPasses()
        {
            var helper = new ListHelper<int>();
            var testData = Enumerable.Range(0, 100);
            foreach (var d in testData)
            {
                helper.Add(d);
            }

            Assert.AreEqual(testData.Count(), helper.Count);

            var c = helper.Count;
            foreach (var d in testData)
            {
                helper.Remove(d);
                c--;
                Assert.AreEqual(c, helper.Count);
            }
        }

        #endregion

        #region ListHelper{T}.Add Items

        /// <summary>
        /// <seealso cref="ListHelper{T}.Add(T[])"/>
        /// <seealso cref="ListHelper{T}.Add(IEnumerable{T})"/>
        /// </summary>
        [Test]
        public void AddItemsPasses()
        {
            var testData = Enumerable.Range(0, 10);

            var helper = new ListHelper<int>();
            helper.Add(testData);

            AssertionUtils.AssertEnumerableByUnordered(
                testData,
                helper.Items,
                ""
            );
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.OnAdded"/>
        /// <seealso cref="ListHelper{T}.Add(T[])"/>
        /// <seealso cref="ListHelper{T}.Add(IEnumerable{T})"/>
        /// </summary>
		[Test]
        public void OnAddedInAddItemsPasses()
        {
            var helper = new ListHelper<int>();
            var recievedList = new List<int>();
            var recievedIndex = new List<int>();
            var counter = 0;
            helper.OnAdded.Add((v, index) => { recievedList.Add(v); recievedIndex.Add(index); counter++; });


            var testData = Enumerable.Range(0, 10);
            helper.Add(testData);

            Assert.AreEqual(testData.Count(), counter);
            AssertionUtils.AssertEnumerableByUnordered(
                testData,
                recievedList,
                ""
            );
            AssertionUtils.AssertEnumerable(
                Enumerable.Range(0, testData.Count()),
                recievedIndex,
                ""
            );
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.Add(IEnumerable{T})"/>
		/// <seealso cref="ListHelper{T}.Add(T[])"/>
		/// </summary>
        [Test]
        public void AddItemsWhenOccurExceptionPasses()
        {
            var helper = new ListHelper<int>();
            var recievedList = new List<(int value, int index)>();
            var counter = 0;
            helper.OnAdded.Add((v, index) => {
                if (v % 2 == 0) throw new System.Exception();
                recievedList.Add((v, index)); counter++;
            });


            var testData = Enumerable.Range(0, 10);
            Assert.DoesNotThrow(() => {
                helper.Add(testData);
            });

            Assert.AreEqual(testData.Count() / 2, counter);
            AssertionUtils.AssertEnumerableByUnordered(
                testData.Where(_v => _v % 2 != 0).Select(_v => (_v, _v)),
                recievedList,
                ""
            );
            AssertionUtils.AssertEnumerableByUnordered(
                testData,
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
		/// <seealso cref="ListHelper{T}.Add(IEnumerable{T})"/>
		/// <seealso cref="ListHelper{T}.Add(T[])"/>
        /// </summary>
        [Test]
        public void OnChangedCountInAddItemsPasses()
        {
            var helper = new ListHelper<int>();

            var callCounter = 0;
            var recievedData = (self: (IReadOnlyListHelper<int>)null, count: (int)0);
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
                recievedData.self = _helper;
                recievedData.count = _count;
            });

            helper.Add(1, 2, 3);
            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
		/// <seealso cref="ListHelper{T}.Add(IEnumerable{T})"/>
		/// <seealso cref="ListHelper{T}.Add(T[])"/>
        /// </summary>
        [Test]
        public void OnChangedCountInAddItemsWhenOccurExceptionPasses()
        {
            var helper = new ListHelper<int>();

            helper.OnChangedCount.Add((_helper, _count) => {
                throw new System.Exception();
            });

            var testData = new int[] { 1, 2, 3};
            helper.Add(testData);
            AssertionUtils.AssertEnumerable(
                testData,
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
		/// <seealso cref="ListHelper{T}.Add(IEnumerable{T})"/>
		/// <seealso cref="ListHelper{T}.Add(T[])"/>
        /// </summary>
        [Test]
        public void OnChangedCountInAddItemsWhenOccurOnAddedExceptionPasses()
        {
            var helper = new ListHelper<int>();
            helper.OnAdded.Add((_, __) => throw new System.Exception());

            var callCounter = 0;
            var recievedData = (self: (IReadOnlyListHelper<int>)null, count: (int)0);
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
                recievedData.self = _helper;
                recievedData.count = _count;
            });

            helper.Add(1, 2, 3);
            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }
        #endregion

        #region ListHelper{T}.Remove Items

        /// <summary>
        /// <seealso cref="ListHelper{T}.Remove(T[])"/>
        /// <seealso cref="ListHelper{T}.Remove(IEnumerable{T})"/>
        /// </summary>
        [Test]
        public void RemoveItemsPasses()
        {
            var helper = new ListHelper<int>();
            var testData = Enumerable.Range(0, 10);
            helper.Add(testData);

            helper.Remove(testData);
            AssertionUtils.AssertEnumerableByUnordered(
                new int[] { },
                helper.Items,
                ""
            );
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.OnRemoved"/>
        /// <seealso cref="ListHelper{T}.Remove(T[])"/>
        /// <seealso cref="ListHelper{T}.Remove(IEnumerable{T})"/>
        /// </summary>
		[Test]
        public void OnRemovedInRemoveItemsPasses()
        {
            var helper = new ListHelper<int>();
            var recievedList = new List<(int value, int index)>();
            helper.OnRemoved.Add((v, index) => { recievedList.Add((v, index)); });

            var testData = Enumerable.Range(0, 10);
            helper.Add(testData);

            var removeList = testData.Where(_v => _v % 2 == 0);
            helper.Remove(removeList);

            AssertionUtils.AssertEnumerable(
                removeList.Select(_v => (_v, _v)).OrderByDescending(_v => _v),
                recievedList,
                ""
            );

            AssertionUtils.AssertEnumerableByUnordered(
                testData.Where(_v => _v % 2 != 0),
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.Remove(IEnumerable{T})"/>
        /// <seealso cref="ListHelper{T}.Remove(T[])"/>
        /// </summary>
        [Test]
        public void RemoveItemsWhenOccurExceptionPasses()
        {
            var helper = new ListHelper<int>();
            var recievedList = new List<(int value, int index)>();
            var counter = 0;
            helper.OnRemoved.Add((v, index) => {
                if (v % 2 == 0) throw new System.Exception();
                recievedList.Add((v, index)); counter++;
            });


            var testData = Enumerable.Range(0, 10);
            helper.Add(testData);
            Assert.DoesNotThrow(() => {
                helper.Remove(testData);
            });

            Assert.AreEqual(testData.Count() / 2, counter);
            AssertionUtils.AssertEnumerableByUnordered(
                testData.Where(_v => _v % 2 != 0).Select(_v => (_v, _v)),
                recievedList,
                ""
            );
            AssertionUtils.AssertEnumerableByUnordered(
                new int[] { },
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
        /// <seealso cref="ListHelper{T}.Remove(IEnumerable{T})"/>
        /// <seealso cref="ListHelper{T}.Remove(T[])"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveItemsPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1, 2, 3);

            var callCounter = 0;
            var recievedData = (self: (IReadOnlyListHelper<int>)null, count: (int)0);
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
                recievedData.self = _helper;
                recievedData.count = _count;
            });

            helper.Remove(2, 3);
            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
		/// <seealso cref="ListHelper{T}.Remove(IEnumerable{T})"/>
		/// <seealso cref="ListHelper{T}.Remove(T[])"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveItemsNotRemovePasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1, 2, 3);
            var callCounter = 0;
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
            });

            helper.Remove(100, 200, 300);
            Assert.AreEqual(0, callCounter);
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
		/// <seealso cref="ListHelper{T}.Remove(IEnumerable{T})"/>
		/// <seealso cref="ListHelper{T}.Remove(T[])"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveItemsWhenOccurExceptionPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1, 2, 3, 4);

            helper.OnChangedCount.Add((_helper, _count) => {
                throw new System.Exception();
            });

            helper.Remove(1, 3);
            AssertionUtils.AssertEnumerable(
                new int[] { 2, 4 },
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
		/// <seealso cref="ListHelper{T}.Remove(IEnumerable{T})"/>
		/// <seealso cref="ListHelper{T}.Remove(T[])"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveItemsWhenOccurOnRemovedExceptionPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1, 2, 3);

            helper.OnRemoved.Add((_, __) => throw new System.Exception());
            var callCounter = 0;
            var recievedData = (self: (IReadOnlyListHelper<int>)null, count: (int)0);
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
                recievedData.self = _helper;
                recievedData.count = _count;
            });

            helper.Remove(1, 2);
            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }
        #endregion

        #region ListHelper{T}.RemoveAt Items

        /// <summary>
        /// <seealso cref="ListHelper{T}.RemoveAt(T[])"/>
        /// <seealso cref="ListHelper{T}.RemoveAt(IEnumerable{T})"/>
        /// </summary>
        [Test]
        public void RemoveAtItemsPasses()
        {
            var helper = new ListHelper<int>();
            var testData = Enumerable.Range(0, 10);
            helper.Add(testData);

            helper.RemoveAt(Enumerable.Range(0, 5));
            AssertionUtils.AssertEnumerable(
                Enumerable.Range(5, 5),
                helper.Items,
                ""
            );
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.OnRemoved"/>
        /// <seealso cref="ListHelper{T}.RemoveAt(T[])"/>
        /// <seealso cref="ListHelper{T}.RemoveAt(IEnumerable{T})"/>
        /// </summary>
		[Test]
        public void OnRemovedInRemoveAtItemsPasses()
        {
            var helper = new ListHelper<int>();
            var recievedList = new List<(int value, int index)>();
            helper.OnRemoved.Add((v, index) => { recievedList.Add((v, index)); });

            var testData = Enumerable.Range(0, 10);
            helper.Add(testData);

            var removeIndecies = Enumerable.Range(0, 10).Where(_i => _i % 2 == 0);
            helper.RemoveAt(removeIndecies);

            AssertionUtils.AssertEnumerable(
                new (int value, int index)[]
                {
                    (8, 8),
                    (6, 6),
                    (4, 4),
                    (2, 2),
                    (0, 0),
                },
                recievedList,
                ""
            );
            AssertionUtils.AssertEnumerableByUnordered(
                testData.Where(_v => _v % 2 != 0),
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.RemoveAt(IEnumerable{T})"/>
        /// <seealso cref="ListHelper{T}.RemoveAt(T[])"/>
        /// </summary>
        [Test]
        public void RemoveAtItemsWhenOccurExceptionPasses()
        {
            var helper = new ListHelper<int>();
            var recievedList = new List<(int value, int index)>();
            helper.OnRemoved.Add((v, index) => {
                if (v % 2 == 0) throw new System.Exception();
                recievedList.Add((v, index));
            });


            var testData = Enumerable.Range(0, 10);
            helper.Add(testData);
            Assert.DoesNotThrow(() => {
                helper.RemoveAt(Enumerable.Range(0, testData.Count()));
            });

            AssertionUtils.AssertEnumerable(
                testData.Where(_v => _v % 2 != 0).Select(_v => (_v, _v))
                    .Reverse(),
                recievedList,
                ""
            );
            AssertionUtils.AssertEnumerableByUnordered(
                new int[] { },
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
        /// <seealso cref="ListHelper{T}.RemoveAt(IEnumerable{T})"/>
        /// <seealso cref="ListHelper{T}.RemoveAt(T[])"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveAtItemsPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1, 2, 3);

            var callCounter = 0;
            var recievedData = (self: (IReadOnlyListHelper<int>)null, count: (int)0);
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
                recievedData.self = _helper;
                recievedData.count = _count;
            });

            helper.RemoveAt(1, 2);
            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
		/// <seealso cref="ListHelper{T}.RemoveAt(IEnumerable{T})"/>
		/// <seealso cref="ListHelper{T}.RemoveAt(T[])"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveAtItemsNotRemovePasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1, 2, 3);
            var callCounter = 0;
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
            });

            helper.RemoveAt(-1, 100, helper.Count);
            Assert.AreEqual(0, callCounter);
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
		/// <seealso cref="ListHelper{T}.RemoveAt(IEnumerable{T})"/>
		/// <seealso cref="ListHelper{T}.RemoveAt(T[])"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveAtItemsWhenOccurExceptionPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1, 2, 3, 4);

            helper.OnChangedCount.Add((_helper, _count) => {
                throw new System.Exception();
            });

            helper.RemoveAt(1, 2);
            AssertionUtils.AssertEnumerable(
                new int[] { 1, 4 },
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
		/// <seealso cref="ListHelper{T}.RemoveAt(IEnumerable{T})"/>
		/// <seealso cref="ListHelper{T}.RemoveAt(T[])"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveAtItemsWhenOccurOnRemovedExceptionPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1, 2, 3);

            helper.OnRemoved.Add((_, __) => throw new System.Exception());
            var callCounter = 0;
            var recievedData = (self: (IReadOnlyListHelper<int>)null, count: (int)0);
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
                recievedData.self = _helper;
                recievedData.count = _count;
            });

            helper.RemoveAt(1, 2);
            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }
        #endregion

        #region ListHelper.Resize

        /// <summary>
        /// <seealso cref="ListHelper{T}.Resize(int)"/>
        /// </summary>
        [Test]
        public void ResizePasses()
        {
            var helper = new ListHelper<int>();

            helper.Resize(5);
            Assert.AreEqual(5, helper.Count);
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.Resize(int)"/>
		/// </summary>
        [Test]
        public void ResizeWhenInvalidArgumentPasses()
        {
            var helper = new ListHelper<int>();
            helper.Resize(4);

            helper.Resize(-1);
            Assert.AreEqual(0, helper.Count);
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.Resize(int)"/>
		/// <seealso cref="ListHelper{T}.OnRemoved"/>
		/// </summary>
        [Test]
        public void OnRemovedInResizePasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1, 2, 3, 4, 5);

            var recievedList = new List<(int value, int index)>();
            helper.OnRemoved.Add((item, index) => { recievedList.Add((item, index)); } );
            helper.Resize(2);

            AssertionUtils.AssertEnumerable(
                new int[] { 1, 2 },
                helper.Items,
                ""
            );

            AssertionUtils.AssertEnumerable(
                new (int value, int index)[] {
                    (5, 4),
                    (4, 3),
                    (3, 2),
                },
                recievedList,
                ""
            );
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.Resize(int)"/>
		/// <seealso cref="ListHelper{T}.OnRemoved"/>
		/// </summary>
        [Test]
        public void OnRemovedInResizeToOccurExceptionPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1, 2, 3, 4, 5);

            helper.OnRemoved.Add((item, index) => {
                throw new System.Exception();
            });
            helper.Resize(2);

            AssertionUtils.AssertEnumerable(
                new int[] { 1, 2 },
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
		/// <seealso cref="ListHelper{T}.Resize(int)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInResizePasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1, 2, 3);

            var callCounter = 0;
            var recievedData = (self: (IReadOnlyListHelper<int>)null, count: (int)0);
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
                recievedData.self = _helper;
                recievedData.count = _count;
            });

            helper.Resize(1);
            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
		/// <seealso cref="ListHelper{T}.Resize(int)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInResizeNotChangedCountPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1, 2, 3);
            var callCounter = 0;
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
            });

            helper.Resize(helper.Count);
            Assert.AreEqual(0, callCounter);
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
		/// <seealso cref="ListHelper{T}.Resize(int)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInResizeWhenOccurExceptionPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1, 2, 3, 4);

            helper.OnChangedCount.Add((_helper, _count) => {
                throw new System.Exception();
            });

            helper.Resize(2);
            AssertionUtils.AssertEnumerable(
                new int[] { 1, 2 },
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
		/// <seealso cref="ListHelper{T}.Resize(int)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInResizeWhenOccurOnRemovedExceptionPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1, 2, 3);

            helper.OnRemoved.Add((_, __) => throw new System.Exception());
            var callCounter = 0;
            var recievedData = (self: (IReadOnlyListHelper<int>)null, count: (int)0);
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
                recievedData.self = _helper;
                recievedData.count = _count;
            });

            helper.Resize(1);
            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }
        #endregion

        #region ListHelper.this[]

        /// <summary>
        /// <seealso cref="ListHelper{T}.this[int]"/>
        /// </summary>
        [Test]
        public void IndexAccessorGetPasses()
        {
            var helper = new ListHelper<int>();
            var testData = new int[] { 100, 200, 300 };
            helper.Add(testData);
            for(var i=0; i< testData.Length; ++i)
            {
                Assert.AreEqual(testData[i], helper[i]);
            }
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.this[int]"/>
		/// </summary>
        [Test]
        public void IndexAccessorGetWhenInvalidIndexPasses()
        {
            var helper = new ListHelper<int>();
            var testData = new int[] { 100, 200, 300 };
            helper.Add(testData);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => {
                var value = helper[-1];
            });

            Assert.Throws<System.ArgumentOutOfRangeException>(() => {
                var value = helper[helper.Count];
            });
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.this[int]"/>
		/// </summary>
        [Test]
        public void IndexAccessorSetPasses()
        {
            var helper = new ListHelper<int>();
            helper.Resize(3);

            helper[1] = 100;
            Assert.AreEqual(100, helper[1]);

            helper[2] = 200;
            Assert.AreEqual(200, helper[2]);
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.this[int]"/>
		/// </summary>
        [Test]
        public void IndexAccessorSetWhenInvalidIndexPasses()
        {
            var helper = new ListHelper<int>();
            Assert.Throws<System.ArgumentOutOfRangeException>(() => helper[helper.Count] = 100);
            Assert.Throws<System.ArgumentOutOfRangeException>(() => helper[-1] = 234);
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.this[int]"/>
		/// <seealso cref="ListHelper{T}.OnRemoved"/>
		/// </summary>
        [Test]
        public void OnRemovedInIndexAccessorSetPasses()
        {
            var helper = new ListHelper<int>();
            var prevValue = 1;
            helper.Add(prevValue);

            var counter = 0;
            var recievedIndex = -1;
            var recievedValue = 0;
            helper.OnRemoved.Add((v, index) => {
                counter++;
                recievedValue = v;
                recievedIndex = index;
            });
            helper[0] = 200;

            Assert.AreEqual(1, counter);
            Assert.AreEqual(0, recievedIndex);
            Assert.AreEqual(prevValue, recievedValue);
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.this[int]"/>
		/// <seealso cref="ListHelper{T}.OnRemoved"/>
		/// </summary>
        [Test]
        public void IndexAccessorSetWhenOccurExceptionInRemovePrevElementPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1);

            helper.OnRemoved.Add((v, index) => {
                throw new System.Exception();
            });

            var newValue = 200;
            Assert.DoesNotThrow(() => helper[0] = newValue);

            Assert.AreEqual(newValue, helper[0]);
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.this[int]"/>
		/// <seealso cref="ListHelper{T}.OnAdded"/>
		/// </summary>
        [Test]
        public void OnAddedInIndexAccessorSetPasses()
        {
            var helper = new ListHelper<int>();
            var prevValue = 2;
            helper.Add(prevValue);

            var counter = 0;
            var recievedIndex = -1;
            var recievedValue = 0;
            helper.OnAdded.Add((v, index) => {
                counter++;
                recievedValue = v;
                recievedIndex = index;
            });
            var newValue = 200;
            helper[0] = newValue;

            Assert.AreEqual(1, counter);
            Assert.AreEqual(0, recievedIndex);
            Assert.AreEqual(newValue, recievedValue);
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.this[int]"/>
		/// <seealso cref="ListHelper{T}.OnAdded"/>
		/// </summary>
        [Test]
        public void IndexAccessorSetWhenOccurExceptionInAddNewElementPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1);

            helper.OnAdded.Add((v, index) => {
                throw new System.Exception();
            });

            var newValue = 200;
            Assert.DoesNotThrow(() => helper[0] = newValue);

            Assert.AreEqual(newValue, helper[0]);
        }
        #endregion

        #region ListHelper.MoveTo

        /// <summary>
        /// <seealso cref="ListHelper{T}.MoveTo(int, int)"/>
        /// </summary>
        [Test]
        public void MoveToPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(10, 20, 30);

            helper.MoveTo(1, 2);

            AssertionUtils.AssertEnumerable(
                new int[] { 10, default(int), 20 },
                helper.Items,
                ""
            );
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.MoveTo(int, int)"/>
		/// </summary>
        [Test]
        public void MoveToWhenInvalidFromIndexFails()
        {
            var helper = new ListHelper<int>();
            var testData = new int[] { 10, 20, 30 };
            helper.Add(testData);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => {
                helper.MoveTo(-1, 2);
            });
            AssertionUtils.AssertEnumerable(
                testData,
                helper.Items,
                ""
            );

            Assert.Throws<System.ArgumentOutOfRangeException>(() => {
                helper.MoveTo(helper.Count, 2);
            });
            AssertionUtils.AssertEnumerable(
                testData,
                helper.Items,
                ""
            );
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.MoveTo(int, int)"/>
		/// </summary>
        [Test]
        public void MoveToWhenInvalidToIndexFails()
        {
            var helper = new ListHelper<int>();
            var testData = new int[] { 10, 20, 30 };
            helper.Add(10, 20, 30);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => {
                helper.MoveTo(2, -1);
            });
            AssertionUtils.AssertEnumerable(
                testData,
                helper.Items,
                ""
            );

            Assert.Throws<System.ArgumentOutOfRangeException>(() => {
                helper.MoveTo(2, helper.Count);
            });
            AssertionUtils.AssertEnumerable(
                testData,
                helper.Items,
                ""
            );
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.MoveTo(int, int)"/>
		/// <seealso cref="ListHelper{T}.OnMoved"/>
		/// </summary>
        [Test]
        public void OnMovedInMoveToPasses()
        {
            var helper = new ListHelper<int>();
            var targetValue = 20;
            helper.Add(10, targetValue, 30);

            var counter = 0;
            var recievedValue = 0;
            var recievedIndex = -1;
            helper.OnMoved.Add((v, newIndex) => {
                counter++;
                recievedValue = v;
                recievedIndex = newIndex;
            });
            helper.MoveTo(1, 2);

            AssertionUtils.AssertEnumerable(
                new int[] { 10, default(int), targetValue },
                helper.Items,
                ""
            );
            Assert.AreEqual(1, counter);
            Assert.AreEqual(2, recievedIndex);
            Assert.AreEqual(targetValue, recievedValue);
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.MoveTo(int, int)"/>
		/// <seealso cref="ListHelper{T}.OnMoved"/>
		/// </summary>
        [Test]
        public void OnMovedInMoveToWhenOccurExceptionPasses()
        {
            var helper = new ListHelper<int>();
            var targetValue = 20;
            helper.Add(10, targetValue, 30);

            helper.OnMoved.Add((v, newIndex) => {
                throw new System.Exception();
            });

            Assert.DoesNotThrow(() => {
                helper.MoveTo(1, 2);
            });

            AssertionUtils.AssertEnumerable(
                new int[] { 10, default(int), targetValue },
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.MoveTo(int, int)"/>
        /// <seealso cref="ListHelper{T}.OnRemoved"/>
        /// </summary>
        [Test]
        public void OnRemoveInMoveToPasses()
        {
            var helper = new ListHelper<int>();
            var targetValue = 20;
            var removeValue = 30;
            helper.Add(10, targetValue, removeValue);

            var counter = 0;
            var recievedValue = 0;
            var recievedIndex = -1;
            helper.OnRemoved.Add((v, newIndex) => {
                counter++;
                recievedValue = v;
                recievedIndex = newIndex;
            });

            Assert.DoesNotThrow(() => {
                helper.MoveTo(1, 2);
            });

            AssertionUtils.AssertEnumerable(
                new int[] { 10, default(int), targetValue },
                helper.Items,
                ""
            );
            Assert.AreEqual(1, counter);
            Assert.AreEqual(2, recievedIndex);
            Assert.AreEqual(removeValue, recievedValue);
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.MoveTo(int, int)"/>
        /// <seealso cref="ListHelper{T}.OnRemoved"/>
        /// </summary>
        [Test]
        public void OnRemoveInMoveToWhenOccurExceptionPasses()
        {
            var helper = new ListHelper<int>();
            var targetValue = 20;
            helper.Add(10, targetValue, 30);

            helper.OnRemoved.Add((v, index) => {
                throw new System.Exception();
            });

            Assert.DoesNotThrow(() => {
                helper.MoveTo(1, 2);
            });

            AssertionUtils.AssertEnumerable(
                new int[] { 10, default(int), targetValue },
                helper.Items,
                ""
            );
        }
        #endregion

        #region ListHelper.Swap

        /// <summary>
        /// <seealso cref="ListHelper{T}.Swap(int, int)"/>
        /// </summary>
        [Test]
        public void SwapPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(10, 20, 30);

            helper.Swap(2, 1);

            AssertionUtils.AssertEnumerable(
                new int[] { 10, 30, 20 },
                helper.Items,
                ""
            );
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.Swap(int, int)"/>
		/// </summary>
        [Test]
        public void SwapWhenInvalidSrcIndexFails()
        {
            var helper = new ListHelper<int>();
            var testData = new int[] { 10, 20, 30 };
            helper.Add(testData);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => helper.Swap(-1, 1));
            AssertionUtils.AssertEnumerable(
                testData,
                helper.Items,
                ""
            );

            Assert.Throws<System.ArgumentOutOfRangeException>(() => helper.Swap(helper.Count, 1));
            AssertionUtils.AssertEnumerable(
                testData,
                helper.Items,
                ""
            );
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.Swap(int, int)"/>
		/// </summary>
        [Test]
        public void SwapWhenInvalidDestIndexFails()
        {
            var helper = new ListHelper<int>();
            var testData = new int[] { 10, 20, 30 };
            helper.Add(testData);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => helper.Swap(1, -1));
            AssertionUtils.AssertEnumerable(
                testData,
                helper.Items,
                ""
            );

            Assert.Throws<System.ArgumentOutOfRangeException>(() => helper.Swap(1, helper.Count));
            AssertionUtils.AssertEnumerable(
                testData,
                helper.Items,
                ""
            );
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.Swap(int, int)"/>
		/// <seealso cref="ListHelper{T}.OnMoved"/>
		/// </summary>
        [Test]
        public void OnMovedInSwapPasses()
        {
            var helper = new ListHelper<int>();
            var testData = new int[] { 10, 20, 30 };
            helper.Add(testData);

            var recievedList = new List<(int value, int newIndex)>();
            helper.OnMoved.Add((v, newIndex) => recievedList.Add((v, newIndex)));

            helper.Swap(2, 1);
            AssertionUtils.AssertEnumerableByUnordered(
                new (int value, int newIndex)[] {
                    (testData[2], 1),
                    (testData[1], 2),
                },
                recievedList,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.Swap(int, int)"/>
        /// <seealso cref="ListHelper{T}.OnMoved"/>
        /// </summary>
        [Test]
        public void OnMovedInSwapWhenOccurExceptionPasses()
        {
            var helper = new ListHelper<int>();
            var testData = new int[] { 10, 20, 30 };
            helper.Add(testData);

            helper.OnMoved.Add((v, newIndex) => throw new System.ArgumentOutOfRangeException());

            Assert.DoesNotThrow(() => helper.Swap(2, 1));
            AssertionUtils.AssertEnumerable(
                new int[] { 10, 30, 20 },
                helper.Items,
                ""
            );
        }

        #endregion

        #region ListHelper.Sort

        /// <summary>
        /// <seealso cref="ListHelper{T}.Sort()"/>
        /// </summary>
        [Test]
        public void SortPasses()
        {
            var helper = new ListHelper<int>();
            var testData = new int[] { 100, 20, 30, 3, 43 };
            helper.Add(testData);

            helper.Sort();
            AssertionUtils.AssertEnumerable(
                testData.OrderBy(_v => _v),
                helper.Items,
                ""
            );
        }

        /// <summary>
		/// 
		/// </summary>
        class SortComparer : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                var isThreeX = (x % 10 == 3);
                var isThreeY = (y % 10 == 3);
                if(isThreeX && isThreeY)
                {
                    return x.CompareTo(y);
                }
                else if(isThreeX)
                {
                    return -1;
                }
                else if(isThreeY)
                {
                    return 1;
                }
                else
                {
                    return x.CompareTo(y);
                }
            }
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.Sort(IComparer{T})"/>
        /// </summary>
        [Test]
        public void SortWithComparerPasses()
        {
            var helper = new ListHelper<int>();
            var testData = new int[] { 200, 20, 30, 3, 43 };
            helper.Add(testData);

            helper.Sort(new SortComparer());
            AssertionUtils.AssertEnumerable(
                new int[] { 3, 43, 20, 30, 200 },
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.Sort(IComparer{T})"/>
		/// <seealso cref="ListHelper{T}.OnMoved"/>
		/// </summary>
        [Test]
        public void OnMovedInSortPasses()
        {
            var helper = new ListHelper<int>();
            var testData = new int[] { 100, 20, 30 };
            helper.Add(testData);

            var recievedList = new List<(int value, int newIndex)>();
            helper.OnMoved.Add((v, newIndex) => recievedList.Add((v, newIndex)));

            helper.Sort();

            AssertionUtils.AssertEnumerableByUnordered(
                new (int value, int newIndex)[] {
                    (100, 2),
                    (20, 0),
                    (30, 1),
                },
                recievedList,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.Sort(IComparer{T})"/>
		/// <seealso cref="ListHelper{T}.OnMoved"/>
		/// </summary>
        [Test]
        public void OnMovedInSortWhenOccurExceptionPasses()
        {
            var helper = new ListHelper<int>();
            var testData = new int[] { 100, 20, 30 };
            helper.Add(testData);

            helper.OnMoved.Add((v, newIndex) => throw new System.Exception());

            Assert.DoesNotThrow(() => helper.Sort());

            AssertionUtils.AssertEnumerable(
                new int[] { 20, 30, 100 },
                helper.Items,
                ""
            );
        }

        #endregion

        #region IEnumerable

        /// <summary>
        /// <seealso cref="ListHelper{T}.GetEnumerator()"/>
        /// </summary>
        [Test]
        public void GetEnumeratorPasses()
        {
            var helper = new ListHelper<int>();
            var testData = new int[] { 10, 20, 40 };
            helper.Add(testData);

            AssertionUtils.AssertEnumerable(
                testData,
                helper,
                ""
            );
        }

        #endregion

        #region ListHelper.IndexOf LastIndexOf

        /// <summary>
        /// <seealso cref="ListHelper{T}.IndexOf(T, int, int)"/>
        /// </summary>
        [Test]
        public void IndexOfPasses()
        {
            var helper = new ListHelper<int>(100, 200, 300, 200, 100);
            Assert.AreEqual(1, helper.IndexOf(200));
            Assert.AreEqual(3, helper.IndexOf(200, 2));
            Assert.AreEqual(2, helper.IndexOf(300));
            Assert.AreEqual(4, helper.IndexOf(100, 1));
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.IndexOf(T, int, int)"/>
        /// </summary>
        [Test]
        public void IndexOfWithCountPasses()
        {
            var helper = new ListHelper<int>(100, 200, 300, 200, 100);
            Assert.AreEqual(1, helper.IndexOf(200, 0, 2));
            Assert.AreEqual(-1, helper.IndexOf(200, 0, 1));
            Assert.AreEqual(3, helper.IndexOf(200, 2, 2));
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.LastIndexOf(T, int, int)"/>
        /// </summary>
        [Test]
        public void LastIndexOfPasses()
        {
            var helper = new ListHelper<int>(100, 200, 300, 200, 100);
            Assert.AreEqual(3, helper.LastIndexOf(200));
            Assert.AreEqual(1, helper.LastIndexOf(200, 2));
            Assert.AreEqual(2, helper.LastIndexOf(300));
            Assert.AreEqual(4, helper.LastIndexOf(100));
            Assert.AreEqual(0, helper.LastIndexOf(100, 1));
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.LastIndexOf(T, int, int)"/>
        /// </summary>
        [Test]
        public void LastIndexOfWithCountPasses()
        {
            var helper = new ListHelper<int>(100, 200, 300, 200, 100);
            Assert.AreEqual(3, helper.LastIndexOf(200, 0, 2));
            Assert.AreEqual(-1, helper.LastIndexOf(200, 0, 1));
            Assert.AreEqual(1, helper.LastIndexOf(200, 2, 2));
            Assert.AreEqual(-1, helper.LastIndexOf(200, 2, 1));
        }

        #endregion

        #region ListHelper.FindIndex FindLastIndex

        /// <summary>
        /// <seealso cref="ListHelper{T}.FindIndex(System.Predicate{T}, int, int)"/>
        /// </summary>
        [Test]
        public void FindIndexPasses()
        {
            var helper = new ListHelper<int>(100, 200, 300, 200, 100);
            Assert.AreEqual(1, helper.FindIndex(v => v == 200));
            Assert.AreEqual(3, helper.FindIndex(v => v == 200, 2));

            Assert.AreEqual(4, helper.FindIndex(v => v == 100, 2));
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.FindIndex(System.Predicate{T}, int, int)"/>
        /// </summary>
        [Test]
        public void FindIndexWithCountPasses()
        {
            var helper = new ListHelper<int>(100, 200, 300, 200, 100);
            Assert.AreEqual(1, helper.FindIndex(v => v == 200, 0, 2));
            Assert.AreEqual(-1, helper.FindIndex(v => v == 200, 0, 1));

            Assert.AreEqual(3, helper.FindIndex(v => v == 200, 2, 2));
            Assert.AreEqual(-1, helper.FindIndex(v => v == 200, 2, 1));
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.FindLastIndex(System.Predicate{T}, int, int)"/>
        /// </summary>
        [Test]
        public void FindLastIndexPasses()
        {
            var helper = new ListHelper<int>(100, 200, 300, 200, 100);
            Assert.AreEqual(3, helper.FindLastIndex(v => v == 200));
            Assert.AreEqual(1, helper.FindLastIndex(v => v == 200, 2));

            Assert.AreEqual(4, helper.FindLastIndex(v => v == 100));
            Assert.AreEqual(0, helper.FindLastIndex(v => v == 100, 2));
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.FindLastIndex(System.Predicate{T}, int, int)"/>
        /// </summary>
        [Test]
        public void FindLastIndexWithCountPasses()
        {
            var helper = new ListHelper<int>(100, 200, 300, 200, 100);
            Assert.AreEqual(3, helper.FindLastIndex(v => v == 200, 0, 2));
            Assert.AreEqual(-1, helper.FindLastIndex(v => v == 200, 0, 1));

            Assert.AreEqual(1, helper.FindLastIndex(v => v == 200, 2, 2));
            Assert.AreEqual(-1, helper.FindLastIndex(v => v == 200, 2, 1));
        }

        #endregion

        #region ListHelper.Insert

        /// <summary>
        /// <seealso cref="ListHelper{T}.InsertTo(int, T[])"/>
        /// <seealso cref="ListHelper{T}.InsertTo(int, IEnumerable{T})"
        /// </summary>
        [Test]
        public void InsertToPasses()
        {
            {
                var helper = new ListHelper<int>();
                helper.Add(10, 20, 30);
                helper.InsertTo(0, 100);
                AssertionUtils.AssertEnumerable(
                    new int[] { 100, 10, 20, 30 },
                    helper.Items,
                    ""
                );
            }

            {
                var helper = new ListHelper<int>();
                helper.Add(10, 20, 30);
                helper.InsertTo(1, 100);
                AssertionUtils.AssertEnumerable(
                    new int[] { 10, 100, 20, 30 },
                    helper.Items,
                    ""
                );
            }
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.InsertTo(int, T[])"/>
        /// <seealso cref="ListHelper{T}.InsertTo(int, IEnumerable{T})"
        /// </summary>
        [Test]
        public void InsertToItemsPasses()
        {
            {
                var helper = new ListHelper<int>();
                helper.Add(10, 20, 30);
                helper.InsertTo(0, 100, 200, 300);
                AssertionUtils.AssertEnumerable(
                    new int[] { 100, 200, 300, 10, 20, 30 },
                    helper.Items,
                    ""
                );
            }

            {
                var helper = new ListHelper<int>();
                helper.Add(10, 20, 30);
                helper.InsertTo(2, 100, 200);
                AssertionUtils.AssertEnumerable(
                    new int[] { 10, 20, 100, 200, 30 },
                    helper.Items,
                    ""
                );
            }
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.InsertTo(int, T[])"/>
		/// <seealso cref="ListHelper{T}.InsertTo(int, IEnumerable{T})"
        /// </summary>
        [Test]
        public void InsertToTailPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(10, 20, 30);
            helper.InsertTo(helper.Count, 100);
            AssertionUtils.AssertEnumerable(
                new int[] { 10, 20, 30, 100 },
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.InsertTo(int, T[])"/>
        /// <seealso cref="ListHelper{T}.InsertTo(int, IEnumerable{T})"
        /// </summary>
        [Test]
        public void InsertToTailFails()
        {
            var helper = new ListHelper<int>();
            helper.Add(10, 20, 30);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => { helper.InsertTo(helper.Count+1, 100); });
            Assert.Throws<System.ArgumentOutOfRangeException>(() => { helper.InsertTo(100000, 100); });            
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.OnAdded"/>
        /// <seealso cref="ListHelper{T}.InsertTo(int, T[])"/>
        /// <seealso cref="ListHelper{T}.InsertTo(int, IEnumerable{T})"
        /// </summary>
        [Test]
        public void OnAddedInInsertToItemsPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(10, 20, 30);
            var recievedList = new List<(int value, int index)>();
            helper.OnAdded.Add((v, index) => { recievedList.Add((v, index)); });

            helper.InsertTo(0, 100, 200);

            AssertionUtils.AssertEnumerable(
                new (int value, int index)[] {
                    (100, 0),
                    (200, 1),
                },
                recievedList,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnAdded"/>
        /// <seealso cref="ListHelper{T}.InsertTo(int, T[])"/>
        /// <seealso cref="ListHelper{T}.InsertTo(int, IEnumerable{T})"
        /// </summary>
        [Test]
        public void OnAddedInInsertToItemsWhenOccurExceptionPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(10, 20, 30);
            helper.OnAdded.Add((v, index) => { throw new System.Exception(); });

            helper.InsertTo(0, 100, 200);

            AssertionUtils.AssertEnumerable(
                new int[] { 100, 200, 10, 20, 30 },
                helper.Items,
                ""
            );
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.OnMoved"/>
        /// <seealso cref="ListHelper{T}.InsertTo(int, T[])"/>
        /// <seealso cref="ListHelper{T}.InsertTo(int, IEnumerable{T})"
        /// </summary>
        [Test]
        public void OnMovedWhenInsertToItemsPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(10, 20, 30);
            var recievedList = new List<(int value, int index)>();
            helper.OnMoved.Add((v, index) => { recievedList.Add((v, index)); });

            helper.InsertTo(0, 100, 200);

            AssertionUtils.AssertEnumerable(
                new (int value, int index)[] {
                    (30, 4),
                    (20, 3),
                    (10, 2),
                },
                recievedList,
                ""
            );
        }

        /// <summary>
		/// <seealso cref="ListHelper{T}.OnMoved"/>
        /// <seealso cref="ListHelper{T}.InsertTo(int, T[])"/>
        /// <seealso cref="ListHelper{T}.InsertTo(int, IEnumerable{T})"
        /// </summary>
        [Test]
        public void OnMovedInInsertToItemsWhenOccurExceptionPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(10, 20, 30);
            helper.OnMoved.Add((v, index) => { throw new System.Exception(); });

            helper.InsertTo(0, 100, 200);

            AssertionUtils.AssertEnumerable(
                new int[] { 100, 200, 10, 20, 30 },
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
        /// <seealso cref="ListHelper{T}.InsertTo(int, T[])"/>
        /// <seealso cref="ListHelper{T}.InsertTo(int, IEnumerable{T})"
        /// </summary>
        [Test]
        public void OnChangedCountInInsertToPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1, 2, 3);

            var callCounter = 0;
            var recievedData = (self: (IReadOnlyListHelper<int>)null, count: (int)0);
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
                recievedData.self = _helper;
                recievedData.count = _count;
            });

            helper.InsertTo(1, 100);
            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
        /// <seealso cref="ListHelper{T}.InsertTo(int, T[])"/>
        /// <seealso cref="ListHelper{T}.InsertTo(int, IEnumerable{T})"
        /// </summary>
        [Test]
        public void OnChangedCountInInsertToNotChangedCountPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(100, 200, 300);
            var callCounter = 0;
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
            });

            helper.InsertTo(2);
            Assert.AreEqual(0, callCounter);
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
        /// <seealso cref="ListHelper{T}.InsertTo(int, T[])"/>
        /// <seealso cref="ListHelper{T}.InsertTo(int, IEnumerable{T})"
        /// </summary>
        [Test]
        public void OnChangedCountInInsertToWhenOccurExceptionPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1, 2, 3, 4);

            helper.OnChangedCount.Add((_helper, _count) => {
                throw new System.Exception();
            });

            helper.InsertTo(2, 10, 20);
            AssertionUtils.AssertEnumerable(
                new int[] { 1, 2, 10, 20, 3, 4 },
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="ListHelper{T}.OnChangedCount"/>
        /// <seealso cref="ListHelper{T}.InsertTo(int, T[])"/>
        /// <seealso cref="ListHelper{T}.InsertTo(int, IEnumerable{T})"
        /// </summary>
        [Test]
        public void OnChangedCountInInsertToWhenOccurOnAddedExceptionPasses()
        {
            var helper = new ListHelper<int>();
            helper.Add(1, 2, 3);

            helper.OnAdded.Add((_, __) => throw new System.Exception());
            var callCounter = 0;
            var recievedData = (self: (IReadOnlyListHelper<int>)null, count: (int)0);
            helper.OnChangedCount.Add((_helper, _count) => {
                callCounter++;
                recievedData.self = _helper;
                recievedData.count = _count;
            });

            helper.InsertTo(1, 2, 3);
            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }
        #endregion
    }
}
