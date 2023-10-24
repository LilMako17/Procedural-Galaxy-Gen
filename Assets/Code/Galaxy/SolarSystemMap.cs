using Cinemachine;
using CubeSphere;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SolarSystemMap : MonoBehaviour
{
    public static SolarSystemMap Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<SolarSystemMap>();
            }
            return _instance;
        }
    }

    private static SolarSystemMap _instance;

    private SolarSystemSettings _solarSystemSettings;
    private GalaxySettings _galaxySettings;

    private GameObject _solarSystemParent;

    private int _selectedStarId;

    private SolarSystemObjectData _selectedPlanet;

    public SolarSystemObjectData SelectedPlanet => _selectedPlanet;

    public StarNode Data
    {
        get
        {
            return UserDataManager.Instance.GetStarNodeById(_selectedStarId);
        }
    }

    private void OnDestroy()
    {
    }

    // should be called on destroy to un-init
    public void Cleanup()
    {
        if (_solarSystemParent != null)
        {
            GameObject.Destroy(_solarSystemParent);
        }
    }

    private void EnsureInit()
    {
        if (_solarSystemParent == null)
        {
            _solarSystemParent = new GameObject("Solar System Objects");
        }

        CameraManager.Instance.ResetCamera(true);
    }

    public void ClearSolarSystem()
    {
        if (_solarSystemParent != null)
        {
            GameObject.Destroy(_solarSystemParent);
            _solarSystemParent = null;
        }
    }

    private Bounds CalculateBounds()
    {
        var distanceX = 0f;
        var distanceZ = 0f;
        foreach (var obj in _solarSystemParent.transform)
        {
            if (obj is Transform t)
            {
                var dx = Mathf.Abs(t.position.x);
                var dz = Mathf.Abs(t.position.z);

                if (dx > distanceX)
                {
                    distanceX = dx;
                }
                if (dz > distanceZ)
                {
                    distanceZ = dz;
                }
            }
        }

        distanceX = Mathf.Max(50f, distanceX);
        distanceZ = Mathf.Max(50f, distanceZ);
        return new Bounds(Vector3.zero, new Vector3(distanceX * 2, 5f, distanceZ * 2)); 
    }

    public void ShowSolarSystem(GalaxySettings galaxySettings, int starId)
    {
        var starData = UserDataManager.Instance.GetStarNodeById(starId);
        _galaxySettings = galaxySettings;
        _solarSystemSettings = _galaxySettings.StarOptions[starData.AssetIndex].SolarSystemSettings;
        _selectedStarId = starId;

        UIManager.Instance.GetPanel<SolarSystemHud>().Show();
        InputManager.Instance.SetState(InputManager.STATE_SOLAR_SYSTEM_BROWSE);

        if (starData.SolarSystem == null)
        {
            starData.SolarSystem = SolarSystemGenerator.Instance.GenerateSolarSystem(_solarSystemSettings, starData.Name, starId);
            UserDataManager.Instance.SetDirty();
        }

        Redraw();

        CameraManager.Instance.SetToSolarSystemCamera(CalculateBounds());
    }

    public void HideSolarSystem()
    {
        ClearSolarSystem();
        _selectedPlanet = null;
        UIManager.Instance.GetPanel<SolarSystemHud>().Hide();
    }

    private void Redraw()
    {
        ClearSolarSystem();
        EnsureInit();

        var data = Data;
        if (data == null)
        {
            return;
        }

        const float StarScaleMultiplier = 5f;

        var starSettings = _galaxySettings.StarOptions[data.AssetIndex];
        var star = GameObject.Instantiate(starSettings.Star3DPrefab, _solarSystemParent.transform);
        star.transform.localPosition = Vector3.zero;
        star.transform.localScale = starSettings.Star3DPrefab.transform.localScale * data.Scale * StarScaleMultiplier;

        for (var i = 0; i < data.SolarSystem.PlanetDataList.Count; i++)
        {
            var p = data.SolarSystem.PlanetDataList[i];

            var orbit = CreateOrbit(p, Vector3.zero);
            var seed = _selectedStarId + (i << 2);
            var planet = CreatePlanet(_solarSystemSettings.PlanetSettingsList, p, seed);

            var pos = orbit.GetPositionAtAngle(p.Angle);
            planet.localPosition = pos;

            for (int j = 0; j < p.Moons.Count; j++)
            {
                var m = p.Moons[j];
                var moonOrbit = CreateOrbit(m, pos);
                var moonSeed = seed + j;
                var moon = CreatePlanet(_solarSystemSettings.MoonSettingsList, m, moonSeed);
                var moonPos = moonOrbit.GetPositionAtAngle(m.Angle);
                moon.localPosition = pos + moonPos;
            }
        }

    }

    private Transform CreatePlanet(List<PlanetSettings> assetList, SolarSystemObjectData planetData, int seed)
    {
        var asset = assetList[planetData.PlanetAssetIndex];

        var obj = GameObject.Instantiate(_solarSystemSettings.PlanetPrefab, _solarSystemParent.transform);
        var planet = obj.GetComponentInChildren<Planet>();

        planet.ColorSettings = asset.colorSettings;
        planet.ShapeSettings = asset.shapeSettings;
        planet.Seed = seed;
        planet.Scale = planetData.Scale;
        planet.Data = planetData;
        planet.OnClick = OnClickPlanet;
        planet.GeneratePlanet();

        return obj.transform;
    }

    private void OnClickPlanet(Planet planet)
    {
        var camera = planet.transform.parent.GetComponentInChildren<CinemachineVirtualCamera>(true);
        if (camera != null && _selectedPlanet == null)
        {
            _selectedPlanet = planet.Data;
            Debug.Log("select planet " + _selectedPlanet.Name);
            CameraManager.Instance.SetFocusCamera(camera, _selectedPlanet.Scale);
            UIManager.Instance.GetPanel<SolarSystemHud>().ShowPlanetDetailsGroup(_selectedPlanet);
        }
    }

    public void DeselectCurrentPlanet()
    {
        UIManager.Instance.GetPanel<SolarSystemHud>().HidePlanetDetailsGroup();
        _selectedPlanet = null;
        CameraManager.Instance.ReturnToSolarSystemCamera();
    }

    private OrbitLine CreateOrbit(SolarSystemObjectData planetData, Vector3 origin)
    {
        var clone = GameObject.Instantiate(_solarSystemSettings.OrbitLinePrefab, _solarSystemParent.transform);
        var script = clone.GetComponent<OrbitLine>();
        script.SetOrbit(planetData.OrbitDistance, planetData.OrbitDistance, origin);
        return script;
    }
}