using System;
using System.Collections.Generic;

namespace BCIEssentials.LSLFramework
{
        public class MultiValueKey
        {
            public readonly int HashCode;
            public readonly string UniqueKey;
            public readonly string[] SecondaryKeys;

            private readonly HashSet<string> _keyLookUp;

            public MultiValueKey(string primaryKey, string[] secondaryKeys = null)
            {
                UniqueKey = primaryKey ?? throw new ArgumentNullException(nameof(primaryKey));
                HashCode = primaryKey.GetHashCode();

                if (secondaryKeys == null) return;
                
                SecondaryKeys = secondaryKeys;
                _keyLookUp = new();
                foreach (var key in secondaryKeys)
                {
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }

                    _keyLookUp.Add(key);
                }
            }

            public bool HasKey(string value)
            {
                if (UniqueKey == value)
                {
                    return true;
                }

                if (_keyLookUp != null && _keyLookUp.Contains(value))
                {
                    return true;
                }

                return false;
            }
        }
}