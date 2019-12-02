using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Tests;
using System.Linq;

namespace Hinode.MVC.Tests.ViewLayout
{
    /// <summary>
	/// <seealso cref="ViewLayoutState"/>
	/// </summary>
    public class TestViewLayoutState
    {
        enum TestKey
        {
            key1,
            key2,
        }

        // A Test behaves as an ordinary method
        [Test]
        public void BasicUsagePasses()
        {
            var testData = new (string key, object value)[] {
                (TestKey.key1.ToString(), 100),
                (TestKey.key2.ToString(), "hoge"),
            };

            var state = new ViewLayoutState();
            {
                foreach (var d in testData)
                {
                    state.SetRaw(d.key, d.value);
                }
                AssertionUtils.AssertEnumerableByUnordered(
                    testData
                    , state.KeyAndValues
                    , ""
                );
            }
            Debug.Log($"Success to Add Raw KeyAndValue!");

            {
                state.RemoveRaw(TestKey.key1);
                Assert.IsFalse(state.ContainsKey(TestKey.key1));
            }
            Debug.Log($"Success to Remove Key1({TestKey.key1})!");
            {
                state.RemoveRaw(TestKey.key2);
                Assert.IsFalse(state.ContainsKey(TestKey.key2));
            }
            Debug.Log($"Success to Remove Key2({TestKey.key2})!");
        }

        [Test]
        public void ClearPasses()
        {
            var bindInfo = new ModelViewBinder.BindInfo(typeof(EmptyViewObject));
            bindInfo.AddViewLayoutValue("apple", "hoge");

            var stylingID = ".style1";
            var testData = new (string key, object value)[] {
                ("orange", 100),
            };
            var viewLayoutOverwriter = new ViewLayoutOverwriter();
            viewLayoutOverwriter.Add(new ViewLayoutSelector(stylingID, ""), new ViewLayoutValueDictionary().AddKeyAndValues(testData));

            var useModel = new Model() { Name = "TestModel" }
                .AddStylingID(stylingID);
            var useViewObj = new EmptyViewObject();
            useViewObj.Bind(useModel, new ModelViewBinder.BindInfo(typeof(EmptyViewObject)), null);

            var state = new ViewLayoutState();
            state
                .SetRaw(TestKey.key1, 100)
                .SetRaw(TestKey.key2, "hoge")
                .SetBindInfo(bindInfo)
                .SetLayoutOverwriter(useViewObj, viewLayoutOverwriter);

            state.Clear();
            Assert.AreEqual(0, state.Count);
            Assert.IsFalse(state.ContainsKey(TestKey.key1));
            Assert.IsFalse(state.ContainsKey(TestKey.key2));
            Assert.IsFalse(state.ContainsKey("apple"));
            Assert.IsFalse(state.ContainsKey("orange"));
        }

        [Test]
        public void ViewLayoutValueDictionaryPasses()
        {
            var testData = new (string key, object value)[] {
                (TestKey.key1.ToString(), 100),
                (TestKey.key2.ToString(), "hoge"),
            };
            var viewLayoutValueDictionary = new ViewLayoutValueDictionary();
            viewLayoutValueDictionary.AddKeyAndValues(testData);

            var state = new ViewLayoutState();

            {
                state.SetRaw(viewLayoutValueDictionary);
                AssertionUtils.AssertEnumerableByUnordered(
                    testData
                    , state.KeyAndValues
                    ,""
                );
            }
            Debug.Log($"Success to Add ViewLayoutValueDictionary!");
            {
                state.RemoveRaw(viewLayoutValueDictionary);
                AssertionUtils.AssertEnumerableByUnordered(
                    new (string key, object value)[] { }
                    , state.KeyAndValues
                    , ""
                );
            }
            Debug.Log($"Success to Remove ViewLayoutValueDictionary!");
        }

        [Test]
        public void BindInfoPasses()
        {
            var testData = new (string key, object value)[] {
                (TestKey.key1.ToString(), 100),
                (TestKey.key2.ToString(), "hoge"),
            };
            var bindInfo = new ModelViewBinder.BindInfo(typeof(EmptyViewObject));
            bindInfo.AddViewLayoutValues(testData);

            var state = new ViewLayoutState();
            Assert.IsFalse(state.ContainsBindInfo);
            Assert.IsNull(state.UseBindInfo);

            {
                state.SetBindInfo(bindInfo);
                Assert.IsTrue(state.ContainsBindInfo);
                Assert.AreSame(bindInfo, state.UseBindInfo);
                AssertionUtils.AssertEnumerableByUnordered(
                    testData
                    , state.KeyAndValues
                    , ""
                );
            }
            Debug.Log($"Success to Add BindInfo!");
            {
                state.RemoveBindInfo();
                Assert.IsFalse(state.ContainsBindInfo);
                Assert.IsNull(state.UseBindInfo);
                AssertionUtils.AssertEnumerableByUnordered(
                    new (string key, object value)[] { }
                    , state.KeyAndValues
                    , ""
                );
            }
            Debug.Log($"Success to Remove BindInfo!");
        }

