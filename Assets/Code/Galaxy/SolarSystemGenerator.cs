using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SolarSystemGenerator
{
    public static SolarSystemGenerator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SolarSystemGenerator();
            }

            return _instance;
        }
    }

    private static SolarSystemGenerator _instance;

    public SolarSystemData GenerateSolarSystem(SolarSystemSettings settings, string starName, int seed)
    {
        var output = new SolarSystemData();
        output.PlanetDataList = new List<PlanetData>();

        var random = new System.Random(seed);

        var numPlanets = random.Next(settings.MinPlanets, settings.MaxPlanets);
        var lastOrbitDistance = 0f;
        for (int i = 0; i < numPlanets; i++)
        {
            var p = new PlanetData();
            p.Moons = new List<SolarSystemObjectData>();
            p.PlanetAssetIndex = random.Next(0, settings.PlanetSettingsList.Count);
            var asset = settings.PlanetSettingsList[p.PlanetAssetIndex];
            var distance = NextRandomFloatInRange(random, settings.MinOrbitSpacing, settings.MaxOrbitSpacing);
            var orbitDistance = Mathf.Max(settings.MinOrbitDistance, lastOrbitDistance + distance);
            p.OrbitDistance = orbitDistance;
            p.Angle = random.Next(0, 360);
            p.Scale = NextRandomFloatInRange(random, asset.minScale, asset.maxScale);
            p.Name = starName + " " + ToRoman(i + 1);
            lastOrbitDistance = orbitDistance;

            // moons
            var moonCount = random.Next(asset.minMoons, asset.maxMoons + 1);
            var moonOrbitDistance = 0f;
            for (int j = 0; j < moonCount; j++)
            {
                var moon = GenerateMoon(random, settings, p.Scale, ref moonOrbitDistance);
                moon.Name = p.Name + ToCharacterNotation(j + 1);
                p.Moons.Add(moon);
            }

            output.PlanetDataList.Add(p);
        }

        return output;
    }

    private SolarSystemObjectData GenerateMoon(System.Random random, SolarSystemSettings settings, float planetRaidus, ref float startOrbitDistance)
    {
        var output = new SolarSystemObjectData();
        var index = random.Next(0, settings.MoonSettingsList.Count);
        var data = settings.MoonSettingsList[index];

        output.PlanetAssetIndex = index;
        output.Scale = NextRandomFloatInRange(random, data.minScale, data.maxScale);
        output.Angle = random.Next(0, 360);
        output.OrbitDistance = Mathf.Max(settings.MinMoonDistance + planetRaidus + output.Scale, startOrbitDistance + NextRandomFloatInRange(random, settings.MinMoonOrbitSpacing, settings.MaxMoonOrbitSpacing));
        startOrbitDistance = output.OrbitDistance;
        return output;
    }

    private float NextRandomFloatInRange(System.Random random, float min, float max)
    {
        return min + ((float)random.NextDouble() * (max - min));
    }

    public static string ToRoman(int number)
    {
        if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException(nameof(number), "insert value between 1 and 3999");
        if (number < 1) return string.Empty;
        if (number >= 1000) return "M" + ToRoman(number - 1000);
        if (number >= 900) return "CM" + ToRoman(number - 900);
        if (number >= 500) return "D" + ToRoman(number - 500);
        if (number >= 400) return "CD" + ToRoman(number - 400);
        if (number >= 100) return "C" + ToRoman(number - 100);
        if (number >= 90) return "XC" + ToRoman(number - 90);
        if (number >= 50) return "L" + ToRoman(number - 50);
        if (number >= 40) return "XL" + ToRoman(number - 40);
        if (number >= 10) return "X" + ToRoman(number - 10);
        if (number >= 9) return "IX" + ToRoman(number - 9);
        if (number >= 5) return "V" + ToRoman(number - 5);
        if (number >= 4) return "IV" + ToRoman(number - 4);
        if (number >= 1) return "I" + ToRoman(number - 1);
        throw new Exception("Impossible state reached");
    }

    private static string ToCharacterNotation(int number)
    {
        switch (number)
        {
            case 0: return "";
            case 1: return "a";
            case 2: return "b";
            case 3: return "c";
            case 4: return "d";
            case 5: return "e";
            case 6: return "f";
            case 7: return "g";
        }

        throw new Exception("out of bounds");
    }
}
