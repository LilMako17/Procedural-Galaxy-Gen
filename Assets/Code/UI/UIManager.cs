using Game.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<UIManager>();
            }
            return _instance;
        }
    }
    private static UIManager _instance;

    [SerializeField]
    private UIDocument _document;

    private Dictionary<Type, UIPanel> _panels = new Dictionary<Type, UIPanel>();

    private void Awake()
    {
        var panels = GetComponentsInChildren<UIPanel>(true);
        foreach (var p in panels)
        {
            _panels[p.GetType()] = p;
        }
    }

    public T GetPanel<T>() where T : UIPanel
    {
        if (_panels.ContainsKey(typeof(T)))
        {
            return _panels[typeof(T)] as T;
        }

        throw new Exception("Panel type " + typeof(T).FullName + " does not exist");
    }

    private VisualElement GetElementFromPosRecursive(VisualElement element, Vector2 posFromTopLeft)
    {
        var e = element.panel.Pick(posFromTopLeft);
        if (e != null)
        {
            return e;
        }
        foreach (var child in element.Children())
        {
            var result = GetElementFromPosRecursive(child, posFromTopLeft);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    public VisualElement GetElementFromScreenPos(Vector2 screenPos)
    {
        var pos = new Vector2(screenPos.x / Screen.width, 1 - (screenPos.y / Screen.height));
        pos = pos * _document.rootVisualElement.panel.visualTree.layout.size;

        return GetElementFromPosRecursive(_document.rootVisualElement, pos);
    }

    public bool IsOverUI(Vector2 screenPos)
    {
        return GetElementFromScreenPos(screenPos) != null;
    }
}
