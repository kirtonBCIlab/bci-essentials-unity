using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace BCIEssentials.Utilities.Tests
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
        public void GenerateRNRA_WhenMaxEqualToMin_ThenReturnsEmptyArray()
        {
            var result = ArrayUtilities.GenerateRNRA(55, 5, 5);

            Assert.AreEqual(result.Length, 0);
        }
        
        [Test]
        [TestCase(0)]
        [TestCase(-5)]
        public void GenerateRNRA_WhenInvalidArrayLength_ThenReturnsEmptyArray(int arrayLength)
        {
            var result = ArrayUtilities.GenerateRNRA(arrayLength, 5, 55);

            Assert.AreEqual(result.Length, 0);
        }
        
        [Test]
        [TestCase(2, 0, 5)]
        [TestCase(10, -5, 0)]
        [TestCase(5, -10, 10)]
        [TestCase(20, -10, 10)]
        public void GenerateRNRA_ThenReturnsArrayWithValuesWithinBoundsAndOfLength(int arrayLength, int minValue, int maxValue)
        {
            var result = ArrayUtilities.GenerateRNRA(arrayLength, minValue, maxValue);
            
            //Within Range
            Array.Sort(result);
            var lowerValue = result.First();
            var higherValue = result.Last();
            Assert.IsTrue(lowerValue >= minValue && lowerValue <= maxValue);
            Assert.IsTrue(higherValue >= minValue && higherValue <= maxValue);
            
            //Of Length
            Assert.AreEqual(result.Length, arrayLength);
        }

        [Test]
        public void GenerateRNRA_WhenValueRangeEqualsThanArrayLength_ThenReturnsArrayWithAllRangesValuesPresent()
        {
            var entries = new[] { 0, 1, 2, 3, 4, 5 };
            
            var result = ArrayUtilities.GenerateRNRA(entries.Length, entries[0], entries[^1]);

            Array.Sort(result);
            for (int i = 0; i < entries.Length; i++)
            {
                Assert.AreEqual(entries[i], result[i]);
            }
        }

        [Test]
        public void GenerateRNRA_WhenValueRangeGreaterThanLength_ThenReturnsArrayWithAllUniqueEntries()
        {
            var result = ArrayUtilities.GenerateRNRA(5, 0,10);

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
            var result = ArrayUtilities.GenerateRNRA(10, 0, 5);

            var entryTable = new Dictionary<int, int>();
            foreach (var entry in result)
            {
                if (entryTable.ContainsKey(entry))
                {
                    
                }
                else
                {
                    entryTable.Add(entry, 1);
                }
            }
        }
    }
}