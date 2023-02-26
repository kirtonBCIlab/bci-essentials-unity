using System;

namespace BCIEssentials.LSL
{
    [Flags]
    public enum MultiValueKeyCompareOptions
    {
        Hashcode = 0, //Compare only the HashCodes
        Unique = 1, //Compare Unique Keys
        Secondary = 2, //Compare the Secondary Keys
        MatchAny = 4, //At least one matching, otherwise all key values must match
        CompareAll = 8, //All keys against each other, otherwise e.g. SecondaryKeys1[1] = SecondaryKeys2[1]
    }
}