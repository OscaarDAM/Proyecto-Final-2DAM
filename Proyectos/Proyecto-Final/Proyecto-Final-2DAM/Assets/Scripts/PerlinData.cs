using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinData : MonoBehaviour
{
    public float fillPercent = 0.15f;
    public float scale;

    void Start()
    {
        scale = UnityEngine.Random.Range(20, 50);
    }
    public int[,] GenerateData(int width, int height)
    {
        int [,] mapData = new int[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float heightPercent = Mathf.Pow(1 - (float)j / (float)height, 3);

                float value = Mathf.PerlinNoise((float)i / scale, (float)j / scale) - heightPercent;

                mapData[i, j] = value < fillPercent ? 1 : 0;
            }
        }

        return mapData;

    }
}
