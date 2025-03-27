using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BCIEssentials.Utilities
{
    public static class ArrayUtilities
    {
        #region Random Non-Repeating Array (RNRA) for Target Selection
        /// <summary>
        /// Generates an array with the given length populated randomly
        /// with values including and between of the range values.
        /// This is intended for use in setting up the training targets.
        /// </summary>
        /// <param name="arrayLength">Number of values to include in the range.</param>
        /// <param name="maxRangeValue">The largest value possible to include.</param>
        /// <param name="minRangeValue">The lowest value possible to include.</param>
        /// <returns>An int array</returns>
        /// <exception cref="ArgumentException">Throws if max value is less than min value</exception>
        public static int[] GenerateRNRA(int arrayLength, int minRangeValue, int maxRangeValue)
        {
            if (maxRangeValue < minRangeValue)
            {
                throw new ArgumentException("MaxRangeValue must be greater than the MinRangeValue");
            }
            
            if (arrayLength <= 0)
            {
                return Array.Empty<int>();
            }

            //Initialize return array
            var randomizedOptions = new int[arrayLength];
            
            //Populate list with all available int between min and max values
            var availableOptions = new List<int>();
            PopulateOptions();

            var random = new Random();

            var redrawCount = 0;
            var lastDraw = int.MaxValue;
            
            //Populate return array with randomly drawn values
            for (var i = 0; i < arrayLength; i++)
            {
                if (availableOptions.Count == 0)
                {
                    PopulateOptions();
                }

                var (drawIndex, draw) = DrawValue();
                while (draw == lastDraw && redrawCount < 100)
                {
                    ++redrawCount; //Lets not get stuck in a loop
                    (drawIndex, draw) = DrawValue();
                }
                
                availableOptions.RemoveAt(drawIndex);
                randomizedOptions[i] = draw;
                lastDraw = draw;
            }

            (int, int) DrawValue()
            {
                var randomIndex = random.Next(0, availableOptions.Count-1);
                return (randomIndex, availableOptions[randomIndex]);
            }
            
            void PopulateOptions()
            {
                availableOptions.Clear();

                var option = minRangeValue;
                availableOptions.Add(option);
                while (option < maxRangeValue-1)
                {
                    // ++option;
                    option = option+1;
                    availableOptions.Add(option);
                }
            }
            
            return randomizedOptions;
        }
        #endregion

        #region RNRA for Low Number of Targets and flashing
        /// <summary>
        /// This is an array to generate randomly flashing targets during the stimulus run for low number of targets.
        /// This is NOT to be used for training targets. It will take the whole list and shuffle
        /// only guaranteeing that the values will not repeat. It won't guarantee that all values
        /// are presented first before repeating.
        /// </summary>
        /// 
        /// <summary>
        /// Generates an array with the given length populated using a MinHeap
        /// to ensure weighted random selection and optimization.
        /// </summary>
        /// <param name="arrayLength">Number of values to include in the range.</param>
        /// <param name="minRangeValue">The lowest value possible to include.</param>
        /// <param name="maxRangeValue">The largest value possible to include.</param>
        /// <returns>An int array</returns>
        // public static int[] GenerateWeightedArray(int arrayLength, int minRangeValue, int maxRangeValue)
        // {
        //     if (maxRangeValue < minRangeValue)
        //     {
        //         throw new ArgumentException("MaxRangeValue must be greater than the MinRangeValue");
        //     }
            
        //     if (arrayLength <= 0)
        //     {
        //         return Array.Empty<int>();
        //     }

        //     var range = new List<int>();
        //     for (int i = minRangeValue; i <= maxRangeValue; i++) range.Add(i);

        //     // Track each value's frequency
        //     var frequency = new Dictionary<int, int>();
        //     foreach (var value in range) frequency[value] = 0;

        //     // Min-heap to prioritize least-picked values
        //     var minHeap = new SortedSet<(int frequency, int value)>();
        //     foreach (var value in range)
        //         minHeap.Add((0, value));

        //     var result = new int[arrayLength];
        //     int lastPicked = -1;
        //     var random = new Random();

        //     for (int i = 0; i < arrayLength; i++)
        //     {
        //         // Select the least-picked values, avoiding the last picked
        //         var selectedCandidates = new List<(int frequency, int value)>();

        //         foreach (var entry in minHeap)
        //         {
        //             if (entry.value != lastPicked)
        //             {
        //                 selectedCandidates.Add(entry);
        //             }
        //         }

        //         // Pick randomly among least-picked candidates
        //         var choice = selectedCandidates[random.Next(selectedCandidates.Count)];
                
        //         // Update frequency in the dictionary and heap
        //         minHeap.Remove(choice);
        //         frequency[choice.value]++;
        //         minHeap.Add((frequency[choice.value], choice.value));

        //         // Add choice to result and update last picked
        //         result[i] = choice.value;
        //         lastPicked = choice.value;
        //     }

        //     return result;
        // }


        #endregion

        #region FisherYates RNRA
        /// <summary>
        /// Generates an array with the given length populated randomly
        /// with values including and between the range values using
        /// the FisherYates shuffle algorithm.
        /// </summary>
        /// <param name="arrayLength">Number of values to include in the range.</param>
        /// <param name="maxRangeValue">The largest value possible to include.</param>
        /// <param name="minRangeValue">The lowest value possible to include.</param>
        /// <returns>An int array</returns>
        /// <exception cref="ArgumentException">Throws if max value is less than min value</exception>
        public static int[] GenerateRNRA_FisherYates(int arrayLength, int minRangeValue, int maxRangeValue)
        {

            if (maxRangeValue < minRangeValue)
            {
                throw new ArgumentException("MaxRangeValue must be greater than the MinRangeValue");
            }
            
            if (arrayLength <= 0)
            {
                return Array.Empty<int>();
            }

             // Handle the special case where minRangeValue and maxRangeValue are the same
            if (minRangeValue == maxRangeValue)
            {
                return Enumerable.Repeat(minRangeValue, arrayLength).ToArray();
            }
            
            //Generate new Random value
            Random random = new Random();

            //prealocate the final options
            var myOptions = new List<int>();

            //Find the difference value between max and min range. 
            var diffVal = maxRangeValue-minRangeValue+1;

            //Get the divison to tell us how many loops we need to do with the full array length.
            int disVal = arrayLength / diffVal;
            //Get the modulo to tell us how long our second array out to be which will be added to the end of the full set of loops.
            int modVal = arrayLength % diffVal;

            // for(int i=0; i < disVal; i++)
            // {
            //     var temp = ShuffleYates(diffVal,minRangeValue,maxRangeValue,random);
            //     myOptions.AddRange(temp);
            // }
            int[] previousArray = null;

            for (int i = 0; i < disVal; i++)
            {
                var temp = ShuffleYates(diffVal, minRangeValue, maxRangeValue, random);

                // Ensure no consecutive repeats between arrays
                if (previousArray != null && previousArray[previousArray.Length - 1] == temp[0])
                {
                    // Swap the first element of temp with another element
                    int swapIndex = random.Next(1, temp.Length);
                    int tempValue = temp[0];
                    temp[0] = temp[swapIndex];
                    temp[swapIndex] = tempValue;
                }

                myOptions.AddRange(temp);
                previousArray = temp;
            }

            //build out the remainder array
            int[] tempArray = ShuffleYates(diffVal,minRangeValue,maxRangeValue,random);
            for(int i=0; i < modVal; i++)
            {
                myOptions.Add(tempArray[i]);
            }
    
            return myOptions.ToArray();
        }

        /// <summary>
        /// The actual Fisher-Yates Shuffle algorithm, with an input for ther Random parameter.
        /// </summary>
        /// <param name="length"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static int[] ShuffleYates(int length, int minValue, int maxValue, Random random)
        {
            if (maxValue < minValue)
            {
                throw new ArgumentException("MaxRangeValue must be greater than the MinRangeValue");
            }
            
            if (length <= 0)
            {
                return Array.Empty<int>();
            }

            //Handle when length is greater than max and min value differences.
            if (length > maxValue - minValue + 1)
            {
                throw new ArgumentException("Invalid input: Length is greater than the range of values.");

            }

            var range = Enumerable.Range(minValue, maxValue - minValue + 1).ToArray();
            var shuffledArray = new int[length];

            for (int i = 0; i < length; i++)
            {
                int j = random.Next(i, range.Length);
                shuffledArray[i] = range[j];
                range[j] = range[i];
            }

            return shuffledArray;
        }
        #endregion
    
        #region Matrix Printing Utilities
        public static string FormatMatrix(int[,] matrix)
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

        #endregion

        #region Extensions

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


        public static int[] GetRow(this int[,] ar, int index)
        => Enumerable.Range(0, ar.GetLength(1))
        .Select(x => ar[index, x]).ToArray();

        public static int[] GetColumn(this int [,] ar, int index)
        => Enumerable.Range(0, ar.GetLength(0))
        .Select(x => ar[x, index]).ToArray();

        public static IEnumerator RunForEachRow
        (
            this int[,] ar, Func<int[], IEnumerator> rowMethod
        )
        {
            for (int i = 0; i < ar.GetLength(0); i++)
            {
                int[] row = ar.GetRow(i);
                yield return rowMethod(row);
            }
        }

        public static IEnumerator RunForEachColumn
        (
            this int[,] ar, Func<int[], IEnumerator> columnMethod
        )
        {
            for (int j = 0; j < ar.GetLength(1); j++)
            {
                int[] column = ar.GetColumn(j);
                yield return columnMethod(column);
            }
        }

        #endregion
    }
}