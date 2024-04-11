using System;
using System.Collections.Generic;
using System.Linq;
using BCIEssentials.Utilities;
using NUnit.Framework;

namespace BCIEssentials.Tests.Editor
{
    internal class ArrayUtilitiesTests
    {
        [Test]
        public void GenerateRNRA_WhenMaxGreaterThanMin_ThenThrow()
        {
            void ThrowTest()
            {
                ArrayUtilities.GenerateRNRA(55, int.MaxValue, int.MinValue);
            }

            Assert.Throws<ArgumentException>(ThrowTest);
        }

        [Test]
        public void GenerateRNRA_WhenMaxEqualToMin_ThenReturnsSingleElementArray()
        {
            var result = ArrayUtilities.GenerateRNRA(3, 2, 2);
            int[] expected = Enumerable.Repeat(2, 3).ToArray();

            CollectionAssert.AreEqual(expected, result);

        }

        [Test]
        [TestCase(0)]
        [TestCase(-5)]
        public void GenerateRNRA_WhenInvalidArrayLength_ThenReturnsEmptyArray(int arrayLength)
        {
            var result = ArrayUtilities.GenerateRNRA(arrayLength, 5, 55);

            Assert.AreEqual(0, result.Length);
        }

        [Test]
        [TestCase(2, 0, 5)]
        [TestCase(10, -5, 0)]
        [TestCase(5, -10, 10)]
        [TestCase(20, -10, 10)]
        public void GenerateRNRA_ThenReturnsArrayWithValuesWithinBoundsAndOfLength(int arrayLength, int minValue,
            int maxValue)
        {
            var result = ArrayUtilities.GenerateRNRA(arrayLength, minValue, maxValue);

            //Within Range
            Array.Sort(result);
            var lowerValue = result.First();
            var higherValue = result.Last();
            Assert.IsTrue(lowerValue >= minValue && lowerValue <= maxValue);
            Assert.IsTrue(higherValue >= minValue && higherValue <= maxValue);

            //Of Length
            Assert.AreEqual(arrayLength, result.Length);
        }

        [Test]
        public void GenerateRNRA_WhenValueRangeEqualsThanArrayLength_ThenReturnsArrayWithAllRangesValuesPresent()
        {
            //var entries = new[] { 0, 1, 2, 3, 4, 5 };

            //var result = ArrayUtilities.GenerateRNRA(entries.Length, entries[0], entries[^1]);

            int[] expected = new int[] { 0, 1, 2, 3, 4, 5 };
            //NOTE - At the moment, we need to have N+1 be the max value in our Array Utilities at the moment,
            //       as the upper bound is not currently inclusive
            var result = ArrayUtilities.GenerateRNRA(expected.Length, 0, 6);

            //It is unclear why this is changing length on each trun, as the entries shouldn't change any time.
            //I have updated this to be hardcoded instead to see if the problem continues.
            Array.Sort(result);
            Assert.AreEqual(expected.Length,result.Length);

            CollectionAssert.AreEqual(expected, result);

        }

        [Test]
        public void GenerateRNRA_WhenValueRangeGreaterThanLength_ThenReturnsArrayWithAllUniqueEntries()
        {
            var result = ArrayUtilities.GenerateRNRA(5, 0, 10);
            CollectionAssert.AllItemsAreUnique(result);
        }

        [Test]
        public void GenerateRNRA_WhenValueRangeLessThanLength_ThenReturnsArrayWithDuplicates()
        {

            //Modified to be exactly 3 iterations, so we don't have to worry about other random numbers generating beyond.
            
            //I HAVE FOUND THE PROBLEM FINALLY! We have a "max_value-1" call that happens so that the upper boundary is not inclusive,
            //e.g. this goes from 0-3 in the current tests, instead of 0-4. When we change this we may need to change that part too.
            var result = ArrayUtilities.GenerateRNRA(12, 0, 4);

            int[] expected = new int[] { 0, 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3 };

            CollectionAssert.AreEquivalent(expected, result);
        }

        // //Starting tests for the Shuffle and GenerateRNRA_FisherYates versions.

        // [Test]
        // public void GenerateRNRA_FisherYates_InvalidRange_ThrowsArgumentException()
        // {
        //     //Expected values
        //     int arrayLength = 10;
        //     int minRangeValue = 100;
        //     int maxRangeValue = 1;

