using UnityEngine;

using System.Collections.Generic;

public class Program : MonoBehaviour
{
    public string seed;
    public InsectGenerationConfig config;

    [ContextMenu("Generate Insect")]
    private void GenerateInsect()
    {
        InsectData.Generate(seed, config);
    }
}