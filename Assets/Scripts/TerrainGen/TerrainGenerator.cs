using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] private int mapX;
    [SerializeField] private int mapY;
    [SerializeField] private float mapScale;
    [SerializeField] private float lacunarity;
    [SerializeField] private float persistence;

    public void GenerateMap()
    {
        float[,] noiseMap = NoiseGen.GenerateNoiseMap(mapX, mapY, lacunarity, persistence, mapScale);
        TerrainDisplay display = FindObjectOfType<TerrainDisplay>();
        display.DrawNoiseMap(noiseMap);
    }
}
