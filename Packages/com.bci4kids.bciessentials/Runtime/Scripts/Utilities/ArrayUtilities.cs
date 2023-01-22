using System;
using System.Collections.Generic;

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
            
            if (arrayLength <= 0 || minRangeValue == maxRangeValue)
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
                var randomIndex = random.Next(0, availableOptions.Count - 1);
                return (randomIndex, availableOptions[randomIndex]);
            }
            
            void PopulateOptions()
            {
                availableOptions.Clear();

                var option = minRangeValue;
                availableOptions.Add(option);
                while (option < maxRangeValue)
                {
                    ++option;
                    availableOptions.Add(option);
                }
            }
            
            return randomizedOptions;
        }
    }
}