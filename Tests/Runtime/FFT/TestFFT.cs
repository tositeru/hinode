using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace Hinode.Tests.FFT
{
    public class TestFFT : TestBase
    {
        static readonly float EPSILON = 0.00001f;

        /// <summary>
        /// FFT可能な要素数かどうかチェックする関数のテスト
        /// </summary>
        [Test]
        public void IsValidLengthForFFTPasses()
        {
            for (var i = 1; i < 31; ++i)
            {
                Assert.IsTrue(FourierTransform.IsValidLengthForFFT(0x1 << i), $"Failed {0x1 << i:x}");
            }

            Assert.IsFalse(FourierTransform.IsValidLengthForFFT(1));
            Assert.IsFalse(FourierTransform.IsValidLengthForFFT(3));
            Assert.IsFalse(FourierTransform.IsValidLengthForFFT(2249));
            Assert.IsFalse(FourierTransform.IsValidLengthForFFT(-23));
            Assert.IsFalse(FourierTransform.IsValidLengthForFFT(-1));
        }

        /// <summary>
        /// ビットリバースのテスト
        /// </summary>
        [Test]
        public void ReverseBitPasses()
        {
            Assert.AreEqual(0, FourierTransform.ReverseBit(0, 8));
            Assert.AreEqual(4, FourierTransform.ReverseBit(1, 8));
            Assert.AreEqual(2, FourierTransform.ReverseBit(2, 8));
            Assert.AreEqual(6, FourierTransform.ReverseBit(3, 8));
            Assert.AreEqual(1, FourierTransform.ReverseBit(4, 8));
            Assert.AreEqual(5, FourierTransform.ReverseBit(5, 8));
            Assert.AreEqual(3, FourierTransform.ReverseBit(6, 8));
            Assert.AreEqual(7, FourierTransform.ReverseBit(7, 8));

            Assert.AreEqual(0, FourierTransform.ReverseBit(0, 16));
            Assert.AreEqual(8, FourierTransform.ReverseBit(1, 16));
            Assert.AreEqual(4, FourierTransform.ReverseBit(2, 16));
            Assert.AreEqual(12, FourierTransform.ReverseBit(3, 16));
            Assert.AreEqual(2, FourierTransform.ReverseBit(4, 16));
            Assert.AreEqual(10, FourierTransform.ReverseBit(5, 16));
            Assert.AreEqual(6, FourierTransform.ReverseBit(6, 16));
            Assert.AreEqual(14, FourierTransform.ReverseBit(7, 16));
            Assert.AreEqual(1, FourierTransform.ReverseBit(8, 16));
            Assert.AreEqual(9, FourierTransform.ReverseBit(9, 16));
            Assert.AreEqual(5, FourierTransform.ReverseBit(10, 16));
            Assert.AreEqual(13, FourierTransform.ReverseBit(11, 16));
            Assert.AreEqual(3, FourierTransform.ReverseBit(12, 16));
            Assert.AreEqual(11, FourierTransform.ReverseBit(13, 16));
            Assert.AreEqual(7, FourierTransform.ReverseBit(14, 16));
            Assert.AreEqual(15, FourierTransform.ReverseBit(15, 16));
        }

        [Test]
        public void FFTPasses()
        {
            var input = Enumerable.Range(0, 8)
                .Select(_i => new Complex(SoundUtils.CalSinWave(_i, 0.25f, 250f, 8000), 0))
                .ToArray();
            var fft = new FourierTransform();
            var output = fft.FFT(input);

            //検証用のデータ作成
            var corrects = fft.FT(input);

            AssertionUtils.AreEqualComplexArray(corrects, output, "", EPSILON);
        }

        [Test]
        public void IFFTPasses()
        {
            var input = Enumerable.Range(0, 8)
                .Select(_i => new Complex(SoundUtils.CalSinWave(_i, 0.25f, 250f, 8000), 0))
                .ToArray();
            var fft = new FourierTransform();
            var output = fft.FFT(input);
            var gots = fft.IFFT(output);

            AssertionUtils.AreEqualComplexArray(input, gots, "逆変換に失敗しています", EPSILON);
        }

    }
}
