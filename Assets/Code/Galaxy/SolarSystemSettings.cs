using CubeSphere;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu()]
public class SolarSystemSettings : ScriptableObject
{
    public int MinPlanets;
    public int MaxPlanets;

    public float MinOrbitDistance;
    public float MinOrbitSpacing;
    public float MaxOrbitSpacing;

    [Header("Moon Settings")]
    public float MinMoonDistance;
    public float MinMoonOrbitSpacing;
    public float MaxMoonOrbitSpacing;

    [Header("Prefabs")]
    public GameObject OrbitLinePrefab;
    public GameObject PlanetPrefab;

    [Header("Planet Gen Settings")]
    public List<PlanetSettings> PlanetSettingsList;
    public List<PlanetSettings> MoonSettingsList;
}

[Serializable]
public class PlanetSettings
{
    public PlanetShapeSettings shapeSettings;
    public PlanetColorSettings colorSettings;
    public float minScale = 1f;
    public float maxScale = 1f;
    public int minMoons = 0;
    public int maxMoons = 0;
}