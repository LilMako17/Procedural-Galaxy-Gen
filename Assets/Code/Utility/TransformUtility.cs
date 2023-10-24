using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility methods for Unity's Transform component.
/// </summary>
public static class TransformUtility
{
    /// <summary>
    /// Resets a transform's TRS to the identity matrix.
    /// </summary>
    public static void Reset(this Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    /// <summary>
    /// Returns a list of all child transforms recursively flattened into a single list.
    /// </summary>
    public static IEnumerable<Transform> GetAllChildrenRecursively(this Transform transform, bool includeSelf = true)
    {
        var list = new List<Transform>();
        if (includeSelf)
        {
            list.Add(transform);
        }
        GetAllChildrenRecursively(transform, list);
        return list;
    }

    /// <summary>
    /// Destroy children under a transform
    /// </summary>
    public static void DestroyAllChildren(this Transform transform)
    {
        foreach (Transform child in transform)
        {
            Object.Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Breadth-first recursion of children adding all transform to list.
    /// </summary>
    private static void GetAllChildrenRecursively(Transform transform, List<Transform> list)
    {
        if (transform.childCount == 0)
        {
            return;
        }
        for (var i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            list.Add(child);
        }
        for (var i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            GetAllChildrenRecursively(child, list);
        }
    }
}
