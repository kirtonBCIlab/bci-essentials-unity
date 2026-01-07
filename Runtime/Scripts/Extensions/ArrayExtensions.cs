using System;
using System.Collections;
using System.Linq;

namespace BCIEssentials.Extensions
{
    public static class ArrayExtensions
    {
        public static string Stringify(this int[,] matrix)
        {
            int numRows = matrix.GetLength(0);
            int numCols = matrix.GetLength(1);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    sb.Append(matrix[i, j].ToString().PadLeft(4));
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static string Stringify(this int[] array)
        => $"[{string.Join(", ", array)}]";


        public static int[] WherePositiveAndLessThan
        (
            this int[] array, int limit
        )
        => array.Where(x => x >= 0 && x < limit).ToArray();


        public static void Fill<T>
        (
            this T[] array, T value
        ) => Array.Fill(array, value);

        public static void FillFrom<T>
        (
            this T[] array, T[] source
        ) => Array.Copy(source, array, source.Length);


        public static T[,] To2D<T>
        (
            this T[] array, int rows, int columns
        )
        {
            T[,] result = new T[rows, columns];
            int sourceIndex = 0;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    result[i, j] = array[sourceIndex++];
                }
            }
            return result;
        }


        public static T[] GetRow<T>
        (
            this T[,] ar, int index
        )
        => Enumerable.Range(0, ar.GetWidth())
        .Select(x => ar[index, x]).ToArray();

        public static T[] GetColumn<T>
        (
            this T[,] ar, int index
        )
        => Enumerable.Range(0, ar.GetHeight())
        .Select(x => ar[x, index]).ToArray();

        public static int GetWidth<T>
        (this T[,] ar) => ar.GetLength(1);

        public static int GetHeight<T>
        (this T[,] ar) => ar.GetLength(0);

        public static IEnumerator RunForEachRow<T>
        (
            this T[,] ar, Func<T[], IEnumerator> rowMethod
        )
        {
            for (int i = 0; i < ar.GetHeight(); i++)
            {
                T[] row = ar.GetRow(i);
                yield return rowMethod(row);
            }
        }

        public static IEnumerator RunForEachColumn<T>
        (
            this T[,] ar, Func<T[], IEnumerator> columnMethod
        )
        {
            for (int j = 0; j < ar.GetWidth(); j++)
            {
                T[] column = ar.GetColumn(j);
                yield return columnMethod(column);
            }
        }
    }
}