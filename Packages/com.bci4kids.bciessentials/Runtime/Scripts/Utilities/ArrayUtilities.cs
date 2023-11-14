using System;
using System.Collections.Generic;
using System.Linq;
using PlasticPipe.PlasticProtocol.Messages;

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
                while (draw == lastDraw && redrawCount < 10)
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

            for(int i=0; i < disVal; i++)
            {
                var temp = ShuffleYates(diffVal,minRangeValue,maxRangeValue,random);
                myOptions.AddRange(temp);
            }

            //bould out the remainder array
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
    }
}