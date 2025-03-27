using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellularData : MonoBehaviour
{
    public float fillPercent = 0.5f;
    public int iterations = 1;

    public int[,] GenerateData(int width, int height)
    {
        int[,] mapData = new int[width, height];

        // Inicializar el mapa con paredes y espacios según el porcentaje de relleno
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Si es un borde, ponerlo como pared (1)
                if (i == 0 || i == width - 1 || j == 0 || j == height - 1)
                {
                    mapData[i, j] = 1;
                }
                else
                {
                    float change = Random.Range(0f, 1f);
                    mapData[i, j] = change < fillPercent ? 1 : 0;
                }
            }
        }

        int[,] bufferMap = new int[width, height];

        // Realizar las iteraciones de la simulación celular
        for (int i = 0; i < iterations; i++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int walls = 0;

                    // Contar las paredes vecinas
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {

                            if (x + dx >= 0 && x + dx < width && y + dy >= 0 && y + dy < height)
                            {
                                if (dx != 0 || dy != 0)
                                {
                                    walls += mapData[x + dx, y + dy];
                                }
                            }

                            else
                            {
                                walls++;
                            }

                        }
                    }

                    bufferMap[x, y] = walls > 4 ? 1 : 0;
                }
            }

            mapData = bufferMap;
        }

        return mapData;
    }
}
