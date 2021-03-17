#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;
using System.Reflection;

namespace Hinode.Tests
{
    /// <summary>
    /// A/B Test用のクラス
    /// </summary>
    public abstract class IABTest
    {
        protected abstract (string name, string paramText)[] GetParamTexts();

        protected abstract void InitParams(System.Random rnd);
        protected abstract void TestMethod();

        public void RunTest(TestSettings settings, int loopCount = -1)
        {
            if (!settings.EnableABTest)
            {
                Debug.Log($"Skip A/B Test.");
                return;
            }

            if (loopCount == -1) loopCount = settings.DefaultABTestLoopCount;

            var isSuccess = true;
            var rnd = settings.GetRandomForABTest();
            var failCounter = 0;
            for (var i = 0; i < loopCount; ++i)
            {
                ResetDoUse();

                InitParams(rnd);
                try
                {
                    TestMethod();
                }
                catch (System.Exception e)
                {
                    isSuccess = false;
                    var paramText = GetParamTexts().Select(_t => $"{_t.name};{_t.paramText}").Aggregate("", (_s, _c) => _s + _c + System.Environment.NewLine);
                    Debug.LogWarning($"Detect Fail Test Case... {System.Environment.NewLine}{e}{System.Environment.NewLine}-- AB Test params{System.Environment.NewLine}{paramText}");
                    failCounter++;
                    if (failCounter > 10) break;
                }
            }

            Assert.IsTrue(isSuccess);
        }

        void ResetDoUse()
        {
            var useParamFields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(_f => _f.FieldType.IsSubclassOf(typeof(UseParam<>)));
            foreach(var field in useParamFields)
            {
                var inst = field.GetValue(this);
                var doUseProp = inst.GetType().GetProperty("DoUse");
                doUseProp.SetValue(inst, false);
            }
        }

        public class UseParam<T>
        {
            public string ParamName { get; private set; }
            public bool DoUse { get; set; }
            public T Value { get; set; }

            public UseParam(string name)
                : this(name, default)
            {}

            public UseParam(string name, T value)
            {
                ParamName = name;
                Value = value;
            }

            public (string name, string paramText) ToText()
            {
                return ($"{ParamName}:DoUse={DoUse}", Value.ToString());
            }

            public (string name, string paramText) ToText(System.Func<T, string> paramTextFunc)
            {
                return ($"{ParamName}:DoUse={DoUse}", paramTextFunc(Value));
            }
        }
    }
}
#endif