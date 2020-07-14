using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Hinode.Tests.CSharp.CollectionHelper
{
    /// <summary>
	/// <seealso cref="HashSetHelper{T}"/>
	/// </summary>
    public class TestHashSetHelper
    {
        #region HashSetHelper.Add
        /// <summary>
        /// <seealso cref="HashSetHelper.Items"/>
        /// <seealso cref="HashSetHelper.Add(T)"/>
        /// </summary>
        [Test]
        public void AddPasses()
        {
            var helper = new HashSetHelper<int>();

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
		/// <seealso cref="HashSetHelper.Items"/>
		/// <seealso cref="HashSetHelper.Add(T)"/>
		/// </summary>
        [Test]
        public void AddWhenContainsPasses()
        {
            var helper = new HashSetHelper<int>();
            var value = 100;
            helper.Add(value);

            Assert.DoesNotThrow(() => {
                helper.Add(value);
            });
            Assert.IsTrue(helper.Contains(value));
            Assert.AreEqual(1, helper.Count);
        }

        /// <summary>
		/// <seealso cref="HashSetHelper.Items"/>
        /// <seealso cref="HashSetHelper.Add(T)"/>
        /// </summary>
        [Test]
        public void AddWhenNullPasses()
        {
            var helper = new HashSetHelper<string>();

            Assert.DoesNotThrow(() => {
                helper.Add(default(string));
            });
            Assert.AreEqual(0, helper.Count);
        }

        /// <summary>
		/// <seealso cref="HashSetHelper{T}.Add(T)"/>
		/// <seealso cref="HashSetHelper{T}.OnAdded"/>
		/// </summary>
        [Test]
        public void OnAddedInAddPasses()
        {
            var helper = new HashSetHelper<int>();
            var counter = 0;
            var recievedValue = 0;
            helper.OnAdded.Add((v) => { recievedValue = v; counter++; });


            var value = 100;
            helper.Add(value);
            Assert.AreEqual(value, recievedValue);
            Assert.AreEqual(1, counter);
        }

        /// <summary>
		/// <seealso cref="HashSetHelper{T}.Add(T)"/>
        /// <seealso cref="HashSetHelper{T}.OnAdded"/>
        /// </summary>
        [Test]
        public void OnAddedInAddWhenNotAddPasses()
        {
            var helper = new HashSetHelper<string>();
            var counter = 0;
            var recievedValue = "";
            helper.OnAdded.Add((v) => { recievedValue = v; counter++; });


            helper.Add(default(string));
            Assert.AreEqual("", recievedValue);
            Assert.AreEqual(0, counter);
        }

        /// <summary>
        /// <seealso cref="HashSetHelper{T}.Add(T)"/>
        /// <seealso cref="HashSetHelper{T}.OnAdded"/>
        /// </summary>
        [Test]
        public void OnAddedInAddWhenOccurExceptionPasses()
        {
            var helper = new HashSetHelper<int>();
            helper.OnAdded.Add((v) => {
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
		/// <seealso cref="HashSetHelper{T}.OnChangedCount"/>
		/// <seealso cref="HashSetHelper{T}.Add(T)"/>
		/// </summary>
        [Test]
        public void OnChangedCountInAddPasses()
        {
            var helper = new HashSetHelper<int>();
            var callCounter = 0;
            var recievedData = (self: (IReadOnlyHashSetHelper<int>)null, count: (int)0);
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
        /// <seealso cref="HashSetHelper{T}.OnChangedCount"/>
        /// <seealso cref="HashSetHelper{T}.Add(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInAddNotAddPasses()
        {
            var helper = new HashSetHelper<int>();
            var value = 1;
            helper.Add(value);

            var isCall = false;
            helper.OnChangedCount.Add((_helper, _count) => {
                isCall = true;
            });

            helper.Add(value);
            Assert.IsFalse(isCall);
        }

        /// <summary>
        /// <seealso cref="HashSetHelper{T}.OnChangedCount"/>
        /// <seealso cref="HashSetHelper{T}.Add(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInAddWhenOccurExceptionPasses()
        {
            var helper = new HashSetHelper<int>();
            helper.OnChangedCount.Add((_helper, _count) => {
                throw new System.NotImplementedException();
            });

            var value = 1;
            helper.Add(value);

            AssertionUtils.AssertEnumerableByUnordered(
                new int[] { value },
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="HashSetHelper{T}.OnChangedCount"/>
        /// <seealso cref="HashSetHelper{T}.Add(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInAddWhenOccurOnAddedExceptionPasses()
        {
            var helper = new HashSetHelper<int>();
            var isCallOnChangedCount = false;
            helper.OnAdded.Add((_v) => {
                throw new System.NotImplementedException();
            });
            helper.OnChangedCount.Add((_, __) => isCallOnChangedCount = true); ;

            var value = 1;
            helper.Add(value);

            Assert.IsTrue(isCallOnChangedCount);
            AssertionUtils.AssertEnumerableByUnordered(
                new int[] { value },
                helper.Items,
                ""
            );
        }

        #endregion

        #region HashSetHelper.Remove
        /// <summary>
        /// <seealso cref="HashSetHelper.Items"/>
        /// <seealso cref="HashSetHelper.Remove(T)"/>
        /// </summary>
        [Test]
        public void RemovePasses()
        {
            var helper = new HashSetHelper<int>();

            var testData = new int[] {
                100, 200
            };
            foreach(var d in testData)
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
		/// <seealso cref="HashSetHelper.Items"/>
		/// <seealso cref="HashSetHelper.Remove(T)"/>
		/// </summary>
        [Test]
        public void RemoveWhenNotContainsPasses()
        {
            var helper = new HashSetHelper<int>();

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
		/// <seealso cref="HashSetHelper.Items"/>
        /// <seealso cref="HashSetHelper.Remove(T)"/>
        /// </summary>
        [Test]
        public void RemoveWhenNullPasses()
        {
            var helper = new HashSetHelper<string>();

            Assert.DoesNotThrow(() => {
                Assert.AreEqual(0, helper.Count);
                helper.Remove(default(string));
                Assert.AreEqual(0, helper.Count);
            });
        }

        /// <summary>
		/// <seealso cref="HashSetHelper{T}.Remove(T)"/>
        /// <seealso cref="HashSetHelper{T}.OnRemoved"/>
        /// </summary>
        [Test]
        public void OnRemovedInRemovePasses()
        {
            var helper = new HashSetHelper<int>();
            var counter = 0;
            var recievedValue = 0;
            helper.OnRemoved.Add((v) => { counter++; recievedValue = v; });

            var testData = new int[] {
                100, 200
            };
            foreach (var d in testData)
            {
                helper.Add(d);
            }

            Assert.AreEqual(0, counter);
            helper.Remove(testData[0]);
            Assert.AreEqual(1, counter);
            Assert.AreEqual(testData[0], recievedValue);

            counter = 0;
            helper.Remove(testData[1]);
            Assert.AreEqual(1, counter);
            Assert.AreEqual(testData[1], recievedValue);
        }

        /// <summary>
		/// <seealso cref="HashSetHelper{T}.Remove(T)"/>
        /// <seealso cref="HashSetHelper{T}.OnRemoved"/>
        /// </summary>
        [Test]
        public void OnRemovedWhenNotContainsPasses()
        {
            var helper = new HashSetHelper<int>();
            var counter = 0;
            var recievedValue = 0;
            helper.OnRemoved.Add((v) => { counter++; recievedValue = v; });

            Assert.AreEqual(0, counter);
            helper.Remove(123);
            Assert.AreEqual(0, counter);
            Assert.AreEqual(0, recievedValue);
        }

        /// <summary>
        /// <seealso cref="HashSetHelper.Remove(T)"/>
        /// <seealso cref="HashSetHelper{T}.OnRemoved"/>
        /// </summary>
        [Test]
        public void OnRemovedInRemoveWhenOccurExceptionPasses()
        {
            var helper = new HashSetHelper<int>();
            helper.OnRemoved.Add((v) => {
                throw new System.Exception();
            });

            var value = 100;
            helper.Add(value);
            Assert.DoesNotThrow(() => {
                helper.Remove(value);
            });
        }

        /// <summary>
        /// <seealso cref="HashSetHelper{T}.OnChangedCount"/>
        /// <seealso cref="HashSetHelper{T}.Remove(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemovePasses()
        {
            var helper = new HashSetHelper<int>();
            var value = 100;
            helper.Add(value, 2, 4);

            var callCounter = 0;
            (IReadOnlyHashSetHelper<int> self, int count) recievedData = default;
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
        /// <seealso cref="HashSetHelper{T}.OnChangedCount"/>
        /// <seealso cref="HashSetHelper{T}.Remove(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveNotRemovePasses()
        {
            var helper = new HashSetHelper<int>();
            helper.Add(1, 2, 4);

            bool isCall = false;
            helper.OnChangedCount.Add((_helper, _count) => {
                isCall = true;
            });

            helper.Remove(1000);
            Assert.IsFalse(isCall);
        }

        /// <summary>
        /// <seealso cref="HashSetHelper{T}.OnChangedCount"/>
        /// <seealso cref="HashSetHelper{T}.Remove(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveWhenOccurExceptionPasses()
        {
            var helper = new HashSetHelper<int>();
            var value = 1;
            helper.Add(value, 2, 3, 4);
            helper.OnChangedCount.Add((_helper, _count) => {
                throw new System.NotImplementedException();
            });

            helper.Remove(value);

            AssertionUtils.AssertEnumerableByUnordered(
                new int[] { 2, 3, 4 },
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="HashSetHelper{T}.OnChangedCount"/>
        /// <seealso cref="HashSetHelper{T}.Remove(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveWhenOccurOnRemovedExceptionPasses()
        {
            var helper = new HashSetHelper<int>();
            var value = 1;
            helper.Add(value, 2, 3, 4);
            helper.OnRemoved.Add((_v) => {
                throw new System.NotImplementedException();
            });
            var isCallOnChangedCount = false;
            helper.OnChangedCount.Add((_, __) => isCallOnChangedCount = true); ;

            helper.Remove(value);

            Assert.IsTrue(isCallOnChangedCount);
            AssertionUtils.AssertEnumerableByUnordered(
                new int[] { 2, 3, 4 },
                helper.Items,
                ""
            );
        }
        #endregion

        #region HashSetHelper.Clear

        /// <summary>
        /// <seealso cref="HashSetHelper{T}.Clear()"/>
        /// <seealso cref="HashSetHelper{T}.OnRemoved"/>
        /// </summary>
        [Test]
        public void OnRemovedInClearPasses()
        {
            var helper = new HashSetHelper<int>();
            var counter = 0;
            var recievedValue = new List<int>();
            helper.OnRemoved.Add((v) => { counter++; recievedValue.Add(v); });

            var testData = new int[] {
                100, 200
            };
            foreach (var d in testData)
            {
                helper.Add(d);
            }

            Assert.AreEqual(0, counter);
            helper.Clear();
            Assert.AreEqual(testData.Length, counter);
            AssertionUtils.AssertEnumerableByUnordered(testData, recievedValue, "");
        }

        /// <summary>
        /// <seealso cref="HashSetHelper{T}.Clear()"/>
        /// <seealso cref="HashSetHelper{T}.OnRemoved"/>
        /// </summary>
        [Test]
        public void OnRemovedInClearWhenEmptyPasses()
        {
            var helper = new HashSetHelper<int>();
            var counter = 0;
            var recievedValue = new List<int>();
            helper.OnRemoved.Add((v) => { counter++; recievedValue.Add(v); });

            Assert.AreEqual(0, counter);
            Assert.IsFalse(recievedValue.Any());
        }

        /// <summary>
		/// <seealso cref="HashSetHelper{T}.Clear()"/>
		/// </summary>
        [Test]
        public void ClearPasses()
        {
            var helper = new HashSetHelper<int>();

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
		/// <seealso cref="HashSetHelper{T}.Clear()"/>
		/// </summary>
        [Test]
        public void ClearWhenEmptyPasses()
        {
            var helper = new HashSetHelper<int>();

            Assert.DoesNotThrow(() => {
                helper.Clear();
            });
        }

        /// <summary>
		/// <seealso cref="HashSetHelper{T}.Clear()"/>
        /// <seealso cref="HashSetHelper{T}.OnCleared"/>
        /// </summary>
        [Test]
        public void OnClearedInClearPasses()
        {
            var helper = new HashSetHelper<int>();
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
		/// <seealso cref="HashSetHelper{T}.Clear()"/>
        /// <seealso cref="HashSetHelper{T}.OnCleared"/>
        /// </summary>
        [Test]
        public void OnClearedInClearWhenEmptyPasses()
        {
            var helper = new HashSetHelper<int>();
            var counter = 0;
            helper.OnCleared.Add(() => counter++);

            helper.Clear();

            Assert.AreEqual(0, counter);
        }

        /// <summary>
        /// <seealso cref="HashSetHelper.Clear()"/>
        /// <seealso cref="HashSetHelper{T}.OnRemoved"/>
        /// <seealso cref="HashSetHelper{T}.OnCleared"/>
        /// </summary>
        [Test]
        public void ClearInClearWhenOccurExceptionPasses()
        {
            var helper = new HashSetHelper<int>();
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
        /// <seealso cref="HashSetHelper{T}.OnChangedCount"/>
        /// <seealso cref="HashSetHelper{T}.Clear(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInClearPasses()
        {
            var helper = new HashSetHelper<int>();
            helper.Add(1, 2, 3);
            var callCounter = 0;
            var recievedData = (self: (IReadOnlyHashSetHelper<int>)null, count: (int)0);
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
        /// <seealso cref="HashSetHelper{T}.OnChangedCount"/>
        /// <seealso cref="HashSetHelper{T}.Clear(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInClearNotRemovePasses()
        {
            var helper = new HashSetHelper<int>();

            var isCall = false;
            helper.OnChangedCount.Add((_helper, _count) => {
                isCall = true;
            });

            helper.Clear();
            Assert.IsFalse(isCall);
        }

        /// <summary>
        /// <seealso cref="HashSetHelper{T}.OnChangedCount"/>
        /// <seealso cref="HashSetHelper{T}.Clear(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInClearWhenOccurExceptionPasses()
        {
            var helper = new HashSetHelper<int>();
            helper.Add(1, 2, 3);
            helper.OnChangedCount.Add((_helper, _count) => {
                throw new System.NotImplementedException();
            });

            helper.Clear();

            AssertionUtils.AssertEnumerableByUnordered(
                new int[] { },
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="HashSetHelper{T}.OnChangedCount"/>
        /// <seealso cref="HashSetHelper{T}.Clear(T)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInClearWhenOccurOnRemovedExceptionPasses()
        {
            var helper = new HashSetHelper<int>();
            var value = 1;
            helper.Add(value, 2, 3, 4);
            helper.OnRemoved.Add((_v) => {
                throw new System.NotImplementedException();
            });
            var callCounter = 0;
            helper.OnChangedCount.Add((_, __) => callCounter++);

            helper.Clear();

            Assert.AreEqual(1, callCounter);
            AssertionUtils.AssertEnumerableByUnordered(
                new int[] { },
                helper.Items,
                ""
            );
        }
        #endregion

        #region HashSetHelper.Contains
        /// <summary>
        /// <seealso cref="HashSetHelper.Items"/>
        /// <seealso cref="HashSetHelper.Contains(T)"/>
        /// </summary>
        [Test]
        public void ContainsPasses()
        {
            var helper = new HashSetHelper<int>();

            var value = 100;
            helper.Add(value);
            Assert.IsTrue(helper.Contains(value));

            helper.Remove(value);
            Assert.IsFalse(helper.Contains(value));

            helper.Add(value);
            Assert.IsTrue(helper.Contains(value));
        }

        /// <summary>
		/// <seealso cref="HashSetHelper.Items"/>
        /// <seealso cref="HashSetHelper.Contains(T)"/>
        /// </summary>
        [Test]
        public void ContainsWhenNotContainsPasses()
        {
            var helper = new HashSetHelper<int>();
            Assert.IsFalse(helper.Contains(100));
        }
        #endregion

        #region HashSetHelper.Count
        /// <summary>
        /// <seealso cref="HashSetHelper{T}.Count"/>
        /// </summary>
        [Test]
        public void CountPasses()
        {
            var helper = new HashSetHelper<int>();
            var testData = Enumerable.Range(0, 100);
            foreach(var d in testData)
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

        #region HashSetHelper.AddItems

        /// <summary>
        /// <seealso cref="HashSetHelper{T}.Add(T[])"/>
        /// <seealso cref="HashSetHelper{T}.Add(IEnumerable{T})"/>
        /// </summary>
        [Test]
        public void AddItemsPasses()
        {
            var testData = Enumerable.Range(0, 10);

            var helper = new HashSetHelper<int>();
            helper.Add(testData);

            AssertionUtils.AssertEnumerableByUnordered(
                testData,
                helper.Items,
                ""
            );
        }

        /// <summary>
		/// <seealso cref="HashSetHelper{T}.OnAdded"/>
        /// <seealso cref="HashSetHelper{T}.Add(T[])"/>
        /// <seealso cref="HashSetHelper{T}.Add(IEnumerable{T})"/>
        /// </summary>
		[Test]
        public void OnAddedInAddItemsPasses()
        {
            var helper = new HashSetHelper<int>();
            var recievedList = new List<int>();
            var counter = 0;
            helper.OnAdded.Add((v) => { recievedList.Add(v); counter++; });


            var testData = Enumerable.Range(0, 10);
            helper.Add(testData);

            Assert.AreEqual(testData.Count(), counter);
            AssertionUtils.AssertEnumerableByUnordered(
                testData,
                recievedList,
                ""
            );
        }

        /// <summary>
		/// <seealso cref="HashSetHelper{T}.Add(IEnumerable{T})"/>
		/// <seealso cref="HashSetHelper{T}.Add(T[])"/>
		/// </summary>
        [Test]
        public void AddItemsWhenOccurExceptionPasses()
        {
            var helper = new HashSetHelper<int>();
            var recievedList = new List<int>();
            var counter = 0;
            helper.OnAdded.Add((v) => {
                if (v % 2 == 0) throw new System.Exception();
                recievedList.Add(v); counter++;
            });


            var testData = Enumerable.Range(0, 10);
            Assert.DoesNotThrow(() => {
                helper.Add(testData);
            });

            Assert.AreEqual(testData.Count() / 2, counter);
            AssertionUtils.AssertEnumerableByUnordered(
                testData.Where(_v => _v % 2 != 0),
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
        /// <seealso cref="HashSetHelper{T}.OnChangedCount"/>
        /// <seealso cref="HashSetHelper{T}.Add{T}(IEnumerable{T})"/>
		/// <seealso cref="HashSetHelper{T}.Add{T}(T[])"/>
        /// </summary>
        [Test]
        public void OnChangedCountInAddItemsPasses()
        {
            var helper = new HashSetHelper<int>();
            var recievedData = (self: (IReadOnlyHashSetHelper<int>)null, count: (int)0);
            helper.OnChangedCount.Add((_helper, _count) => {
                recievedData.self = _helper;
                recievedData.count = _count;
            });

            var testData = new int[] { 1, 2, 3 };
            helper.Add(testData);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }

        /// <summary>
        /// <seealso cref="HashSetHelper{T}.OnChangedCount"/>
        /// <seealso cref="HashSetHelper{T}.Add{T}(IEnumerable{T})"/>
		/// <seealso cref="HashSetHelper{T}.Add{T}(T[])"/>
        /// </summary>
        [Test]
        public void OnChangedCountInAddItemsNotAddPasses()
        {
            var helper = new HashSetHelper<int>();
            var testData = new int[] { 1, 2, 3 };
            helper.Add(testData);
            var isCall = false;
            helper.OnChangedCount.Add((_helper, _count) => {
                isCall = true;
            });

            helper.Add(testData);
            Assert.IsFalse(isCall);
        }

        /// <summary>
        /// <seealso cref="HashSetHelper{T}.OnChangedCount"/>
        /// <seealso cref="HashSetHelper{T}.Add{T}(IEnumerable{T})"/>
        /// <seealso cref="HashSetHelper{T}.Add{T}(T[])"/>
        /// </summary>
        [Test]
        public void OnChangedCountInAddItemsWhenOccurExceptionPasses()
        {
            var helper = new HashSetHelper<int>();
            helper.OnChangedCount.Add((_helper, _count) => {
                throw new System.NotImplementedException();
            });

            var testData = new int[] { 1, 2, 3 };
            helper.Add(testData);

            AssertionUtils.AssertEnumerableByUnordered(
                testData,
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="HashSetHelper{T}.OnChangedCount"/>
        /// <seealso cref="HashSetHelper{T}.Add{T}(IEnumerable{T})"/>
		/// <seealso cref="HashSetHelper{T}.Add{T}(T[])"/>
        /// </summary>
        [Test]
        public void OnChangedCountInAddItemsWhenOccurOnAddedExceptionPasses()
        {
            var helper = new HashSetHelper<int>();
            var value = 1;
            helper.Add(value);
            helper.OnAdded.Add((_v) => {
                throw new System.NotImplementedException();
            });
            var callCounter = 0;
            helper.OnChangedCount.Add((_, __) => callCounter++);

            helper.Add(2, 3, 4);

            Assert.AreEqual(1, callCounter);
            AssertionUtils.AssertEnumerableByUnordered(
                new int[] { value, 2, 3, 4 },
                helper.Items,
                ""
            );
        }
        #endregion

        #region HashSetHelper.RemoveItems

        /// <summary>
        /// <seealso cref="HashSetHelper{T}.Add(T[])"/>
        /// <seealso cref="HashSetHelper{T}.Add(IEnumerable{T})"/>
        /// </summary>
        [Test]
        public void RemoveItemsPasses()
        {
            var helper = new HashSetHelper<int>();
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
		/// <seealso cref="HashSetHelper{T}.OnRemoved"/>
        /// <seealso cref="HashSetHelper{T}.Remove(T[])"/>
        /// <seealso cref="HashSetHelper{T}.Remove(IEnumerable{T})"/>
        /// </summary>
		[Test]
        public void OnRemovedInRemoveItemsPasses()
        {
            var helper = new HashSetHelper<int>();
            var recievedList = new List<int>();
            var counter = 0;
            helper.OnRemoved.Add((v) => { recievedList.Add(v); counter++; });


            var testData = Enumerable.Range(0, 10);
            helper.Add(testData);

            helper.Remove(testData.Where(_v => _v % 2 == 0));

            Assert.AreEqual(testData.Count() / 2, counter);
            AssertionUtils.AssertEnumerableByUnordered(
                testData.Where(_v => _v % 2 == 0),
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
        /// <seealso cref="HashSetHelper{T}.Remove(IEnumerable{T})"/>
        /// <seealso cref="HashSetHelper{T}.Remove(T[])"/>
        /// </summary>
        [Test]
        public void RemoveItemsWhenOccurExceptionPasses()
        {
            var helper = new HashSetHelper<int>();
            var recievedList = new List<int>();
            var counter = 0;
            helper.OnRemoved.Add((v) => {
                if (v % 2 == 0) throw new System.Exception();
                recievedList.Add(v); counter++;
            });


            var testData = Enumerable.Range(0, 10);
            helper.Add(testData);
            Assert.DoesNotThrow(() => {
                helper.Remove(testData);
            });

            Assert.AreEqual(testData.Count() / 2, counter);
            AssertionUtils.AssertEnumerableByUnordered(
                testData.Where(_v => _v % 2 != 0),
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
        /// <seealso cref="HashSetHelper{T}.OnChangedCount"/>
        /// <seealso cref="HashSetHelper{T}.Remove{T}(IEnumerable{T})"/>
        /// <seealso cref="HashSetHelper{T}.Remove{T}(T[])"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveItemsPasses()
        {
            var helper = new HashSetHelper<int>();
            var testData = new int[] { 1, 2, 3 };
            helper.Add(testData);
            var callCounter = 0;
            var recievedData = (self: (IReadOnlyHashSetHelper<int>)null, count: (int)0);
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
        /// <seealso cref="HashSetHelper{T}.OnChangedCount"/>
        /// <seealso cref="HashSetHelper{T}.Remove{T}(IEnumerable{T})"/>
		/// <seealso cref="HashSetHelper{T}.Remove{T}(T[])"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveItemsNotRemovePasses()
        {
            var helper = new HashSetHelper<int>();
            var isCall = false;
            helper.OnChangedCount.Add((_helper, _count) => {
                isCall = true;
            });

            var testData = new int[] { 1, 2, 3 };
            helper.Remove(testData);
            Assert.IsFalse(isCall);
        }

        /// <summary>
        /// <seealso cref="HashSetHelper{T}.OnChangedCount"/>
        /// <seealso cref="HashSetHelper{T}.Remove{T}(IEnumerable{T})"/>
		/// <seealso cref="HashSetHelper{T}.Remove{T}(T[])"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveItemsWhenOccurExceptionPasses()
        {
            var helper = new HashSetHelper<int>();
            var testData = new int[] { 1, 2, 3 };
            helper.Add(testData);
            helper.OnChangedCount.Add((_helper, _count) => {
                throw new System.NotImplementedException();
            });

            helper.Remove(testData);

            AssertionUtils.AssertEnumerableByUnordered(
                new int[] { },
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="HashSetHelper{T}.OnChangedCount"/>
        /// <seealso cref="HashSetHelper{T}.Remove{T}(IEnumerable{T})"/>
		/// <seealso cref="HashSetHelper{T}.Remove{T}(T[])"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveItemsWhenOccurOnRemovedExceptionPasses()
        {
            var helper = new HashSetHelper<int>();
            var value = 1;
            var testData = new int[] { 2, 3, 4 };
            helper.Add(value).Add(testData);
            helper.OnRemoved.Add((_v) => {
                throw new System.NotImplementedException();
            });
            var callCounter = 0;
            helper.OnChangedCount.Add((_, __) => callCounter++);

            helper.Remove(testData);

            Assert.AreEqual(1, callCounter);
            AssertionUtils.AssertEnumerableByUnordered(
                new int[] { value },
                helper.Items,
                ""
            );
        }
        #endregion

        #region IEnumerable

        /// <summary>
        /// <seealso cref="HashSetHelper{T}.GetEnumerator()"/>
        /// </summary>
        [Test]
        public void GetEnumeratorPasses()
        {
            var helper = new HashSetHelper<int>();
            var testData = new int[] { 10, 20, 40 };
            helper.Add(testData);

            AssertionUtils.AssertEnumerableByUnordered(
                testData,
                helper,
                ""
            );
        }
        #endregion
    }
}
