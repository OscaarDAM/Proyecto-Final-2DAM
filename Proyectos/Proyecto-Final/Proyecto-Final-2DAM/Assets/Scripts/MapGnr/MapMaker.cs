using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapMaker : MonoBehaviour
{
    // TILEMAPS
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;

    // TILES
    public TileBase floorTile;
    public TileBase wallTile;

    // MAPA
    public int mapWidth = 32;
    public int mapHeight = 32;
    private int[,] mapData;

    // GENERADOR DE HABITACIONES
    public RoomGenerator roomGenerator;

    // JUGADOR
    public GameObject playerPrefab;

    void Start()
    {
        mapData = new int[mapWidth, mapHeight];
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
                mapData[i, j] = -1;
        }

        roomGenerator.GenerateRooms(mapData, mapWidth, mapHeight);
        GenerateTiles();
        SpawnPlayer();
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
                    case 0:
                        floorTilemap.SetTile(pos, floorTile);
                        break;
                    case 1:
                        wallTilemap.SetTile(pos, wallTile);
                        break;
                }
            }
        }
    }

    void SpawnPlayer()
    {
        if (roomGenerator.startRoom != null)
        {
            Vector3? spawnPos = GetValidSpawnPosition(mapData, roomGenerator.startRoom);
            if (spawnPos.HasValue)
            {
                Instantiate(playerPrefab, spawnPos.Value, Quaternion.identity);
            }
            else
            {
                Debug.LogError("No se encontró una posición de suelo válida dentro de la habitación inicial.");
            }
        }
    }

    private Vector3? GetValidSpawnPosition(int[,] map, RoomData room)
    {
        for (int x = room.bounds.xMin + 1; x < room.bounds.xMax - 1; x++)
        {
            for (int y = room.bounds.yMin + 1; y < room.bounds.yMax - 1; y++)
            {
                if (map[x, y] == 0)
                {
                    return new Vector3(x + 0.5f, y + 0.5f, 0); // Centrado
                }
            }
        }
        return null;
    }
}

