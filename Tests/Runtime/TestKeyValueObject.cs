using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Hinode.Tests
{
    public class TestKeyValueObject : TestBase
    {
        enum TestEnum
        {
            Apple,
            Orange,
            Grape,
            Banana,
        }

        [Test]
        public void KeyEnumValuePasses()
        {
            var keyValue = new KeyEnumObject("key", TestEnum.Apple, typeof(TestEnum));
            Assert.AreEqual("key", keyValue.Key);
            Assert.AreEqual(TestEnum.Apple, keyValue.Value);

            Assert.AreEqual(0, keyValue.EnumIndex);
            Assert.AreEqual(typeof(TestEnum), keyValue.HasType);
            Assert.IsFalse(keyValue.IsFlags);
            Assert.IsTrue(keyValue.IsValid);
            Assert.IsTrue(keyValue.IsValidValue((int)TestEnum.Apple));
        }

        /// <summary>
        /// ScriptableObject#CreateInstanceで警告が出るが、無視しても大丈夫なのでそのままにしている。
        /// </summary>
        [System.Serializable]
        class TestFailedEnum : ScriptableObject
        {
            public KeyEnumObject keyValue;
        }

        [Test]
        public void KeyEnumValueFailed()
        {
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
            {
                var keyValue = KeyEnumObject.Create("key", (TestEnum)100);
            });

            //何らかの操作の結果内部の値が不正なものになった時のテスト
            var target = ScriptableObject.CreateInstance<TestFailedEnum>();
            target.keyValue = KeyEnumObject.Create("key", TestEnum.Banana);
            var SO = new SerializedObject(target);
            var prop = SO.FindProperty("keyValue").FindPropertyRelative("_value");
            prop.intValue = -100;
            SO.ApplyModifiedProperties();

            Assert.IsFalse(target.keyValue.IsValid);
            Assert.IsFalse(target.keyValue.IsValidValue(100));
            SO.Dispose();
        }

        [System.Flags]
        enum TestFlags
        {
            Red = 1,
            Blue = 2,
            Green = 4,
            Yellow = 8,
        }

        [Test]
        public void KeyFlagsEnumValuePasses()
        {
            var keyValue = new KeyEnumObject("key", TestFlags.Blue | TestFlags.Green, typeof(TestFlags));
            Assert.AreEqual("key", keyValue.Key);
            var e = TestFlags.Blue | TestFlags.Green;
            Assert.AreEqual(e, keyValue.Value);

            Assert.AreEqual((int)e, keyValue.EnumIndex);
            Assert.AreEqual(typeof(TestFlags), keyValue.HasType);
            Assert.IsTrue(keyValue.IsFlags);
            Assert.IsTrue(keyValue.IsValid);
            Assert.IsTrue(keyValue.IsValidValue((int)(TestFlags.Blue | TestFlags.Green)));

            var noneFlag = KeyEnumObject.Create("none", (TestFlags)0);
            Assert.AreEqual(0, noneFlag.EnumIndex);
            Assert.AreEqual((TestFlags)0, noneFlag.Value);
            Assert.IsTrue(noneFlag.IsValid);
            Assert.IsTrue(noneFlag.IsValidValue(0));

            var allFlag = KeyEnumObject.Create("none", (TestFlags)(-1));
            Assert.AreEqual(-1, allFlag.EnumIndex);
            Assert.AreEqual((TestFlags)(-1), allFlag.Value);
            Assert.IsTrue(allFlag.IsValid);
            Assert.IsTrue(allFlag.IsValidValue(-1));
        }

        [Test]
        public void KeyFlagsEnumValueFailed()
        {
            //不正な値で初期化された時の挙動チェック
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
            {
                var _keyValue = new KeyEnumObject("key", (TestFlags)100, typeof(TestFlags));
            });
        }

    }
}
