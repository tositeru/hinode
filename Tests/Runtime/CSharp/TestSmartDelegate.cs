using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp
{
    /// <summary>
    /// <seealso cref="SmartDelegate{T}"/>
    /// <seealso cref="NotInvokableDelegate{T}"/>
    /// </summary>
    public class TestSmartDelegate
    {
        delegate void BasicUsagePassesDelegate();

        [Test]
        public void BasicUsagePasses()
        {
            var predicate = new SmartDelegate<BasicUsagePassesDelegate>();
            Assert.IsFalse(predicate.IsValid);
            Assert.IsNull(predicate.Instance);

            int count = 0;
            BasicUsagePassesDelegate incrementFunc = () => { count++; };
            predicate.Set(incrementFunc);
            Assert.IsTrue(predicate.IsValid);
            Assert.IsNotNull(predicate.Instance);

            predicate.Instance.Invoke();
            Assert.AreEqual(1, count);

            predicate.Remove(incrementFunc);
            Assert.IsFalse(predicate.IsValid);
            Assert.IsNull(predicate.Instance);

            predicate.Add(incrementFunc);
            Assert.IsTrue(predicate.IsValid);
            Assert.IsNotNull(predicate.Instance);


            var predicate2 = new SmartDelegate<BasicUsagePassesDelegate>(predicate);
            Assert.IsTrue(predicate2.IsValid);
            Assert.IsNotNull(predicate2.Instance);

            count = 0;
            predicate2.Instance.Invoke();
            Assert.AreEqual(1, count);

            predicate.Clear();
            Assert.IsFalse(predicate.IsValid);
            Assert.IsNull(predicate.Instance);
        }

        void Apple() { }
        [Test]
        public void RegistedDelegateCountPasses()
        {
            var d = new SmartDelegate<BasicUsagePassesDelegate>();

            d.Add(() => { });
            d.Add(Apple);
            Assert.AreEqual(2, d.RegistedDelegateCount);

            d.Remove(Apple);
            Assert.AreEqual(1, d.RegistedDelegateCount);
        }

        #region SafeDynamicInvoke
        delegate void SafeDynamicInvokeDelegate();
        delegate void SafeDynamicInvokeDelegateArg1(int a);
        delegate void SafeDynamicInvokeDelegateArg2(int a, int b);
        delegate void SafeDynamicInvokeDelegateArg3(int a, int b, int c);
        delegate void SafeDynamicInvokeDelegateArg4(int a, int b, int c, int d);
        delegate void SafeDynamicInvokeDelegateArg5(int a, int b, int c, int d, int e);

        /// <summary>
        /// <seealso cref="SmartDelegate{T}.SafeDynamicInvoke(System.Func{string}, string[])"/>
        /// </summary>
        [Test]
        public void SafeDynamicInvoke_NoneArgPasses()
        {
            {
                var d = new SmartDelegate<SafeDynamicInvokeDelegate>();
                int callCounter = 0;
                d.Add(() => callCounter++);

                d.SafeDynamicInvoke(() => "ErrorLog");
                Assert.AreEqual(1, callCounter);
            }

            {
                var d = new SmartDelegate<SafeDynamicInvokeDelegate>();
                d.Add(() => { throw new System.Exception(); });

                Assert.DoesNotThrow(() => 
                    d.SafeDynamicInvoke(() => "ErrorLog")
                );
            }

            {
                var d = new SmartDelegate<SafeDynamicInvokeDelegate>();
                Assert.DoesNotThrow(() =>
                    d.SafeDynamicInvoke(() => "ErrorLog")
                );
                Debug.Log($"Success to Empty Delegate");
            }
        }

        /// <summary>
        /// <seealso cref="SmartDelegate{T}.SafeDynamicInvoke{T1}(T1, System.Func{string}, string[])"/>
        /// </summary>
        [Test]
        public void SafeDynamicInvoke_Arg1Passes()
        {
            {
                var d = new SmartDelegate<SafeDynamicInvokeDelegateArg1>();
                int callCounter = 0;
                int recievedValue = 0;
                d.Add((_n) => { callCounter++; recievedValue = _n; });

                var value = 100;
                d.SafeDynamicInvoke(value, () => "ErrorLog");
                Assert.AreEqual(1, callCounter);
                Assert.AreEqual(value, recievedValue);
            }

            {
                var d = new SmartDelegate<SafeDynamicInvokeDelegateArg1>();
                d.Add((_n) => { throw new System.Exception(); });

                Assert.DoesNotThrow(() =>
                    d.SafeDynamicInvoke(100, () => "ErrorLog")
                );
            }
            {
                var d = new SmartDelegate<SafeDynamicInvokeDelegateArg1>();
                Assert.DoesNotThrow(() =>
                    d.SafeDynamicInvoke(() => "ErrorLog")
                );
                Debug.Log($"Success to Empty Delegate");
            }
        }

        /// <summary>
        /// <seealso cref="SmartDelegate{T}.SafeDynamicInvoke{T1, T2}(T1, T2, System.Func{string}, string[])"/>
        /// </summary>
        [Test]
        public void SafeDynamicInvoke_Arg2Passes()
        {
            {
                var d = new SmartDelegate<SafeDynamicInvokeDelegateArg2>();
                int callCounter = 0;
                (int, int) recievedValue = default;
                d.Add((_n, _m) => { callCounter++; recievedValue = (_n, _m); });

                var value1 = 100;
                var value2 = 200;
                d.SafeDynamicInvoke(value1, value2, () => "ErrorLog");
                Assert.AreEqual(1, callCounter);
                Assert.AreEqual(value1, recievedValue.Item1);
                Assert.AreEqual(value2, recievedValue.Item2);
            }

            {
                var d = new SmartDelegate<SafeDynamicInvokeDelegateArg2>();
                d.Add((_n, _m) => { throw new System.Exception(); });

                Assert.DoesNotThrow(() =>
                    d.SafeDynamicInvoke(100, () => "ErrorLog")
                );
            }
            {
                var d = new SmartDelegate<SafeDynamicInvokeDelegateArg2>();
                Assert.DoesNotThrow(() =>
                    d.SafeDynamicInvoke(() => "ErrorLog")
                );
                Debug.Log($"Success to Empty Delegate");
            }
        }

        /// <summary>
        /// <seealso cref="SmartDelegate{T}.SafeDynamicInvoke{T1, T2, T3}(T1, T2, T3, System.Func{string}, string[])"/>
        /// </summary>
        [Test]
        public void SafeDynamicInvoke_Arg3Passes()
        {
            {
                var d = new SmartDelegate<SafeDynamicInvokeDelegateArg3>();
                int callCounter = 0;
                (int, int, int) recievedValue = default;
                d.Add((_n, _m, _l) => { callCounter++; recievedValue = (_n, _m, _l); });

                var value1 = 100;
                var value2 = 200;
                var value3 = 300;
                d.SafeDynamicInvoke(value1, value2, value3, () => "ErrorLog");
                Assert.AreEqual(1, callCounter);
                Assert.AreEqual(value1, recievedValue.Item1);
                Assert.AreEqual(value2, recievedValue.Item2);
                Assert.AreEqual(value3, recievedValue.Item3);
            }

            {
                var d = new SmartDelegate<SafeDynamicInvokeDelegateArg3>();
                d.Add((_n, _m, _l) => { throw new System.Exception(); });

                Assert.DoesNotThrow(() =>
                    d.SafeDynamicInvoke(100, () => "ErrorLog")
                );
            }
            {
                var d = new SmartDelegate<SafeDynamicInvokeDelegateArg3>();
                Assert.DoesNotThrow(() =>
                    d.SafeDynamicInvoke(() => "ErrorLog")
                );
                Debug.Log($"Success to Empty Delegate");
            }
        }

        /// <summary>
        /// <seealso cref="SmartDelegate{T}.SafeDynamicInvoke{T1, T2, T3, T4}(T1, T2, T3, T4, System.Func{string}, string[])"/>
        /// </summary>
        [Test]
        public void SafeDynamicInvoke_Arg4Passes()
        {
            {
                var d = new SmartDelegate<SafeDynamicInvokeDelegateArg4>();
                int callCounter = 0;
                (int, int, int, int) recievedValue = default;
                d.Add((_n, _m, _l, _k) => { callCounter++; recievedValue = (_n, _m, _l, _k); });

                var value1 = 100;
                var value2 = 200;
                var value3 = 300;
                var value4 = 400;
                d.SafeDynamicInvoke(value1, value2, value3, value4, () => "ErrorLog");
                Assert.AreEqual(1, callCounter);
                Assert.AreEqual(value1, recievedValue.Item1);
                Assert.AreEqual(value2, recievedValue.Item2);
                Assert.AreEqual(value3, recievedValue.Item3);
                Assert.AreEqual(value4, recievedValue.Item4);
            }

            {
                var d = new SmartDelegate<SafeDynamicInvokeDelegateArg4>();
                d.Add((_n, _m, _l, _k) => { throw new System.Exception(); });

                Assert.DoesNotThrow(() =>
                    d.SafeDynamicInvoke(100, () => "ErrorLog")
                );
            }
            {
                var d = new SmartDelegate<SafeDynamicInvokeDelegateArg4>();
                Assert.DoesNotThrow(() =>
                    d.SafeDynamicInvoke(() => "ErrorLog")
                );
                Debug.Log($"Success to Empty Delegate");
            }
        }


        /// <summary>
        /// <seealso cref="SmartDelegate{T}.SafeDynamicInvoke{T1, T2, T3, T4, T5}(T1, T2, T3, T4, T5, System.Func{string}, string[])"/>
        /// </summary>
        [Test]
        public void SafeDynamicInvoke_Arg5Passes()
        {
            {
                var d = new SmartDelegate<SafeDynamicInvokeDelegateArg5>();
                int callCounter = 0;
                (int, int, int, int, int) recievedValue = default;
                d.Add((_n, _m, _l, _k, _j) => { callCounter++; recievedValue = (_n, _m, _l, _k, _j); });

                var value1 = 100;
                var value2 = 200;
                var value3 = 300;
                var value4 = 400;
                var value5 = 400;
                d.SafeDynamicInvoke(value1, value2, value3, value4, value5, () => "ErrorLog");
                Assert.AreEqual(1, callCounter);
                Assert.AreEqual(value1, recievedValue.Item1);
                Assert.AreEqual(value2, recievedValue.Item2);
                Assert.AreEqual(value3, recievedValue.Item3);
                Assert.AreEqual(value4, recievedValue.Item4);
                Assert.AreEqual(value5, recievedValue.Item5);
            }

            {
                var d = new SmartDelegate<SafeDynamicInvokeDelegateArg5>();
                d.Add((_n, _m, _l, _k, _j) => { throw new System.Exception(); });

                Assert.DoesNotThrow(() =>
                    d.SafeDynamicInvoke(1, 2, 3, 4, 5, () => "ErrorLog")
                );
            }
            {
                var d = new SmartDelegate<SafeDynamicInvokeDelegateArg5>();
                Assert.DoesNotThrow(() =>
                    d.SafeDynamicInvoke(() => "ErrorLog")
                );
                Debug.Log($"Success to Empty Delegate");
            }
        }
        #endregion
    }
}
