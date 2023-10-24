using UnityEngine;

public static class CanvasUtility
{
    public static Vector2 WorldToCanvas(this Canvas canvas, Vector3 worldPos, Camera camera)
    {
        var viewportPos = camera.WorldToViewportPoint(worldPos);
        var canvasRect = (RectTransform)canvas.transform;
        return new Vector2((viewportPos.x * canvasRect.sizeDelta.x), (viewportPos.y * canvasRect.sizeDelta.y));
    }

    public static Vector2 SwitchToRectTransform(RectTransform from, RectTransform to)
    {
        Vector2 localPoint;
        Vector2 fromPivotDerivedOffset = new Vector2(from.rect.width * from.pivot.x + from.rect.xMin, from.rect.height * from.pivot.y + from.rect.yMin);
        Vector2 screenP = RectTransformUtility.WorldToScreenPoint(null, from.position);
        screenP += fromPivotDerivedOffset;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(to, screenP, null, out localPoint);
        Vector2 pivotDerivedOffset = new Vector2(to.rect.width * to.pivot.x + to.rect.xMin, to.rect.height * to.pivot.y + to.rect.yMin);
        return to.anchoredPosition + localPoint - pivotDerivedOffset;
    }

    public static bool IsWithinRect(Vector2 screenPoint, RectTransform rect, Camera camera)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, camera, out var point))
        {
            // point is a Vector2 where (0,0) is the local center of the rect
            point.x += rect.rect.width / 2f;
            point.y += rect.rect.height / 2f;
            var r = new Rect(0, 0, rect.rect.width, rect.rect.height);
            if (r.Contains(point))
            {
                return true;
            }
        }

        return false;
    }
}
