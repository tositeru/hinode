using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using Hinode.Tests;

namespace Hinode.Layouts.Tests
{
    /// <summary>
	/// <seealso cref="LayoutManager"/>
	/// </summary>
    public class TestLayoutManager
    {
        class LayoutCallbackCounter : LayoutBase
        {
            public void ResetCounter()
            {
                CallUpdateUnitSizeCounter = 0;
                CallUpdateLayoutCounter = 0;
            }
            public int CallUpdateUnitSizeCounter { get; private set; }
            public int CallUpdateLayoutCounter { get; private set; }

            public override LayoutOperationTarget OperationTargetFlags { get => 0; }

            public void SetDoChanged(bool doChanged) => DoChanged = doChanged;

            public override void UpdateUnitSize()
            {
                CallUpdateUnitSizeCounter++;
            }

            public override void UpdateLayout()
            {
                CallUpdateLayoutCounter++;
            }
        }

        /// <summary>
		/// <seealso cref="LayoutManager.Layouts.OnAdded"/>
		/// </summary>
        [Test]
        public void OnAddedILayoutPasses()
        {
            var manager = new LayoutManager();
            var layout = new LayoutCallbackCounter();

            manager.Layouts.Add(layout);

            Assert.AreEqual(1, layout.OnDisposed.RegistedDelegateCount);
            Assert.IsTrue(manager.Layouts.Contains(layout));
        }

        /// <summary>
		/// <seealso cref="LayoutManager.Layouts.OnAdded"/>
		/// <seealso cref="ILayout.OnDisposed"/>
		/// </summary>
        [Test]
        public void LayoutOnDisposedPasses()
        {
            var manager = new LayoutManager();
            var layout = new LayoutCallbackCounter();

            manager.Layouts.Add(layout);

            layout.Dispose();
            Assert.IsFalse(manager.Layouts.Contains(layout));
        }

        /// <summary>
        /// <seealso cref="LayoutManager.Layouts.OnAdded"/>
        /// <seealso cref="ILayout.OnDisposed"/>
        /// </summary>
        [Test]
        public void LayoutOnDisposedAfterLayoutDisposePasses()
        {
            var manager = new LayoutManager();
            var layout = new LayoutCallbackCounter();

            manager.Layouts.Add(layout);

            layout.Dispose();

            Assert.DoesNotThrow(() =>  layout.Dispose() );
            Assert.IsFalse(manager.Layouts.Contains(layout));
        }

        /// <summary>
        /// <seealso cref="LayoutManager.Layouts.OnRemoved"/>
        /// <seealso cref="ILayout.OnDisposed"/>
        /// </summary>
        [Test]
        public void OnRemovedILayoutPasses()
        {
            var manager = new LayoutManager();
            var layout = new LayoutCallbackCounter();

            manager.Layouts.Add(layout);

            manager.Layouts.Remove(layout);

            Assert.AreEqual(0, layout.OnDisposed.RegistedDelegateCount);
            Assert.IsFalse(manager.Layouts.Contains(layout));
        }

        /// <summary>
		/// <seealso cref="LayoutManager.CaluculateLayouts()"/>
		/// </summary>
        [Test]
        public void CaluculateLayoutsPasses()
        {
            var manager = new LayoutManager();
            var layouts = new LayoutCallbackCounter[] {
                new LayoutCallbackCounter() { Target = new LayoutTargetObject() },
                new LayoutCallbackCounter() { Target = new LayoutTargetObject() },
                new LayoutCallbackCounter() { Target = new LayoutTargetObject() },
            };
            manager.Layouts.Add(layouts);

            {
                foreach(var l in layouts) { l.ResetCounter(); l.SetDoChanged(true); }

                manager.CaluculateLayouts();
                Assert.AreEqual(layouts.Length, layouts.Sum(_l => _l.CallUpdateUnitSizeCounter));
                Assert.AreEqual(layouts.Length, layouts.Sum(_l => _l.CallUpdateLayoutCounter));
            }
            Debug.Log($"Success when All ILayout#DoChanged is true!");

            {
                foreach (var l in layouts) { l.ResetCounter(); l.SetDoChanged(true); }

                layouts[1].SetDoChanged(false);
                layouts[0].SetDoChanged(false);

                manager.CaluculateLayouts();
                Assert.AreEqual(layouts.Length-2, layouts.Sum(_l => _l.CallUpdateUnitSizeCounter));
                Assert.AreEqual(layouts.Length, layouts.Sum(_l => _l.CallUpdateLayoutCounter));
            }
            Debug.Log($"Success when a two in ILayout#DoChanged is false!");
        }

        /// <summary>
        /// <seealso cref="LayoutManager.CaluculateLayouts()"/>
        /// </summary>
        [Test]
        public void CaluculateLayoutsIncludeNotContainsTargetPasses()
        {
            var manager = new LayoutManager();
            var layouts = new LayoutCallbackCounter[] {
                new LayoutCallbackCounter() { Target = new LayoutTargetObject() },
                new LayoutCallbackCounter() { Target = null },
                new LayoutCallbackCounter() { Target = new LayoutTargetObject() },
            };
            foreach (var l in layouts) { l.SetDoChanged(true); }
            manager.Layouts.Add(layouts);

            manager.CaluculateLayouts();
            var testData = new (int callCounterUnitSize, int callCounterLayout)[]
            {
                (1, 1),
                (0, 0),
                (1, 1),
            };
            foreach(var (l, d, index) in layouts
                .Zip(testData, (_t, _d) => (_t, _d))
                .Zip(Enumerable.Range(0, layouts.Length), (_t, _i) => (_t.Item1, _t.Item2, _i)))
            {
                var errorMessage = $"Fail... index={index}";
                Assert.AreEqual(d.callCounterUnitSize, l.CallUpdateUnitSizeCounter, errorMessage);
                Assert.AreEqual(d.callCounterLayout, l.CallUpdateLayoutCounter, errorMessage);
            }
        }
    }
}
