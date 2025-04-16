using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public RoomGenerator roomGenerator;
    public GameObject playerPrefab;

    public void Start()
    {
        int width = 50;
        int height = 50;
        int[,] map = new int[width, height];

        // Inicializar el mapa con valores inválidos
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map[x, y] = -1;

        roomGenerator.GenerateRooms(map, width, height);

        if (roomGenerator.startRoom != null)
        {
            Vector3? spawnPos = GetValidSpawnPosition(map, roomGenerator.startRoom);
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

    // Buscar un punto válido de suelo dentro de la habitación, evitando bordes
    private Vector3? GetValidSpawnPosition(int[,] map, RoomData room)
    {
        for (int x = room.bounds.xMin + 1; x < room.bounds.xMax - 1; x++)
        {
            for (int y = room.bounds.yMin + 1; y < room.bounds.yMax - 1; y++)
            {
                if (map[x, y] == 0) // suelo
                {
                    // Verificar que la posición esté dentro de los límites del mapa
                    if (x > 0 && x < map.GetLength(0) && y > 0 && y < map.GetLength(1))
                    {
                        return new Vector3(x + 0.5f, y + 0.5f, 0); // +0.5f para centrar en la celda
                    }
                }
            }
        }
        return null;
    }
}
