using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public GameObject nextLevelTriggerPrefab;  // Prefab del objeto vacío
    public Tilemap tilemap;  // Tilemap donde se encuentra el tile especial
    public TileBase specialTile;  // Tile especial que aparece después de eliminar todos los enemigos

    private int totalEnemies;
    private int enemiesKilled;

    void Start()
    {
        totalEnemies = FindObjectsOfType<EnemyFollow>().Length;  // O el método que uses para contar enemigos
        enemiesKilled = 0;
    }

    public void EnemyKilled()
    {
        enemiesKilled++;

        if (enemiesKilled == totalEnemies)
        {
            SpawnNextLevelTrigger();
        }
    }

    private void SpawnNextLevelTrigger()
    {
        // Buscar la posición del tile especial
        Vector3Int tilePosition = tilemap.WorldToCell(transform.position);  // Usar la posición adecuada

        TileBase tile = tilemap.GetTile(tilePosition);
        if (tile == specialTile)
        {
            // Instanciar el objeto vacío sobre la posición del tile especial
            Vector3 worldPos = tilemap.CellToWorld(tilePosition);
            Instantiate(nextLevelTriggerPrefab, worldPos, Quaternion.identity);
        }
    }
}
