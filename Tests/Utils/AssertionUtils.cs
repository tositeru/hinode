using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NUnit.Framework;

namespace Hinode.Tests
{
    public static class AssertionUtils
    {
        public static void AssertEnumerable<T>(IEnumerable<T> gots, IEnumerable<T> corrects, string message)
        {
            var index = 0;
            foreach (var (t, correct) in gots.Zip(corrects, (_t, _c) => (t: _t, c: _c)))
            {
                Assert.AreEqual(correct, t, $"{message}: don't be same... index=>{index}, correct={correct}, got={t}");
                index++;
            }
            Assert.AreEqual(corrects.Count(), index, $"{message}: Don't Equal count...");
        }
    }
}
