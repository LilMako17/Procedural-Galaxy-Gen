using MagicMau.ProceduralNameGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameGeneratorManager
{
    public static NameGeneratorManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new NameGeneratorManager();
            }

            return instance;
        }
    }

    private static NameGeneratorManager instance;

    private NameGenerator _starGenerator;
    private NameGenerator _planetGenerator;

    private void EnsureSetup()
    {
        if (_starGenerator == null)
        {
            var starNames = LoadStrings("star names");
            _starGenerator = new NameGenerator(starNames, 1, 0.01);
        }
        if (_planetGenerator == null)
        {
            var planetNames = LoadStrings("planet names");
            _planetGenerator = new NameGenerator(planetNames, 1, 0.01);
        }
    }

    public string GenerateGalaxyName(int seed)
    {
        return GeneratePlanetName(seed) + " Galaxy";
    }

    public string GenerateStarName(int seed)
    {
        EnsureSetup();

        var rand = new System.Random(seed);
        const int maxAttempts = 100;
        var counter = 0;
        while (counter < maxAttempts)
        {
            var output = _starGenerator.GenerateName(4, 10, 0, null, rand);
            if (output != null)
            {
                return output;
            }

            counter++;
        }

        Debug.LogError("no output");
        return null;
    }

    public string GeneratePlanetName(int seed)
    {
        EnsureSetup();

        var rand = new System.Random(seed);
        const int maxAttempts = 100;
        var counter = 0;
        while (counter < maxAttempts)
        {
            var output = _planetGenerator.GenerateName(4, 10, 0, null, rand);
            if (output != null)
            {
                return output;
            }

            counter++;
        }

        Debug.LogError("no output");
        return null;
    }

    private string[] LoadStrings(string fileName)
    {
        var path = "RandomStrings/" + fileName;
        var asset = (TextAsset)Resources.Load(path, typeof(TextAsset));
        var arr = asset.text.Split('\n');
        for (int i = 0; i < arr.Length; i++)
        {
            var entry = arr[i];
            entry = entry.Replace("\r", "");
            entry = entry.Replace("\n", "");

            arr[i] = entry;
        }
       return arr;
    }
}
