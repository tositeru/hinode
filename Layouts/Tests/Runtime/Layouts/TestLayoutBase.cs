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
            Vector3 _unitSize;

            public override bool DoChanged { get => _doChanged; }
            public override Vector3 UnitSize { get => _unitSize; }

            public override void UpdateLayout()
            {
            }

            public override void UpdateUnitSize()
            {
            }
        }

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

            target.Target = obj;
            Assert.AreSame(obj, target.Target);
            Assert.AreEqual(1, obj.OnDisposed.RegistedDelegateCount);

            target.Target = null;
            Assert.IsNull(target.Target);
            Assert.AreEqual(0, obj.OnDisposed.RegistedDelegateCount);
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
    }
}
