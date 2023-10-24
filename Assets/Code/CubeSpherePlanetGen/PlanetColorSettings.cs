using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CubeSphere
{
    [CreateAssetMenu()]
    public class PlanetColorSettings : ScriptableObject
    {
        public BiomeColorSettings BiomeColorSettings;
        public Material Material;
    }

    [Serializable]
    public class BiomeColorSettings
    {
        public Biome[] Biomes;
        public NoiseFilter Noise;
        public float NoiseOffset;
        public float NoiseStrength;
        [Range(0, 1)]
        public float BlendAmount;
    }

    [Serializable]
    public class Biome
    {
        public Gradient Gradient;
        public Color Tint;
        [Range(0, 1)]
        public float TintPercent;
        [Range(-0.5f, 1.5f)]
        public float StartHeight;
    }
}
