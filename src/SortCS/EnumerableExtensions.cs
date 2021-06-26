using System.Collections.Generic;
using System.Linq;

namespace SortCS
{
    internal static class EnumerableExtensions
    {
        public static T[,] ToArray<T>(this IEnumerable<T> source, int firstDimensionLength, int secondDimensionLength)
        {
            var array = source.ToArray();
            var result = new T[firstDimensionLength, secondDimensionLength];

            for (var i = 0; i < array.Length; i++)
            {
                result[i / secondDimensionLength, i % secondDimensionLength] = array[i];
            }

            return result;
        }
    }
}
