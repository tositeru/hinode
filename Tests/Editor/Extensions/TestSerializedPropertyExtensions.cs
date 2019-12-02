using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Hinode.Editors;
using System.Reflection;
using System.Linq;

namespace Hinode.Tests.Editors
{
    public class TestSerializedPropertyExtensions : TestBase
    {
        [System.Serializable]
        class TestPropertyPathEnumerableClass : ScriptableObject
        {
#pragma warning disable CS0649
            [SerializeField] int value;
            public TestPropertyPathEnumerableSubClass sub;
            public List<int> list; //SerializedPropertyのArrayの時の検証のため
            public int[] array;　//SerializedPropertyのArrayの時の検証のため
#pragma warning restore CS0649
        }

        [System.Serializable]
        class TestPropertyPathEnumerableSubClass
        {
#pragma warning disable CS0649
            [Min(0)] public float v;
#pragma warning restore CS0649
        }

        [Test]
        public void PropertyPathEnumerablePasses()
        {
            var obj = ScriptableObject.CreateInstance<TestPropertyPathEnumerableClass>();
            obj.list = new List<int> { 0, 1, 2 };
            obj.array = new int[] { -1, -2, -3, -4 };
            var SO = new SerializedObject(obj);

            System.Func<IEnumerable<PropertyPathEnumerableData>, string> makeFullpath = (e) => e.Select(_d => _d.path).Aggregate((_s, _c) => _s += (_s.Length <= 0) ? "" : "." + _c);

            var prop = SO.FindProperty("sub").FindPropertyRelative("v");
            Assert.AreEqual(prop.propertyPath, makeFullpath(prop.GetPropertyPathEnumerable()));

            var listProp = SO.FindProperty("list");
            Assert.AreEqual(listProp.propertyPath, makeFullpath(listProp.GetPropertyPathEnumerable()));

            var arrayProp = SO.FindProperty("array");
            Assert.AreEqual(arrayProp.propertyPath, makeFullpath(arrayProp.GetPropertyPathEnumerable()));
        }

        [System.Serializable]
        class TestGetFieldInfoClass : ScriptableObject
        {
#pragma warning disable CS0649
            [SerializeField] int value;
            public TestGetFieldInfoSubClass sub;
            public List<int> list; //SerializedPropertyのArrayの時の検証のため
            public int[] array;　//SerializedPropertyのArrayの時の検証のため
#pragma warning restore CS0649
        }

        [System.Serializable]
        class TestGetFieldInfoSubClass
        {
#pragma warning disable CS0649
            [Min(0)] public float v;
#pragma warning restore CS0649
        }

        [Test]
        public void GetFieldInfoPasses()
        {
            var obj = ScriptableObject.CreateInstance<TestGetFieldInfoClass>();
            obj.list = new List<int> { 0, 1, 2 };
            obj.array = new int[] { -1, -2, -3, -4 };

            var SO = new SerializedObject(obj);
            var bindFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

            var prop = SO.FindProperty("sub").FindPropertyRelative("v");
            Assert.AreEqual(typeof(TestGetFieldInfoClass).GetField("sub").FieldType.GetField("v", bindFlags), prop.GetFieldInfo());

            var arrayElementProp = SO.FindProperty("array").GetArrayElementAtIndex(1);
            Assert.AreEqual(typeof(TestGetFieldInfoClass).GetField("array"),
                arrayElementProp.GetFieldInfo());

            var listElementProp = SO.FindProperty("list").GetArrayElementAtIndex(0);
            Assert.AreEqual(typeof(TestGetFieldInfoClass).GetField("list"),
                listElementProp.GetFieldInfo());
        }

        [System.Serializable]
        class TestGetFieldInfoOfArrayElementClass : ScriptableObject
        {
#pragma warning disable CS0649
            public List<TestGetFieldInfoOfArrayElementSubClass> list; //SerializedPropertyのArrayの時の検証のため
            public TestGetFieldInfoOfArrayElementSubClass[] array;　//SerializedPropertyのArrayの時の検証のため
            public int[] manyArray;
#pragma warning restore CS0649
        }

        [System.Serializable]
        class TestGetFieldInfoOfArrayElementSubClass
        {
#pragma warning disable CS0649
            [Min(0)] public float v;
#pragma warning restore CS0649
        }

