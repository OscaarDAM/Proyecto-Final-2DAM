using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    public int roomCount = 7;
    public int roomMinSize = 11;
    public int roomMaxSize = 11;

    public List<RoomData> rooms = new List<RoomData>();
    public RoomData startRoom;

    public void GenerateRooms(int[,] mapData, int width, int height)
    {
        rooms.Clear();
        int attempts = 0;
        int maxAttempts = roomCount * 5;

        while (rooms.Count < roomCount && attempts < maxAttempts)
        {
            attempts++;

            int w = Random.Range(roomMinSize, roomMaxSize);
            if (w % 2 == 0) w++; // Asegurarse de que el ancho sea impar (en el caso de querer hacer par se debe quitar)

            int h = Random.Range(roomMinSize, roomMaxSize);
            if (h % 2 == 0) h++; // Asegurarse de que la altura sea impar (en el caso de querer hacer par se debe quitar)

            int x = Random.Range(1, width - w - 2);
            int y = Random.Range(1, height - h - 2);

            // Verificar que la habitación no se salga del mapa
            if (x < 1 || y < 1 || x + w >= width || y + h >= height) continue;

            if (!IsAreaEmpty(mapData, x - 2, y - 2, w + 4, h + 4))
                continue;

            // Crear habitación con borde de muro
            for (int dx = x - 1; dx <= x + w; dx++)
            {
                for (int dy = y - 1; dy <= y + h; dy++)
                {
                    if (dx == x - 1 || dx == x + w || dy == y - 1 || dy == y + h)
                        mapData[dx, dy] = 1; // muro
                    else
                        mapData[dx, dy] = 0; // suelo
                }
            }

            Vector2Int center = new Vector2Int(x + w / 2, y + h / 2);
            RectInt bounds = new RectInt(x, y, w, h);
            rooms.Add(new RoomData(center, bounds));

            Debug.Log("Habitación creada en: " + bounds);
        }

        // Asignar la primera habitación como la habitación de inicio
        if (rooms.Count > 0)
        {
            startRoom = rooms[Random.Range(0, rooms.Count)];
        }

        // Conectar habitaciones con pasillos
        for (int i = 1; i < rooms.Count; i++)
        {
            ConnectRooms(mapData, rooms[i - 1].center, rooms[i].center);
        }

        Debug.Log("Habitaciones generadas: " + rooms.Count);
    }

    // Verificar si el área está vacía (sin muros ni suelos)
    bool IsAreaEmpty(int[,] mapData, int x, int y, int w, int h)
{
    for (int dx = x; dx < x + w; dx++)
    {
        for (int dy = y; dy < y + h; dy++)
        {
            if (dx < 0 || dy < 0 || dx >= mapData.GetLength(0) || dy >= mapData.GetLength(1))
                return false;

            if (mapData[dx, dy] != -1)
                return false;
        }
    }
    return true;
}


    // Conectar dos habitaciones con un pasillo
    void ConnectRooms(int[,] mapData, Vector2Int centerA, Vector2Int centerB)
    {
        int x1 = centerA.x, y1 = centerA.y;
        int x2 = centerB.x, y2 = centerB.y;

        if (Random.value < 0.5f)
        {
            CreateHorizontalTunnel(mapData, x1, x2, y1);
            CreateVerticalTunnel(mapData, y1, y2, x2);
        }
        else
        {
            CreateVerticalTunnel(mapData, y1, y2, x1);
            CreateHorizontalTunnel(mapData, x1, x2, y2);
        }
    }

    // Crear túneles horizontales
    void CreateHorizontalTunnel(int[,] mapData, int x1, int x2, int y)
    {
        for (int x = Mathf.Min(x1, x2); x <= Mathf.Max(x1, x2); x++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int yy = y + dy;
                if (x >= 0 && x < mapData.GetLength(0) && yy >= 0 && yy < mapData.GetLength(1))
                {
                    if (dy == 0)
                        mapData[x, yy] = 0; // suelo
                    else if (mapData[x, yy] == -1)
                        mapData[x, yy] = 1; // borde
                }
            }
        }
    }

    // Crear túneles verticales
    void CreateVerticalTunnel(int[,] mapData, int y1, int y2, int x)
    {
        for (int y = Mathf.Min(y1, y2); y <= Mathf.Max(y1, y2); y++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int xx = x + dx;
                if (xx >= 0 && xx < mapData.GetLength(0) && y >= 0 && y < mapData.GetLength(1))
                {
                    if (dx == 0)
                        mapData[xx, y] = 0; // suelo
                    else if (mapData[xx, y] == -1)
                        mapData[xx, y] = 1; // borde
                }
            }
        }
    }
}
