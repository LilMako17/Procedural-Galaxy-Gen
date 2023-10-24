using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoadHelper : MonoBehaviour
{
    private static List<GameObject> savedObjects = new List<GameObject>();

    public static void DontDestroyOnLoad(GameObject obj)
    {
        savedObjects.Add(obj);
        Object.DontDestroyOnLoad(obj);
    }

    public static void DestoryAllDontDestroyOnLoadObjects()
    {
        foreach (var obj in savedObjects)
        {
            Object.Destroy(obj);
        }
        savedObjects.Clear();
    }

    public static IReadOnlyList<GameObject> GetSavedObjects()
    {
        return savedObjects;
    }

    private void Awake()
    {
        DontDestroyOnLoadHelper.DontDestroyOnLoad(this.gameObject);
    }
}
