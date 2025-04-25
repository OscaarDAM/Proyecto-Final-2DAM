using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomTrigger : MonoBehaviour
{
    public List<EnemyFollow> roomEnemies = new List<EnemyFollow>();  // Enemigos en esta sala
    List<Vector3Int> blockedTiles = new List<Vector3Int>();
    private bool isRoomCleared = false;

    public Tilemap wallTilemap;
    public Tilemap floorTilemap;
    public TileBase wallTile;
    public TileBase floorTile;
    public RectInt roomBounds;

    private void Start()
    {
        wallTilemap = GameObject.Find("WallTilemap").GetComponent<Tilemap>();
        floorTilemap = GameObject.Find("FloorTilemap").GetComponent<Tilemap>();
        wallTile = GameObject.FindObjectOfType<MapMaker>().wallTile;
        floorTile = GameObject.FindObjectOfType<MapMaker>().floorTile;
    }

    private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player") && !isRoomCleared)
    {
        Debug.Log("Player entró en trigger de sala.");

        // Activar seguimiento de los enemigos solo en esta sala
        foreach (var enemy in roomEnemies)
        {
            if (enemy != null)
                enemy.SetCanFollow(true);
        }

        BlockRoomBorders();
        StartCoroutine(CheckEnemiesCoroutine());
    }
}

private void Update()
{
    // Detectar la tecla de matar enemigos (por ejemplo, "M")
    if (Input.GetKeyDown(KeyCode.M) && !isRoomCleared)  // Solo ejecutar si la sala no está limpia
    {
        KillEnemiesInRoom();  // Solo se eliminan los enemigos de la sala actual
    }
}

private void KillEnemiesInRoom()
{
    Debug.Log("Matando a todos los enemigos en esta sala.");

    // Solo matar los enemigos que están en esta sala
    foreach (var enemy in roomEnemies)
    {
        if (enemy != null)
        {
            enemy.KillEnemy();  // Llamamos al método que destruye al enemigo
        }
    }
}

    private void BlockRoomBorders()
    {
        blockedTiles.Clear(); // Por si acaso

        Vector3Int[] directions = new Vector3Int[] {
            Vector3Int.right,
            Vector3Int.left,
            Vector3Int.up,
            Vector3Int.down
        };

        // Revisamos el borde de la sala
        for (int x = roomBounds.xMin; x <= roomBounds.xMax; x++)
        {
            for (int y = roomBounds.yMin; y <= roomBounds.yMax; y++)
            {
                bool isEdge = (x == roomBounds.xMin || x == roomBounds.xMax ||
                            y == roomBounds.yMin || y == roomBounds.yMax);

                if (!isEdge)
                    continue;

                Vector3Int tilePos = new Vector3Int(x, y, 0);

                // Solo si es piso, y no hay muro ya
                if (floorTilemap.GetTile(tilePos) == floorTile && wallTilemap.GetTile(tilePos) == null)
                {
                    // Detectamos si hay conexión hacia afuera de la sala (pasillo)
                    foreach (var dir in directions)
                    {
                        Vector3Int neighbor = tilePos + dir;

                        // Solo bloqueamos si ese vecino tiene también piso (es pasillo)
                        if (!roomBounds.Contains((Vector2Int)neighbor) && floorTilemap.GetTile(neighbor) == floorTile)
                        {
                            // Colocamos el muro en el borde de la sala (tilePos)
                            floorTilemap.SetTile(tilePos, null);  // Borramos el piso en el borde
                            wallTilemap.SetTile(tilePos, wallTile);  // Ponemos el muro en el borde
                            blockedTiles.Add(tilePos);  // Guardamos el borde como bloqueado
                            break;  // Ya bloqueamos esta entrada
                        }
                    }
                }
            }
        }
    }

    private bool IsConnectedToCorridor(Vector3Int pos)
    {
        // Revisar las 4 direcciones cardinales
        Vector3Int[] directions = {
            new Vector3Int(pos.x + 1, pos.y, 0),
            new Vector3Int(pos.x - 1, pos.y, 0),
            new Vector3Int(pos.x, pos.y + 1, 0),
            new Vector3Int(pos.x, pos.y - 1, 0)
        };

        foreach (var dir in directions)
        {
            if (!roomBounds.Contains(new Vector2Int(dir.x, dir.y)) &&
                floorTilemap.GetTile(dir) == floorTile)
            {
                return true; // Está conectado a un pasillo
            }
        }

        return false;
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
