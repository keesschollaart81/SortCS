using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace SortCS.Kalman
{
    internal class Vector
    {
        private readonly double[] _values;

        public Vector(params double[] values)
        {
            _values = values;
        }

        public Vector(int size)
        {
            _values = new double[size];
        }

        public int Length => _values.Length;

        public double this[int index]
        {
            get => _values[index];
            set => _values[index] = value;
        }

        public static Vector operator -(Vector first, Vector second)
        {
            Debug.Assert(first.Length == second.Length, "Vectors should be of equal size");
            var resultArray = new double[first.Length];
            for (int i = 0; i < first.Length; i++)
            {
                resultArray[i] = first[i] - second[i];
            }

            return new Vector(resultArray);
        }

        public static Vector operator +(Vector first, Vector second)
        {
            Debug.Assert(first.Length == second.Length, "Vectors should be of equal size");
            var resultArray = new double[first.Length];
            for (int i = 0; i < first.Length; i++)
            {
                resultArray[i] = first[i] + second[i];
            }

            return new Vector(resultArray);
        }

        public double Dot(Vector other)
        {
            Debug.Assert(_values.Length == other._values.Length, "Vectors should be of equal length.");
            Debug.Assert(_values.Length > 0, "Vectors must have at least one element.");
            double sum = 0;
            for (int i = 0; i < _values.Length; i++)
            {
                sum += _values[i] * other._values[i];
            }

            return sum;
        }

        public override string ToString()
        {
            return string.Join(", ", _values.Select(v => v.ToString("###0.00", CultureInfo.InvariantCulture)));
        }

        internal Vector Append(params double[] extraElements)
        {
            return new Vector(_values.Concat(extraElements).ToArray());
        }
    }
}