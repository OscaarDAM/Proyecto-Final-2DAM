using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapMaker : MonoBehaviour
{

    // TILEMAP
    public Tilemap tilemap;
    public Tile tile;

    // ALTO Y ANCHO DEL MAPA
    public int mapWidth = 32;
    public int mapHeight = 32;

    // DATOS DEL MAPA
    private int[,] mapData;

    // SCRIPT DE CELULAR AUTOMATA
    public CellularData cellularData;

    // SCRIPT DE PERLIN NOISE
    public PerlinData perlinData;

    void Start()
    {
        this.mapData = perlinData.GenerateData(this.mapWidth, this.mapHeight);

        this.GeneratedTiles();
    }

    void GeneratedTiles()       
    {
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                if (mapData[i, j] == 1)
                {
                    tilemap.SetTile(new Vector3Int(i, j, 0), tile);
                }
            }
        }
    }

    void Update()
    {
        
    }
}
