using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BCIEssentials.Editor
{
    using Unity.Properties;
    using Utilities;

    [CustomPropertyDrawer(typeof(IndexedKeyBindSet))]
    public class IndexedKeyBindSetDrawer : PropertyDrawer
    {
        const float ButtonWidth = 20f;
        const float ItemCountLabelWidth = 48f;
        const float NormalizedIndexFieldSize = 0.25f;
        static readonly Vector2 ItemSpacing = new(4f, 2f);
        static readonly float LineHeight = EditorGUIUtility.singleLineHeight;
        static readonly float ColumnHeaderSpacing = LineHeight * 1.2f;

        static readonly GUIContent AddButtonContent
        = new(EditorGUIUtility.IconContent("d_Toolbar Plus@2x"));
        static readonly GUIContent RemoveButtonContent
        = new(EditorGUIUtility.IconContent("d_Toolbar Minus@2x"));

        static readonly GUIStyle FoldoutHeaderStyle
        = new(EditorStyles.foldoutHeader)
        { clipping = TextClipping.Clip };

        private bool IsExpanded
        {
            get => _property.isExpanded;
            set => _property.isExpanded = value;
        }
        private SerializedProperty _property;

        private int ItemCount => _arrayProperty.arraySize;
        private SerializedProperty _arrayProperty;


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ResolveTarget(property);

            float height = LineHeight;

            if (property.isExpanded)
            {
                height += ColumnHeaderSpacing;
                height += (LineHeight + ItemSpacing.y) * ItemCount;
            }
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ResolveTarget(property);

            position.height = LineHeight;
            DrawHeader(position, label);

            if (!property.isExpanded) return;

            position.y += LineHeight + ColumnHeaderSpacing;
            DrawItems(position);
        }


        private void ResolveTarget(SerializedProperty property)
        {
            _property = property;
            _arrayProperty = property.FindPropertyRelative(
                nameof(IndexedKeyBindSet.Bindings)
            );
        }

        private Rect GetRightButtonRect(Rect position)
        {
            Rect buttonRect = position.Resized(ButtonWidth, LineHeight);
            buttonRect.x += position.width - ButtonWidth;
            return buttonRect;
        }


        private void DrawHeader(Rect position, GUIContent label)
        {
            if (label.text != "Bindings")
            {
                label.text = label.text.TrimSuffix("Bindings");
            }

            var foldoutRect = position.Narrowed(
                ButtonWidth + ItemCountLabelWidth + ItemSpacing.x
            );
            if (ItemCount > 0)
            {
                IsExpanded = EditorGUI.BeginFoldoutHeaderGroup(
                    foldoutRect, IsExpanded, label, FoldoutHeaderStyle
                );
                EditorGUI.EndFoldoutHeaderGroup();
            }
            else
            {
                EditorGUI.LabelField(foldoutRect, label);
                IsExpanded = false;
            }

            Rect buttonRect = GetRightButtonRect(position);
            if (GUI.Button(buttonRect, AddButtonContent, EditorStyles.iconButton))
            {
                _arrayProperty.InsertArrayElementAtIndex(ItemCount);
                IsExpanded = true;
            }

            DrawItemCountLabel(position);

            if (IsExpanded) DrawColumnHeaders(position);
        }


        private void DrawItemCountLabel(Rect position)
        {
            position = GetRightButtonRect(position)
                .Resized(ItemCountLabelWidth, LineHeight);
            position.x -= ItemCountLabelWidth;

            string label = ItemCount switch
            {
                0 => "Empty",
                1 => "1 Item",
                int n => $"{n} Items"
            };

            GUI.Label(position, label, EditorStyles.miniLabel);
        }

        private void DrawColumnHeaders(Rect position)
        {
            position.y += ColumnHeaderSpacing;

            Rect indexHeaderRect = position
                .HorizontalSlice(0, NormalizedIndexFieldSize)
                .Narrowed(ItemSpacing.x);
            GUI.Label(indexHeaderRect, "Index");

            Rect keyCodeHeaderRect = position
                .HorizontalSlice(NormalizedIndexFieldSize)
                .Narrowed(ButtonWidth + ItemSpacing.x);
            GUI.Label(keyCodeHeaderRect, "KeyCode");
        }


        private void DrawItems(Rect position)
        {
            int? itemToDelete = null;
            for (int i = 0; i < ItemCount; i++)
            {
                DrawArrayElementIndexField(i, position);
                DrawArrayElementKeyCodeField(i, position);

                Rect buttonRect = GetRightButtonRect(position);
                if (GUI.Button(buttonRect, RemoveButtonContent, EditorStyles.iconButton))
                {
                    itemToDelete = i;
                }

                position.y += LineHeight + ItemSpacing.y;
            }

            if (itemToDelete.HasValue)
            {
                _arrayProperty.DeleteArrayElementAtIndex(itemToDelete.Value);
                if (ItemCount == 0) IsExpanded = false;
            }
        }


        private void DrawArrayElementIndexField
        (int arrayIndex, Rect elementRect)
        {
            SerializedProperty indexProperty
            = GetElementIndexProperty(arrayIndex);

            int index = indexProperty.intValue;
            Rect indexRect = elementRect
                .HorizontalSlice(0, NormalizedIndexFieldSize)
                .Narrowed(ItemSpacing.x);

            EditorGUI.BeginChangeCheck();
            index = EditorGUI.IntField(indexRect, index);
            if (EditorGUI.EndChangeCheck())
            {
                indexProperty.intValue = index;
            }
        }

        private void DrawArrayElementKeyCodeField
        (int arrayIndex, Rect elementRect)
        {
            SerializedProperty keyCodeProperty
            = GetElementKeyCodeProperty(arrayIndex);

            Key keyCode = (Key)keyCodeProperty.enumValueFlag;
            Rect keyCodeRect = elementRect
                .HorizontalSlice(NormalizedIndexFieldSize)
                .Narrowed(ButtonWidth + ItemSpacing.x);

            EditorGUI.BeginChangeCheck();
            keyCode = GUIInputFields.KeyCodeField(keyCodeRect, keyCode);
            if (EditorGUI.EndChangeCheck())
            {
                keyCodeProperty.enumValueFlag = (int)keyCode;
            }
        }


        private SerializedProperty GetElementIndexProperty(int arrayIndex)
        => GetPropertyFromArrayEntry(
            arrayIndex, nameof(IndexedKeyBind.Index)
        );
        private SerializedProperty GetElementKeyCodeProperty(int arrayIndex)
        => GetPropertyFromArrayEntry(
            arrayIndex, nameof(IndexedKeyBind.BoundKey)
        );

        private SerializedProperty GetPropertyFromArrayEntry
        (int arrayIndex, string propertyName)
        => GetArrayElementProperty(arrayIndex)
            .FindPropertyRelative(propertyName);
        private SerializedProperty GetArrayElementProperty
        (int arrayIndex)
        => _arrayProperty.GetArrayElementAtIndex(arrayIndex);
    }
}