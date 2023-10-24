using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Utility methods for Unity's run-time Scene object.
/// </summary>
public static class SceneUtility
{
    /// <summary>
    /// Gets a component from the root of a Scene object.
    /// </summary>
    public static T GetRootComponent<T>(this Scene scene) where T : Component
    {
        if (!scene.isLoaded)
        {
            UnityEngine.Debug.LogError($"Error attempting to GetRootComponent on unloaded scene: {scene.name}.");
            return null;
        }
        var rootObjects = ListPool<GameObject>.Retrieve();
        try
        {

            scene.GetRootGameObjects(rootObjects);
            foreach (var gameObject in rootObjects)
            {
                var component = gameObject.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
            }
            return null;
        }
        finally
        {
            rootObjects.ReleaseToPool();
        }
    }

    /// <summary>
    /// Gets a component from the root of a Scene object.
    /// </summary>
    public static T[] GetRootComponentsInChidren<T>(this Scene scene) where T : Component
    {
        if (!scene.isLoaded)
        {
            UnityEngine.Debug.LogError($"Error attempting to GetRootComponent on unloaded scene: {scene.name}.");
            return null;
        }
        var rootObjects = ListPool<GameObject>.Retrieve();
        try
        {
            var foundComponents = new List<T>();
            scene.GetRootGameObjects(rootObjects);
            foreach (var gameObject in rootObjects)
            {
                var components = gameObject.GetComponentsInChildren<T>();
                if (components.Length > 0)
                {
                    foundComponents.AddRange(components);
                }
            }
            return foundComponents.ToArray();
        }
        finally
        {
            rootObjects.ReleaseToPool();
        }
    }
}
