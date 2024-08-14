using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public static class NoiseGen
{
   /// <summary>
   /// Method used to create maps of perlin noise for terrain generation
   /// </summary>
   /// <param name="seed"> Random seed for the sampling positions </param>
   /// <param name="mapWidth"> 2D array width </param>
   /// <param name="mapHeight"> 2D array height</param>
   /// <param name="lacunarity"> Controls increase in frequency of layers </param>
   /// <param name="persistence"> Controls decrease in amplitude of layers </param>
   /// <param name="octaves"> Controls decrease in amplitude of layers </param>
   /// <param name="noiseScale"> Size of the generated map </param>
   /// <param name="offset"> X and Y sampling offsets </param>
   /// <returns> A layered noise map of values between 0 and 1 </returns>
   public static float[,] GenerateNoiseMapPerlin(int seed, int mapWidth, int mapHeight, int octaves, float lacunarity, float persistence, float noiseScale, Vector2 offset)
   {
      System.Random pseudoRNG = new Random(seed);
      Vector2[] octaveOffsets = new Vector2[octaves];

      for (int i = 0; i < octaves; i++)
      {
         float offsetX = pseudoRNG.Next(-100000, 100000) + offset.x;
         float offsetY = pseudoRNG.Next(-100000, 100000) + offset.y;
         octaveOffsets[i] = new Vector2(offsetX, offsetY);
      }
      noiseScale = Mathf.Clamp(noiseScale, 0.0001f, Single.MaxValue);
      float[,] output = new float[mapWidth,mapHeight];
      
      float maxHeight = Single.MinValue, minHeight = Single.MaxValue;

      #region Perlin Noise Gen

      //Stepping through every y value
      for (int y = 0; y < mapHeight; y++)
      {
         //stepping through every x value per y value
         for (int x = 0; x < mapWidth; x++)
         {
            float amplitude = 1;
            float freq = 1;
            float noiseHeight = 0;

            //Generating for each octave
            for (int i = 0; i < octaves; i++)
            {
               float sampleX = x / noiseScale * freq + octaveOffsets[i].x;
               float sampleY = y / noiseScale * freq + octaveOffsets[i].y;
               
               //Getting a value from -1 to 1
               float perlinVal = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
               noiseHeight += perlinVal * amplitude;
               
               //With persistence some value btwn 0 and 1...
               amplitude *= persistence;
               //And lacunarity greater than 1
               freq *= lacunarity;
            }

            #region min-max ranges

            if (noiseHeight > maxHeight)
            {
               maxHeight = noiseHeight;
            }

            if (noiseHeight < minHeight)
            {
               minHeight = noiseHeight;
            }

            #endregion

            output[x, y] = noiseHeight;
         }
      }

      #endregion

      #region Range Normalisation

      for (int i = 0; i < output.GetLength(0); i++)
      {
         for (int j = 0; j < output.GetLength(1); j++)
         {
            output[i,j] = Mathf.InverseLerp(minHeight, maxHeight, output[i,j]);
         }
      }

      #endregion
      
      return output;
   }
}
