using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Hinode
{
    public static partial class StringExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static string GetNewlineStr(this string text, int pos = 0)
        {
            pos = System.Math.Min(System.Math.Max(pos, 0), text.Length-1);
            var newlinePos = text.IndexOf('\n', pos);
            if (newlinePos == -1) return "";

            return (text.Length >= 2 && text[newlinePos - 1] == '\r')
                ? "\r\n"
                : "\n";
        }

        /// <summary>
		/// 
		/// </summary>
		/// <param name="str"></param>
		/// <param name="pos"></param>
		/// <returns></returns>
        public static int GetLineNo(this string str, int pos=-1)
        {
            pos = System.Math.Min(pos, str.Length-1);
            if (pos == -1) pos = str.Length-1;

            var line = 1;
            for(var i=0; i<pos; ++i)
            {
                if (str[i] == '\n') line++;
            }
            return line;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ch"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static int GoNext(this string str, char ch, int pos=0, int offset=0)
        {
            var next = str.IndexOf(ch, pos);
            return next != -1
                ? System.Math.Max(0, System.Math.Min(next+offset, str.Length))
                : str.Length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="s"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static int GoNext(this string str, string s, int pos = 0, int offset=0)
        {
            var next = str.IndexOf(s, pos);
            return next != -1
                ? System.Math.Max(0, System.Math.Min(next + offset, str.Length))
                : str.Length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="regex"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static int GoNext(this string str, Regex regex, out Match outMatch, int pos = 0, int offset=0)
        {
            outMatch = regex.Match(str, pos);
            return outMatch.Success
                ? System.Math.Max(0, System.Math.Min(outMatch.Index + offset, str.Length))
                : str.Length;
        }

        public class LineEnumerableParam
        {
            public string Target { get; }
            public int Start { get; set; }
            public int End { get; set; }
            public string Newline { get; set; }
            public int LineCounter { get; set; } = 0;
            public int InitialStart { get; }
            public int TerminalPos { get; }

            public bool IsTerminal { get => Start >= TerminalPos; }
            public bool IsEmptyOrWhiteSpaceLine
            {
                get
                {
                    if (IsTerminal) return true;
                    if (Start == End) return true;

                    for(int i=Start; i<=End; ++i)
                    {
                        if (!char.IsWhiteSpace(Target[i])) return false;
                    }
                    return true;
                }
            }

            public string GetLine()
                => IsTerminal
                ? null
                : Target.Substring(Start, End-Start);

            public LineEnumerableParam(string target, int start, int end)
            {
                Target = target;
                InitialStart = start;
                TerminalPos = end == -1
                    ? Target.Length
                    : System.Math.Min(Target.Length, end);
                Reset();
            }

            public void Reset()
            {
                Start = InitialStart;
                End = Start;
                LineCounter = 0;
                Newline = "";
            }

            public bool GoNext()
            {
                Start = End + Newline.Length;
                if (IsTerminal) return false;

                End = Target.IndexOf('\n', Start);
                if(End == -1)
                {
                    End = Target.Length;
                    Newline = "";
                }
                else
                {
                    Newline = Target.GetNewlineStr(End);
                    End -= Newline.Length-1;
                }
                LineCounter++;
                return true;
            }
        }

        public static IEnumerable<LineEnumerableParam> GetLineEnumerable(this string str, int pos=0, int endPos=-1)
        {
            return new LineEnumerable(str, pos, endPos);
        }

        class LineEnumerable : IEnumerable<LineEnumerableParam>, IEnumerable
        {
            public LineEnumerableParam CurrentParam { get; }

            public LineEnumerable(string str, int startPos, int endPos)
            {
                CurrentParam = new LineEnumerableParam(str, startPos, endPos);
            }

            public IEnumerator<LineEnumerableParam> GetEnumerator()
            {
                CurrentParam.Reset();

                while (CurrentParam.GoNext())
                {
                    yield return CurrentParam;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }

        public static IEnumerable<(char ch, int index)> GetCharEnumerable(this string str, int start=0, int end=-1)
        {
            return new CharEnumerable(str, start, end);
        }

        class CharEnumerable : IEnumerable<(char ch, int index)>, IEnumerable
        {
            string _target;
            int _start;
            int _end;

            public CharEnumerable(string target, int start, int end)
            {
                _target = target;
                _start = System.Math.Max(0, start);
                _end = end;
            }

            public IEnumerator<(char ch, int index)> GetEnumerator()
            {
                int end = _end == -1
                    ? _target.Length
                    : System.Math.Min(_target.Length, _end);
                for(var i=_start; i<end; ++i)
                {
                    yield return (_target[i], i);
                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }

    }
}
