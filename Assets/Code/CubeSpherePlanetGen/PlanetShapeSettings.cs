using System;
using System.Collections.Generic;
using UnityEngine;

namespace CubeSphere
{
    [CreateAssetMenu()]
    public class PlanetShapeSettings : ScriptableObject
    {
        public float Radius;
        public NoiseFilter NoiseLayers;
    }
}
