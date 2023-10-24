using MagicMau.ProceduralNameGenerator;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class NameGenTest : MonoBehaviour
{
    public int MinChar = 2;
    public int MaxChar = 8;
    public int OrderOfMagnitude = 3;
    [Range(0f, 1f)]
    public float Smoothing = 0.01f;

    private string GenerateNamesTest(int seed)
    {
        var data = LoadStrings("star names");

        var generator = new NameGenerator(data, OrderOfMagnitude, (double)Smoothing);
        var rand = new System.Random();
        const int maxAttempts = 100;
        var counter = 0;
        while (counter < maxAttempts)
        {
            var output = generator.GenerateName(MinChar, MaxChar, 0, null, rand);
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
        return asset.text.Split('\n');
    }


    [Button("Test Output")]
    private void Test()
    {
        var name = GenerateNamesTest(UnityEngine.Random.Range(0, int.MaxValue));
        Debug.Log(name);
    }
}
