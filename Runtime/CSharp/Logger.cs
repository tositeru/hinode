using System.Collections;
using System.Collections.Generic;
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

        public static void Log(Priority priority, System.Func<string> getLog)
        {
            if (PriorityLevel < priority)
                return;

            Debug.Log(GetPrefix(priority) + getLog());
        }

        public static void LogWarning(Priority priority, System.Func<string> getLog)
        {
            if (PriorityLevel < priority)
                return;

            Debug.LogWarning("Warning!! " + GetPrefix(priority) + getLog());
        }

        public static void LogError(Priority priority, System.Func<string> getLog)
        {
            if (PriorityLevel < priority)
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
