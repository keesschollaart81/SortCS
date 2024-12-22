using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace SortCS.Kalman
{
    internal struct Vector
    {
        private readonly double[] _values;

        public Vector(params double[] values)
        {
            _values = values;
            Size = values.Length;
        }

        public Vector(double[] values, int size)
        {
            if (size > values.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            _values = values;
            Size = size;
        }

        public Vector(int size)
        {
            _values = new double[size];
            Size = size;
        }

        public int Size { get; }

        public double this[int index]
        {
            get => index <= Size ? _values[index] : throw new Exception("nope");
            set
            {
                if (index > Size)
                {
                    throw new Exception("asd");
                }

                _values[index] = value;
            }
        }

        public static Vector operator -(Vector first, Vector second)
        {
            Debug.Assert(first.Size == second.Size, "Vectors should be of equal size");
            var resultArray = new double[first.Size];
            for (int i = 0; i < first.Size; i++)
            {
                resultArray[i] = first[i] - second[i];
            }

            return new Vector(resultArray);
        }

        public static Vector operator +(Vector first, Vector second)
        {
            Debug.Assert(first.Size == second.Size, "Vectors should be of equal size");
            var resultArray = new double[first.Size];
            for (int i = 0; i < first.Size; i++)
            {
                resultArray[i] = first[i] + second[i];
            }

            return new Vector(resultArray);
        }

        public double Dot(Vector other)
        {
            Debug.Assert(Size == other.Size, $"Vectors should be of equal length {Size} != {other.Size}.");
            Debug.Assert(Size > 0, "Vectors must have at least one element.");
            double sum = 0;
            for (int i = 0; i < Size; i++)
            {
                sum += _values[i] * other[i];
            }

            return sum;
        }

        public override string ToString()
        {
            return string.Join(", ", _values.Select(v => v.ToString("###0.00", CultureInfo.InvariantCulture)));
        }

        internal Vector Append(params double[] extraElements)
        {
            return new Vector(_values.Take(Size).Concat(extraElements).ToArray());
        }
    }
}