using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class SolarSystemHud : UIPanel
{
    private Button _backButton;
    private VisualElement _planetDetailsGroup;
    private Label _planetHeader;
    private Button _planetCloseButton;

    private void OnEnable()
    {
        _backButton = _document.rootVisualElement.Q<Button>("BackButton");
        _backButton.RegisterCallback<ClickEvent>(OnBackButton);

        _planetDetailsGroup = _document.rootVisualElement.Q<VisualElement>("PlanetDetailsGroup");
        _planetHeader = _planetDetailsGroup.Q<Label>("title");
        _planetCloseButton = _planetDetailsGroup.Q<Button>("CloseButton");
        _planetCloseButton.RegisterCallback<ClickEvent>(OnPlanetDetailsClose);

        HidePlanetDetailsGroup();
    }

    private void OnBackButton(ClickEvent e)
    {
        if (SolarSystemMap.Instance.SelectedPlanet != null)
        {
            SolarSystemMap.Instance.DeselectCurrentPlanet();
        }
        else
        {
            SolarSystemMap.Instance.HideSolarSystem();
            GalaxyMap.Instance.ShowGalaxy();
        }
    }

    private void OnPlanetDetailsClose(ClickEvent e)
    {
        SolarSystemMap.Instance.DeselectCurrentPlanet();
    }

    public void ShowPlanetDetailsGroup(SolarSystemObjectData data)
    {
        _planetDetailsGroup.visible = true;
        _planetHeader.text = data.Name;
    }

    public void HidePlanetDetailsGroup()
    {
        _planetDetailsGroup.visible = false;
    }
}
