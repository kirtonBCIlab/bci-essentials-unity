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

            foreach (var element in result)
            {
                Assert.AreEqual(2, element);
            }

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
            var entries = new[] { 0, 1, 2, 3, 4, 5 };

            var result = ArrayUtilities.GenerateRNRA(entries.Length, entries[0], entries[^1]);

            Array.Sort(result);
            Assert.AreEqual(entries.Length,result.Length);
            for (int i = 0; i < entries.Length; i++)
            {
                var output = result[i];
                var expout = entries[i];
                TestContext.WriteLine($"My Array: {output}");
                TestContext.WriteLine($"Expected Array: {expout}");
                Assert.AreEqual(entries[i], result[i]);
            }


        }

        [Test]
        public void GenerateRNRA_WhenValueRangeGreaterThanLength_ThenReturnsArrayWithAllUniqueEntries()
        {
            var result = ArrayUtilities.GenerateRNRA(5, 0, 10);

            var entryList = new HashSet<int>();
            foreach (var entry in result)
            {
                Assert.IsFalse(entryList.Contains(entry));
                entryList.Add(entry);
            }
        }

        [Test]
        public void GenerateRNRA_WhenValueRangeLessThanLength_ThenReturnsArrayWithDuplicates()
        {
            var entryTable = new Dictionary<int, int>
            {
                { 0, 0 },
                { 1, 0 },
                { 2, 0 },
                { 3, 0 },
                { 4, 0 }
            };

            var result = ArrayUtilities.GenerateRNRA(12, 0, 4);

            //Count entries
            foreach (var entry in result)
            {
                TestContext.WriteLine($"The entry is: {entry}");
                ++entryTable[entry];
            }

            //Confirm each entry is repeated an expected amount
            foreach (var (entry, count) in entryTable)
            {
                Assert.True(count is 2 or 3);
            }
        }

        //Starting tests for the Shuffle and GenerateRNRA_FisherYates versions.

        [Test]
        public void GenerateRNRA_FisherYates_InvalidRange_ThrowsArgumentException()
        {
            //Expected values
            int arrayLength = 10;
            int minRangeValue = 100;
            int maxRangeValue = 1;

            void ThrowTest()
            {
                ArrayUtilities.GenerateRNRA_FisherYates(arrayLength, minRangeValue, maxRangeValue);
            }

            Assert.Throws<ArgumentException>(ThrowTest);
        }

        [Test]
        [TestCase(0)]
        [TestCase(-5)]
        public void GenerateRNRA_FisherYates_WhenArrayLengthIsInvalid_ThenReturnEmptyArray(int arrayLength)
        {
            var result = ArrayUtilities.GenerateRNRA(arrayLength, 5, 55);

            Assert.AreEqual(0, result.Length);
        }

        [Test]
        public void GenerateRNRA_FisherYates_WhenArrayLengthIsLessThanMinMaxDiff_ThenExpectedLength()
        {
            //Variables
            int arrayLength = 3;
            int minRangeValue = 0;
            int maxRangeValue = 5;

            int[] result = ArrayUtilities.GenerateRNRA_FisherYates(arrayLength, minRangeValue, maxRangeValue);

            // Assert
            Assert.AreEqual(arrayLength, result.Length);
        }

        [Test]
        public void GenerateRNRA_FisherYates_WhenArrayLengthIsEqualToMinMaxDiff_ThenExpectedLength()
        {
            //Variables
            int arrayLength = 10;
            int minRangeValue = 0;
            int maxRangeValue = 9;

            int[] result = ArrayUtilities.GenerateRNRA_FisherYates(arrayLength, minRangeValue, maxRangeValue);

            // Assert
            Assert.AreEqual(arrayLength, result.Length);
        }


        [Test]
        public void GenerateRNRA_FisherYates_WhenArrayLengthIsGreaterThanMinMaxDiff_ThenExpectedLength()
        {
            //Variables
            int arrayLength = 15;
            int minRangeValue = 0;
            int maxRangeValue = 5;

            int[] result = ArrayUtilities.GenerateRNRA_FisherYates(arrayLength, minRangeValue, maxRangeValue);

            // Assert
            Assert.AreEqual(arrayLength, result.Length);
        }

        //I don't actually know how to check this...I think this works?
        [Test]
        public void GenerateRNRA_FisherYates_WhenArrayIsGreaterThanDiff_ThenOutputRepeatIsRandom()
        {
            //Variables
            int arrayLength = 10;
            int minRangeValue = 0;
            int maxRangeValue = 4;

            int[] result = ArrayUtilities.GenerateRNRA_FisherYates(arrayLength, minRangeValue, maxRangeValue);

            //Make some copies of each of the series that repeat a total of 3 times in this example
            var partOne = result.Take(5);
            var partTwo = result.Skip(5).Take(5);
            // Assert
            Assert.AreNotEqual(partOne,partTwo);
        }

        [Test]
        public void GenerateRNRA_FisherYates_WhenValueRangeEqualsThanArrayLength_ThenReturnsArrayWithAllRangesValuesPresent()
        {
            var entries = new[] { 0, 1, 2, 3, 4, 5 };

            var result = ArrayUtilities.GenerateRNRA_FisherYates(entries.Length, entries[0], entries[^1]);

            Array.Sort(result);
            Assert.AreEqual(entries.Length,result.Length);
            for (int i = 0; i < entries.Length; i++)
            {
                var output = result[i];
                var expout = entries[i];
                TestContext.WriteLine($"My Array: {output}");
                TestContext.WriteLine($"Expected Array: {expout}");
                Assert.AreEqual(entries[i], result[i]);
            }


        }


        [Test]
        public void ShuffleYates_WhenInvalidInputs_ThenThrow()
        {
            //Expected values
            int arrayLength = 10;
            int minRangeValue = 100;
            int maxRangeValue = 1;
            Random random = new Random(42);

            void ThrowTest()
            {
                ArrayUtilities.ShuffleYates(arrayLength, minRangeValue, maxRangeValue,random);
            }

            Assert.Throws<ArgumentException>(ThrowTest);
        }

        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        public void ShuffleYates_WhenLengthIsInvalid_ReturnEmpty(int arrayLength)
        {
            Random random = new Random(42);
            var result = ArrayUtilities.ShuffleYates(arrayLength,0,5,random);

            Assert.AreEqual(0,result.Length);

        }



    }
}