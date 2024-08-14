using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseGen
{
   /// <summary>
   /// Method used to create maps of perlin noise for terrain generation
   /// </summary>
   /// <param name="mapWidth"> 2D array width </param>
   /// <param name="mapHeight"> 2D array height</param>
   /// <param name="lacunarity"> Controls increase in frequency of layers </param>
   /// <param name="persistence"> Controls decrease in amplitude of layers </param>
   /// <returns> A layered noise map of values between 0 and 1 </returns>
   public static float[,] GenerateNoiseMapPerlin(int mapWidth, int mapHeight, float lacunarity, float persistence, float noiseScale)
   {
      noiseScale = Mathf.Clamp(noiseScale, 0.0001f, Single.MaxValue);
      float[,] output = new float[mapWidth,mapHeight];
      //Stepping through every y value
      for (int y = 0; y < mapHeight; y++)
      {
         //stepping through every x value per y value
         for (int x = 0; x < mapWidth; x++)
         {
            float sampleX = x / noiseScale;
            float sampleY = y / noiseScale;

            float perlinVal = Mathf.PerlinNoise(sampleX, sampleY);
            output[x, y] = perlinVal;
         }
      }
      return output;
   }
}
