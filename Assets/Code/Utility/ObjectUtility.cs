using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Utility methods for Unity's Object class.
/// </summary>
public static class ObjectUtility
{
    /// <summary>
    /// A version of FindObjectOfType which can find components on inactive GameObjects.
    /// Much slower than the default Object.FindObjectByType as it searches all root Objects in the
    /// active scene and calls searches all components in them and their children.
    /// </summary>
    public static T FindObjectOfType<T>() where T : Component
    {
        var dontDestroyOnLoadObjs = DontDestroyOnLoadHelper.GetSavedObjects();
        foreach (var obj in dontDestroyOnLoadObjs)
        {
            var cmp = obj.GetComponentInChildren<T>(true);
            if (cmp != null)
            {
                return cmp;
            }
        }

        var rootObjects = ListPool<GameObject>.Retrieve();
        try
        {
            for (int i = 0, sceneCount = SceneManager.sceneCount; i < sceneCount; i++)
            {
                rootObjects.Clear();
                SceneManager.GetSceneAt(i).GetRootGameObjects(rootObjects);
                for (int j = 0, len = rootObjects.Count; j < len; j++)
                {
                    var cmp = rootObjects[j].GetComponentInChildren<T>(true);
                    if (cmp != null)
                    {
                        return cmp;
                    }
                }
            }
        }
        finally
        {
            ListPool<GameObject>.Release(rootObjects);
        }
        return null;
    }

    public static string ToDebugString<T>(this ICollection<T> list)
    {
        var sb = StringBuilderPool.Retrieve();

        if (list != null)
        {
            var count = list.Count;
            sb.Append("(");
            foreach (var element in list)
            {
                sb.Append(element.ToString());
                count--;
                if (count > 0)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(")");
        }

        var output = sb.ToString();
        StringBuilderPool.Release(sb);
        return output;
    }
}
