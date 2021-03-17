using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp.Extensions
{
    /// <summary>
    /// <seealso cref="RegexExtensions"/>
    /// </summary>
    public class TestRegexExntensions
    {
        const int ORDER_Basic = 0;

        /// <summary>
        /// <seealso cref="RegexExtensions.GetMatchEnumerable(Regex, string, int)"/>
        /// </summary>
        [Test, Order(ORDER_Basic)]
        public void GetMatchEnumerable_Passes()
        {
            var text = "abc def ghi";

            var regex = new Regex(@"\w+");
            AssertionUtils.AssertEnumerable(
                new string[] {
                    "abc", "def", "ghi"
                }
                , regex.GetMatchEnumerable(text).Select(_m => _m.ToString())
                , $"Fail..."
            );

            AssertionUtils.AssertEnumerable(
                new string[] {
                    "ef", "ghi"
                }
                , regex.GetMatchEnumerable(text, 5).Select(_m => _m.ToString())
                , $"Fail..."
            );

        }
    }
}
