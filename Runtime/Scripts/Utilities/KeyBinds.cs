using System;
using UnityEngine;

namespace BCIEssentials.Utilities
{
    [Serializable]
    public class KeyBind
    {
        public KeyCode BoundKey;
        public KeyBind(KeyCode keyCode) { BoundKey = keyCode; }
        public static implicit operator KeyCode(KeyBind b) => b.BoundKey;
        public static implicit operator KeyBind(KeyCode k) => new(k);

        public bool WasPressedThisFrame
        => Input.GetKeyDown(BoundKey);
    }

    [Serializable]
    public class IndexedKeyBind : KeyBind
    {
        public int Index;
        public IndexedKeyBind(int index, KeyCode keyCode)
        : base(keyCode) => Index = index;
    }

    [Serializable]
    public class IndexedKeyBindSet
    {
        public IndexedKeyBind[] Bindings;

        public IndexedKeyBind this[int index]
        {
            get => Bindings[index];
            set => Bindings[index] = value;
        }

        public IndexedKeyBindSet
        (params (int, KeyCode)[] tuples)
        {
            int count = tuples.Length;
            Bindings = new IndexedKeyBind[count];
            for (int i = 0; i < count; i++)
            {
                (int index, KeyCode keyCode) = tuples[i];
                Bindings[i] = new(index, keyCode);
            }
        }

        public void Process(Action<int> onPressed)
        {
            foreach (IndexedKeyBind binding in Bindings)
            {
                if (binding.WasPressedThisFrame)
                    onPressed(binding.Index);
            }
        }
    }
}