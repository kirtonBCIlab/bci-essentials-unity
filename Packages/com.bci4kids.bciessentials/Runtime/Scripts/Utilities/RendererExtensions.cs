using UnityEngine;

namespace BCIEssentials.Utilities
{
    public static class RendererExtensions
    {
        public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
        }

        public static bool IsVisibleFromCanvas(this CanvasRenderer canvasRenderer, Camera camera)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            RectTransform rectTransform = canvasRenderer.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                return false;
            }
            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rectTransform);
            return GeometryUtility.TestPlanesAABB(planes, bounds);
        }
    }
}