        [Test]
        public void GetFieldInfoOfArrayElementPasses()
        {
            var obj = ScriptableObject.CreateInstance<TestGetFieldInfoOfArrayElementClass>();
            obj.list = new List<TestGetFieldInfoOfArrayElementSubClass> { new TestGetFieldInfoOfArrayElementSubClass { v = 1 } };
            obj.array = new TestGetFieldInfoOfArrayElementSubClass[] { new TestGetFieldInfoOfArrayElementSubClass { v = -1 } };
            obj.manyArray = Enumerable.Range(0, 101).ToArray();
            var SO = new SerializedObject(obj);

            var arrayElementProp = SO.FindProperty("array").GetArrayElementAtIndex(0).FindPropertyRelative("v");
            Assert.AreEqual(typeof(TestGetFieldInfoOfArrayElementSubClass).GetField("v"),
                arrayElementProp.GetFieldInfo());

            var listElementProp = SO.FindProperty("list").GetArrayElementAtIndex(0).FindPropertyRelative("v");
            Assert.AreEqual(typeof(TestGetFieldInfoOfArrayElementSubClass).GetField("v"),
                listElementProp.GetFieldInfo());

            var manyArrayElementProp = SO.FindProperty("manyArray").GetArrayElementAtIndex(100);
            Assert.AreEqual(typeof(TestGetFieldInfoOfArrayElementClass).GetField("manyArray"),
                manyArrayElementProp.GetFieldInfo());
        }

        [System.Serializable]
        class TestGetFieldAttributesClass : ScriptableObject
        {
#pragma warning disable CS0649
            [SerializeField] int value;
            [SerializeField] public TestGetFieldAttributesSubClass sub;
            [Min(0)] public List<float> list; //SerializedPropertyのArrayの時の検証のため
            [Min(-10)] public float[] array;　//SerializedPropertyのArrayの時の検証のため
#pragma warning restore CS0649
        }

        [System.Serializable]
        class TestGetFieldAttributesSubClass
        {
#pragma warning disable CS0649
            [SerializeField, Min(0)] public float v;
#pragma warning restore CS0649
        }

        [Test]
        public void GetFieldAttributesPasses()
        {
            var obj = ScriptableObject.CreateInstance<TestGetFieldAttributesClass>();
            obj.list = new List<float> { 1, 2, 3 };
            obj.array = new float[] { -1, -2, -3 };
            var SO = new SerializedObject(obj);

            //TestGetFieldAttributesClass#sub#vのアトリビュートを取得
            {
                var propSubV = SO.FindProperty("sub").FindPropertyRelative("v");
                var attributes = propSubV.GetFieldAttributes();
                Assert.AreEqual(2, attributes.Count());
                Assert.IsTrue(attributes.Any(_a => _a.GetType().Equals(typeof(MinAttribute))));

                var minAttr = propSubV.GetFieldAttributes<MinAttribute>().First();
                Assert.IsNotNull(minAttr);
                Assert.AreEqual(0, minAttr.min);
            }

            //TestGetFieldAttributesClass#arrayのアトリビュートを取得
            {
                var propList = SO.FindProperty("array");
                var attributes = propList.GetFieldAttributes();
                Assert.AreEqual(1, attributes.Count());
                Assert.IsTrue(attributes.Any(_a => _a.GetType().Equals(typeof(MinAttribute))));

                var minAttr = propList.GetFieldAttributes<MinAttribute>().First();
                Assert.IsNotNull(minAttr);
                Assert.AreEqual(-10, (int)minAttr.min);

                // Listの要素のGetFieldAttributesの結果は所属しているListと同じ結果を返すようにしている
                var propElement = propList.GetArrayElementAtIndex(0);
                var elementAttributes = propElement.GetFieldAttributes();
                Assert.AreEqual(attributes.Count(), elementAttributes.Count(), $"配列の要素の結果が所属しているListの結果と一致していません。同じ結果を返すようにしてください");
                foreach (var (element, list) in elementAttributes.Zip(attributes, (_e, _a) => (element: _e, list: _a)))
                {
                    Assert.AreEqual(list, element, $"配列の要素の結果が所属しているListの結果と一致していません。同じ結果を返すようにしてください");
                }
            }

            //TestGetFieldAttributesClass#listのアトリビュートを取得
            {
                var propList = SO.FindProperty("list");
                var attributes = propList.GetFieldAttributes();
                Assert.AreEqual(1, attributes.Count());
                Assert.IsTrue(attributes.Any(_a => _a.GetType().Equals(typeof(MinAttribute))));

                var minAttr = propList.GetFieldAttributes<MinAttribute>().First();
                Assert.IsNotNull(minAttr);
                Assert.AreEqual(0, (int)minAttr.min);

                // Listの要素のGetFieldAttributesの結果は所属しているListと同じ結果を返すようにしている
                var propElement = propList.GetArrayElementAtIndex(0);
                var elementAttributes = propElement.GetFieldAttributes();
                Assert.AreEqual(attributes.Count(), elementAttributes.Count(), $"Listの要素の結果が所属しているListの結果と一致していません。同じ結果を返すようにしてください");
                foreach(var (element, list) in elementAttributes.Zip(attributes, (_e, _a) => (element: _e, list: _a)))
                {
                    Assert.AreEqual(list, element, $"Listの要素の結果が所属しているListの結果と一致していません。同じ結果を返すようにしてください");
                }
            }
        }

