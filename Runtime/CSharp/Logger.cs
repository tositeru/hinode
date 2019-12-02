using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// ログを出力するかどうか制御できるLogger
    /// </summary>
    public class Logger
    {
        public enum Priority
        {
            None,
            High,
            Middle,
            Low,
            Debug,
        }

        /// <summary>
        /// 現在の有効なログのPriorityLevel
        ///
        /// これと同じか低いものがログ出力されます。
        /// </summary>
        public static Priority PriorityLevel { get; set; } = Priority.High;

        static HashSet<string> _selectors = new HashSet<string>();
        public static IEnumerable<string> Selectors { get => _selectors; }

        public static bool IsMatchSelectors(params string[] selectors)
            => !Selectors.Any() || selectors.All(_s => Selectors.Contains(_s));

        public static void AddSelector(string selector)
        {
            if(!_selectors.Contains(selector))
                _selectors.Add(selector);
        }

        public static void RemoveSelector(string selector)
        {
            _selectors.Remove(selector);
        }

        public static void Log(Priority priority, System.Func<string> getLog, params string[] selectors)
        {
            if (PriorityLevel < priority)
                return;

            if(!IsMatchSelectors(selectors))
                return;

            Debug.Log(GetPrefix(priority) + getLog());
        }

        public static void LogWarning(Priority priority, System.Func<string> getLog, params string[] selectors)
        {
            if (PriorityLevel < priority)
                return;

            if (!IsMatchSelectors(selectors))
                return;

            Debug.LogWarning("Warning!! " + GetPrefix(priority) + getLog());
        }

        public static void LogError(Priority priority, System.Func<string> getLog, params string[] selectors)
        {
            if (PriorityLevel < priority)
                return;

            if (!IsMatchSelectors(selectors))
                return;

            Debug.LogError("Error!! " + GetPrefix(priority) + getLog());
        }

        static string GetPrefix(Priority priority)
        {
            switch(priority)
            {
                case Priority.Debug: return "debug -- ";
                default: return "";
            }
        }
    }
}
