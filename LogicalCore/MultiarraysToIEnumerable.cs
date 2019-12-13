using System.Collections.Generic;
using System.Linq;

namespace LogicalCore
{
    public static class MultiarraysToIEnumerable
    {
        public static IEnumerable<T> GetRow<T>(this T[,] array, int row)
        {
            for (int i = 0; i < array.GetLength(1); i++)
            {
                yield return array[row, i];
            }
        }

        public static IEnumerable<IEnumerable<T>> ToMultiEnumerable<T>(this T[,] array)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                yield return array.GetRow(i);
            }
        }
    }
}
