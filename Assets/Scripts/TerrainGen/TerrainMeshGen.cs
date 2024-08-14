using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainMeshGen
{
    public static MeshData GenerateTerrainMesh(float[,] noiseMap, float verticalDisplacementScale)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        
        //Centering code
        float topLX = (width - 1) / -2f;
        float topLZ = (height - 1) / 2f;

        MeshData meshData = new MeshData(width, height);
        int index = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                meshData.Vertices[index] = new Vector3(topLX + x, verticalDisplacementScale * noiseMap[x, y], topLZ - y);
                meshData.UVs[index] = new Vector2(x / (float)width, y / (float)height);

                //Setting triangles
                //Ignoring the bottom and right edges of the square
                if (x < width - 1 && y < height - 1)
                {
                    //Using width to iterate as a 1D array
                    meshData.AddTriangle(index, index + width + 1, index + width);
                    meshData.AddTriangle(index + width + 1, index, index + 1);
                }
                
                index++;
            }
        }

        return meshData;
    }
}

public class MeshData
{
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;
    
    private int triangleIndex;

    public Vector2[] UVs
    {
        get => uvs;
        set => uvs = value;
    }

    public Vector3[] Vertices
    {
        get => vertices;
        set => vertices = value;
    }

    public int[] Triangles
    {
        get => triangles;
        set => triangles = value;
    }

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex+1] = b;
        triangles[triangleIndex+2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}