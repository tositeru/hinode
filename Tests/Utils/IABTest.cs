using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;

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
                InitParams(rnd);
                try
                {
                    TestMethod();
                }
                catch (System.Exception e)
                {
                    isSuccess = false;
                    var paramText = GetParamTexts().Select(_t => $"{_t.name}={_t.paramText}").Aggregate("", (_s, _c) => _s + _c + ",");
                    Debug.LogWarning($"Detect Fail Test Case... {System.Environment.NewLine}{e}{System.Environment.NewLine}params => {paramText}");
                    failCounter++;
                    if (failCounter > 10) break;
                }
            }

            Assert.IsTrue(isSuccess);
        }

    }
}
