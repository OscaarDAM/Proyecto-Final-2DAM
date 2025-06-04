using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomTrigger : MonoBehaviour
{
    public List<EnemyFollow> roomEnemies = new List<EnemyFollow>();
    public List<Vector3Int> blockedTiles = new List<Vector3Int>();

    public Tilemap wallTilemap;
    public Tilemap floorTilemap;
    public TileBase[] floorTiles;
    public TileBase wallTile;
    public RectInt roomBounds;

    private bool isRoomCleared = false;
    private bool playerLockedIn = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isRoomCleared && !playerLockedIn)
        {
            Debug.Log("PLAYER ENTRÓ AL TRIGGER");

            foreach (var enemy in roomEnemies)
            {
                if (enemy != null)
                {
                    enemy.SetCanFollow(true);
                    enemy.SetShooting(true);
                }
            }

            StartBlocking();
            StartCoroutine(CheckEnemiesCoroutine());
        }
    }

    public void StartBlocking()
    {
        StartCoroutine(BlockRoomBorders());
    }

    private IEnumerator BlockRoomBorders()
    {
        yield return new WaitForSeconds(1f);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;

        Vector3Int playerCell = Vector3Int.FloorToInt(player.transform.position);
        if (!roomBounds.Contains((Vector2Int)playerCell))
        {
            Debug.Log("Jugador salió de la sala antes de bloquear, no se cierran las puertas.");
            yield break;
        }

        playerLockedIn = true;
        blockedTiles.Clear();

        Vector3Int[] directions = { Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down };

        for (int x = roomBounds.xMin; x <= roomBounds.xMax; x++)
        {
            for (int y = roomBounds.yMin; y <= roomBounds.yMax; y++)
            {
                bool isEdge = (x == roomBounds.xMin || x == roomBounds.xMax || y == roomBounds.yMin || y == roomBounds.yMax);
                if (!isEdge) continue;

                Vector3Int tilePos = new Vector3Int(x, y, 0);
                TileBase tileAtPos = floorTilemap.GetTile(tilePos);

                if (tileAtPos != null && System.Array.Exists(floorTiles, t => t == tileAtPos) && wallTilemap.GetTile(tilePos) == null)
                {
                    foreach (var dir in directions)
                    {
                        Vector3Int neighbor = tilePos + dir;
                        if (!roomBounds.Contains((Vector2Int)neighbor) && System.Array.Exists(floorTiles, t => t == floorTilemap.GetTile(neighbor)))
                        {
                            floorTilemap.SetTile(tilePos, null);
                            wallTilemap.SetTile(tilePos, wallTile);
                            Debug.Log("Tile bloqueado en: " + tilePos + " con tile: " + wallTile?.name);
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

        isRoomCleared = true;
        foreach (var tilePos in blockedTiles)
        {
            wallTilemap.SetTile(tilePos, null);
            floorTilemap.SetTile(tilePos, floorTiles.Length > 0 ? floorTiles[0] : null);
        }
        blockedTiles.Clear();

        FindObjectOfType<MapMaker>().CheckAllEnemiesDead();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = new Vector3(roomBounds.center.x, roomBounds.center.y, 0f);
        Vector3 size = new Vector3(roomBounds.size.x, roomBounds.size.y, 1f);
        Gizmos.DrawWireCube(center, size);
    }
#endif
}
