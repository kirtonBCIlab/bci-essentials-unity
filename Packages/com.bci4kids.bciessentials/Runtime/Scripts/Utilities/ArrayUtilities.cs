using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BCIEssentials.Utilities
{
    public static class ArrayUtilities
    {
        /// <summary>
        /// Generates an array with the given length populated randomly
        /// with values including and between of the range values.
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

            int numOptions = maxRangeValue - minRangeValue + 1;

            // Make random object
            //Debug.Log("Random seed is 42");
            System.Random trainRandom = new System.Random();

            // Initialize array
            int[] array = new int[arrayLength];

            // Create an unshuffled array of the possible options
            int[] unshuffledArray = new int[numOptions];
            for (int i = 0; i < numOptions; i++)
            {
                unshuffledArray[i] = i;
            }
            //PrintArray(unshuffledArray);

            // Get the number of loops required to generate a list of desired length
            int numLoops = (arrayLength / numOptions);
            int remainder = arrayLength % numOptions;

            // Set last value to something well outside the realm of possible options
            int lastValue = 999;

            // Create new shuffled list containing all selections 
            for (int i = 0; i <= numLoops; i++)
            {
                // Shuffle the array 
                int[] shuffledArray = unshuffledArray.OrderBy(x => trainRandom.Next()).ToArray();
                // Reshuffle until first val of shuffled array doesn't match last
                while (shuffledArray[0] == lastValue)
                {
                    shuffledArray = unshuffledArray.OrderBy(x => trainRandom.Next()).ToArray();
                }
                //PrintArray(shuffledArray);

                // If this is not the last loop
                if (i < numLoops)
                {
                    // Add the full shuffled array to the big array
                    for (int j = 0; j < numOptions; j++)
                    {
                        int ind = (i * (numOptions)) + j;
                        //print(ind.ToString());
                        array[ind] = shuffledArray[j];
                    }
                    lastValue = shuffledArray[numOptions - 1];
                }

                // If this is the last loop
                if (i == numLoops)
                {
                    // Add the partial array to the big array
                    for (int k = 0; k < remainder; k++)
                    {
                        int ind = (i * (numOptions)) + k;
                        //print(ind.ToString());
                        array[ind] = shuffledArray[k];

                    }
                }
            }

            // Add min value to all elements
            for (int i = 0; i < array.Length; i++)
            {
                array[i] += minRangeValue;
            }

            return array;
        }

        /// <summary>
        /// Generates an array with the given length populated randomly
        /// with values including and between of the range values usting
        /// the FisherYates shuffle algorithm.
        /// </summary>
        /// <param name="arrayLength">Number of values to include in the range.</param>
        /// <param name="maxRangeValue">The largest value possible to include.</param>
        /// <param name="minRangeValue">The lowest value possible to include.</param>
        /// <returns>An int array</returns>
        /// <exception cref="ArgumentException">Throws if max value is less than min value</exception>
        // public static int[] GenerateRNRA_FisherYates(int arrayLength, int minRangeValue, int maxRangeValue)
        // {

        //     if (maxRangeValue < minRangeValue)
        //     {
        //         throw new ArgumentException("MaxRangeValue must be greater than the MinRangeValue");
        //     }
            
        //     if (arrayLength <= 0)
        //     {
        //         return Array.Empty<int>();
        //     }
            
        //     //Generate new Random value
        //     Random random = new Random();

        //     //prealocate the final options
        //     var myOptions = new List<int>();

        //     //Find the difference value between max and min range. 
        //     var diffVal = maxRangeValue-minRangeValue+1;

        //     //Get the divison to tell us how many loops we need to do with the full array length.
        //     int disVal = arrayLength / diffVal;
        //     //Get the modulo to tell us how long our second array out to be which will be added to the end of the full set of loops.
        //     int modVal = arrayLength % diffVal;

        //     for(int i=0; i < disVal; i++)
        //     {
        //         var temp = ShuffleYates(diffVal,minRangeValue,maxRangeValue,random);
        //         myOptions.AddRange(temp);
        //     }

        //     //bould out the remainder array
        //     int[] tempArray = ShuffleYates(diffVal,minRangeValue,maxRangeValue,random);
        //     for(int i=0; i < modVal; i++)
        //     {
        //         myOptions.Add(tempArray[i]);
        //     }
    
        //     return myOptions.ToArray();
        // }

        // /// <summary>
        // /// The actual Fisher-Yates Shuffle algorithm, with an input for ther Random parameter.
        // /// </summary>
        // /// <param name="length"></param>
        // /// <param name="minValue"></param>
        // /// <param name="maxValue"></param>
        // /// <param name="random"></param>
        // /// <returns></returns>
        // /// <exception cref="ArgumentException"></exception>
        // public static int[] ShuffleYates(int length, int minValue, int maxValue, Random random)
        // {
        //     if (maxValue < minValue)
        //     {
        //         throw new ArgumentException("MaxRangeValue must be greater than the MinRangeValue");
        //     }
            
        //     if (length <= 0)
        //     {
        //         return Array.Empty<int>();
        //     }

        //     //Handle when length is greater than max and min value differences.
        //     if (length > maxValue - minValue + 1)
        //     {
        //         throw new ArgumentException("Invalid input: Length is greater than the range of values.");

        //     }

        //     var range = Enumerable.Range(minValue, maxValue - minValue + 1).ToArray();
        //     var shuffledArray = new int[length];

        //     for (int i = 0; i < length; i++)
        //     {
        //         int j = random.Next(i, range.Length);
        //         shuffledArray[i] = range[j];
        //         range[j] = range[i];
        //     }

        //     return shuffledArray;
        // }
    }
}