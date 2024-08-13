using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TerrainGenerator terrainGen = (TerrainGenerator) target;

        DrawDefaultInspector();

        if (GUILayout.Button("Generate Map"))
        {
            terrainGen.GenerateMap();
        }
    }
}
