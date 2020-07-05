using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp.IUpdateObserver
{
    /// <summary>
	/// <seealso cref="PredicateUpdateObserver"/>
	/// </summary>
    public class TestPredicateUpdateObserver
    {
        /// <summary>
        /// <seealso cref="PredicateUpdateObserver{T}.DidUpdated"/>
        /// <seealso cref="PredicateUpdateObserver{T}.RawValue"/>
        /// <seealso cref="PredicateUpdateObserver{T}.Value"/>
        /// <seealso cref="PredicateUpdateObserver{T}.Update()"/>
        /// </summary>
        [Test]
        public void BasicUsagePasses()
        {
            var value = 0;
            var counter = 0;
            var observer = new PredicateUpdateObserver<int>(() => {
                counter++;
                return value;
            });

			{//初期状態のテスト
                Assert.IsFalse(observer.DidUpdated);
                Assert.AreEqual(value, observer.RawValue);
                Assert.AreEqual(value, observer.Value);
                Assert.AreEqual(1, counter, "初期値の設定のため、コンストラクタ内で一度設定したPredicateを呼び出してください。");
            }

            {//Predicateが返す値が変わった時のテスト
                value = 1;
                var errorMessage = "PredicateUpdateObserver#Updateが呼ばれるまでValue/RawValueは更新されないようにしてください";
                Assert.AreNotEqual(value, observer.RawValue, errorMessage);
                Assert.AreNotEqual(value, observer.Value, errorMessage);
                Assert.AreEqual(1, counter, "PredicateUpdateObserver#Updateが呼ばれるまで設定したPredicateを呼び出さないようにしてください");

                counter = 0;
                Assert.IsTrue(observer.Update());
                Assert.IsTrue(observer.DidUpdated);
                errorMessage = "PredicateUpdateObserver#Updateが呼ばれた時、値が変更されていた時はValue/RawValueも更新するようにしてください";
                Assert.AreEqual(value, observer.RawValue, errorMessage);
                Assert.AreEqual(value, observer.Value, errorMessage);
                Assert.AreEqual(1, counter, "PredicateUpdateObserver#Updateが呼び出された時に設定したPredicateを呼び出すようにしてください");

                // 一度PredicateUpdateObserver#DidUpdateddがtrueになった後の挙動テスト
                counter = 0;
                Assert.IsFalse(observer.Update());
                Assert.IsFalse(observer.DidUpdated);
                Assert.AreEqual(1, counter, "PredicateUpdateObserver#Updateが呼ばれる度に設定したPredicateを呼び出すようにしてください。");
                Assert.AreEqual(value, observer.RawValue);
                Assert.AreEqual(value, observer.Value);
            }
        }

        /// <summary>
        /// <seealso cref="PredicateUpdateObserver{T}.DidUpdated"/>
        /// <seealso cref="PredicateUpdateObserver{T}.Update()"/>
        /// <seealso cref="PredicateUpdateObserver{T}.RawValue"/>
        /// <seealso cref="PredicateUpdateObserver{T}.Value"/>
        /// <seealso cref="PredicateUpdateObserver{T}.Reset()"/>
        /// <seealso cref="PredicateUpdateObserver{T}.SetDefaultValue(bool)"/>
        /// </summary>
        [Test]
        public void ResetAndSetDefaultValuePasses()
        {
            var value = 0;
            var counter = 0;
            var observer = new PredicateUpdateObserver<int>(() => {
                counter++;
                return value;
            });

            {//Reset
                value = 1;
                counter = 0;
                observer.Update();
                Assert.IsTrue(observer.DidUpdated);
                Assert.AreEqual(1, counter);

                observer.Reset();
                Assert.IsFalse(observer.DidUpdated);

                Assert.AreEqual(observer.RawValue, observer.Value);
                Assert.AreEqual(value, observer.RawValue, "Reset()を呼び出した後、Value/RawValueの値は変更しないようにしてください");
                Assert.AreEqual(value, observer.Value, "Reset()を呼び出した後、Value/RawValueの値は変更しないようにしてください");
                Assert.AreEqual(1, counter, $"Reset()を呼び出した時は設定したPredicateを呼び出さないようにしてください。");
            }

            {//SetDefaultValue(false)
                value++;
                observer.Update();

                counter = 0;
                observer.SetDefaultValue(false);
                var errorMessage = "PredicateUpdateObserver#SetDefaultValueが呼ばれた時はValue/RawValueをdefault値にしてください";
                Assert.AreEqual(default(int), observer.RawValue, errorMessage);
                Assert.AreEqual(default(int), observer.Value, errorMessage);
                Assert.IsTrue(observer.DidUpdated, "引数にfalseを指定した時はDidUpdateddの値は変更しないようにしてください。");
                Assert.AreEqual(0, counter, "PredicateUpdateObserver#SetDefaultValueが呼び出された時は設定したPredicateを呼び出さないようにしてください");
            }

            {//SetDefaultValue(false)
                value++;
                observer.Update();

                counter = 0;
                observer.SetDefaultValue(true);
                var errorMessage = "PredicateUpdateObserver#SetDefaultValueが呼ばれた時はValue/RawValueをdefault値にしてください";
                Assert.AreEqual(default(int), observer.RawValue, errorMessage);
                Assert.AreEqual(default(int), observer.Value, errorMessage);
                Assert.IsFalse(observer.DidUpdated, "引数にtrueを指定した時はDidUpdateddの値をFalseにしてください。");
                Assert.AreEqual(0, counter, "PredicateUpdateObserver#SetDefaultValueが呼び出された時は設定したPredicateを呼び出さないようにしてください");
            }
        }

        /// <summary>
        /// <seealso cref="PredicateUpdateObserver{T}.OnChangedValue"/>
        /// <seealso cref="PredicateUpdateObserver{T}.Update()"/>
        /// </summary>
        [Test]
        public void OnUpdatedPasses()
        {
            var value = 0;
            var observer = new PredicateUpdateObserver<int>(() => value);

            var counter = 0;
            var recievedValue = 0;
            observer.OnChangedValue.Add((_v) => {
                counter++;
                recievedValue = _v;
            });

            {//
                value = 1;
                observer.Update();
                Assert.AreEqual(1, counter);
                Assert.AreEqual(value, recievedValue);

                observer.Update();
                Assert.AreEqual(1, counter, "値が変更されない場合はOnChangedValueコールバックを呼び出さないようにしてください。");
            }
        }

        class NullValuePassesClass
        { }

        /// <summary>
        /// <seealso cref="PredicateUpdateObserver{T}.Update()"/>
        /// </summary>
        [Test]
        public void NullValuePasses()
        {
            var obj = new NullValuePassesClass();
            var observer = new PredicateUpdateObserver<NullValuePassesClass>(() => obj);

            obj = null;
            Assert.IsTrue(observer.Update());

            Assert.DoesNotThrow(() => {
                obj = new NullValuePassesClass();
                observer.Update();
            });
        }
    }
}
