using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// <see cref="IFrameDataRecorder"/>
    /// </summary>
    public static class IFrameInputDateRecorderHelper
    {
        /// <summary>
        /// <seealso cref="FrameInputData.RegistChildFrameInputDataType(string, System.Type)"/>
        /// </summary>
        public static void RegistTypeToFrameInputData(string key, System.Type type)
        {
            if (!FrameInputData.ContainsChildFrameInputDataType(type)
                && !FrameInputData.ContainsChildFrameInputDataType(key))
            {
                FrameInputData.RegistChildFrameInputDataType(key, type);
            }
            else
            {
                Logger.LogWarning(Logger.Priority.High, () => $"{type.Name} already regist in FrameInputData... key={key}", InputLoggerDefines.SELECTOR_MAIN, InputLoggerDefines.SELECTOR_RECORDER);
            }
        }

    }
}
