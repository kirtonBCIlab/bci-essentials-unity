using UnityEditor;
using UnityEngine;

namespace BCIEssentials.Editor
{
    using Controllers;

    [CustomPropertyDrawer(typeof(IndexedKeyBindSet))]
    public class IndexedKeyBindSetDrawer: PropertyDrawer
    {
        const float ButtonWidth = 20f;
        static readonly Vector2 ItemSpacing = new(4f, 2f);
        static readonly float LineHeight = EditorGUIUtility.singleLineHeight;

        static readonly GUIContent AddButtonContent
        = new GUIContent(EditorGUIUtility.IconContent("d_CreateAddNew@2x"));
        static readonly GUIContent RemoveButtonContent
        = new GUIContent(EditorGUIUtility.IconContent("d_winbtn_win_close@2x"));

        private IndexedKeyBindSet _target;
        private bool _foldout;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (_target == null) Initialize(property);

            if (_foldout)
                return LineHeight + (LineHeight + ItemSpacing.y) * (1 + _target.Count);

            return LineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_target == null) Initialize(property);

            if (property.TryGetAttribute<SpaceAttribute>(out var spaceAttribute))
                EditorGUILayout.Space(spaceAttribute.height);

            position.height = LineHeight;
            DrawHeader(position, label);

            if(!_foldout) return;

            position.y += LineHeight * 2;
            DrawItems(position);
        }

        private void DrawHeader(Rect position, GUIContent label)
        {
            var foldoutRect = position.Narrowed(ButtonWidth);
            if (_target.Count > 0)
            {
                _foldout = EditorGUI.Foldout(foldoutRect, _foldout, label, true);
            }
            else
            {
                EditorGUI.LabelField(foldoutRect, label);
                _foldout = false;
            }

            Rect buttonRect = GetRightButtonRect(position);
            GUI.Button(buttonRect, AddButtonContent, EditorStyles.iconButton);

            Rect itemCountRect = buttonRect.Resized(60f, LineHeight);
            itemCountRect.x -= 60f;
            string itemCountLabel = _target.Count switch
            {
                0 => "Empty", 1 => "1 Item", int n => $"{n} Items"
            };
            GUI.Label(itemCountRect, itemCountLabel);
        }

        private void DrawItems(Rect position)
        {
            int listIndex = 0;
            foreach (IndexedKeyBind binding in _target)
            {
                int index = binding.Index;
                Rect indexRect = position
                    .HorizontalSlice(0, 0.2f)
                    .Narrowed(ItemSpacing.x);

                EditorGUI.BeginChangeCheck();
                index = EditorGUI.IntField(indexRect, index);
                if (EditorGUI.EndChangeCheck())
                    _target[listIndex].Index = index;

                KeyCode keyCode = binding;
                Rect keyCodeRect = position
                    .HorizontalSlice(0.2f)
                    .Narrowed(ButtonWidth + ItemSpacing.x);

                EditorGUI.BeginChangeCheck();
                keyCode = GUIInputFields.KeyCodeField(keyCodeRect, keyCode);
                if (EditorGUI.EndChangeCheck())
                    _target[listIndex].BoundKey = keyCode;

                Rect buttonRect = GetRightButtonRect(position);
                GUI.Button(buttonRect, RemoveButtonContent, EditorStyles.iconButton);

                position.y += LineHeight + ItemSpacing.y;
                listIndex++;
            }
        }

        private Rect GetRightButtonRect(Rect position)
        {
            Rect buttonRect = position.Resized(ButtonWidth, LineHeight);
            buttonRect.x += position.width - ButtonWidth;
            return buttonRect;
        }

        private void Initialize(SerializedProperty property)
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