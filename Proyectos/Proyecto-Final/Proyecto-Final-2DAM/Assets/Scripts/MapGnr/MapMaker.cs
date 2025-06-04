using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapMaker : MonoBehaviour
{
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public Tilemap backgroundTilemap;

    public TileBase[] floorTiles;
    public TileBase[] wallTiles;
    public TileBase backgroundTile;

    public int mapWidth = 32;
    public int mapHeight = 32;
    private int[,] mapData;

    public RoomGenerator roomGenerator;

    public GameObject playerPrefab;
    public GameObject enemyPrefab;

    private TileBase currentWallTile;
    public List<EnemyFollow> allEnemies = new List<EnemyFollow>();

    public TileBase specialTile;
    public AudioClip specialTileSound;
    private AudioSource audioSource;

    public TileBase cleanWallTile;
    public float wallChangeDelay = 0.02f;

    void Start()
    {
        int currentLevel = PlayerPrefs.GetInt("Level", 1);
        Debug.Log("Nivel actual: " + currentLevel);

        audioSource = gameObject.AddComponent<AudioSource>();
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
                    default:
                        if (backgroundTile != null)
                            backgroundTilemap.SetTile(pos, backgroundTile);
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
        int currentLevel = PlayerPrefs.GetInt("Level", 1);

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
            roomTrigger.floorTiles = floorTiles;
            roomTrigger.roomEnemies = new List<EnemyFollow>();

            // 👇 Aquí se define cuántos enemigos poner en esta sala
            int enemyCount = 3; // Valor base

            // 🎲 Aleatoriamente, en algunas salas, se añade un enemigo más
            if (Random.value < 0.3f) // 30% de probabilidad
            {
                enemyCount += 1;
            }

            enemyCount = Mathf.Min(enemyCount, validPositions.Count);

            for (int i = 0; i < enemyCount; i++)
            {
                int index = Random.Range(0, validPositions.Count);
                Vector2Int pos = validPositions[index];
                validPositions.RemoveAt(index);

                Vector3 spawnPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);
                GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

                // ✅ Usar GetComponentInChildren si EnemyFollow está en un hijo del prefab
                EnemyFollow enemyScript = enemy.GetComponentInChildren<EnemyFollow>();
                if (enemyScript != null)
                {
                    enemyScript.roomBounds = new Bounds(
                        new Vector3(room.bounds.center.x, room.bounds.center.y, 0f),
                        new Vector3(room.bounds.size.x, room.bounds.size.y, 1f)
                    );

                    // 📈 Escalar la vida máxima según el nivel
                    enemyScript.maxHealth += currentLevel - 1;
                    enemyScript.currentHealth = enemyScript.maxHealth;

                    roomTrigger.roomEnemies.Add(enemyScript);
                    allEnemies.Add(enemyScript);
                }
            }
        }
    }


    public void CheckAllEnemiesDead()
    {
        if (allEnemies.TrueForAll(e => e == null))
        {
            Debug.Log("¡Todos los enemigos han muerto!");

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            Vector3 playerPos = player.transform.position;
            RoomData chosenRoom = null;

            foreach (var room in roomGenerator.rooms)
            {
                if (room.bounds.Contains((Vector2Int)Vector3Int.FloorToInt(playerPos)))
                {
                    chosenRoom = room;
                    break;
                }
            }

            if (chosenRoom == null)
            {
                Debug.LogWarning("No se encontró la sala del jugador.");
                return;
            }

            for (int attempt = 0; attempt < 50; attempt++)
            {
                int x = Random.Range(chosenRoom.bounds.xMin + 1, chosenRoom.bounds.xMax - 1);
                int y = Random.Range(chosenRoom.bounds.yMin + 1, chosenRoom.bounds.yMax - 1);
                Vector3Int tilePos = new Vector3Int(x, y, 0);

                if (floorTilemap.GetTile(tilePos) != null && wallTilemap.GetTile(tilePos) == null)
                {
                    floorTilemap.SetTile(tilePos, specialTile);

                    if (specialTileSound != null && audioSource != null)
                        audioSource.PlayOneShot(specialTileSound);

                    Debug.Log("Tile especial colocado en: " + tilePos);

                    GameObject triggerGO = new GameObject("NextLevelTrigger");
                    Vector3 worldPos = floorTilemap.CellToWorld(tilePos) + new Vector3(0.5f, 0.5f, 0f);
                    triggerGO.transform.position = worldPos;

                    BoxCollider2D col = triggerGO.AddComponent<BoxCollider2D>();
                    col.isTrigger = true;
                    col.size = new Vector2(0.9f, 0.9f);

                    triggerGO.AddComponent<NextLevelTrigger>();
                    StartCoroutine(GradualWallTileChange());
                    break;
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

    IEnumerator GradualWallTileChange()
    {
        BoundsInt bounds = wallTilemap.cellBounds;

        int minSum = bounds.xMin + bounds.yMin;
        int maxSum = bounds.xMax + bounds.yMax;

        for (int sum = minSum; sum <= maxSum; sum++)
        {
            for (int x = bounds.xMin; x <= bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y <= bounds.yMax; y++)
                {
                    if (x + y == sum)
                    {
                        Vector3Int pos = new Vector3Int(x, y, 0);
                        TileBase current = wallTilemap.GetTile(pos);
                        if (current == currentWallTile)
                        {
                            wallTilemap.SetTile(pos, cleanWallTile);
                        }
                    }
                }
            }

            yield return new WaitForSeconds(wallChangeDelay);
        }

        Debug.Log("Transformación de paredes completada.");
    }

    void RefreshWallColliders()
    {
        wallTilemap.CompressBounds();
    }
}
