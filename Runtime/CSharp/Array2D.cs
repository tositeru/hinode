using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Hinode
{
    public interface IReadOnlyArray2D<T> : IEnumerable<T>, IEnumerable
    {
        int Width { get; }
        int Height { get; }
        (int width, int height) Size { get; }

        int Count { get; }

        Array2D<T> Copy();

        bool IsInRange(int x, int y);
        T this[int x, int y] { get; }

        int ToIndex(int x, int y);
        (int x, int y) ToXY(int index);

        IEnumerable<(T value, int index)> GetEnumerableWithIndex();
        IEnumerable<(T value, int x, int y)> GetEnumerableWithIndexXY();
    }

    /// <summary>
    /// <seealso cref="Hinode.Tests.TestArray2D"/>
    /// </summary>
    [System.Serializable]
    public class Array2D<T> : IReadOnlyArray2D<T>, IEnumerable<T>, IEnumerable
    {
        [SerializeField] T[] _data;
        [SerializeField] int _width;
        [SerializeField] int _height;

        public int Width { get => _width; }
        public int Height { get => _height; }
        public (int width, int height) Size { get => (Width, Height); }

        public int Count { get => Width * Height; }

        public bool IsInRange(int x, int y)
            => 0 <= x && x < Width
            && 0 <= y && y < Height;

        public T this[int x, int y]
        {
            get => _data[ToIndex(x, y)];
            set
            {
                _data[ToIndex(x, y)] = value;
            }
        }

        public Array2D()
        { }

        public Array2D(int width, int height, params T[] values)
            : this(width, height, values.GetEnumerable<T>())
        { }

        public Array2D(int width, int height, IEnumerable<T> values)
        {
            Resize(width, height, values);
        }

        public Array2D<T> Copy()
        {
            return new Array2D<T>(Width, Height, _data);
        }

        public void Resize(int width, int height)
        {
            if (_width == width && _height == height) return;

            _width = width;
            _height = height;
            _data = new T[Count];
        }

        public void Resize(int width, int height, int offsetX, int offsetY)
        {
            var prev = (data: _data, Width, Height);
            if (_width != width || _height != height)
            {
                _width = width;
                _height = height;
                _data = new T[Count];
            }

            ShiftX(_data, prev.data, offsetX, Size, (prev.Width, prev.Height));
            ShiftY(_data, _data, offsetY, Size, Size);
        }

        public void Resize(int width, int height, params T[] values)
            => Resize(width, height, values.GetEnumerable<T>());

        public void Resize(int width, int height, IEnumerable<T> values)
        {
            if (_width != width || _height != height)
            {
                _width = width;
                _height = height;
                _data = new T[Count];
            }

            int index = 0;
            foreach(var v in values)
            {
                if (index >= Count) break;
                _data[index] = v;
                index++;
            }
        }

        public void Shift(int offsetX, int offsetY)
        {
            var size = (Width, Height);
            ShiftX(_data, _data, offsetX, size, size);
            ShiftY(_data, _data, offsetY, size, size);
        }

        static void ShiftX(T[] dest, T[] src, int offset, (int width, int height) destSize, (int width, int height) srcSize)
        {
            if (offset < 0)
            {
                for (var y = 0; y < destSize.height; ++y)
                {
                    for (var x = 0; x < destSize.width; ++x)
                    {
                        var srcX = x - offset;
                        if (0 <= srcX && srcX < srcSize.width
                            && y < srcSize.height)
                            dest[ToIndex(x, y, destSize.width)] = src[ToIndex(srcX, y, srcSize.width)];
                        else
                            dest[ToIndex(x, y, destSize.width)] = default;
                    }
                }
            }
            else if (offset > 0)
            {
                for (var y = 0; y < destSize.height; ++y)
                {
                    for (var x = destSize.width - 1; x >= 0; --x)
                    {
                        var srcX = x - offset;
                        if (0 <= srcX && srcX < srcSize.width
                            && y < srcSize.height)
                            dest[ToIndex(x, y, destSize.width)] = src[ToIndex(srcX, y, srcSize.width)];
                        else
                            dest[ToIndex(x, y, destSize.width)] = default;
                    }
                }
            }
            else if(dest != src)
            {
                for (var y = 0; y < destSize.height; ++y)
                {
                    for (var x = 0; x < destSize.width; ++x)
                    {
                        var srcX = x;
                        if (0 <= srcX && srcX < srcSize.width
                            && y < srcSize.height)
                            dest[ToIndex(x, y, destSize.width)] = src[ToIndex(srcX, y, srcSize.width)];
                        else
                            dest[ToIndex(x, y, destSize.width)] = default;
                    }
                }
            }
        }

        static void ShiftY(T[] dest, T[] src, int offset, (int width, int height) destSize, (int width, int height) srcSize)
        {
            if (offset < 0)
            {
                for (var x = 0; x < destSize.width; ++x)
                {
                    for (var y = 0; y < destSize.height; ++y)
                    {
                        var srcY = y - offset;
                        if (0 <= srcY && srcY < srcSize.height
                            && x < srcSize.width)
                            dest[ToIndex(x, y, destSize.width)] = src[ToIndex(x, srcY, srcSize.width)];
                        else
                            dest[ToIndex(x, y, destSize.width)] = default;
                    }
                }
            }
            else if (offset > 0)
            {
                for (var x = 0; x < destSize.width; ++x)
                {
                    for (var y = destSize.height - 1; y >= 0; --y)
                    {
                        var srcY = y - offset;
                        if (0 <= srcY && srcY < srcSize.height
                            && x < srcSize.width)
                            dest[ToIndex(x, y, destSize.width)] = src[ToIndex(x, srcY, srcSize.width)];
                        else
                            dest[ToIndex(x, y, destSize.width)] = default;
                    }
                }
            }
            else if (dest != src)
            {
                for (var x = 0; x < destSize.width; ++x)
                {
                    for (var y = 0; y < destSize.height; ++y)
                    {
                        var srcY = y;
                        if (0 <= srcY && srcY < srcSize.height
                            && x < srcSize.width)
                            dest[ToIndex(x, y, destSize.width)] = src[ToIndex(x, srcY, srcSize.width)];
                        else
                            dest[ToIndex(x, y, destSize.width)] = default;
                    }
                }
            }
        }

        public int ToIndex(int x, int y)
        {
            if (!IsInRange(x, y))
                throw new System.IndexOutOfRangeException($"Out of Range Index... index=({x},{y})");

            return ToIndex(x, y, Width);
        }
        public (int x, int y) ToXY(int index)
        {
            if (index < 0 || Count <= index)
                throw new System.IndexOutOfRangeException($"Out of Range Index... index={index}");
            return ToXY(index, Width);
        }

        static int ToIndex(int x, int y, int width) => y * width + x;
        static (int x, int y) ToXY(int index, int width) => (index % width, index / width);

        #region IEnumerable
        public IEnumerator<T> GetEnumerator()
            => _data.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _data.GetEnumerator();

        public IEnumerable<(T value, int index)> GetEnumerableWithIndex()
            => _data.AsEnumerable()
                .Zip(Enumerable.Range(0, Count), (v, i) => (value: v, index: i));

        public IEnumerable<(T value, int x, int y)> GetEnumerableWithIndexXY()
            => _data.AsEnumerable()
                .Zip(Enumerable.Range(0, Count), (v, i) => {
                    var xy = ToXY(i);
                    return (value: v, x: xy.x, y: xy.y);
                });

        #endregion
    }
}
