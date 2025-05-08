using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapMaker : MonoBehaviour
{
    // TILEMAPS
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;

    [Header("Tilesets")]
    public TileBase[] floorTiles; // Variantes de suelos
    public TileBase[] wallTiles;  // Variantes de paredes

    // Variables internas
    [HideInInspector] public TileBase[] currentFloorPattern;
    [HideInInspector] public TileBase currentWallTile;

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

        // Generar patrón aleatorio de tiles
        GenerateRandomTileSet();

        roomGenerator.GenerateRooms(mapData, mapWidth, mapHeight);
        GenerateTiles();
        SpawnPlayer();
        SpawnEnemies();
    }

    void GenerateRandomTileSet()
    {
        // Seleccionar de 1 a 2 tipos de suelo
        int floorCount = Random.Range(1, 3);
        List<TileBase> pattern = new List<TileBase>();
        for (int i = 0; i < floorCount; i++)
        {
            pattern.Add(floorTiles[Random.Range(0, floorTiles.Length)]);
        }
        currentFloorPattern = pattern.ToArray();

        // Seleccionar un tipo aleatorio de pared
        currentWallTile = wallTiles[Random.Range(0, wallTiles.Length)];
    }

    void GenerateTiles()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        int patternIndex = 0;

        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                Vector3Int pos = new Vector3Int(i, j, 0);
                switch (mapData[i, j])
                {
                    case 0:
                        TileBase tileToUse = currentFloorPattern[patternIndex % currentFloorPattern.Length];
                        floorTilemap.SetTile(pos, tileToUse);
                        patternIndex++;
                        break;
                    case 1:
                        wallTilemap.SetTile(pos, currentWallTile);
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

            GameObject triggerGO = new GameObject("RoomTrigger");
            BoxCollider2D triggerCol = triggerGO.AddComponent<BoxCollider2D>();
            triggerCol.isTrigger = true;
            triggerGO.layer = LayerMask.NameToLayer("Default");
            triggerGO.transform.position = new Vector2(
                room.bounds.center.x,
                room.bounds.center.y
            );
            triggerCol.size = new Vector2(
                room.bounds.width - 0.5f,
                room.bounds.height - 0.5f
            );
            RoomTrigger roomTrigger = triggerGO.AddComponent<RoomTrigger>();

            // Pasar los tiles actuales al RoomTrigger
            roomTrigger.roomBounds = room.bounds;
            roomTrigger.floorTilemap = floorTilemap;
            roomTrigger.wallTilemap = wallTilemap;
            roomTrigger.floorTile = currentFloorPattern[0];
            roomTrigger.wallTile = currentWallTile;

            int enemyCount = Mathf.Min(3, validPositions.Count);
            for (int i = 0; i < enemyCount; i++)
            {
                int index = Random.Range(0, validPositions.Count);
                Vector2Int pos = validPositions[index];
                validPositions.RemoveAt(index);

                Vector3 spawnPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);
                GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

                EnemyFollow enemyScript = enemy.GetComponentInChildren<EnemyFollow>();
                if (enemyScript != null)
                {
                    roomTrigger.roomEnemies.Add(enemyScript);
                }
                else
                {
                    Debug.LogError("EnemyFollow no se encontró en el prefab del enemigo.");
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
                    return new Vector3(x + 0.5f, y + 0.5f, 0);
                }
            }
        }
        return null;
    }
}
