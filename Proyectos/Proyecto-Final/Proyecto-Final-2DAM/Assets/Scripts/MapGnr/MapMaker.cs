using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapMaker : MonoBehaviour
{
    // TILEMAP
    public Tilemap tilemap;

    // TILES para suelo y muro
    public TileBase floorTile;
    public TileBase wallTile;

    // DIMENSIONES DEL MAPA
    public int mapWidth = 32;
    public int mapHeight = 32;

    // DATOS DEL MAPA
    private int[,] mapData;

    // SCRIPTS PARA GENERAR MAPA
    public RoomGenerator roomGenerator;

    void Start()
    {
        // Inicializar mapa con muros
        mapData = new int[mapWidth, mapHeight];
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                mapData[i, j] = -1; // 1 = muro por defecto
            }
        }

        // Generar habitaciones y pasillos
        roomGenerator.GenerateRooms(mapData, mapWidth, mapHeight);

        // Generar tiles visuales
        GenerateTiles();
    }

    void GenerateTiles()
    {
        tilemap.ClearAllTiles();

        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                Vector3Int pos = new Vector3Int(i, j, 0);

                switch (mapData[i, j])
                {
                    case 0: // suelo
                        tilemap.SetTile(pos, floorTile);
                        break;
                    case 1: // muro
                        tilemap.SetTile(pos, wallTile);
                        break;
                    default:
                        tilemap.SetTile(pos, null);
                        break;
                }
            }
        }
    }

    void Update()
    {

    }
}
