using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hinode.Tests.CSharp.Extensions
{
    /// <summary>
	/// <seealso cref="StringExtensions"/>
	/// </summary>
    public class TestStringExtensions
    {
        const int ORDER_Basic = 0;
        const int ORDER_AfterBasic = ORDER_Basic + 100;
        const int ORDER_GetLineEnumerable = ORDER_Basic + 100;
        const int ORDER_GetCharEnumerable = ORDER_Basic + 100;

        [Test, Order(ORDER_Basic)]
        public void GetNewlineStr_Passes()
        {
            var text = "a\r\n"
                + "b\n"
                + "c\r\n"
                + "d"
                ;

            //check first line 'a\r\n'
            Assert.AreEqual("\r\n", text.GetNewlineStr());
            Assert.AreEqual("\r\n", text.GetNewlineStr(text.IndexOf('a') + 1));
            Assert.AreEqual("\r\n", text.GetNewlineStr(text.IndexOf('a') + 2));

            //check first line 'b\n'
            Assert.AreEqual("\n", text.GetNewlineStr(text.IndexOf('b')));
            Assert.AreEqual("\n", text.GetNewlineStr(text.IndexOf('b') + 1));

            //check first line 'c\r\n'
            Assert.AreEqual("\r\n", text.GetNewlineStr(text.IndexOf('c')));
            Assert.AreEqual("\r\n", text.GetNewlineStr(text.IndexOf('c') + 1));
            Assert.AreEqual("\r\n", text.GetNewlineStr(text.IndexOf('c') + 2));

            //check first line 'd'
            Assert.AreEqual("", text.GetNewlineStr(text.IndexOf('d')));

            // invalid pos
            Assert.AreEqual(text.GetNewlineStr(0), text.GetNewlineStr(-1));
            Assert.AreEqual(text.GetNewlineStr(text.Length-1), text.GetNewlineStr(text.Length));
        }

        [Test, Order(ORDER_Basic)]
        public void GetLineNo_Passes()
        {
            var br = System.Environment.NewLine;

            {
                var text = "a\n"
                    + "b\n"
                    + "c\n"
                    + "d"
                    ;

                var lineNo = 1;
                for(var i=0; i<text.Length; ++i)
                {
                    Assert.AreEqual(lineNo, text.GetLineNo(i), $"Fail... pos={i}");
                    if (text[i] == '\n') lineNo++;
                }
            }

            var rnd = new System.Random();
            for (var i=0; i<1000; ++i)
            {
                var newlineCount = rnd.Range(1, 20);
                var text = Enumerable.Range(0, newlineCount)
                    .Select(_i => rnd.RandomString(rnd.Range(1, 10)))
                    .Aggregate("", (_s, _c) => _s + _c + br);
                Assert.AreEqual(newlineCount, text.GetLineNo(), $"Fail...");

                var pos = rnd.Range(0, text.Length);
                var newlineCount2 = 1;
                for(var j=0;j<pos; ++j)
                {
                    if(text[j] == '\n') newlineCount2++;
                }
                Assert.AreEqual(newlineCount2, text.GetLineNo(pos), $"Fail to specify pos({pos})...");
            }
        }

        /// <summary>
        /// <seealso cref="StringExtensions.GoNext(string, char, int)"/>
        /// <seealso cref="StringExtensions.GoNext(string, string, int)"/>
        /// <seealso cref="StringExtensions.GoNext(string, Regex, out Match, int)"/>
        /// </summary>
        [Test, Order(ORDER_Basic)]
        public void GoNext_Passes()
        {
            var text = "abcdefghijklmnopqrstuvwxyz"
                + "abcdefghijklmnopqrstuvwxyz";

            {
                var pos = text.IndexOf('d');
                Assert.AreEqual(pos, text.GoNext('d'));
                Assert.AreEqual(pos+1, text.GoNext('d', 0, 1));

                Assert.AreEqual(text.IndexOf('d', pos + 1), text.GoNext('d', 20));
                Assert.AreEqual(text.IndexOf('d', pos + 1) + 2, text.GoNext('d', 20, 2));

                var overflow = text.Length * 2;
                Assert.AreEqual(text.Length, text.GoNext('d', 20, overflow));
                Assert.AreEqual(0, text.GoNext('d', 0, -overflow));
            }

            {
                var key = "xyz";
                var pos = text.IndexOf(key);
                Assert.AreEqual(text.IndexOf(key), text.GoNext(key));
                Assert.AreEqual(text.IndexOf(key) + 1, text.GoNext(key, 0, 1));

                Assert.AreEqual(text.IndexOf(key, pos + 1), text.GoNext(key, pos + 1));
                Assert.AreEqual(text.IndexOf(key, pos + 1) + 2, text.GoNext(key, pos + 1, 2));

                var overflow = text.Length * 2;
                Assert.AreEqual(text.Length, text.GoNext("xyz", 0, overflow));
                Assert.AreEqual(0, text.GoNext("xyz", 0, -overflow));
            }

            {
                var regex = new Regex(@"abc");
                Assert.AreEqual(regex.Match(text).Index, text.GoNext(regex, out var match));
                Assert.AreEqual(regex.Match(text).Index + 1, text.GoNext(regex, out match, 0, 1));

                Assert.AreEqual(regex.Match(text).NextMatch().Index, text.GoNext(regex, out match, 10));
                Assert.AreEqual(regex.Match(text).NextMatch().Index + 1, text.GoNext(regex, out match, 10, 1));

                var overflow = text.Length * 2;
                Assert.AreEqual(text.Length, text.GoNext(regex, out match, 0, overflow));
                Assert.AreEqual(0, text.GoNext(regex, out match, 0, -overflow));
            }
        }

        #region GetLineEnumerable
        /// <summary>
        /// <seealso cref="StringExtensions.GetLineEnumerable(string, int)"/>
        /// </summary>
        [Test, Order(ORDER_GetLineEnumerable)]
        public void GetLineEnumerable_Passes()
        {
            var text = "a\r\n"
                + "bb\n"
                + "c\r\n"
                + "d"
                ;

            AssertionUtils.AssertEnumerable(
                new (string line, int counter)[] {
                    ("a\r\n", 1),
                    ("bb\n", 2),
                    ("c\r\n", 3),
                    ("d", 4),
                }
                , text.GetLineEnumerable().Select(_p => (_p.GetLine() + _p.Newline, _p.LineCounter))
                , "Fail when basic case...");

            AssertionUtils.AssertEnumerable(
                new (string line, int counter)[] {
                    ("b\n", 1),
                    ("c\r\n", 2),
                    ("d", 3),
                }
                , text.GetLineEnumerable(text.GoNext('\n', 0, 2))
                    .Select(_p => (_p.GetLine() + _p.Newline, _p.LineCounter))
                , "Fail to specify 'pos' parameter...");

            AssertionUtils.AssertEnumerable(
                new (string line, int counter)[] {
                    ("a\r\n", 1),
                    ("bb\n", 2),
                }
                , text.GetLineEnumerable(0, text.IndexOf('c', 0))
                    .Select(_p => (_p.GetLine() + _p.Newline, _p.LineCounter))
                , "Fail to specify 'endPos' parameter...");


        }

        /// <summary>
        /// 
        /// </summary>
        [Test, Order(ORDER_GetLineEnumerable)]
        public void GetLineEnumerable_IsEmptyOrWhiteSpace_Passes()
        {
            var text = "a\r\n"
                + "\t \tb\n"
                + "\n"
                + "  c\r\n"
                + "   \n"
                + "A   \n"
                + "d"
                ;

            AssertionUtils.AssertEnumerable(
                new bool[] {
                    false,
                    false,
                    true,
                    false,
                    true,
                    false,
                    false,
                }
                , text.GetLineEnumerable().Select(_p => _p.IsEmptyOrWhiteSpaceLine)
                , "");
        }
        #endregion

        #region
        /// <summary>
        /// <seealso cref="StringExtensions.GetCharEnumerable(string, int, int)"/>
        /// </summary>
        [Test, Order(ORDER_GetCharEnumerable)]
        public void GetCharEnumerable_Passes()
        {
            var text = "abcde";
            AssertionUtils.AssertEnumerable(
                new (char, int)[] {
                    ('a', 0), ('b', 1), ('c', 2), ('d', 3), ('e', 4)
                }
                , text.GetCharEnumerable()
                , ""
            );

            AssertionUtils.AssertEnumerable(
                new (char, int)[] {
                    ('c', 2), ('d', 3), ('e', 4)
                }
                , text.GetCharEnumerable(2)
                , ""
            );

            AssertionUtils.AssertEnumerable(
                new (char, int)[] {
                    ('a', 0), ('b', 1),
                }
                , text.GetCharEnumerable(0, 2)
                , ""
            );

            AssertionUtils.AssertEnumerable(
                new (char, int)[] {
                }
                , text.GetCharEnumerable(10)
                , ""
            );
            AssertionUtils.AssertEnumerable(
                new (char, int)[] {
                    ('a', 0), ('b', 1), ('c', 2), ('d', 3), ('e', 4)
                }
                , text.GetCharEnumerable(0, 100)
                , ""
            );

        }
        #endregion
    }
}
