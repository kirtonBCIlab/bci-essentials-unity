using UnityEngine;

namespace BCIEssentials.Editor
{
    public static class RectExtensions
    {
        public static Rect Resized
        (this Rect r, float width, float height)
        => new Rect(r.position, new (width, height));

        public static Rect HorizontalSlice
        (this Rect r, float start, float end = 1)
        {
            Rect result = new (r);
            result.x += r.width * start;
            result.width *= end - start;
            return result;
        }

        public static (Rect, Rect) SplitHorizontally
        (this Rect r, float normalizedPosition, float spacing = 0)
        {
            float halfSpacing = spacing / 2;

            Rect left = r
                .HorizontalSlice(0, normalizedPosition)
                .Narrowed(halfSpacing);
            Rect right = r
                .HorizontalSlice(normalizedPosition)
                .Narrowed(halfSpacing);
            right.x += halfSpacing;

            return (left, right);
        }

        public static Rect Narrowed(this Rect r, float delta)
        => new(r.position, new (r.width - delta, r.height));
        public static Rect Widened(this Rect r, float delta)
        => new(r.position, new (r.width + delta, r.height));
    }
}