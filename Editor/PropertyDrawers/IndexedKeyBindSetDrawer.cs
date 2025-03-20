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


        private IndexedKeyBindSet _target;
        private bool _foldout;


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            GetTarget(property);

            float height = LineHeight;

            if (_foldout)
            {
                height += ColumnHeaderSpacing;
                height += (LineHeight + ItemSpacing.y) * _target.Count;
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

        private void DrawHeader(Rect position, GUIContent label)
        {
            var foldoutRect = position.Narrowed(
                ButtonWidth + ItemCountLabelWidth + ItemSpacing.x
            );
            if (_target.Count > 0)
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

            Rect buttonRect = GetRightButtonRect(position);
            if (GUI.Button(buttonRect, AddButtonContent, EditorStyles.iconButton))
            {
                _target.Add(_target.Count, KeyCode.None);
                _foldout = true;
            }

            Rect itemCountRect = buttonRect
                .Resized(ItemCountLabelWidth, LineHeight);
            itemCountRect.x -= ItemCountLabelWidth;
            string itemCountLabel = _target.Count switch
            {
                0 => "Empty", 1 => "1 Item", int n => $"{n} Items"
            };
            GUI.Label(itemCountRect, itemCountLabel, EditorStyles.miniLabel);

            if (_foldout)
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
        }

        private void DrawItems(Rect position)
        {
            int listIndex = 0;
            int? itemToDelete = null;
            foreach (IndexedKeyBind binding in _target)
            {
                int index = binding.Index;
                Rect indexRect = position
                    .HorizontalSlice(0, NormalizedIndexFieldSize)
                    .Narrowed(ItemSpacing.x);

                EditorGUI.BeginChangeCheck();
                index = EditorGUI.IntField(indexRect, index);
                if (EditorGUI.EndChangeCheck())
                    _target[listIndex].Index = index;

                KeyCode keyCode = binding;
                Rect keyCodeRect = position
                    .HorizontalSlice(NormalizedIndexFieldSize)
                    .Narrowed(ButtonWidth + ItemSpacing.x);

                EditorGUI.BeginChangeCheck();
                keyCode = GUIInputFields.KeyCodeField(keyCodeRect, keyCode);
                if (EditorGUI.EndChangeCheck())
                    _target[listIndex].BoundKey = keyCode;

                Rect buttonRect = GetRightButtonRect(position);
                if (GUI.Button(buttonRect, RemoveButtonContent, EditorStyles.iconButton))
                    itemToDelete = listIndex;

                position.y += LineHeight + ItemSpacing.y;
                listIndex++;
            }

            if (itemToDelete.HasValue)
            {
                _target.RemoveAt(itemToDelete.Value);
                if (_target.Count == 0)
                    _foldout = false;
            }
        }

        private Rect GetRightButtonRect(Rect position)
        {
            Rect buttonRect = position.Resized(ButtonWidth, LineHeight);
            buttonRect.x += position.width - ButtonWidth;
            return buttonRect;
        }

        private void GetTarget(SerializedProperty property)
        {
            object targetObject = property.serializedObject.targetObject;
            _target = fieldInfo.GetValue(targetObject) as IndexedKeyBindSet;

            if (_target == null)
            {
                targetObject = new();
                fieldInfo.SetValue(targetObject, _target);
            }
        }
    }

    public static class RectExtensions
    {
        public static Rect Resized(this Rect r, float width, float height)
        => new Rect(r.position, new (width, height));

        public static Rect HorizontalSlice(this Rect r, float start, float end = 1)
        {
            Rect result = new (r);
            result.x += r.width * start;
            result.width *= end - start;
            return result;
        }

        public static Rect Narrowed(this Rect r, float delta)
        => new(r.position, new (r.width - delta, r.height));
        public static Rect Widened(this Rect r, float delta)
        => new(r.position, new (r.width + delta, r.height));
    }
}