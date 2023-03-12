using System;
using System.Collections.Generic;
using LSL;

namespace BCIEssentials.LSLFramework
{
    public static class MultiValueKeyHelpers
    {
        
        /// <summary>
        /// Compare all Keys in order and all need to match.
        /// </summary>
        public const MultiValueKeyCompareOptions StrictKeyCompare = MultiValueKeyCompareOptions.Unique | MultiValueKeyCompareOptions.Secondary;

        /// <summary>
        /// Compare all Keys in order but only one needs to match.
        /// </summary>
        public const MultiValueKeyCompareOptions FlexibleKeyCompare = StrictKeyCompare | MultiValueKeyCompareOptions.MatchAny;

        /// <summary>
        /// Compare all keys with each other and only one needs to match.
        /// </summary>
        public const MultiValueKeyCompareOptions AnyKeyCompare = FlexibleKeyCompare | MultiValueKeyCompareOptions.CompareAll;
        

        public static bool DoKeysMatch(MultiValueKey keyA, MultiValueKey keyB,
            MultiValueKeyCompareOptions compareOptions)
        {
            if (compareOptions == MultiValueKeyCompareOptions.Hashcode)
            {
                return keyA.HashCode == keyB.HashCode;
            }

            var compareUnique = IncludesFlag(compareOptions, MultiValueKeyCompareOptions.Unique);
            var compareSecondary = IncludesFlag(compareOptions, MultiValueKeyCompareOptions.Secondary);
            var compareAll = IncludesFlag(compareOptions, MultiValueKeyCompareOptions.CompareAll);
            var matchAny = IncludesFlag(compareOptions, MultiValueKeyCompareOptions.MatchAny);

            if (!compareUnique && !compareSecondary)
            {
                return false;
            }

            if (compareAll)
            {
                if (compareUnique)
                {
                    var matchingUniqueKey = keyA.HasKey(keyB.UniqueKey);
                    if (matchingUniqueKey && matchAny)
                    {
                        return true;
                    }

                    if (!matchingUniqueKey && !matchAny)
                    {
                        return false;
                    }
                }

                if (compareSecondary)
                {
                    foreach (var key in keyB.SecondaryKeys)
                    {
                        var matchingUniqueKey = keyA.HasKey(key);
                        if (matchingUniqueKey && matchAny)
                        {
                            return true;
                        }

                        if (!matchingUniqueKey && !matchAny)
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (compareUnique)
                {
                    var matchingUniqueKey = keyA.UniqueKey == keyB.UniqueKey;
                    if (matchingUniqueKey && matchAny)
                    {
                        return true;
                    }

                    if (!matchingUniqueKey && !matchAny)
                    {
                        return false;
                    }
                }

                if (compareSecondary && keyA.SecondaryKeys != null && keyB.SecondaryKeys != null)
                {
                    var keyCount = MathF.Min(keyA.SecondaryKeys.Length, keyB.SecondaryKeys.Length);
                    if (keyCount == 0)
                    {
                        return false;
                    }
                    
                    for (int i = 0; i < keyCount; i++)
                    {
                        var matchingUniqueKey = keyA.SecondaryKeys[i] == keyB.SecondaryKeys[i];
                        if (matchingUniqueKey && matchAny)
                        {
                            return true;
                        }

                        if (!matchingUniqueKey && !matchAny)
                        {
                            return false;
                        }
                    }
                }
            }

            return false;
        }

        public static MultiValueKey GetKeyFromStreamInfo(StreamInfo streamInfo)
        {
            return GetKeyFromStreamProperties(streamInfo.uid(),
                streamInfo.name(),
                streamInfo.source_id(),
                streamInfo.type());
        }

        public static MultiValueKey GetKeyFromStreamProperties(string uid = "", string name = "", string sourceId = "",
            string type = "")
        {
            return new MultiValueKey(uid, new[]
            {
                name ?? string.Empty,
                sourceId ?? string.Empty,
                type ?? string.Empty,
            });
        }
        

        public static bool IncludesFlag(MultiValueKeyCompareOptions options, MultiValueKeyCompareOptions flag)
        {
            return (options & flag) == flag;
        }
        

        public class MultiValueKeyComparer : IEqualityComparer<MultiValueKey>
        {
            public bool Equals(MultiValueKey x, MultiValueKey y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.HashCode == y.HashCode;
            }

            public int GetHashCode(MultiValueKey obj)
            {
                return obj.HashCode;
            }
        }
    }
}