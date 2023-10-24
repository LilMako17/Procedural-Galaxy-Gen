using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class SerializedGalaxyData
{
    public string Name;
    public int Seed;
    public Dictionary<int, StarNode> StarMap;
}

[Serializable]
public class StarNode
{
    public int AssetIndex;
    public Vector3 Pos;
    public float Scale;
    public float MultipliedRadius;
    public float Rotation;
    public float ScatterDistance;
    public HashSet<int> Connections;
    public int InstanceIndex;
    public string Name;
    public SolarSystemData SolarSystem;
}

[Serializable]
public class SolarSystemData
{
    public List<PlanetData> PlanetDataList;
}

[Serializable]
public class PlanetData : SolarSystemObjectData
{
    public List<SolarSystemObjectData> Moons;
}

[Serializable]
public class SolarSystemObjectData
{
    public float OrbitDistance;
    public float Angle;
    public float Scale;
    public int PlanetAssetIndex;
    public string Name;
}