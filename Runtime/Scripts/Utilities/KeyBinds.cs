using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace BCIEssentials.Utilities
{
    [Serializable]
    public class KeyBind
    {
        public Key BoundKey;
        public KeyBind(Key keyCode) { BoundKey = keyCode; }
        public static implicit operator Key(KeyBind b) => b.BoundKey;
        public static implicit operator KeyBind(Key k) => new(k);

        public bool IsPressed => Control.isPressed;// Input.GetKey(BoundKey);
        public bool WasPressedThisFrame => Control.wasPressedThisFrame; //Input.GetKeyDown(BoundKey);
        public bool WasReleasedThisFrame => Control.wasReleasedThisFrame;// Input.GetKeyUp(BoundKey);
        protected KeyControl Control => Keyboard.current[BoundKey];

        public void CallIfPressedThisFrame(Action method)
        {
            if (WasPressedThisFrame) method();
        }
    }

    [Serializable]
    public class IndexedKeyBind : KeyBind
    {
        public int Index;
        public IndexedKeyBind(int index, Key keyCode)
        : base(keyCode) => Index = index;

        public void CallIfPressedThisFrame(Action<int> indexedMethod)
        {
            if (WasPressedThisFrame) indexedMethod(Index);
        }
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
        (params (int, Key)[] tuples)
        {
            int count = tuples.Length;
            Bindings = new IndexedKeyBind[count];
            for (int i = 0; i < count; i++)
            {
                (int index, Key keyCode) = tuples[i];
                Bindings[i] = new(index, keyCode);
            }
        }

        public void Process(Action<int> onPressed)
        {
            foreach (IndexedKeyBind binding in Bindings)
            {
                binding.CallIfPressedThisFrame(onPressed);
            }
        }
    }
}