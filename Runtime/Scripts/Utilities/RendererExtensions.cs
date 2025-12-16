using UnityEngine;

namespace BCIEssentials.Utilities
{
    public static class RendererExtensions
    {
        public static bool HasRendererVisibleFromMainCamera(this GameObject gameObject)
        => gameObject.HasRendererVisibleFromCamera(Camera.main);
        public static bool HasRendererVisibleFromCamera(this GameObject gameObject, Camera camera)
        => (
            gameObject.TryGetComponent(out Renderer renderer)
            && renderer.enabled
            && renderer.IsVisibleFromCamera(camera)
        )
        || (
            gameObject.TryGetComponent(out CanvasRenderer canvasRenderer)
            && canvasRenderer.IsVisibleFromCanvas(camera)
        );


        public static bool IsVisibleFromCamera(this Renderer renderer, Camera camera)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
        }

        public static bool IsVisibleFromCanvas(this CanvasRenderer canvasRenderer, Camera camera)
        {
            Canvas parentCanvas = canvasRenderer.GetComponentInParent<Canvas>();
            if (parentCanvas && !parentCanvas.enabled) return false;

            if (canvasRenderer.TryGetComponent(out RectTransform rectTransform))
            {
                Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
                Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rectTransform);
                return GeometryUtility.TestPlanesAABB(planes, bounds);
            }
            return false;
        }
    }
}