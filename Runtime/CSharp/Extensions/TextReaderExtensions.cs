using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="TextReader"/>
    /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestTextReaderExtensions"/>
    /// </summary>
    public static class TextReaderExtensions
    {
        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestTextReaderExtensions.MoveToWithCharPasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static bool MoveTo(this TextReader t, char ch)
        {
            int p = t.Read();
            while(p != -1 && (char)p != ch) p = t.Read();
            return p != -1;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestTextReaderExtensions.MoveToWithCharArrayPasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="charList"></param>
        /// <returns></returns>
        public static bool MoveTo(this TextReader t, params char[] charList)
        {
            int p = t.Read();
            while (p != -1 && !charList.Any(_c => _c == (char)p)) p = t.Read();
            return p != -1;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestTextReaderExtensions.MoveToWithRegexPasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="regexForChar"></param>
        /// <returns></returns>
        public static bool MoveTo(this TextReader t, Regex regexForChar)
        {
            int p = t.Read();
            while (p != -1 && !regexForChar.IsMatch(((char)p).ToString())) p = t.Read();
            return p != -1;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestTextReaderExtensions.SkipToWithCharPasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static bool SkipTo(this TextReader t, char ch)
        {
            int p = t.Peek();
            while (p != -1 && (char)p == ch) { t.Read(); p = t.Peek(); }
            return p != -1;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestTextReaderExtensions.SkipToWithCharArrayPasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="charList"></param>
        /// <returns></returns>
        public static bool SkipTo(this TextReader t, params char[] charList)
        {
            int p = t.Peek();
            while (p != -1 && charList.Any(_c => _c == (char)p)) { t.Read(); p = t.Peek(); }
            return p != -1;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestTextReaderExtensions.SkipToWithRegexPasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="regexForChar"></param>
        /// <returns></returns>
        public static bool SkipTo(this TextReader t, Regex regexForChar)
        {
            int p = t.Peek();
            while (p != -1 && regexForChar.IsMatch(((char)p).ToString())) { t.Read(); p = t.Peek(); }
            return p != -1;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestTextReaderExtensions.IsMatchPeekWithCharPasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static bool IsMatchPeek(this TextReader t, char ch)
        {
            var p = t.Peek();
            return p == -1 || (char)p == ch;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestTextReaderExtensions.IsMatchPeekWithCharArrayPasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="chars"></param>
        /// <returns></returns>
        public static bool IsMatchPeek(this TextReader t, params char[] chars)
        {
            var p = t.Peek();
            return p == -1 || chars.Any(_c => _c == (char)p);
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestTextReaderExtensions.IsMatchPeekWithRegexPasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="regex"></param>
        /// <returns></returns>
        public static bool IsMatchPeek(this TextReader t, Regex regex)
        {
            var p = t.Peek();
            return p == -1 || regex.IsMatch(((char)p).ToString());
        }

    }
}