        [System.Serializable]
        class TestGetSelfInstanceClass : ScriptableObject
        {
#pragma warning disable CS0649
            [SerializeField] int value;
            [SerializeField] public TestGetSelfInstanceSubClass sub;
            [Min(0)] public List<float> list; //SerializedPropertyのArrayの時の検証のため
            [Min(-10)] public TestGetSelfInstanceSubClass[] array;　//SerializedPropertyのArrayの時の検証のため
#pragma warning restore CS0649
        }

        [System.Serializable]
        class TestGetSelfInstanceSubClass
        {
#pragma warning disable CS0649
            [SerializeField, Min(0)] public float v;
#pragma warning restore CS0649
        }

        [Test]
        public void GetSelfInstancePasses()
        {
            var obj = ScriptableObject.CreateInstance<TestGetSelfInstanceClass>();
            obj.sub = new TestGetSelfInstanceSubClass { v = 1 };
            obj.list = new List<float> { 1, 2, 3 };
            obj.array = new TestGetSelfInstanceSubClass[] { new TestGetSelfInstanceSubClass { v = -1 } };
            var SO = new SerializedObject(obj);

            {//TestGetSelfInstanceClass#sub
                var prop = SO.FindProperty("sub");
                var propObj = prop.GetSelf();
                Assert.AreSame(obj.sub, propObj);

                var propSubV = prop.FindPropertyRelative("v");
                Assert.AreEqual(1f, (float)propSubV.GetSelf());
            }

            {//TestGetSelfInstanceClass#list
                var prop = SO.FindProperty("list");
                var propObj = prop.GetSelf();
                Assert.AreSame(obj.list, propObj);

                var propElement = prop.GetArrayElementAtIndex(0);
                Assert.AreEqual(1f, (float)propElement.GetSelf());
            }

            {//TestGetSelfInstanceClass#array and TestGetSelfInstanceClass#array[0]#v
                var prop = SO.FindProperty("array");
                var propObj = prop.GetSelf();
                Assert.AreSame(obj.array, propObj);

                var propElement = prop.GetArrayElementAtIndex(0);
                Assert.AreSame(obj.array[0], propElement.GetSelf());

                var propElementV = propElement.FindPropertyRelative("v");
                Assert.AreEqual(obj.array[0].v, (float)propElementV.GetSelf());
            }
        }


        class GetInheritedFieldBaseClass : ScriptableObject
        {
            [SerializeField] List<int> value = new List<int> { 111 };
        }
        class GetInheritedFieldSubClass : GetInheritedFieldBaseClass
        {
            public int apple = 10;
        }
        [Test]
        public void GetInheritedFieldPasses()
        {
            var obj = ScriptableObject.CreateInstance<GetInheritedFieldSubClass>();
            var SO = new SerializedObject(obj);

            var prop = SO.FindProperty("value").GetArrayElementAtIndex(0);
            Assert.AreEqual(typeof(GetInheritedFieldBaseClass).GetField("value", BindingFlags.Instance | BindingFlags.NonPublic), prop.GetFieldInfo());
        }

        class ArrayElementEnumerableClass : ScriptableObject
        {
            public List<int> list;
        }

        [Test]
        public void ArrayElementEnumerablePasses()
        {
            var obj = ScriptableObject.CreateInstance<ArrayElementEnumerableClass>();
            obj.list = Enumerable.Range(0, 10).ToList();

            var SO = new SerializedObject(obj);
            var listProp = SO.FindProperty("list");
            Assert.AreEqual(obj.list.Count, listProp.GetArrayElementEnumerable().Count());
            foreach(var (p, element) in listProp.GetArrayElementEnumerable().Zip(obj.list, (_p, _e) => (_p, _e)))
            {
                Assert.AreEqual(p.prop.intValue, element);
            }
        }
    }
}
