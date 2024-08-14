using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public enum Drawmode
    {
        NoiseMap,
        ColorMap,
        Geometry
    }
    
    public bool autoUpdate;
    [SerializeField] private Drawmode drawMode;
    [SerializeField] private int seed;
    [SerializeField] private int mapX;
    [SerializeField] private int mapY;
    [SerializeField] private float mapScale;
    [SerializeField] private int octaves;
    [SerializeField] private float lacunarity;
    [Range(0f,1f)] [SerializeField] private float persistence;
    [SerializeField] private Vector2 offset;
    [SerializeField] private float verticalDisplacementScale;
    [SerializeField] private TerrainType[] terrainTypes;

    public void GenerateMap()
    {
        float[,] noiseMap = NoiseGen.GenerateNoiseMapPerlin(seed,mapX, mapY, octaves, lacunarity, persistence, mapScale, offset);
        TerrainDisplay display = FindObjectOfType<TerrainDisplay>();

        Color[] colorMap = new Color[mapX * mapY];
        for (int y = 0; y < mapY; y++)
        {
            for (int x = 0; x < mapX; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < terrainTypes.Length; i++)
                {
                    if (currentHeight <= terrainTypes[i].height)
                    {
                        colorMap[y * mapX + x] = terrainTypes[i].color;
                        break;
                    }
                }
            }
        }

        switch (drawMode)
        {
            case Drawmode.NoiseMap:
                display.DrawTexture(TerrainDisplay.TextureFromHeightMap(noiseMap));
                break;
            case Drawmode.ColorMap:
                display.DrawTexture(TerrainDisplay.TextureFromColorMap(colorMap, mapX, mapY));
                break;
            case Drawmode.Geometry:
                display.DrawMesh(TerrainMeshGen.GenerateTerrainMesh(noiseMap, verticalDisplacementScale), TerrainDisplay.TextureFromColorMap(colorMap, mapX, mapY));
                break;
        }
    }
    
    [Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color color;
    }
}
