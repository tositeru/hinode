using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Hinode
{
    public static class TextReaderExtensions
    {
        public static bool MoveTo(this TextReader t, char ch)
        {
            int p = t.Read();
            while(p != -1 && (char)p != ch) p = t.Read();
            return p != -1;
        }

        public static bool MoveTo(this TextReader t, params char[] charList)
        {
            int p = t.Read();
            while (p != -1 && !charList.Any(_c => _c == (char)p)) p = t.Read();
            return p != -1;
        }

        public static bool MeveTo(this TextReader t, Regex regexForChar)
        {
            int p = t.Read();
            while (p != -1 && !regexForChar.IsMatch(((char)p).ToString())) p = t.Read();
            return p != -1;
        }

        public static bool SkipTo(this TextReader t, char ch)
        {
            int p = t.Peek();
            while (p != -1 && (char)p == ch) { t.Read(); p = t.Peek(); }
            return p != -1;
        }

        public static bool SkipTo(this TextReader t, params char[] charList)
        {
            int p = t.Peek();
            while (p != -1 && charList.Any(_c => _c == (char)p)) { t.Read(); p = t.Peek(); }
            return p != -1;
        }

        public static bool SkipTo(this TextReader t, Regex regexForChar)
        {
            int p = t.Peek();
            while (p != -1 && regexForChar.IsMatch(((char)p).ToString())) { t.Read(); p = t.Peek(); }
            return p != -1;
        }

        public static bool IsMatchPeek(this TextReader t, char ch)
        {
            var p = t.Peek();
            return p == -1 || (char)p == ch;
        }

        public static bool IsMatchPeek(this TextReader t, params char[] chars)
        {
            var p = t.Peek();
            return p == -1 || chars.Any(_c => _c == (char)p);
        }

        public static bool IsMatchPeek(this TextReader t, Regex regex)
        {
            var p = t.Peek();
            return p == -1 || regex.IsMatch(((char)p).ToString());
        }

    }
}
