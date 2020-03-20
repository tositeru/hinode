using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    public static class SoundUtils
    {
        /// <summary>
        /// Sin波を計算する
        /// </summary>
        /// <param name="index"></param>
        /// <param name="A">振幅</param>
        /// <param name="basicFreq">基本周波数</param>
        /// <param name="samplingRate">標本化周波数</param>
        /// <returns></returns>
        public static float CalSinWave(float second, float A, float basicFreq, float samplingRate)
        {
            return A * Mathf.Sin(second * basicFreq * 2 * Mathf.PI / samplingRate);
        }
    }
}
