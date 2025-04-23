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

    // ENEMIGOS
    public GameObject enemyPrefab;

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
        SpawnEnemies();
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

    void SpawnEnemies()
    {
        foreach (var room in roomGenerator.rooms)
        {
            if (room == roomGenerator.startRoom)
                continue;

            List<Vector2Int> validPositions = new List<Vector2Int>();
            for (int x = room.bounds.xMin + 2; x < room.bounds.xMax - 2; x++)
            {
                for (int y = room.bounds.yMin + 2; y < room.bounds.yMax - 2; y++)
                {
                    if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight && mapData[x, y] == 0)
                        validPositions.Add(new Vector2Int(x, y));
                }
            }

            // Crear un GameObject vacío para el trigger de la habitación
            GameObject triggerGO = new GameObject("RoomTrigger");
            BoxCollider2D triggerCol = triggerGO.AddComponent<BoxCollider2D>();
            triggerCol.isTrigger = true;
            triggerGO.layer = LayerMask.NameToLayer("Default");
            triggerGO.transform.position = new Vector2(
                room.bounds.center.x,
                room.bounds.center.y
            );
            triggerCol.size = new Vector2(
                room.bounds.width - 2,
                room.bounds.height - 2
            );
            RoomTrigger roomTrigger = triggerGO.AddComponent<RoomTrigger>();


            // Inicializar la lista de enemigos en el RoomTrigger
            int enemyCount = Mathf.Min(3, validPositions.Count);
            for (int i = 0; i < enemyCount; i++)
            {
                int index = Random.Range(0, validPositions.Count);
                Vector2Int pos = validPositions[index];
                validPositions.RemoveAt(index);

                Vector3 spawnPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);
                GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

                // Vincular el enemigo al RoomTrigger
                EnemyFollow enemyScript = enemy.GetComponentInChildren<EnemyFollow>();
                
                // Verificar si el script EnemyFollow está presente en el prefab del enemigo
                if (enemyScript == null)
                {
                    Debug.LogError("EnemyFollow no se encontró en el prefab del enemigo.");
                }
                else
                {
                    roomTrigger.roomEnemies.Add(enemyScript);
                }

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

