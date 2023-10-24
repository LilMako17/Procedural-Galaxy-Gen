using UnityEngine;

public static class GameObjectUtility
{
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        var component = gameObject.GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }
        return component;
    }

    public static T GetOrAddComponent<T>(this GameObject gameObject, HideFlags hideFlags) where T : Component
    {
        var component = gameObject.GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }
        component.hideFlags = hideFlags;
        return component;
    }

    public static void SetLayerRecursively(this GameObject gameObject, int layer)
    {
        gameObject.layer = layer;
        foreach (Transform t in gameObject.transform)
        {
            SetLayerRecursively(t.gameObject, layer);
        }
    }

    public static void SetParentAndAlign(GameObject child, GameObject parent)
    {
        if (parent == null)
            return;

        child.transform.SetParent(parent.transform, false);

        RectTransform rectTransform = child.transform as RectTransform;
        if (rectTransform)
        {
            rectTransform.anchoredPosition = Vector2.zero;
            Vector3 localPosition = rectTransform.localPosition;
            localPosition.z = 0;
            rectTransform.localPosition = localPosition;
        }
        else
        {
            child.transform.localPosition = Vector3.zero;
        }
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one;

        SetLayerRecursively(child, parent.layer);
    }

}
