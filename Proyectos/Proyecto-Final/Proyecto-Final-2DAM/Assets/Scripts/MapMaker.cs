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
        // Inicializar el mapa con paredes sólidas (1)
        mapData = new int[mapWidth, mapHeight];
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                mapData[i, j] = 1; // Poner todo como pared
            }
        }

        // Generar habitaciones y pasillos
        roomGenerator.GenerateRooms(mapData, mapWidth, mapHeight);

        // Generar tiles en el Tilemap
        GenerateTiles();
    }

    void GenerateTiles()
    {
        tilemap.ClearAllTiles(); // Limpiar antes de dibujar

        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                if (mapData[i, j] == 0) // 0 representa el suelo
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
