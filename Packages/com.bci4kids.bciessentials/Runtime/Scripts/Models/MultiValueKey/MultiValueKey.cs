using System;
using System.Collections.Generic;

namespace BCIEssentials.LSL
{
        public class MultiValueKey
        {
            public readonly int HashCode;
            public readonly string UniqueKey;
            public readonly string[] SecondaryKeys;

            private readonly SortedSet<string> _sortedKeys;

            public MultiValueKey(string primaryKey, string[] secondaryKeys = null)
            {
                UniqueKey = primaryKey ?? throw new ArgumentNullException(nameof(primaryKey));
                HashCode = primaryKey.GetHashCode();

                if (secondaryKeys == null) return;
                _sortedKeys = new();
                foreach (var key in secondaryKeys)
                {
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }

                    _sortedKeys.Add(key);
                }

                SecondaryKeys = new string[_sortedKeys.Count];
                _sortedKeys.CopyTo(SecondaryKeys);
            }

            public bool HasKey(string value)
            {
                if (UniqueKey == value)
                {
                    return true;
                }

                if (_sortedKeys != null && _sortedKeys.Contains(value))
                {
                    return true;
                }

                return false;
            }
        }
}