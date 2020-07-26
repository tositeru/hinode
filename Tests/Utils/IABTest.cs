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
                    var paramText = GetParamTexts().Select(_t => _t.paramText).Aggregate("", (_s, _c) => _s + _c + ",");
                    Debug.LogWarning($"Detect Fail Test Case... Message={e.Message}{System.Environment.NewLine}params => {paramText}");
                }
                catch
                {
                    isSuccess = false;
                    var paramText = GetParamTexts().Select(_t => _t.paramText).Aggregate("", (_s, _c) => _s + _c + ",");
                    Debug.LogWarning($"Detect Fail Test Case... Message=<<Unknown Exception>>.{System.Environment.NewLine}params => {paramText}");
                }
            }

            Assert.IsTrue(isSuccess);
        }

    }
}
