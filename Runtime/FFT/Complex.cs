using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
	/// <summary>
	/// float型の複素数
	/// </summary>
	public struct Complex
	{
		public float real;
		public float image;

		public Complex(float real) : this(real, (float)0) { }
		public Complex(Complex other) : this(other.real, other.image) { }
		public Complex(float real, float image)
		{
			this.real = real;
			this.image = image;
		}

		/// <summary>
		/// 余分なnew()をしたくない時はこちらを使用してください。
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public Complex Add(Complex other)
		{
			this.real += other.real;
			this.image += other.image;
			return this;
		}
		/// <summary>
		/// 余分なnew()をしたくない時はこちらを使用してください。
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public Complex Sub(Complex other)
		{
			this.real -= other.real;
			this.image -= other.image;
			return this;
		}
		/// <summary>
		/// 余分なnew()をしたくない時はこちらを使用してください。
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public Complex Mul(Complex other)
		{
			var r = real * other.real - image * other.image;
			var i = real * other.image + image * other.real;
			this.real = r;
			this.image = i;
			return this;
		}
		/// <summary>
		/// 余分なnew()をしたくない時はこちらを使用してください。
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public Complex Div(Complex other)
		{
			var lenSq = other.LengthSq;
		    var r = (real * other.real + image * other.image) / lenSq;
		    var i = (real * other.image - image * other.real) / lenSq;
			this.real = r;
			this.image = i;
			return this;
		}

        /// <summary>
        /// 近似等値反転
        /// </summary>
        /// <param name="other"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public bool NearlyEqual(Complex other, float epsilon)
        {
			return Mathf.Abs(real - other.real) <= epsilon
                &&  Mathf.Abs(image - other.image) <= epsilon;
        }

        public float Length { get => Mathf.Sqrt(LengthSq); }
		public float LengthSq { get => real * real + image * image; }

        public override string ToString()
        {
            return $"({real},{image})";
        }

        public static Complex operator +(Complex n) => n;
		public static Complex operator -(Complex n) => new Complex(-n.real, -n.image);

		public static Complex operator +(Complex l, Complex r) => new Complex(l.real + r.real, l.image + r.image);
		public static Complex operator -(Complex l, Complex r) => new Complex(l.real - r.real, l.image - r.image);
		public static Complex operator *(Complex l, Complex r)
			=> new Complex(l.real * r.real - l.image * r.image, l.real * r.image + l.image * r.real);
		public static Complex operator /(Complex l, Complex r)
		{
			var lenSq = r.LengthSq;
			return new Complex(
                (l.real * r.real + l.image * r.image) / lenSq,
				(l.real * r.image - l.image * r.real)/ lenSq);
        }

		public static implicit operator (float, float)(Complex n) => (n.real, n.image);
		public static implicit operator Complex((float, float) t) => new Complex(t.Item1, t.Item2);
		public static implicit operator Complex(Vector2 v) => new Complex(v.x, v.y);
		public static implicit operator Vector2(Complex n) => new Vector2(n.real, n.image);
	}
}