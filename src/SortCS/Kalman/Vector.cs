using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace SortCS.Kalman;

internal struct Vector
{
    private readonly float[] _values;

    public Vector(params float[] values)
    {
        _values = values;
        Size = values.Length;
    }

    public Vector(float[] values, int size)
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
        _values = new float[size];
        Size = size;
    }

    public int Size { get; }

    public float this[int index]
    {
        get => index <= Size ? _values[index] : throw new ArgumentOutOfRangeException(nameof(index));
        set
        {
            if (index > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            _values[index] = value;
        }
    }

    public static Vector operator -(Vector first, Vector second)
    {
        Debug.Assert(first.Size == second.Size);
        var resultArray = new float[first.Size];
        for (var i = 0; i < first.Size; i++)
        {
            resultArray[i] = first[i] - second[i];
        }

        return new Vector(resultArray);
    }

    public static Vector operator +(Vector first, Vector second)
    {
        Debug.Assert(first.Size == second.Size);
        var resultArray = new float[first.Size];
        for (var i = 0; i < first.Size; i++)
        {
            resultArray[i] = first[i] + second[i];
        }

        return new Vector(resultArray);
    }

    public float Dot(Vector other)
    {
        Debug.Assert(Size == other.Size, $"Vectors should be of equal length {Size} != {other.Size}.");
        Debug.Assert(Size > 0);
        float sum = 0;
        for (var i = 0; i < Size; i++)
        {
            sum += _values[i] * other[i];
        }

        return sum;
    }

    public override string ToString()
    {
        return string.Join(", ", _values.Select(v => v.ToString("###0.00", CultureInfo.InvariantCulture)));
    }

    internal Vector Append(params float[] extraElements)
    {
        return new Vector(_values.Take(Size).Concat(extraElements).ToArray());
    }
}