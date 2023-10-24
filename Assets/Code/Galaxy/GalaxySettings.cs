using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class GalaxySettings : ScriptableObject
{
    [SerializeField]
    public List<GalaxyStarSettings> StarOptions;
    [SerializeField]
    public Texture2D GlobalShapeMask;
    [SerializeField]
    [Range(0f, 1f)]
    public float MaskCutoffThreshold = 0.01f;
    [SerializeField]
    public float MinScatterDistance = 0.5f;
    [SerializeField]
    public float MaxScatterDistance = 0.5f;
    [SerializeField]
    public AnimationCurve YHeightCutOffCurve;
    [SerializeField]
    public float MaxObjects = 50f;
    [SerializeField]
    public Vector3 Bounds = new Vector3(50, 5, 50);
    [SerializeField]
    public int NeighborSearchDistance = 8;
    [SerializeField]
    public LineRenderer LineRendererPrefab;

    [SerializeField]
    public bool AllowOverlappingLines;
    [SerializeField]
    public bool RemoveOrphans;
}


[Serializable]
public class GalaxyStarSettings
{
    public SolarSystemSettings SolarSystemSettings;
    public GameObject Star3DPrefab;
    public GameObject StarBillboardPrefab;
    //[MinMaxFloatSlider(0.1f, 5f)]
    public Vector2 RandomScale = new Vector2(1f, 1f);
    //[MinMaxFloatSlider(0f, 360f)]
    public Vector2 RandomRotation = new Vector2(0f, 360f);
    public float Radius = 1f;
    public Texture2D DistributionMask;
    [Range(0f, 1f)]
    public float DistributionMinDistanceFromCenter = 0f;
    [Range(0f, 1f)]
    public float DistributionMaxDistanceFromCenter = 1f;
    public int MaxConnections = 4;
}
