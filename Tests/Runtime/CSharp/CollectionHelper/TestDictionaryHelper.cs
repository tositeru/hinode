using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Hinode.Tests.CSharp.CollectionHelper
{
    /// <summary>
	/// <seealso cref="DictionaryHelper"/>
	/// </summary>
    public class TestDictionaryHelper
    {
        #region DictionaryHelper.Add
        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Add(TKey, TValue)"/>
        /// </summary>
        [Test]
        public void AddPasses()
        {
            var helper = new DictionaryHelper<string, int>();

            var key = "test";
            var value = 100;
            helper.Add(key, value);

            Assert.AreEqual(1, helper.Count);
            Assert.AreEqual(value, helper[key]);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Add(TKey, TValue)"/>
        /// </summary>
        [Test]
        public void AddWhenContainsKeyPasses()
        {
            var helper = new DictionaryHelper<string, int>();

            var key = "test";
            helper.Add(key, 123);

            var value = 100;
            Assert.DoesNotThrow(() => {
                helper.Add(key, value);
            });
            Assert.AreEqual(1, helper.Count);
            Assert.AreEqual(value, helper[key]);
        }

        /// <summary>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.OnAdded"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Add(TKey, TValue)"/>
        /// </summary>
        [Test]
        public void OnAddedInAddPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            var callCounter = 0;
            var recievedData = (key: default(string), value: default(int));
            helper.OnAdded.Add((_key, _value) => {
                callCounter++;
                recievedData.key = _key;
                recievedData.value = _value;
            });

            var key = "test";
            var value = 100;
            helper.Add(key, value);

            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(key, recievedData.key);
            Assert.AreEqual(value, recievedData.value);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.OnAdded"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Add(TKey, TValue)"/>
        /// </summary>
        [Test]
        public void OnAddedInAddWhenOccurExceptionPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            helper.OnAdded.Add((_, __) => {
                throw new System.Exception();
            });

            var key = "test";
            var value = 100;
            helper.Add(key, value);

            Assert.AreEqual(1, helper.Count);
            Assert.AreEqual(value, helper[key]);
        }

        /// <summary>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.OnSwaped"/>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Add(IEnumerable{KeyValuePair{TKey, TValue}})"/>
		/// </summary>
        [Test]
        public void OnSwapedInAdd()
        {
            var helper = new DictionaryHelper<string, int>();
            var key = "test";
            var value = 100;
            helper.Add(key, value);

            var callCounter = 0;
            var recievedData = (key: (string)null, oldValue: default(int), newValue: default(int));
            helper.OnSwaped.Add((_key, oldValue, _newValue) => {
                callCounter++;
                recievedData.key = _key;
                recievedData.oldValue = oldValue;
                recievedData.newValue = _newValue;
            });

            var newValue = 231;
            helper.Add(key, newValue);

            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(key, recievedData.key);
            Assert.AreEqual(value, recievedData.oldValue);
            Assert.AreEqual(newValue, recievedData.newValue);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.OnSwaped"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Add(IEnumerable{KeyValuePair{TKey, TValue}})"/>
        /// </summary>
        [Test]
        public void OnSwapedInAddWhenOccurException()
        {
            var helper = new DictionaryHelper<string, int>();
            var key = "test";
            var value = 100;
            helper.Add(key, value);

            helper.OnSwaped.Add((_, __, ___) => {
                throw new System.Exception();
            });

            var newValue = 231;
            helper.Add(key, newValue);

            Assert.AreEqual(newValue, helper[key]);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{T}.OnChangedCount"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Add(TKey, TValue)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInAddPasses()
        {
            var helper = new DictionaryHelper<string, int>();

            var callCounter = 0;
            var recievedData = (self: (IReadOnlyDictionaryHelper<string, int>)null, count: -1);
            helper.OnChangedCount.Add((self, count) => {
                callCounter++;
                recievedData.self = self;
                recievedData.count = count;
            });

            var key = "test";
            var value = 100;
            helper.Add(key, value);

            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{T}.OnChangedCount"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Add(TKey, TValue)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInAddNotChangedCountPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            var key = "test";
            var value = 100;
            helper.Add(key, value);

            var callCounter = 0;
            helper.OnChangedCount.Add((self, count) => {
                callCounter++;
            });

            var newValue = 321;
            helper.Add(key, newValue);

            Assert.AreEqual(0, callCounter);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{T}.OnChangedCount"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Add(TKey, TValue)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInAddWhenOccurExceptionPasses()
        {
            var helper = new DictionaryHelper<string, int>();

            helper.OnChangedCount.Add((self, count) => {
                throw new System.Exception();
            });

            var key = "test";
            var value = 100;
            helper.Add(key, value);

            Assert.AreEqual(1, helper.Count);
            Assert.AreEqual(value, helper[key]);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{T}.OnChangedCount"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Add(TKey, TValue)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInAddWhenOccurOnAddedExceptionPasses()
        {
            var helper = new DictionaryHelper<string, int>();

            helper.OnAdded.Add((_, __) => {
                throw new System.Exception();
            });

            var callCounter = 0;
            var recievedData = (self: (IReadOnlyDictionaryHelper<string, int>)null, count: -1);
            helper.OnChangedCount.Add((self, count) => {
                callCounter++;
                recievedData.self = self;
                recievedData.count = count;
            });

            var key = "test";
            var value = 100;
            helper.Add(key, value);

            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{T}.OnChangedCount"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Add(TKey, TValue)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInSwapAddWhenOccurOnSwapedExceptionPasses()
        {
            var helper = new DictionaryHelper<string, int>();

            var key = "test";
            var value = 100;
            helper.Add(key, value);

            helper.OnSwaped.Add((_, __, ___) => {
                throw new System.Exception();
            });

            var callCounter = 0;
            helper.OnChangedCount.Add((self, count) => {
                callCounter++;
            });

            var newValue = 321;
            helper.Add(key, newValue);

            Assert.AreEqual(0, callCounter);
        }
        #endregion

        #region DictionaryHelper.Add Items
        /// <summary>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Add((TKey key, TValue value)[])"/>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Add(IEnumerable{(TKey key, TValue value)})"/>
        /// <seealso cref="DictionaryHelper.Add{TKey, TValue}(KeyValuePair{TKey, TValue}[])"/>
        /// <seealso cref="DictionaryHelper.Add{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}})"/>
        /// </summary>
        [Test]
        public void AddItemsPasses()
        {
            var helper = new DictionaryHelper<string, int>();

            var testData = new (string, int)[]
            {
                ("A", 10), ("B", 20), ("C", 30)
            };
            helper.Add(testData);

            AssertionUtils.AssertEnumerableByUnordered(
                testData,
                helper.TupleItems,
                ""
            );
        }

        /// <summary>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Add((TKey key, TValue value)[])"/>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Add(IEnumerable{(TKey key, TValue value)})"/>
        /// <seealso cref="DictionaryHelper.Add{TKey, TValue}(KeyValuePair{TKey, TValue}[])"/>
        /// <seealso cref="DictionaryHelper.Add{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}})"/>
        /// </summary>
        [Test]
        public void AddItemsWhenContainsKeyPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            helper
                .Add("A", -100)
                .Add("B", -234);

            var testData = new (string, int)[]
            {
                ("A", 10), ("B", 20), ("C", 30)
            };
            helper.Add(testData);

            AssertionUtils.AssertEnumerableByUnordered(
                testData,
                helper.TupleItems,
                ""
            );
        }

        /// <summary>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.OnAdded"/>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Add((TKey key, TValue value)[])"/>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Add(IEnumerable{(TKey key, TValue value)})"/>
		/// <seealso cref="DictionaryHelper.Add{TKey, TValue}(KeyValuePair{TKey, TValue}[])"/>
		/// <seealso cref="DictionaryHelper.Add{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}})"/>
		/// </summary>
        [Test]
        public void OnAddedInAddItemsPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            var recievedList = new List<(string key, int value)>();
            helper.OnAdded.Add((key, value) => {
                recievedList.Add((key, value));
            });

            var testData = new (string, int)[]
            {
                ("A", 10), ("B", 20), ("C", 30)
            };
            helper.Add(testData);

            AssertionUtils.AssertEnumerableByUnordered(
                testData,
                recievedList,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.OnAdded"/>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Add((TKey key, TValue value)[])"/>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Add(IEnumerable{(TKey key, TValue value)})"/>
		/// <seealso cref="DictionaryHelper.Add{TKey, TValue}(KeyValuePair{TKey, TValue}[])"/>
        /// <seealso cref="DictionaryHelper.Add{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}})"/>
        /// </summary>
        [Test]
        public void OnAddedInAddItemsWhenOccurExceptionPasses()
        {
            var helper = new DictionaryHelper<string, int>();

            helper.OnAdded.Add((_, __) => throw new System.Exception());

            var testData = new (string, int)[]
            {
                ("A", 10), ("B", 20), ("C", 30)
            };
            helper.Add(testData);

            AssertionUtils.AssertEnumerableByUnordered(
                testData,
                helper.TupleItems,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.OnSwaped"/>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Add((TKey key, TValue value)[])"/>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Add(IEnumerable{(TKey key, TValue value)})"/>
		/// <seealso cref="DictionaryHelper.Add{TKey, TValue}(KeyValuePair{TKey, TValue}[])"/>
        /// <seealso cref="DictionaryHelper.Add{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}})"/>
        /// </summary>
        [Test]
        public void OnSwapedInAddItems()
        {
            var helper = new DictionaryHelper<string, int>();
            helper.Add("A", -102);
            helper.Add("B", -231);

            var recievedList = new List<(string key, int oldValue, int newValue)>();
            helper.OnSwaped.Add((key, oldValue, newValue) => {
                recievedList.Add((key, oldValue, newValue));
            });

            var testData = new (string key, int value)[]
            {
                ("A", 10), ("B", 20), ("C", 30)
            };
            helper.Add(testData);

            AssertionUtils.AssertEnumerableByUnordered(
                new (string, int, int)[] {
                    ("A", -102, 10),
                    ("B", -231, 20),
                },
                recievedList,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.OnSwaped"/>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Add((TKey key, TValue value)[])"/>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Add(IEnumerable{(TKey key, TValue value)})"/>
		/// <seealso cref="DictionaryHelper.Add{TKey, TValue}(KeyValuePair{TKey, TValue}[])"/>
        /// <seealso cref="DictionaryHelper.Add{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}})"/>
        /// </summary>
        [Test]
        public void OnSwapedInAddItemsWhenOccurException()
        {
            var helper = new DictionaryHelper<string, int>();
            helper.Add("A", -102);
            helper.Add("B", -231);

            helper.OnSwaped.Add((key, oldValue, newValue) => {
                throw new System.Exception();
            });

            var testData = new (string key, int value)[]
            {
                ("A", 10), ("B", 20), ("C", 30)
            };
            helper.Add(testData);

            AssertionUtils.AssertEnumerableByUnordered(
                new (string, int)[] {
                    ("A", 10),
                    ("B", 20),
                    ("C", 30),
                },
                helper.TupleItems,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{T}.OnChangedCount"/>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Add((TKey key, TValue value)[])"/>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Add(IEnumerable{(TKey key, TValue value)})"/>
		/// <seealso cref="DictionaryHelper.Add{TKey, TValue}(KeyValuePair{TKey, TValue}[])"/>
        /// <seealso cref="DictionaryHelper.Add{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}})"/>
        /// </summary>
        [Test]
        public void OnChangedCountInAddItemsPasses()
        {
            var helper = new DictionaryHelper<string, int>();

            var callCounter = 0;
            var recievedData = (self: default(IReadOnlyDictionaryHelper<string, int>), count: default(int));
            helper.OnChangedCount.Add((self, count) => {
                callCounter++;
                recievedData.self = self;
                recievedData.count = count;
            });

            var testData = new (string key, int value)[]
            {
                ("A", 10), ("B", 20), ("C", 30)
            };
            helper.Add(testData);

            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{T}.OnChangedCount"/>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Add((TKey key, TValue value)[])"/>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Add(IEnumerable{(TKey key, TValue value)})"/>
		/// <seealso cref="DictionaryHelper.Add{TKey, TValue}(KeyValuePair{TKey, TValue}[])"/>
        /// <seealso cref="DictionaryHelper.Add{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}})"/>
        /// </summary>
        [Test]
        public void OnChangedCountInAddItemsNotChangedCountPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            var testData = new (string key, int value)[]
            {
                ("A", 10), ("B", 20), ("C", 30)
            };
            helper.Add(testData);

            bool isCall = false;
            helper.OnChangedCount.Add((self, count) => {
                isCall = true;
            });

            var testData2 = new (string key, int value)[]
            {
                ("A", 101), ("B", 20)
            };
            helper.Add(testData2);

            Assert.IsFalse(isCall);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{T}.OnChangedCount"/>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Add((TKey key, TValue value)[])"/>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Add(IEnumerable{(TKey key, TValue value)})"/>
		/// <seealso cref="DictionaryHelper.Add{TKey, TValue}(KeyValuePair{TKey, TValue}[])"/>
        /// <seealso cref="DictionaryHelper.Add{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}})"/>
        /// </summary>
        [Test]
        public void OnChangedCountInAddItemsWhenOccurExceptionPasses()
        {
            var helper = new DictionaryHelper<string, int>();

            helper.OnChangedCount.Add((_, __) => {
                throw new System.Exception();
            });

            var testData = new (string key, int value)[]
            {
                ("A", 10), ("B", 20), ("C", 30)
            };
            helper.Add(testData);

            AssertionUtils.AssertEnumerableByUnordered(
                testData,
                helper.TupleItems,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{T}.OnChangedCount"/>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Add((TKey key, TValue value)[])"/>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Add(IEnumerable{(TKey key, TValue value)})"/>
		/// <seealso cref="DictionaryHelper.Add{TKey, TValue}(KeyValuePair{TKey, TValue}[])"/>
        /// <seealso cref="DictionaryHelper.Add{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}})"/>
        /// </summary>
        [Test]
        public void OnChangedCountInAddItemsWhenOccurOnAddedExceptionPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            helper.OnAdded.Add((_, __) => throw new System.Exception());

            var callCounter = 0;
            var recievedData = (self: default(IReadOnlyDictionaryHelper<string, int>), count: default(int));
            helper.OnChangedCount.Add((self, count) => {
                callCounter++;
                recievedData.self = self;
                recievedData.count = count;
            });

            var testData = new (string key, int value)[]
            {
                ("A", 10), ("B", 20), ("C", 30)
            };
            helper.Add(testData);

            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{T}.OnChangedCount"/>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Add((TKey key, TValue value)[])"/>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Add(IEnumerable{(TKey key, TValue value)})"/>
        /// <seealso cref="DictionaryHelper.Add{TKey, TValue}(KeyValuePair{TKey, TValue}[])"/>
        /// <seealso cref="DictionaryHelper.Add{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}})"/>
        /// </summary>
        [Test]
        public void OnChangedCountInSwapAddItemsWhenOccurOnSwapedExceptionPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            var testData = new (string key, int value)[]
            {
                ("A", 10), ("B", 20), ("C", 30)
            };
            helper.Add(testData);

            helper.OnSwaped.Add((_, __, ___) => throw new System.Exception());
            var callCounter = 0;
            helper.OnChangedCount.Add((self, count) => {
                callCounter++;
            });

            var testData2 = new (string key, int value)[]
            {
                ("A", 10), ("B", 202)
            };
            helper.Add(testData2);

            Assert.AreEqual(0, callCounter);
        }
        #endregion

        #region DictionaryHelper.Remove
        /// <summary>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Remove(TKey)"/>
		/// </summary>
        [Test]
        public void RemovePasses()
        {
            var helper = new DictionaryHelper<string, int>();
            var key = "A";
            helper.Add(key, 100);

            helper.Remove(key);

            Assert.AreEqual(0, helper.Count);
            Assert.IsFalse(helper.ContainsKey(key));
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Remove(TKey)"/>
        /// </summary>
        [Test]
        public void RemoveWhenNotContainsKeyPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            var key = "A";

            Assert.DoesNotThrow(() => {
                helper.Remove(key);
            });

            Assert.AreEqual(0, helper.Count);
            Assert.IsFalse(helper.ContainsKey(key));
        }

        /// <summary>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.OnRemoved"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Remove(TKey)"/>
        /// </summary>
        [Test]
        public void OnRemovedInRemovePasses()
        {
            var helper = new DictionaryHelper<string, int>();
            var key = "A";
            helper.Add(key, 100);

            int callCounter = 0;
            string recievedKey = null;
            helper.OnRemoved.Add((_, __) => {
                callCounter++;
                recievedKey = key;
            });

            helper.Remove(key);

            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(key, recievedKey);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.OnRemoved"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Remove(TKey)"/>
        /// </summary>
        [Test]
        public void OnRemovedInRemoveWhenOccurExceptionPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            var key = "A";
            helper.Add(key, 100);

            helper.OnRemoved.Add((_, __) => {
                throw new System.Exception();
            });

            helper.Remove(key);

            Assert.AreEqual(0, helper.Count);
            Assert.IsFalse(helper.ContainsKey(key));
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{T}.OnChangedCount"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Remove(TKey, TValue)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemovePasses()
        {
            var helper = new DictionaryHelper<string, int>();
            var key = "A";
            helper.Add(key, 100).Add("B", 200);

            int callCounter = 0;
            (IReadOnlyDictionaryHelper<string, int> self, int count) recievedData = default;
            helper.OnChangedCount.Add((self, count) => {
                callCounter++;
                recievedData.self = self;
                recievedData.count = count;
            });

            helper.Remove(key);

            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{T}.OnChangedCount"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Remove(TKey, TValue)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveNotChangedCountPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            var key = "A";
            helper.Add(key, 100).Add("B", 200);

            int callCounter = 0;
            helper.OnChangedCount.Add((_, __) => {
                callCounter++;
            });

            helper.Remove("Unknown");

            Assert.AreEqual(0, callCounter);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{T}.OnChangedCount"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Remove(TKey, TValue)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveWhenOccurExceptionPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            var key = "A";
            helper
                .Add(key, 100)
                .Add("B", 200);

            helper.OnChangedCount.Add((_, __) => {
                throw new System.Exception();
            });

            helper.Remove(key);

            AssertionUtils.AssertEnumerableByUnordered(
                new (string, int)[]
                {
                    ("B", 200),
                },
                helper.TupleItems,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{T}.OnChangedCount"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Remove(TKey, TValue)"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveWhenOccurOnRemovedExceptionPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            var key = "A";
            helper
                .Add(key, 100)
                .Add("B", 200);

            helper.OnRemoved.Add((_, __) => throw new System.Exception());

            int callCounter = 0;
            (IReadOnlyDictionaryHelper<string, int> self, int count) recievedData = default;
            helper.OnChangedCount.Add((self, count) => {
                callCounter++;
                recievedData.self = self;
                recievedData.count = count;
            });

            helper.Remove(key);

            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }
        #endregion

        #region DictionaryHelper.Remove Items
        /// <summary>
        /// <seealso cref="DictionaryHelper.Remove{TKey, TValue}(IEnumerable{TKey})"/>
        /// <seealso cref="DictionaryHelper.Remove{TKey, TValue}(TKey[])"/>
        /// </summary>
        [Test]
        public void RemoveItemsPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            var testData = new (string, int)[]
            {
                ("A", 100),
                ("B", 200),
                ("C", 300),
            };
            helper.Add(testData);

            helper.Remove("B", "C");

            AssertionUtils.AssertEnumerableByUnordered(
                new (string, int)[]
                {
                    ("A", 100)
                },
                helper.TupleItems,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper.Remove{TKey, TValue}(IEnumerable{TKey})"/>
        /// <seealso cref="DictionaryHelper.Remove{TKey, TValue}(TKey[])"/>
		/// </summary>
        [Test]
        public void RemoveItemsWhenNotContainsKeyPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            var testData = new (string, int)[]
            {
                ("A", 100),
                ("B", 200),
                ("C", 300),
            };
            helper.Add(testData);

            helper.Remove("Invalid", "C");

            AssertionUtils.AssertEnumerableByUnordered(
                new (string, int)[]
                {
                    ("A", 100),
                    ("B", 200),
                },
                helper.TupleItems,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.OnRemoved"/>
        /// <seealso cref="DictionaryHelper.Remove{TKey, TValue}(IEnumerable{TKey})"/>
        /// <seealso cref="DictionaryHelper.Remove{TKey, TValue}(TKey[])"/>
        /// </summary>
        [Test]
        public void OnRemovedInRemoveItemsPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            var testData = new (string, int)[]
            {
                ("A", 100),
                ("B", 200),
                ("C", 300),
            };
            helper.Add(testData);

            var recievedList = new List<(string key, int value)>();
            helper.OnRemoved.Add((key, value) => {
                recievedList.Add((key, value));
            });

            helper.Remove("B", "C");

            AssertionUtils.AssertEnumerableByUnordered(
                new (string, int)[]
                {
                    ("B", 200),
                    ("C", 300)
                },
                recievedList,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.OnRemoved"/>
        /// <seealso cref="DictionaryHelper.Remove{TKey, TValue}(IEnumerable{TKey})"/>
        /// <seealso cref="DictionaryHelper.Remove{TKey, TValue}(TKey[])"/>
        /// </summary>
        [Test]
        public void OnRemovedInRemoveItemsWhenOccurExceptionPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            var testData = new (string, int)[]
            {
                ("A", 100),
                ("B", 200),
                ("C", 300),
            };
            helper.Add(testData);

            helper.OnRemoved.Add((key, value) => {
                throw new System.Exception();
            });

            helper.Remove("B", "C");

            AssertionUtils.AssertEnumerableByUnordered(
                new (string, int)[]
                {
                    ("A", 100),
                },
                helper.TupleItems,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{T}.OnChangedCount"/>
        /// <seealso cref="DictionaryHelper.Remove{TKey, TValue}(KeyValuePair{TKey, TValue}[])"/>
        /// <seealso cref="DictionaryHelper.Remove{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}})"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveItemsPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            var testData = new (string, int)[]
            {
                ("A", 100),
                ("B", 200),
                ("C", 300),
            };
            helper.Add(testData);

            var callCounter = 0;
            var recievedData = (self: default(IReadOnlyDictionaryHelper<string, int>), count: default(int));
            helper.OnChangedCount.Add((self, count) => {
                callCounter++;
                recievedData.self = self;
                recievedData.count = count;
            });

            helper.Remove("B", "C");

            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{T}.OnChangedCount"/>
		/// <seealso cref="DictionaryHelper.Remove{TKey, TValue}(KeyValuePair{TKey, TValue}[])"/>
        /// <seealso cref="DictionaryHelper.Remove{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}})"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveItemsNotChangedCountPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            var testData = new (string, int)[]
            {
                ("A", 100),
                ("B", 200),
                ("C", 300),
            };
            helper.Add(testData);

            var callCounter = 0;
            helper.OnChangedCount.Add((self, count) => {
                callCounter++;
            });

            helper.Remove("Apple", "Orange");

            Assert.AreEqual(0, callCounter);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{T}.OnChangedCount"/>
		/// <seealso cref="DictionaryHelper.Remove{TKey, TValue}(KeyValuePair{TKey, TValue}[])"/>
        /// <seealso cref="DictionaryHelper.Remove{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}})"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveItemsWhenOccurExceptionPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            var testData = new (string, int)[]
            {
                ("A", 100),
                ("B", 200),
                ("C", 300),
            };
            helper.Add(testData);

            helper.OnChangedCount.Add((self, count) => {
                throw new System.Exception();
            });

            helper.Remove("B", "C");

            AssertionUtils.AssertEnumerableByUnordered(
                new (string, int)[] {
                    ("A", 100)
                },
                helper.TupleItems,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{T}.OnChangedCount"/>
		/// <seealso cref="DictionaryHelper.Remove{TKey, TValue}(KeyValuePair{TKey, TValue}[])"/>
        /// <seealso cref="DictionaryHelper.Remove{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}})"/>
        /// </summary>
        [Test]
        public void OnChangedCountInRemoveItemsWhenOccurOnRemovedExceptionPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            var testData = new (string, int)[]
            {
                ("A", 100),
                ("B", 200),
                ("C", 300),
            };
            helper.Add(testData);

            helper.OnRemoved.Add((_, __) => {
                throw new System.Exception();
            });
            var callCounter = 0;
            var recievedData = (self: default(IReadOnlyDictionaryHelper<string, int>), count: default(int));
            helper.OnChangedCount.Add((self, count) => {
                callCounter++;
                recievedData.self = self;
                recievedData.count = count;
            });

            helper.Remove("B", "C");

            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }
        #endregion

        #region DictionaryHelper.Clear()
        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Clear()"/>
        /// </summary>
        [Test]
        public void ClearPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            helper
                .Add("A", 10)
                .Add("B", 20);

            helper.Clear();

            Assert.AreEqual(0, helper.Count);
            Assert.IsFalse(helper.Items.Any());
            Assert.IsFalse(helper.TupleItems.Any());
        }

        /// <summary>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.OnRemoved"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Clear()"/>
        /// </summary>
        [Test]
        public void OnRemovedInClearPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            helper
                .Add("A", 10)
                .Add("B", 20);

            var recievedList = new List<(string key, int value)>();
            helper.OnRemoved.Add((key, value) => {
                recievedList.Add((key, value));
            });
            helper.Clear();

            AssertionUtils.AssertEnumerableByUnordered(
                new (string, int)[] {
                    ("A", 10),
                    ("B", 20),
                },
                recievedList,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.OnRemoved"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Clear()"/>
        /// </summary>
        [Test]
        public void OnRemovedInClearWhenOccurExceptionPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            helper
                .Add("A", 10)
                .Add("B", 20);

            helper.OnRemoved.Add((key, value) => {
                throw new System.Exception();
            });

            helper.Clear();

            Assert.AreEqual(0, helper.Count);
            Assert.IsFalse(helper.Items.Any());
            Assert.IsFalse(helper.TupleItems.Any());
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.OnCleared"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Clear()"/>
        /// </summary>
        [Test]
        public void OnClearedInClearPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            helper
                .Add("A", 10)
                .Add("B", 20);

            var callCounter = 0;
            helper.OnCleared.Add(() => {
                callCounter++;
            });

            helper.Clear();

            Assert.AreEqual(1, callCounter);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.OnCleared"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Clear()"/>
        /// </summary>
        [Test]
        public void OnClearedInClearWhenOccurExceptionPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            helper
                .Add("A", 10)
                .Add("B", 20);

            helper.OnCleared.Add(() => {
                throw new System.Exception();
            });

            helper.Clear();

            Assert.AreEqual(0, helper.Count);
            Assert.IsFalse(helper.Items.Any());
            Assert.IsFalse(helper.TupleItems.Any());
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{T}.OnChangedCount"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Clear()"/>
        /// </summary>
        [Test]
        public void OnChangedCountInClearPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            helper
                .Add("A", 10)
                .Add("B", 20);

            var callCounter = 0;
            var recievedData = (self: default(IReadOnlyDictionaryHelper<string, int>), count: default(int));
            helper.OnChangedCount.Add((self, count) => {
                callCounter++;
                recievedData.self = self;
                recievedData.count = count;
            });

            helper.Clear();

            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{T}.OnChangedCount"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Clear()"/>
        /// </summary>
        [Test]
        public void OnChangedCountInClearNotChangedCountPasses()
        {
            var helper = new DictionaryHelper<string, int>();

            var callCounter = 0;
            helper.OnChangedCount.Add((self, count) => {
                callCounter++;
            });

            helper.Clear();

            Assert.AreEqual(0, callCounter);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{T}.OnChangedCount"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Clear()"/>
        /// </summary>
        [Test]
        public void OnChangedCountInClearWhenOccurExceptionPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            helper
                .Add("A", 10)
                .Add("B", 20);

            helper.OnChangedCount.Add((self, count) => {
                throw new System.Exception();
            });

            helper.Clear();

            AssertionUtils.AssertEnumerableByUnordered(
                new (string, int)[] {},
                helper.TupleItems,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{T}.OnChangedCount"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.Clear()"/>
        /// </summary>
        [Test]
        public void OnChangedCountInClearWhenOccurOnRemovedExceptionPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            helper
                .Add("A", 10)
                .Add("B", 20);

            helper.OnRemoved.Add((_, __) => throw new System.Exception());

            var callCounter = 0;
            var recievedData = (self: default(IReadOnlyDictionaryHelper<string, int>), count: default(int));
            helper.OnChangedCount.Add((self, count) => {
                callCounter++;
                recievedData.self = self;
                recievedData.count = count;
            });

            helper.Clear();

            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }
        #endregion

        #region DictionaryHelper.ContainsKey/Value
        /// <summary>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.ContainsKey(TKey)"/>
		/// </summary>
        [Test]
        public void ContainsKeyPasses()
        {
            var helper = new DictionaryHelper<string, int>();

            helper
                .Add("A", 100)
                .Add("B", 200);

            Assert.IsTrue(helper.ContainsKey("A"));
            Assert.IsTrue(helper.ContainsKey("B"));

            Assert.IsFalse(helper.ContainsKey("C"));
            Assert.IsFalse(helper.ContainsKey("invalid"));
        }

        /// <summary>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.ContainsValue(TValue)"/>
		/// </summary>
        [Test]
        public void ContainsValuePasses()
        {
            var helper = new DictionaryHelper<string, int>();

            helper
                .Add("A", 100)
                .Add("B", 200);

            Assert.IsTrue(helper.ContainsValue(100));
            Assert.IsTrue(helper.ContainsValue(200));

            Assert.IsFalse(helper.ContainsValue(-1234));
            Assert.IsFalse(helper.ContainsValue(2983));
        }
        #endregion

        #region Collection
        /// <summary>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Items"/>
		/// </summary>
        [Test]
        public void ItemsPasses()
        {
            var helper = new DictionaryHelper<string, int>();

            helper
                .Add("A", 100)
                .Add("B", 200);

            AssertionUtils.AssertEnumerableByUnordered(
                new KeyValuePair<string, int>[] {
                    new KeyValuePair<string, int>("A", 100),
                    new KeyValuePair<string, int>("B", 200),
                },
                helper.Items,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.TupleItems"/>
        /// </summary>
        [Test]
        public void TupleItemsPasses()
        {
            var helper = new DictionaryHelper<string, int>();

            helper
                .Add("A", 100)
                .Add("B", 200);

            AssertionUtils.AssertEnumerableByUnordered(
                new (string, int)[] {
                    ("A", 100),
                    ("B", 200),
                },
                helper.TupleItems,
                ""
            );
        }

        /// <summary>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Keys"/>
		/// </summary>
        [Test]
        public void KeysPasses()
        {
            var helper = new DictionaryHelper<string, int>();

            helper
                .Add("A", 100)
                .Add("B", 200);

            AssertionUtils.AssertEnumerableByUnordered(
                new string[] {
                    "A",
                    "B",
                },
                helper.Keys,
                ""
            );
        }

        /// <summary>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Values"/>
		/// </summary>
        [Test]
        public void ValuesPasses()
        {
            var helper = new DictionaryHelper<string, int>();

            helper
                .Add("A", 100)
                .Add("B", 200);

            AssertionUtils.AssertEnumerableByUnordered(
                new int[] {
                    200,
                    100,
                },
                helper.Values,
                ""
            );
        }

        /// <summary>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.Count"/>
		/// </summary>
        [Test]
        public void CountPasses()
        {
            var helper = new DictionaryHelper<string, int>();

            helper
                .Add("A", 100)
                .Add("B", 200);

            Assert.AreEqual(2, helper.Count);
        }
        #endregion

        #region IEnumerable
        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.GetEnumerator()"/>
        /// </summary>
        [Test]
        public void GetEnumeratorPasses()
        {
            var helper = new DictionaryHelper<string, int>();

            helper
                .Add("A", 100)
                .Add("B", 200);

            AssertionUtils.AssertEnumerableByUnordered(
                new (string, int)[] {
                    ("A", 100),
                    ("B", 200),
                },
                helper.TupleItems,
                ""
            );
        }
        #endregion

        #region DictionaryHelper this[TKey]
        /// <summary>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.this[TKey]"/>
		/// </summary>
        [Test]
        public void IndexerGetPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            helper.Add("A", 100);

            Assert.AreEqual(100, helper["A"]);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.this[TKey]"/>
        /// </summary>
        [Test]
        public void IndexerGetFails()
        {
            var helper = new DictionaryHelper<string, int>();

            Assert.Throws<KeyNotFoundException>(() => { var v = helper["A"]; });
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.this[TKey]"/>
        /// </summary>
        [Test]
        public void IndexerSetPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            helper.Add("A", 100);

            helper["A"] = 200;
            Assert.AreEqual(200, helper["A"]);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.this[TKey]"/>
        /// </summary>
        [Test]
        public void IndexerSetWhenNotContainsKeyFails()
        {
            var helper = new DictionaryHelper<string, int>();

            helper["A"] = 200;

            Assert.AreEqual(1, helper.Count);
            Assert.IsTrue(helper.ContainsKey("A"));
            Assert.AreEqual(200, helper["A"]);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.OnAdded"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.this[TKey]"/>
        /// </summary>
        [Test]
        public void OnAddedInIndexerSetPasses()
        {
            var helper = new DictionaryHelper<string, int>();

            var callCounter = 0;
            var recievedData = default((string key, int value));
            helper.OnAdded.Add((key, value) => {
                callCounter++;
                recievedData.key = key;
                recievedData.value = value;
            });

            helper["A"] = 200;
            Assert.AreEqual(1, callCounter);
            Assert.AreEqual("A", recievedData.key);
            Assert.AreEqual(200, recievedData.value);
        }

        /// <summary>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.OnAdded"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.this[TKey]"/>
        /// </summary>
        [Test]
        public void OnAddedInIndexerSetWhenOccurExceptionPasses()
        {
            var helper = new DictionaryHelper<string, int>();

            helper.OnAdded.Add((key, value) => {
                throw new System.Exception();
            });

            helper["A"] = 200;

            Assert.AreEqual(1, helper.Count);
            Assert.IsTrue(helper.ContainsKey("A"));
            Assert.AreEqual(200, helper["A"]);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.OnSwaped"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.this[TKey]"/>
        /// </summary>
        [Test]
        public void OnSwapedInIndexerSetPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            helper.Add("A", 100);

            var callCounter = 0;
            var recievedData = default((string key, int oldValue, int newValue));
            helper.OnSwaped.Add((key, oldValue, newValue) => {
                callCounter++;
                recievedData.key = key;
                recievedData.oldValue = oldValue;
                recievedData.newValue = newValue;
            });

            helper["A"] = 200;
            Assert.AreEqual(1, callCounter);
            Assert.AreEqual("A", recievedData.key);
            Assert.AreEqual(100, recievedData.oldValue);
            Assert.AreEqual(200, recievedData.newValue);
        }

        /// <summary>
		/// <seealso cref="DictionaryHelper{TKey, TValue}.OnSwaped"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.this[TKey]"/>
        /// </summary>
        [Test]
        public void OnSwapedInIndexerSetWhenOccurExceptionPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            helper.Add("A", 100);

            helper.OnSwaped.Add((key, oldValue, newValue) => {
                throw new System.Exception();
            });

            helper["A"] = 200;
            Assert.AreEqual(200, helper["A"]);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.OnChangedCount"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.this[TKey]"/>
        /// </summary>
        [Test]
        public void OnChangedCountedInIndexerAddtionSetPasses()
        {
            var helper = new DictionaryHelper<string, int>();

            var callCounter = 0;
            var recievedData = default((IReadOnlyDictionaryHelper<string, int> self, int count));
            helper.OnChangedCount.Add((self, count) => {
                callCounter++;
                recievedData.self = self;
                recievedData.count = count;
            });

            helper["A"] = 200;

            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.OnChangedCount"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.this[TKey]"/>
        /// </summary>
        [Test]
        public void OnChangedCountedInIndexerAddtionSetWhenOccurExceptionPasses()
        {
            var helper = new DictionaryHelper<string, int>();

            helper.OnChangedCount.Add((self, count) => {
                throw new System.Exception();
            });

            helper["A"] = 200;

            Assert.AreEqual(1, helper.Count);
            Assert.IsTrue(helper.ContainsKey("A"));
            Assert.AreEqual(200, helper["A"]);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.OnChangedCount"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.this[TKey]"/>
        /// </summary>
        [Test]
        public void OnChangedCountedInIndexerAddtionSetWhenOccurOnAddedExceptionPasses()
        {
            var helper = new DictionaryHelper<string, int>();

            helper.OnAdded.Add((_, __) => throw new System.Exception());

            var callCounter = 0;
            var recievedData = default((IReadOnlyDictionaryHelper<string, int> self, int count));
            helper.OnChangedCount.Add((self, count) => {
                callCounter++;
                recievedData.self = self;
                recievedData.count = count;
            });

            helper["A"] = 200;

            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(helper, recievedData.self);
            Assert.AreEqual(helper.Count, recievedData.count);
        }

        /// <summary>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.OnChangedCount"/>
        /// <seealso cref="DictionaryHelper{TKey, TValue}.this[TKey]"/>
        /// </summary>
        [Test]
        public void OnChangedCountedInIndexerSwapSetPasses()
        {
            var helper = new DictionaryHelper<string, int>();
            helper["A"] = 100;

            var callCounter = 0;
            helper.OnChangedCount.Add((self, count) => {
                callCounter++;
            });

            helper["A"] = 200;

            Assert.AreEqual(0, callCounter);
        }
        #endregion
    }
}
