using UnityEditor;
using UnityEngine;

namespace BCIEssentials.Editor
{
    using Controllers;

    [CustomPropertyDrawer(typeof(IndexedKeyBindSet))]
    public class IndexedKeyBindSetDrawer: PropertyDrawer
    {
        const float ButtonWidth = 20f;
        static readonly float LineHeight = EditorGUIUtility.singleLineHeight;

        static readonly GUIContent AddButtonContent
        = new GUIContent(EditorGUIUtility.IconContent("d_CreateAddNew@2x"));
        static readonly GUIStyle AddButtonStyle
        = new(EditorStyles.miniButton)
        {
            padding = new RectOffset(2, 2, 2, 2)
        };

        private IndexedKeyBindSet _target;
        private bool _foldout;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_target == null)
                Initialize(property);

            if (property.TryGetAttribute<SpaceAttribute>(out var spaceAttribute))
                EditorGUILayout.Space(spaceAttribute.height);
            

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

            Rect buttonRect = position.Resized(ButtonWidth, LineHeight);
            buttonRect.x += position.width - ButtonWidth;
            GUI.Button(buttonRect, AddButtonContent, AddButtonStyle);

            Rect itemCountRect = buttonRect.Resized(60f, LineHeight);
            itemCountRect.x -= 60f;
            string itemCountLabel = _target.Count switch
            {
                0 => "Empty", 1 => "1 Item", int n => $"{n} Items"
            };
            GUI.Label(itemCountRect, itemCountLabel);
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

        public static Rect WithWidth(this Rect r, float width)
        => new Rect(r.position, new (width, r.height));
        public static Rect WithHeight(this Rect r, float height)
        => new Rect(r.position, new (r.width, height));

        public static Rect Narrowed(this Rect r, float delta)
        => r.Widened(-delta);
        public static Rect Widened(this Rect r, float delta)
        => new(r.position, new (r.width - delta, r.height));
        public static Rect Shortened(this Rect r, float delta)
        => r.Heightened(delta);
        public static Rect Heightened(this Rect r, float delta)
        => new(r.position, new(r.width, r.height + delta));
    }
}