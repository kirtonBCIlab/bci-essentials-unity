using UnityEditor;
using UnityEngine;

namespace BCIEssentials.Editor
{
    using Controllers;

    [CustomPropertyDrawer(typeof(IndexedKeyBindSet))]
    public class IndexedKeyBindSetDrawer: PropertyDrawer
    {
        const float ButtonWidth = 20f;
        const float ItemCountLabelWidth = 48f;
        const float NormalizedIndexFieldSize = 0.25f;
        static readonly Vector2 ItemSpacing = new(4f, 2f);
        static readonly float LineHeight = EditorGUIUtility.singleLineHeight;
        static readonly float ColumnHeaderSpacing = LineHeight * 1.2f;

        static readonly GUIContent AddButtonContent
        = new (EditorGUIUtility.IconContent("d_CreateAddNew@2x"));
        static readonly GUIContent RemoveButtonContent
        = new (EditorGUIUtility.IconContent("d_winbtn_win_close@2x"));

        static readonly GUIStyle FoldoutHeaderStyle
        = new (EditorStyles.foldoutHeader)
        { clipping = TextClipping.Clip };


        private int ItemCount => _arrayProperty.arraySize;
        private SerializedProperty _arrayProperty;
        private string _prefsKey;
        private bool _foldout;


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            GetTarget(property);

            float height = LineHeight;

            if (_foldout)
            {
                height += ColumnHeaderSpacing;
                height += (LineHeight + ItemSpacing.y) * ItemCount;
            }
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GetTarget(property);

            position.height = LineHeight;
            DrawHeader(position, label);

            if(!_foldout) return;

            position.y += LineHeight + ColumnHeaderSpacing;
            DrawItems(position);
        }


        private void GetTarget(SerializedProperty property)
        {
            _arrayProperty = property.FindPropertyRelative(
                nameof(IndexedKeyBindSet.Bindings)
            );

            Object targetObject = property.serializedObject.targetObject;
            _prefsKey = $"{targetObject.name}/{property.propertyPath}";
        }

        private void SetAndSaveFoldout(bool value)
        {
            _foldout = value;
            EditorPrefs.SetBool(_prefsKey, value);
        }

        private Rect GetRightButtonRect(Rect position)
        {
            Rect buttonRect = position.Resized(ButtonWidth, LineHeight);
            buttonRect.x += position.width - ButtonWidth;
            return buttonRect;
        }


        private void DrawHeader(Rect position, GUIContent label)
        {
            _foldout = EditorPrefs.GetBool(_prefsKey);
            label.text = label.text.TrimSuffix("Bindings");

            var foldoutRect = position.Narrowed(
                ButtonWidth + ItemCountLabelWidth + ItemSpacing.x
            );
            EditorGUI.BeginChangeCheck();
            if (ItemCount > 0)
            {
                _foldout = EditorGUI.BeginFoldoutHeaderGroup(
                    foldoutRect, _foldout, label, FoldoutHeaderStyle
                );
                EditorGUI.EndFoldoutHeaderGroup();
            }
            else
            {
                EditorGUI.LabelField(foldoutRect, label);
                _foldout = false;
            }
            if (EditorGUI.EndChangeCheck())
            {
                SetAndSaveFoldout(_foldout);
            }

            Rect buttonRect = GetRightButtonRect(position);
            if (GUI.Button(buttonRect, AddButtonContent, EditorStyles.iconButton))
            {
                _arrayProperty.InsertArrayElementAtIndex(ItemCount);
                SetAndSaveFoldout(true);
            }

            DrawItemCountLabel(position);

            if (_foldout)
            {
                DrawColumnHeaders(position);
            }
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
                if (ItemCount == 0)
                    SetAndSaveFoldout(false);
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

            KeyCode keyCode = (KeyCode)keyCodeProperty.enumValueFlag;
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