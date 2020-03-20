using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
	/// フーリエ変換
	/// </summary>
    public class FourierTransform
    {
        Complex CreateW(float theta, int no)
        {
            return new Complex(Mathf.Cos(theta * no), Mathf.Sin(theta * no));
        }

        public void FT(Complex[] input, Complex[] output)
        {
            Assert.IsTrue(input.Length == output.Length);

            var baseTheta = -2f * Mathf.PI / (float)(input.Length);
            var SinCosTable = new Dictionary<int, Complex>();
            System.Func<int, Complex> createElement = (int no) => {
                if(SinCosTable.ContainsKey(no))
                    return SinCosTable[no];
                var c = new Complex(Mathf.Cos(baseTheta * no), Mathf.Sin(baseTheta * no));
                SinCosTable.Add(no, c);
                return c;
            };

            var validMatrix = new Complex[input.Length, input.Length];
            for (var row = 0; row < validMatrix.GetLength(0); ++row)
            {
                for (var col = 0; col < validMatrix.GetLength(1); ++col)
                {
                    output[row].Add(createElement(row * col) * input[col]);
                }
            }
        }

        public Complex[] FT(Complex[] input)
        {
            var output = new Complex[input.Length];
            FT(input, output);
            return output;
        }

        /// <summary>
        /// 高速フーリエ変換
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        public void FFT(Complex[] input, Complex[] output)
        {
            var baseTheta = -2 * Mathf.PI / input.Length;
            FFTCore(input, output, baseTheta);
        }

        /// <summary>
        /// 高速フーリエ変換
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Complex[] FFT(Complex[] input)
        {
            var result = new Complex[input.Length];
            FFT(input, result);
            return result;
        }

        /// <summary>
        /// 逆高速フーリエ変換
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        public void IFFT(Complex[] input, Complex[] output)
        {
            var baseTheta = 2 * Mathf.PI / input.Length;
            FFTCore(input, output, baseTheta);

            // 1 / N
            var ratio = 1f / (float)output.Length;
            for (var i=0; i< output.Length; ++i)
            {
                output[i].real *= ratio;
                output[i].image *= 0;
            }
        }

        /// <summary>
        /// 逆高速フーリエ変換
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Complex[] IFFT(Complex[] input)
        {
            var result = new Complex[input.Length];
            IFFT(input, result);
            return result;
        }

        /// <summary>
        /// 高速フーリエ変換の計算のコア部分
        /// バタフライ演算を行なっている
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="baseTheta"></param>
        public void FFTCore(Complex[] input, Complex[] output, float baseTheta)
        {
            Assert.IsTrue(IsValidLengthForFFT(input.Length));
            Assert.IsTrue(input.Length == output.Length);

            var halfLen = input.Length / 2;
            var WTables = new Dictionary<int, Complex>();
            for (var i = 0; i < halfLen; ++i)
            {
                WTables[i] = CreateW(baseTheta, i);
            }

            var src = new Complex[input.Length];
            var dest = new Complex[input.Length];
            CaluculateButtefly(input, src, WTables, 0, input.Length);
            var tmpLen = halfLen;
            while (tmpLen > 2)
            {
                var calCount = input.Length / tmpLen;
                for (var i = 0; i < calCount; ++i)
                {
                    CaluculateButtefly(src, dest, WTables, i, tmpLen);
                }
                tmpLen >>= 1;

                var swap = src; src = dest; dest = swap;
            }

            //要素の入れ替え
            CaluculateButteflyWithSwapIndex(output, src);
        }

        void CaluculateButteflyWithSwapIndex(Complex[] dest, Complex[] src)
        {
            for (var i = 0; i < src.Length; i += 2)
            {
                var index1 = ReverseBit(i, src.Length);
                dest[index1] = src[i + 0] + src[i + 1];

                var index2 = ReverseBit(i + 1, src.Length);
                dest[index2] = src[i + 0] - src[i + 1];
            }
        }

        void CaluculateButtefly(Complex[] input, Complex[] output, Dictionary<int, Complex> WTables, int index, int len)
        {
            var s = index * len;
            var half = len / 2;
            var halfS = s + half;
            var ratio = (int)Mathf.Log(input.Length, 2) - (int)Mathf.Log(len, 2);
            for (var i = 0; i < half; ++i)
            {
                output[s + i] = input[s + i] + input[halfS + i];
                output[halfS + i] = WTables[i << ratio] * (input[s + i] - input[halfS + i]);
            }
            //Debug.Log($"debug -- half={half}, s={s}, ratio={ratio}, index={index}, len={len}");
        }

        /// <summary>
        /// 高速フーリエ変換が可能な要素数かどうかチェックする
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        public static bool IsValidLengthForFFT(int len)
		{
			if (len <= 1) return false;
			int c = 0;
            while(len != 0 && c <= 1)
			{
				c += len & 0x1;
				len >>= 1;
			}
			return c == 1;
		}

        /// <summary>
        /// ビットリバースを行う関数
        /// </summary>
        /// <param name="index"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static int ReverseBit(int index, int len)
        {
            //２の乗数か？
            Assert.IsTrue(IsValidLengthForFFT(len));

            var bit = 0x0;
            index |= len;
            while (index > 1)
            {
                bit <<= 1;
                bit |= index & 0x1;
                index >>= 1;
            }
            return bit;
        }
    }
}
