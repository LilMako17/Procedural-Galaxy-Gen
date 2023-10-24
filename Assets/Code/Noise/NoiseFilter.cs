using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace CubeSphere
{
    [Serializable]
    public class NoiseFilter
    {
        // global granularity
        public float Frequency = 0.001f;
        public NoiseLayer[] NoiseLayers;
    }

    [Serializable]
    public class NoiseLayer
    {
        public FilterType Type;
        public NoiseSettings NoiseSettings;

        // if true, result will be multiplied by layer 0.
        // not aplicable to first layer
        public bool UseFirstLayerAsMask;

        // if false, will always evaluate to 0
        public bool Enabled = true;
    }

    [Serializable]
    public class NoiseSettings
    {
        // xyz input point noise offset
        public Vector3 Center;

        // num interations of noise offset
        [Range(1, 8)]
        public int NumLayers = 1;

        // base roughness value
        public float BaseRoughness = 1;

        // addidive per layer
        public float Roughness = 1f;

        // multiplier per layer
        public float Persitence = 0.5f;

        // final multiplier
        public float MultiplierValue = 1f;

        // final additive
        public float OffsetValue;

        // final clamping - Unity doesnt support nullables in serializables
        public float MinValue = 0f;
        public float MaxValue = 1f;
        public bool UseMin;
        public bool UseMax;
    }

    // types of noise forumlas
    public enum FilterType 
    { 
        Simple, 
        Rigid,
        Fractal,
    };
}
