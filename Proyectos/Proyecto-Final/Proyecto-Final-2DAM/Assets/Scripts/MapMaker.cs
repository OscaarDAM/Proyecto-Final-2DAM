using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapMaker : MonoBehaviour
{
    // TILEMAP
    public Tilemap tilemap;
    public Tile tile;

    // DIMENSIONES DEL MAPA
    public int mapWidth = 32;
    public int mapHeight = 32;

    // DATOS DEL MAPA
    private int[,] mapData;

    // SCRIPTS PARA GENERAR MAPA
    public RoomGenerator roomGenerator;

    void Start()
    {
        mapData = new int[mapWidth, mapHeight];
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                mapData[i, j] = 1;
            }
        }

        roomGenerator.GenerateRooms(mapData, mapWidth, mapHeight);

        GenerateTiles();
    }

    void GenerateTiles()
    {
        tilemap.ClearAllTiles();

        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                if (mapData[i, j] == 0) 
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
