using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// IEventHandlerのキーワードにできるものを指定するAttribute
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class EnableKeywordForEventHandlerAttribute : System.Attribute
    {
        readonly string[] _keywords;

        public bool DoMatchKeyword(string keyword)
            => _keywords.Contains(keyword);

        public EnableKeywordForEventHandlerAttribute(params string[] keywords)
        {
            _keywords = keywords;
        }
    }
}