        [Test]
        public void OverwriteBindInfoPasses()
        {
            var testData1 = new (string key, object value)[] {
                (TestKey.key1.ToString(), 100),
            };
            var bindInfo1 = new ModelViewBinder.BindInfo(typeof(EmptyViewObject))
                .AddViewLayoutValues(testData1);

            var testData2 = new (string key, object value)[] {
                (TestKey.key2.ToString(), -100),
            };
            var bindInfo2 = new ModelViewBinder.BindInfo(typeof(EmptyViewObject))
                .AddViewLayoutValues(testData2);

            var state = new ViewLayoutState();
            state.SetBindInfo(bindInfo1);
            state.SetBindInfo(bindInfo2); // <- Use BindInfo

            Assert.AreSame(bindInfo2, state.UseBindInfo);
            Assert.AreEqual(1, state.Count);
            Assert.AreEqual(testData2[0].value, state.GetValue(testData2[0].key));

            state.RemoveBindInfo();
            Assert.AreEqual(0, state.Count);
            Assert.IsFalse(state.ContainsBindInfo);
            Assert.IsNull(state.UseBindInfo);
        }

        [Test]
        public void ViewLayoutOverwritterPasses()
        {
            var stylingID = ".style1";
            var testData = new (string key, object value)[] {
                (TestKey.key1.ToString(), 100),
                (TestKey.key2.ToString(), "hoge"),
            };
            var viewLayoutOverwriter = new ViewLayoutOverwriter();
            viewLayoutOverwriter.Add(new ViewLayoutSelector(stylingID, ""), new ViewLayoutValueDictionary().AddKeyAndValues(testData));

            var state = new ViewLayoutState();
            Assert.IsFalse(state.ContainsLayoutOverwriter);
            Assert.IsNull(state.UseModel);
            Assert.IsNull(state.UseViewObject);

            var useModel = new Model() { Name = "TestModel" }
                .AddStylingID(stylingID);
            var useViewObj = new EmptyViewObject();
            useViewObj.Bind(useModel, new ModelViewBinder.BindInfo(typeof(EmptyViewObject)), null);

            {
                state.SetLayoutOverwriter(useViewObj, viewLayoutOverwriter);
                Assert.IsTrue(state.ContainsLayoutOverwriter);
                Assert.AreSame(useModel, state.UseModel);
                Assert.AreSame(useViewObj, state.UseViewObject);
                AssertionUtils.AssertEnumerableByUnordered(
                    testData
                    , state.KeyAndValues
                    , ""
                );
            }
            Debug.Log($"Success to Add ViewLayoutOverwriter!");
            {
                state.RemoveLayoutOverwriter();
                Assert.IsFalse(state.ContainsLayoutOverwriter);
                Assert.IsNull(state.UseModel);
                Assert.IsNull(state.UseViewObject);
                AssertionUtils.AssertEnumerableByUnordered(
                    new (string key, object value)[] { }
                    , state.KeyAndValues
                    , ""
                );
            }
            Debug.Log($"Success to Remove ViewLayoutOverwriter!");
        }

        [Test]
        public void RemoveAtEmptyPasses()
        {
            var state = new ViewLayoutState();
            Assert.AreSame(state, state.RemoveRaw(TestKey.key1));
            Assert.AreSame(state, state.RemoveBindInfo());
            Assert.AreSame(state, state.RemoveLayoutOverwriter());
        }

