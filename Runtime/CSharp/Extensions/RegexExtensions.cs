using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Hinode
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class RegexExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="regex"></param>
        /// <param name="str"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static IEnumerable<Match> GetMatchEnumerable(this Regex regex, string str, int pos = 0)
        {
            return new MatchEnumerable(regex, str, pos);
        }

        class MatchEnumerable : IEnumerable<Match>, IEnumerable
        {
            Regex _target;
            string _str;
            int _pos;

            public MatchEnumerable(Regex target, string str, int pos)
            {
                _target = target;
                _str = str;
                _pos = pos;
            }

            public IEnumerator<Match> GetEnumerator()
            {
                for (Match match = _target.Match(_str, _pos); match.Success; match = match.NextMatch())
                {
                    yield return match;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }

    }
}
