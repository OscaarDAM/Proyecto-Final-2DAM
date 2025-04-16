using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapMaker : MonoBehaviour
{
    // TILEMAPS
    public Tilemap floorTilemap; // Suelo
    public Tilemap wallTilemap;  // Muros con colisión

    // TILES
    public TileBase floorTile;
    public TileBase wallTile;

    // DIMENSIONES DEL MAPA
    public int mapWidth = 32;
    public int mapHeight = 32;

    // DATOS DEL MAPA
    private int[,] mapData;

    // GENERADOR DE HABITACIONES
    public RoomGenerator roomGenerator;

    void Start()
    {
        mapData = new int[mapWidth, mapHeight];

        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                mapData[i, j] = -1; // Espacio vacío
            }
        }

        roomGenerator.GenerateRooms(mapData, mapWidth, mapHeight);
        GenerateTiles();
    }

    void GenerateTiles()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                Vector3Int pos = new Vector3Int(i, j, 0);

                switch (mapData[i, j])
                {
                    case 0: // suelo
                        floorTilemap.SetTile(pos, floorTile);
                        break;
                    case 1: // muro
                        wallTilemap.SetTile(pos, wallTile);
                        break;
                }
            }
        }
    }
}
