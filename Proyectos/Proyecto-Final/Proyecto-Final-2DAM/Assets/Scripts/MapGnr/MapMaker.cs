using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapMaker : MonoBehaviour
{
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;

    public TileBase[] floorTiles;
    public TileBase[] wallTiles;

    public int mapWidth = 32;
    public int mapHeight = 32;
    private int[,] mapData;

    public RoomGenerator roomGenerator;

    public GameObject playerPrefab;
    public GameObject enemyPrefab;

    private TileBase currentWallTile;

    void Start()
    {
        currentWallTile = wallTiles[Random.Range(0, wallTiles.Length)];

        List<TileBase> selectedFloorPattern = new List<TileBase>();
        int totalLength = mapWidth * mapHeight;
        while (selectedFloorPattern.Count < totalLength)
        {
            TileBase floor = floorTiles[Random.Range(0, floorTiles.Length)];
            int repeat = Random.Range(1, 4);
            for (int i = 0; i < repeat; i++)
                selectedFloorPattern.Add(floor);
        }

        mapData = new int[mapWidth, mapHeight];
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
                mapData[i, j] = -1;
        }

        roomGenerator.GenerateRooms(mapData, mapWidth, mapHeight);
        GenerateTiles(selectedFloorPattern);
        SpawnPlayer();
        SpawnEnemies();
    }

    void GenerateTiles(List<TileBase> floorPattern)
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        int index = 0;
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                Vector3Int pos = new Vector3Int(i, j, 0);
                switch (mapData[i, j])
                {
                    case 0:
                        floorTilemap.SetTile(pos, floorPattern[index % floorPattern.Count]);
                        index++;
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
        }
    }

    void SpawnEnemies()
    {
        foreach (var room in roomGenerator.rooms)
        {
            if (room == roomGenerator.startRoom) continue;

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
            triggerGO.transform.position = new Vector2(room.bounds.center.x, room.bounds.center.y);
            triggerCol.size = new Vector2(room.bounds.width - 0.5f, room.bounds.height - 0.5f);
            RoomTrigger roomTrigger = triggerGO.AddComponent<RoomTrigger>();

            roomTrigger.roomBounds = room.bounds;
            roomTrigger.floorTilemap = floorTilemap;
            roomTrigger.wallTilemap = wallTilemap;
            roomTrigger.wallTile = currentWallTile;
            roomTrigger.floorTiles = floorTiles; // <-- importante
            roomTrigger.roomEnemies = new List<EnemyFollow>();

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
