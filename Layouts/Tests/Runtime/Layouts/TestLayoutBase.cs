using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Layouts.Tests
{
    /// <summary>
    /// <seealso cref="LayoutBase"/>
    /// </summary>
    public class TestLayoutBase
    {
        class LayoutClass : LayoutBase
        {
            bool _doChanged;

            public void SetDoChanged(bool doChanged)
            {
                DoChanged = doChanged;
            }

            public int CallUpdateLayoutCounter { get; set; }

            public override void UpdateLayout()
            {
                CallUpdateLayoutCounter++;
            }

            public override bool Validate() => true;

            public int CallCounterOnChangedTarget { get; set; }
            public ILayoutTarget CurrentOnChangedTarget { get; set;}
            public ILayoutTarget PrevOnChangedTarget { get; set; }
            protected override void InnerOnChangedTarget(ILayoutTarget current, ILayoutTarget prev)
            {
                CallCounterOnChangedTarget++;
                CurrentOnChangedTarget = current;
                PrevOnChangedTarget = prev;
            }

            public int CallCounterOnChanged { get; set; }
            public bool DoChangedOnChanged { get; set; }

            public override LayoutOperationTarget OperationTargetFlags { get => 0; }

            protected override void InnerOnChanged(bool doChanged)
            {
                CallCounterOnChanged++;
                DoChangedOnChanged = doChanged;
            }
        }

        #region OperationPriority Property
        /// <summary>
        /// <seealso cref="LayoutBase.OperationPriority"/>
        /// </summary>
        [Test]
        public void OperationPriorityPasses()
        {
            var layout = new LayoutClass();
            layout.OperationPriority = 100;
            Assert.AreEqual(100, layout.OperationPriority);

            layout.OperationPriority = 101;
            Assert.AreEqual(101, layout.OperationPriority);
        }

        /// <summary>
        /// <seealso cref="LayoutBase.OperationPriority"/>
        /// <seealso cref="LayoutBase.OnChangedOperationPriority"/>
        /// </summary>
        [Test]
        public void OnChangedOperationPriorityPasses()
        {
            var layout = new LayoutClass();

            int callCounter = 0;
            (ILayout self, int prevPriority) recievedValues = default;
            layout.OnChangedOperationPriority.Add((_self, _prev) => {
                callCounter++;
                recievedValues.self = _self;
                recievedValues.prevPriority = _prev;
            });

            var prevPriority = layout.OperationPriority;
            layout.OperationPriority = layout.OperationPriority + 1;

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(layout, recievedValues.self);
            Assert.AreEqual(prevPriority, recievedValues.prevPriority);
        }

        /// <summary>
        /// <seealso cref="LayoutBase.OperationPriority"/>
        /// <seealso cref="LayoutBase.OnChangedOperationPriority"/>
        /// </summary>
        [Test]
        public void OnChangedOperationPriority_WhenNotChangePasses()
        {
            var layout = new LayoutClass();

            int callCounter = 0;
            layout.OnChangedOperationPriority.Add((_self, _prev) => {
                callCounter++;
            });

            layout.OperationPriority = layout.OperationPriority;

            Assert.AreEqual(0, callCounter);
        }

        /// <summary>
        /// <seealso cref="LayoutBase.OperationPriority"/>
        /// <seealso cref="LayoutBase.OnChangedOperationPriority"/>
        /// </summary>
        [Test]
        public void OnChangedOperationPriority_WhenThrowExceptionPasses()
        {
            var layout = new LayoutClass();
            layout.OnChangedOperationPriority.Add((_self, _prev) => {
                throw new System.Exception();
            });

            var value = layout.OperationPriority + 1;
            layout.OperationPriority = value;

            Assert.AreEqual(value, layout.OperationPriority);
        }
        #endregion

        #region Target Property
        /// <summary>
        /// <seealso cref="LayoutBase.Target"/>
        /// </summary>
        [Test]
        public void TargetPropertyPasses()
        {
            var target = new LayoutClass();
            Assert.IsNull(target.Target);

            var obj = new LayoutTargetObject();
            Assert.AreEqual(0, obj.OnDisposed.RegistedDelegateCount);

            {
                target.Target = obj; // test point
                Assert.AreSame(obj, target.Target);
                Assert.IsTrue(target.DoChanged);
                Assert.AreEqual(1, obj.OnDisposed.RegistedDelegateCount);

                Assert.AreEqual(1, target.CallCounterOnChanged);
            }


            {
                target.SetDoChanged(false);
                target.CallCounterOnChanged = 0;

                target.Target = null; // test point
                Assert.IsNull(target.Target);
                Assert.IsTrue(target.DoChanged);
                Assert.AreEqual(0, obj.OnDisposed.RegistedDelegateCount);
                Assert.AreEqual(1, target.CallCounterOnChanged);
            }
        }

        /// <summary>
        /// <seealso cref="LayoutBase.Target"/>
        /// <seealso cref="ILayoutTarget.OnDisposed"/>
        /// </summary>
        [Test]
        public void TargetAfterILayoutTargetDisposePasses()
        {
            var obj = new LayoutTargetObject();
            var target = new LayoutClass();
            target.Target = obj;

            obj.Dispose();

            Assert.IsNull(target.Target);
        }

        /// <summary>
        /// <seealso cref="LayoutBase.OnChangedTarget(ILayoutTarget, ILayoutTarget)"/>
        /// </summary>
        [Test]
        public void CallOnChangedTargetPasses()
        {
            var target = new LayoutClass();
            var obj = new LayoutTargetObject();

            {
                target.Target = obj; // <- test point
                Assert.AreEqual(1, target.CallCounterOnChangedTarget);
                Assert.AreSame(obj, target.CurrentOnChangedTarget);
                Assert.AreSame(null, target.PrevOnChangedTarget);
            }

            {
                target.CallCounterOnChangedTarget = 0;
                target.CurrentOnChangedTarget = null;
                target.PrevOnChangedTarget = null;

                target.Target = null; // <- test point

                Assert.AreEqual(1, target.CallCounterOnChangedTarget);
                Assert.AreSame(null, target.CurrentOnChangedTarget);
                Assert.AreSame(obj, target.PrevOnChangedTarget);
            }
        }
        #endregion

        #region Dispose
        /// <summary>
        /// <seealso cref="LayoutBase.Dispose"/>
        /// </summary>
        [Test]
        public void DisposePasses()
        {
            var target = new LayoutClass();
            var obj = new LayoutTargetObject();

            Assert.AreEqual(0, obj.OnDisposed.RegistedDelegateCount);

            target.Target = obj;
            Assert.AreEqual(1, obj.OnDisposed.RegistedDelegateCount);

            target.Dispose();
            Assert.IsNull(target.Target);
            Assert.AreEqual(0, obj.OnDisposed.RegistedDelegateCount, $"Remove a callback of LayoutBase from ILayoutTarget#OnDisposed After LayoutBase#Dispose()");
        }

        /// <summary>
        /// <seealso cref="LayoutBase.Dispose"/>
        /// </summary>
        [Test]
        public void DisposeWhenNullTargetPasses()
        {
            var target = new LayoutClass();

            target.Dispose();
            Assert.IsNull(target.Target);
        }

        /// <summary>
        /// <seealso cref="LayoutBase.Dispose()"/>
        /// <seealso cref="LayoutBase.OnDisposed"/>
        /// </summary>
        [Test]
        public void OnDisposedPasses()
        {
            var target = new LayoutClass();
            var callCounter = 0;
            ILayout recievedValue = null;
            target.OnDisposed.Add((_self) => {
                callCounter++;
                recievedValue = _self;
            });
            target.Dispose();

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(target, recievedValue);
            Assert.AreEqual(0, target.OnDisposed.RegistedDelegateCount);
        }

        /// <summary>
        /// <seealso cref="LayoutBase.Dispose()"/>
        /// <seealso cref="LayoutBase.OnDisposed"/>
        /// </summary>
        [Test]
        public void OnDisposedWhenThrowExceptionPasses()
        {
            var target = new LayoutClass();
            target.Target = new LayoutTargetObject();
            target.OnDisposed.Add((_self) => {
                throw new System.Exception();
            });

            Assert.DoesNotThrow(() => target.Dispose());

            Assert.IsNull(target.Target);
            Assert.AreEqual(0, target.OnDisposed.RegistedDelegateCount);
        }
        #endregion

        #region DoChanged
        /// <summary>
        /// <seealso cref="LayoutBase.DoChanged"/>
        /// </summary>
        [Test]
        public void DoChangedPropertyPasses()
        {
            var target = new LayoutClass();
            Assert.IsFalse(target.DoChanged);

            target.SetDoChanged(true);
            Assert.IsTrue(target.DoChanged);

            target.SetDoChanged(target.DoChanged);
            Assert.IsTrue(target.DoChanged);

            target.SetDoChanged(false);
            Assert.IsFalse(target.DoChanged);
        }

        /// <summary>
        /// <seealso cref="LayoutBase.DoChanged"/>
        /// <seealso cref="LayoutBase.InnerOnChanged(bool)"/>
        /// </summary>
        [Test]
        public void InnerOnChangedPasses()
        {
            var target = new LayoutClass();
            Assert.IsFalse(target.DoChanged);

            target.SetDoChanged(true);

            Assert.AreEqual(1, target.CallCounterOnChanged);
            Assert.AreEqual(target.DoChanged, target.DoChangedOnChanged);
        }

        /// <summary>
        /// <seealso cref="LayoutBase.DoChanged"/>
        /// <seealso cref="LayoutBase.OnChanged"/>
        /// </summary>
        [Test]
        public void OnChangedPasses()
        {
            var target = new LayoutClass();
            Assert.IsFalse(target.DoChanged);

            var callCounter = 0;
            (ILayout self, bool doChanged) recievedValue = default;
            target.OnChanged.Add((_self, _do) => {
                callCounter++;
                recievedValue = (_self, _do);
            });

            target.SetDoChanged(true);

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(target, recievedValue.self);
            Assert.IsTrue(recievedValue.doChanged);
        }

        /// <summary>
        /// <seealso cref="LayoutBase.DoChanged"/>
        /// <seealso cref="LayoutBase.OnChanged"/>
        /// </summary>
        [Test]
        public void OnChangedWhenThrowExceptionPasses()
        {
            var target = new LayoutClass();
            Assert.IsFalse(target.DoChanged);

            target.OnChanged.Add((_self, _do) => {
                throw new System.Exception();
            });

            target.SetDoChanged(true);

            Assert.IsTrue(target.DoChanged);
        }
        #endregion

        #region ForceUpdateLayout
        /// <summary>
        /// <seealso cref="LayoutBase.ForceUpdateLayout()"/>
        /// </summary>
        [Test]
        public void ForceUpdateLayoutPasses()
        {
            var target = new LayoutClass();

            Assert.IsFalse(target.DoChanged);
            Assert.AreEqual(0, target.CallUpdateLayoutCounter);

            target.ForceUpdateLayout();
            Assert.AreEqual(1, target.CallUpdateLayoutCounter);
        }
        #endregion
    }
}
