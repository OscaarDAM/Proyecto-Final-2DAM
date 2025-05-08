using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomTrigger : MonoBehaviour
{
    public List<EnemyFollow> roomEnemies = new List<EnemyFollow>();
    List<Vector3Int> blockedTiles = new List<Vector3Int>();
    private bool isRoomCleared = false;

    public Tilemap wallTilemap;
    public Tilemap floorTilemap;
    public TileBase wallTile;
    public TileBase floorTile;
    public RectInt roomBounds;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isRoomCleared)
        {
            Debug.Log("Player entró en trigger de sala.");

            foreach (var enemy in roomEnemies)
            {
                if (enemy != null)
                    enemy.SetCanFollow(true);
            }

            StartBlocking();
            StartCoroutine(CheckEnemiesCoroutine());
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M) && !isRoomCleared)
        {
            KillEnemiesInRoom();
        }
    }

    private void KillEnemiesInRoom()
    {
        Debug.Log("Matando a todos los enemigos en esta sala.");
        foreach (var enemy in roomEnemies)
        {
            if (enemy != null)
            {
                enemy.KillEnemy();
            }
        }
    }

    public void StartBlocking()
    {
        StartCoroutine(BlockRoomBorders());
    }

    private IEnumerator BlockRoomBorders()
    {
        yield return new WaitForSeconds(1f);

        blockedTiles.Clear();

        Vector3Int[] directions = new Vector3Int[] {
            Vector3Int.right,
            Vector3Int.left,
            Vector3Int.up,
            Vector3Int.down
        };

        for (int x = roomBounds.xMin; x <= roomBounds.xMax; x++)
        {
            for (int y = roomBounds.yMin; y <= roomBounds.yMax; y++)
            {
                bool isEdge = (x == roomBounds.xMin || x == roomBounds.xMax ||
                               y == roomBounds.yMin || y == roomBounds.yMax);

                if (!isEdge)
                    continue;

                Vector3Int tilePos = new Vector3Int(x, y, 0);

                if (floorTilemap.GetTile(tilePos) == floorTile && wallTilemap.GetTile(tilePos) == null)
                {
                    foreach (var dir in directions)
                    {
                        Vector3Int neighbor = tilePos + dir;
                        if (!roomBounds.Contains((Vector2Int)neighbor) && floorTilemap.GetTile(neighbor) == floorTile)
                        {
                            floorTilemap.SetTile(tilePos, null);
                            wallTilemap.SetTile(tilePos, wallTile);
                            blockedTiles.Add(tilePos);
                            break;
                        }
                    }
                }
            }
        }
    }

    private IEnumerator CheckEnemiesCoroutine()
    {
        while (roomEnemies.Exists(e => e != null))
            yield return new WaitForSeconds(1f);

        Debug.Log("Todos los enemigos eliminados. Restaurando accesos.");
        isRoomCleared = true;

        foreach (var tilePos in blockedTiles)
        {
            wallTilemap.SetTile(tilePos, null);
            floorTilemap.SetTile(tilePos, floorTile);
        }
        blockedTiles.Clear();
    }
}
