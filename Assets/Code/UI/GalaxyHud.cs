using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class GalaxyHud : UIPanel
{
    [SerializeField]
    private float _tooltipRightPadding = 10;
    [SerializeField]
    private float _tooltipBottomPadding = 2.5f;

    private TextElement _text;
    private Button _saveButton;
    private Button _loadButton;
    private Button _createButton;

    private VisualElement _tooltip;
    private TextElement _tooltipText;

    private void OnEnable()
    {
        _saveButton = _document.rootVisualElement.Q<Button>("SaveButton");
        _saveButton.RegisterCallback<ClickEvent>(OnSaveButton);

        _loadButton = _document.rootVisualElement.Q<Button>("LoadButton");
        _loadButton.RegisterCallback<ClickEvent>(OnLoadButton);

        _createButton = _document.rootVisualElement.Q<Button>("CreateButton");
        _createButton.RegisterCallback<ClickEvent>(OnCreateButton);

        _text = _document.rootVisualElement.Q<TextElement>("CurrentFile");

        _tooltip = _document.rootVisualElement.Q<VisualElement>("Tooltip");
        _tooltipText = _tooltip.Q<TextElement>("Label");
        HideTooltip();
    }

    public void HideTooltip()
    {
        _tooltip.visible = false;
    }

    public void ShowTooltip(Vector2 screenPos, string text)
    {
        _tooltip.visible = true;

        var textSize = _tooltipText.MeasureTextSize(text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
        _tooltipText.text = text;

        var pos = new Vector2(screenPos.x / Screen.width, 1 - (screenPos.y / Screen.height));
        pos = pos * _tooltip.panel.visualTree.layout.size;

        // clamp to right edge of screen
        if (pos.x + textSize.x + _tooltipRightPadding > _tooltip.panel.visualTree.layout.size.x)
        {
            pos.x = _tooltip.panel.visualTree.layout.size.x - textSize.x - _tooltipRightPadding;
        }
        // clamp to bottom
        if (pos.y + textSize.y + _tooltipBottomPadding > _tooltip.panel.visualTree?.layout.size.y)
        {
            pos.y = _tooltip.panel.visualTree.layout.size.y - textSize.y - _tooltipBottomPadding;
        }

        _tooltip.style.left = pos.x;
        _tooltip.style.top = pos.y;
    }

    private void OnSaveButton(ClickEvent clickEvent)
    {
        UserDataManager.Instance.Save();
    }

    private void OnLoadButton(ClickEvent clickEvent)
    {
        UserDataManager.Instance.Load();
        if (UserDataManager.Instance.Current != null)
        {
            GalaxyMap.Instance.LoadStarMap(UserDataManager.Instance.Current.GalaxyData);
        }
    }

    private void OnCreateButton(ClickEvent clickEvent)
    {
        UserDataManager.Instance.CreateNew();
        GalaxyMap.Instance.RandomizedSeed();
    }

    private void Update()
    {
        var data = GalaxyMap.Instance.StarData;
        if (data != null)
        {
            SetText(data.Name + " (" + data.Seed + ")");
        }
        else
        {
            SetText("No data loaded");
        }
    }

    public void SetText(string text)
    {
        _text.text = text;
    }
}