        [Test]
        public void ViewLayoutOverwritterNotBindingViewObjectFail()
        {
            var stylingID = ".style1";
            var testData = new (string key, object value)[] {
                (TestKey.key1.ToString(), 100),
                (TestKey.key2.ToString(), "hoge"),
            };
            var viewLayoutOverwriter = new ViewLayoutOverwriter();
            viewLayoutOverwriter.Add(new ViewLayoutSelector(stylingID, ""), new ViewLayoutValueDictionary().AddKeyAndValues(testData));

            var state = new ViewLayoutState();
            Assert.IsFalse(state.ContainsLayoutOverwriter);
            Assert.IsNull(state.UseModel);
            Assert.IsNull(state.UseViewObject);

            var useViewObj = new EmptyViewObject();
            Assert.IsFalse(useViewObj.DoBinding()); // <- Not Bind!!

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                state.SetLayoutOverwriter(useViewObj, viewLayoutOverwriter);
            });
        }

        [Test]
        public void PriorityOrderPasses()
        {
            var stylingID = ".style1";
            var modelName = "TestModel";

            var testKeyAndValue = (key: TestKey.key1.ToString(), value: 100);

            var bindInfo = new ModelViewBinder.BindInfo(typeof(EmptyViewObject));
            bindInfo.AddViewLayoutValue(TestKey.key1, 222);

            var viewLayoutOverwriter = new ViewLayoutOverwriter();
            viewLayoutOverwriter
                .Add(new ViewLayoutSelector(stylingID, ""), new ViewLayoutValueDictionary().AddValue(TestKey.key1, 444));

            var useModel = new Model() { Name = modelName }
                .AddStylingID(stylingID);
            var useViewObj = new EmptyViewObject();
            useViewObj.Bind(useModel, new ModelViewBinder.BindInfo(typeof(EmptyViewObject)), null);

            var state = new ViewLayoutState();
            state.SetRaw(testKeyAndValue.key, testKeyAndValue.value);
            state.SetBindInfo(bindInfo);
            state.SetLayoutOverwriter(useViewObj, viewLayoutOverwriter);

            {
                Assert.AreEqual(testKeyAndValue.value, state.GetValue(TestKey.key1));
            }
            Debug.Log($"Success Layout Priority(Raw Value is Highest Priority)!");

            {
                state.RemoveRaw(TestKey.key1);
                var overwritedLayouts = viewLayoutOverwriter.MatchLayoutValueDicts(useModel, useViewObj).First();
                Assert.AreEqual(overwritedLayouts.GetValue(TestKey.key1), state.GetValue(TestKey.key1));
            }
            Debug.Log($"Success Layout Priority(ViewLayoutOverwriter is Second High Priority)!");

            {
                state.RemoveLayoutOverwriter();
                Assert.AreEqual(bindInfo.GetViewLayoutValue(TestKey.key1), state.GetValue(TestKey.key1));
            }
            Debug.Log($"Success Layout Priority(BindInfo is Lowest Priority)!");

            {
                state.RemoveBindInfo();
                Assert.IsFalse(state.ContainsKey(TestKey.key1));
            }
            Debug.Log($"Success Layout Priority(Key don't exist)!");
        }

        [Test]
        public void LayoutOverwriterPriorityPasses()
        {
            var stylingID = ".style1";
            var modelName = "TestModel";

            var testKeyAndValue = (key: TestKey.key1.ToString(), value: 100);

            var viewLayoutOverwriter = new ViewLayoutOverwriter();
            viewLayoutOverwriter
                .Add(new ViewLayoutSelector($"{modelName} {stylingID}", ""), new ViewLayoutValueDictionary().AddValue(TestKey.key1, 444))
                .Add(new ViewLayoutSelector(stylingID, ""), new ViewLayoutValueDictionary().AddValue(TestKey.key1, 444));

            var useModel = new Model() { Name = modelName }
                .AddStylingID(stylingID);
            var useViewObj = new EmptyViewObject();
            useViewObj.Bind(useModel, new ModelViewBinder.BindInfo(typeof(EmptyViewObject)), null);

            var state = new ViewLayoutState();
            state.SetLayoutOverwriter(useViewObj, viewLayoutOverwriter);

            {
                var fristLayoutOverwriters = viewLayoutOverwriter.MatchLayoutValueDicts(useModel, useViewObj).First();
                Assert.AreEqual(fristLayoutOverwriters.GetValue(TestKey.key1), state.GetValue(TestKey.key1));
            }
            Debug.Log($"Success ViewLayoutOverwriter Priority(Highest QueryPriority)!");

            {
                useModel.Name = "otherName";

                var overwritedLayouts = viewLayoutOverwriter.MatchLayoutValueDicts(useModel, useViewObj).First();
                Assert.AreEqual(overwritedLayouts.GetValue(TestKey.key1), state.GetValue(TestKey.key1));
            }
            Debug.Log($"Success ViewLayoutOverwriter Priority(Second High QueryPriority)!");

            {
                useModel.RemoveStylingID(stylingID);

                Assert.IsFalse(state.ContainsKey(TestKey.key1));
            }
            Debug.Log($"Success ViewLayoutOverwriter Priority(Key don't exist)!");
        }

        [Test]
        public void OnChangedValueCallbackPasses()
        {
            var layoutState = new ViewLayoutState();
            var cachedKeyValue = (changedType: (int)-1, key: "", value: (object)null);
            layoutState.OnChangedValue.Add((ViewLayoutState.OnUpdatedCallbackData eventData) => {
                cachedKeyValue.changedType = (int)eventData.ChangedType;
                cachedKeyValue.key = eventData.Key;
                cachedKeyValue.value = eventData.Value;
            });

            {
                cachedKeyValue = (changedType: (int)-1, key: "", value: (object)null);
                string key = "key";
                object value = 100;
                layoutState.SetRaw(key, value);
                Assert.AreEqual((int)ViewLayoutState.OnChangedType.Set, cachedKeyValue.changedType);
                Assert.AreEqual(key, cachedKeyValue.key);
                Assert.AreEqual(value, cachedKeyValue.value);

                cachedKeyValue = (changedType: (int)-1, key: "", value: (object)null);
                layoutState.RemoveRaw(key);
                Assert.AreEqual((int)ViewLayoutState.OnChangedType.Remove, cachedKeyValue.changedType);
                Assert.AreEqual(key, cachedKeyValue.key);
                Assert.IsNull(cachedKeyValue.value);
            }
            Debug.Log($"Success to OnChangedValue Callback(When SetRaw/RemoveRaw)");

            {
                cachedKeyValue = (changedType: (int)-1, key: "", value: (object)null);
                string key = "key";
                object value = 100;
                var bindInfo = new ModelViewBinder.BindInfo(typeof(EmptyViewObject))
                    .AddViewLayoutValue(key, value);
                layoutState.SetBindInfo(bindInfo);
                Assert.AreEqual((int)ViewLayoutState.OnChangedType.Set, cachedKeyValue.changedType);
                Assert.AreEqual(key, cachedKeyValue.key);
                Assert.AreEqual(value, cachedKeyValue.value);

                cachedKeyValue = (changedType: (int)-1, key: "", value: (object)null);
                layoutState.RemoveBindInfo();
                Assert.AreEqual((int)ViewLayoutState.OnChangedType.Remove, cachedKeyValue.changedType);
                Assert.AreEqual(key, cachedKeyValue.key);
                Assert.IsNull(cachedKeyValue.value);
            }
            Debug.Log($"Success to OnChangedValue Callback(When SetBindInfo/RemoveBindInfo)");

            {
                cachedKeyValue = (changedType: (int)-1, key: "", value: (object)null);
                string modelName = "TestModel";

                string key = "key";
                object value = 100;
                var viewLayoutOverwriter = new ViewLayoutOverwriter();
                viewLayoutOverwriter
                    .Add(new ViewLayoutSelector($"{modelName}", ""), new ViewLayoutValueDictionary()
                        .AddValue(key, value));

                var useModel = new Model() { Name = modelName };
                var useViewObj = new EmptyViewObject();
                useViewObj.Bind(useModel, new ModelViewBinder.BindInfo(typeof(EmptyViewObject)), null);

                layoutState.SetLayoutOverwriter(useViewObj, viewLayoutOverwriter);
                Assert.AreEqual(key, cachedKeyValue.key);
                Assert.AreEqual(value, cachedKeyValue.value);


                cachedKeyValue = (changedType: (int)-1, key: "", value: (object)null);
                layoutState.RemoveLayoutOverwriter();
                Assert.AreEqual((int)ViewLayoutState.OnChangedType.Remove, cachedKeyValue.changedType);
                Assert.AreEqual(key, cachedKeyValue.key);
                Assert.IsNull(cachedKeyValue.value);
            }
            Debug.Log($"Success to OnChangedValue Callback(When SetLayoutOverwriter/RemoveLayoutOverwriter)");
        }

        [Test]
        public void LayoutOverwriterOnChangedValueCallbackPasses()
        {
            var layoutState = new ViewLayoutState();
            Dictionary<string, int> counter = new Dictionary<string, int>();
            layoutState.OnChangedValue.Add((ViewLayoutState.OnUpdatedCallbackData eventData) => {
                if (counter.ContainsKey(eventData.Key))
                {
                    counter[eventData.Key]++;
                }
                else
                {
                    counter.Add(eventData.Key, 1);
                }
            });

            {
                string modelName = "TestModel";
                string styleID = ".style";

                string key = "key";
                object value = 100;
                var viewLayoutOverwriter = new ViewLayoutOverwriter();
                viewLayoutOverwriter
                    .Add(new ViewLayoutSelector($"{modelName} {styleID}", ""), new ViewLayoutValueDictionary()
                        .AddValue(key, value))
                    .Add(new ViewLayoutSelector($"{modelName}", ""), new ViewLayoutValueDictionary()
                        .AddValue(key, value));

                var useModel = new Model() { Name = modelName }
                    .AddStylingID(styleID);
                var useViewObj = new EmptyViewObject();
                useViewObj.Bind(useModel, new ModelViewBinder.BindInfo(typeof(EmptyViewObject)), null);

                layoutState.SetLayoutOverwriter(useViewObj, viewLayoutOverwriter);
                Assert.IsTrue(counter.ContainsKey(key));
                Assert.AreEqual(1, counter[key]);

                counter.Clear();

                layoutState.RemoveLayoutOverwriter();
                Assert.IsTrue(counter.ContainsKey(key));
                Assert.AreEqual(1, counter[key]);
            }
            Debug.Log($"Success to call OnChangedValue when the same key on ViewLayoutOverwriter LayoutValue Dictionaries!");
        }
    }
}
