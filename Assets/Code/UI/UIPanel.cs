using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class UIPanel : MonoBehaviour
{
    [SerializeField]
    protected UIDocument _document;

    public void Show()
    {
        gameObject.SetActive(true);
        //_document.rootVisualElement.visible = true;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        //_document.rootVisualElement.visible = false;
    }
}