        //     void ThrowTest()
        //     {
        //         ArrayUtilities.GenerateRNRA_FisherYates(arrayLength, minRangeValue, maxRangeValue);
        //     }

        //     Assert.Throws<ArgumentException>(ThrowTest);
        // }

        // [Test]
        // [TestCase(0)]
        // [TestCase(-5)]
        // public void GenerateRNRA_FisherYates_WhenArrayLengthIsInvalid_ThenReturnEmptyArray(int arrayLength)
        // {
        //     var result = ArrayUtilities.GenerateRNRA(arrayLength, 5, 55);

        //     Assert.AreEqual(0, result.Length);
        // }

        // [Test]
        // public void GenerateRNRA_FisherYates_WhenArrayLengthIsLessThanMinMaxDiff_ThenExpectedLength()
        // {
        //     //Variables
        //     int arrayLength = 3;
        //     int minRangeValue = 0;
        //     int maxRangeValue = 5;

        //     int[] result = ArrayUtilities.GenerateRNRA_FisherYates(arrayLength, minRangeValue, maxRangeValue);

        //     // Assert
        //     Assert.AreEqual(arrayLength, result.Length);
        // }

        // [Test]
        // public void GenerateRNRA_FisherYates_WhenArrayLengthIsEqualToMinMaxDiff_ThenExpectedLength()
        // {
        //     //Variables
        //     int arrayLength = 10;
        //     int minRangeValue = 0;
        //     int maxRangeValue = 9;

        //     int[] result = ArrayUtilities.GenerateRNRA_FisherYates(arrayLength, minRangeValue, maxRangeValue);

        //     // Assert
        //     Assert.AreEqual(arrayLength, result.Length);
        // }


        // [Test]
        // public void GenerateRNRA_FisherYates_WhenArrayLengthIsGreaterThanMinMaxDiff_ThenExpectedLength()
        // {
        //     //Variables
        //     int arrayLength = 15;
        //     int minRangeValue = 0;
        //     int maxRangeValue = 5;

        //     int[] result = ArrayUtilities.GenerateRNRA_FisherYates(arrayLength, minRangeValue, maxRangeValue);

        //     // Assert
        //     Assert.AreEqual(arrayLength, result.Length);
        // }

        // //I don't actually know how to check this...I think this works?
        // [Test]
        // public void GenerateRNRA_FisherYates_WhenArrayIsGreaterThanDiff_ThenOutputRepeatIsRandom()
        // {
        //     //Variables
        //     int arrayLength = 10;
        //     int minRangeValue = 0;
        //     int maxRangeValue = 4;

        //     int[] result = ArrayUtilities.GenerateRNRA_FisherYates(arrayLength, minRangeValue, maxRangeValue);

        //     //Make some copies of each of the series that repeat a total of 3 times in this example
        //     var partOne = result.Take(5);
        //     var partTwo = result.Skip(5).Take(5);
        //     // Assert
        //     Assert.AreNotEqual(partOne,partTwo);
        // }

        // [Test]
        // public void GenerateRNRA_FisherYates_WhenValueRangeEqualsThanArrayLength_ThenReturnsArrayWithAllRangesValuesPresent()
        // {
        //     var entries = new[] { 0, 1, 2, 3, 4, 5 };

        //     var result = ArrayUtilities.GenerateRNRA_FisherYates(entries.Length, entries[0], entries[^1]);

        //     Array.Sort(result);
        //     Assert.AreEqual(entries.Length,result.Length);

        //     CollectionAssert.AreEqual(entries, result);
        // }


        // [Test]
        // public void ShuffleYates_WhenInvalidInputs_ThenThrow()
        // {
        //     //Expected values
        //     int arrayLength = 10;
        //     int minRangeValue = 100;
        //     int maxRangeValue = 1;
        //     Random random = new Random(42);

        //     void ThrowTest()
        //     {
        //         ArrayUtilities.ShuffleYates(arrayLength, minRangeValue, maxRangeValue,random);
        //     }

        //     Assert.Throws<ArgumentException>(ThrowTest);
        // }

        // [Test]
        // [TestCase(0)]
        // [TestCase(-1)]
        // public void ShuffleYates_WhenLengthIsInvalid_ReturnEmpty(int arrayLength)
        // {
        //     Random random = new Random(42);
        //     var result = ArrayUtilities.ShuffleYates(arrayLength,0,5,random);

        //     Assert.AreEqual(0,result.Length);

        // }



    }
}