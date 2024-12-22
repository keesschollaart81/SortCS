using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SortCS.Kalman;

[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
[DebuggerTypeProxy(typeof(MatrixDisplay))]
internal class Matrix
{
    private readonly double[,] _values;

    public Matrix(double[,] values)
    {
        _values = values;
        Rows = _values.GetLength(0);
        Columns = _values.GetLength(1);
    }

    public Matrix(int[,] values)
        : this(values.GetLength(0), values.GetLength(1))
    {
        for (var row = 0; row < Rows; row++)
        {
            for (var col = 0; col < Columns; col++)
            {
                _values[row, col] = (double)values[row, col];
            }
        }
    }

    public Matrix(int rows, int columns)
        : this(new double[rows, columns])
    {
    }

    public int Rows { get; }

    public int Columns { get; }

    public Matrix Transposed
    {
        get
        {
            if (field != null)
            {
                return field;
            }

            var result = new double[Columns, Rows];

            for (var row = 0; row < Rows; row++)
            {
                for (var col = 0; col < Columns; col++)
                {
                    result[col, row] = _values[row, col];
                }
            }

            field = new Matrix(result);

            return field;
        }
    }

    public Matrix Inverted
    {
        get
        {
            Debug.Assert(Rows == Columns);

            var (lu, indices) = GetDecomposition();
            var result = new double[Rows, Columns];

            for (var col = 0; col < Columns; col++)
            {
                var column = new double[Columns];

                column[col] = 1.0d;

                var x = BackSubstitution(lu, indices, column);

                for (var row = 0; row < Rows; row++)
                {
                    result[row, col] = x[row];
                }
            }

            return new Matrix(result);
        }
    }

    private string DebuggerDisplay => ToString();

    public static Matrix operator +(Matrix first, Matrix second)
    {
        Debug.Assert(first.Rows == second.Rows && first.Columns == second.Columns);

        var result = new double[first.Rows, first.Columns];

        for (var row = 0; row < first.Rows; row++)
        {
            for (var col = 0; col < first.Columns; col++)
            {
                result[row, col] = first._values[row, col] + second._values[row, col];
            }
        }

        return new Matrix(result);
    }

    public static Matrix operator -(Matrix first, Matrix second)
    {
        Debug.Assert(first.Rows == second.Rows && first.Columns == second.Columns);

        var result = new double[first.Rows, first.Columns];

        for (var row = 0; row < first.Rows; row++)
        {
            for (var col = 0; col < first.Columns; col++)
            {
                result[row, col] = first._values[row, col] - second._values[row, col];
            }
        }

        return new Matrix(result);
    }

    public static Matrix operator *(double scalar, Matrix matrix)
    {
        var result = new double[matrix.Rows, matrix.Columns];

        for (var row = 0; row < matrix.Rows; row++)
        {
            for (var col = 0; col < matrix.Columns; col++)
            {
                result[row, col] = matrix._values[row, col] * scalar;
            }
        }

        return new Matrix(result);
    }

    public static Matrix operator *(Matrix matrix, double scalar)
    {
        return scalar * matrix;
    }

    public static Matrix operator *(Matrix first, Matrix second)
    {
        var result = new double[first.Rows, second.Columns];
        var rows = result.GetLength(0);
        var cols = result.GetLength(1);

        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < cols; col++)
            {
                var bufFirst = ArrayPool<double>.Shared.Rent(first.Columns);
                var bufSecond = ArrayPool<double>.Shared.Rent(first.Rows);
                result[row, col] = first.Row(row, bufFirst).Dot(second.Column(col, bufSecond));
                ArrayPool<double>.Shared.Return(bufFirst, true);
                ArrayPool<double>.Shared.Return(bufSecond, true);
            }
        }

        return new Matrix(result);
    }

    public static Matrix Identity(int size)
    {
        var identity = new double[size, size];

        for (var row = 0; row < size; row++)
        {
            for (var col = 0; col < size; col++)
            {
                identity[row, col] = row == col ? 1.0d : 0d;
            }
        }

        return new Matrix(identity);
    }

    public override string ToString()
    {
        return $"{{{Rows}x{Columns}}} |{string.Join("|", Enumerable.Range(0, Rows).Select(row => $" {Row(row):###0.##} "))}|";
    }

    public Vector Dot(Vector vector)
    {
        Debug.Assert(Columns == vector.Size);

        var result = new double[Rows];
        for (var i = 0; i < Rows; i++)
        {
            var buf = ArrayPool<double>.Shared.Rent(Columns);
            var row = Row(i, buf);
            result[i] = row.Dot(vector);
            ArrayPool<double>.Shared.Return(buf);
        }

        return new Vector(result);
    }

    public Vector Row(int index)
    {
        return Row(index, new double[Columns]);
    }

    public Vector Row(int index, double[] buffer)
    {
        Debug.Assert(index <= Rows);
        for (var col = 0; col < Columns; col++)
        {
            buffer[col] = _values[index, col];
        }

        return new Vector(buffer, Columns);
    }

    public Vector Column(int index, double[] buf)
    {
        Debug.Assert(index <= Columns);
        for (var row = 0; row < Rows; row++)
        {
            buf[row] = _values[row, index];
        }

        return new Vector(buf, Rows);
    }

    private double[] BackSubstitution(double[,] lu, int[] indices, double[] b)
    {
        var x = (double[])b.Clone();
        var ii = 0;
        for (var row = 0; row < Rows; row++)
        {
            var ip = indices[row];
            var sum = x[ip];

            x[ip] = x[row];

            if (Math.Sign(ii) == 0)
            {
                for (var col = ii; col <= row - 1; col++)
                {
                    sum -= lu[row, col] * x[col];
                }
            }
            else if (Math.Sign(sum) == 0)
            {
                ii = row;
            }

            x[row] = sum;
        }

        for (var row = Rows - 1; row >= 0; row--)
        {
            var sum = x[row];
            for (var col = row + 1; col < Columns; col++)
            {
                sum -= lu[row, col] * x[col];
            }

            x[row] = sum / lu[row, row];
        }

        return x;
    }

    private (double[,] Result, int[] Indices) GetDecomposition()
    {
        var maxRow = 0;
        var vv = Enumerable.Range(0, Rows)
            .Select(row => 1.0d / Enumerable.Range(0, Columns).Select(col => Math.Abs(_values[row, col])).Max()).ToArray();
        var result = (double[,])_values.Clone();
        var index = new int[Rows];
        var d = 1.0d;

        for (var col = 0; col < Columns; col++)
        {
            for (var row = 0; row < col; row++)
            {
                var sum = result[row, col];
                for (var k = 0; k < row; k++)
                {
                    sum -= result[row, k] * result[k, col];
                }

                result[row, col] = sum;
            }

            var max = 0d;
            for (var row = col; row < Rows; row++)
            {
                var sum = result[row, col];
                for (var k = 0; k < col; k++)
                {
                    sum -= result[row, k] * result[k, col];
                }

                result[row, col] = sum;

                var tmp = vv[row] * Math.Abs(sum);

                if (tmp >= max)
                {
                    max = tmp;
                    maxRow = row;
                }
            }

            if (col != maxRow)
            {
                for (var k = 0; k < Rows; k++)
                {
                    (result[maxRow, k], result[col, k]) = (result[col, k], result[maxRow, k]);
                }

                d = -d;
                vv[maxRow] = vv[col];
            }

            index[col] = maxRow;

            if (col != Rows - 1)
            {
                var tmp = 1.0d / result[col, col];
                for (var row = col + 1; row < Rows; row++)
                {
                    result[row, col] *= tmp;
                }
            }
        }

        return (result, index);
    }

    internal class MatrixDisplay
    {
        public MatrixDisplay(Matrix matrix)
        {
            Cells = Enumerable.Range(0, matrix.Rows)
                .Select(row =>
                    new Cell(string.Join("  ", Enumerable.Range(0, matrix.Columns).Select(col => matrix._values[row, col]))))
                .ToArray();
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public Cell[] Cells { get; }

        [DebuggerDisplay("{" + nameof(Value) + ", nq}")]
        internal class Cell
        {
            public Cell(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }
    }
}