using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace SortCS.Kalman
{
    public class Vector
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

        public double this[int index] => _values[index];

        public double Dot(Vector other)
        {
            Debug.Assert(_values.Length == other._values.Length, "Vectors should be of equal length.");
            Debug.Assert(_values.Length > 0, "Vectors must have at least one element.");

            return _values.Zip(other._values, (a, b) => a * b).Sum();
        }

        public static Vector operator -(Vector first, Vector second)
        {
            Debug.Assert(first.Length == second.Length, "Vectors should be of equal size");
            return new Vector(first._values.Zip(second._values, (a, b) => a - b).ToArray());
        }

        public static Vector operator +(Vector first, Vector second)
        {
            Debug.Assert(first.Length == second.Length, "Vectors should be of equal size");
            return new Vector(first._values.Zip(second._values, (a, b) => a + b).ToArray());
        }

        public override string ToString()
        {
            return string.Join(", ", _values.Select(v => v.ToString("###0.00", CultureInfo.InvariantCulture)));
        }

        internal Vector Append(params double[] extraElements)
        {
            return new Vector(_values.Concat(extraElements).ToArray());
        }

        internal double[] ToArray()
        {
            return _values.ToArray();
        }
    }
}