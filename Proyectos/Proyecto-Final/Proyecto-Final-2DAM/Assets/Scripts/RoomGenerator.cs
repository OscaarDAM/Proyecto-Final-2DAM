using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    public int roomCount = 5;
    public int roomMinSize = 4;
    public int roomMaxSize = 10;

    public void GenerateRooms(int[,] mapData, int width, int height)
    {
        List<Vector2Int> roomCenters = new List<Vector2Int>();
        int attempts = 0;
        int maxAttempts = roomCount * 5;

        while (roomCenters.Count < roomCount && attempts < maxAttempts)
        {
            attempts++;

            int w = Random.Range(roomMinSize, roomMaxSize);
            int h = Random.Range(roomMinSize, roomMaxSize);
            int x = Random.Range(1, width - w - 1);
            int y = Random.Range(1, height - h - 1);

            if (!IsAreaEmpty(mapData, x, y, w, h))
                continue;

            // Crear habitación
            for (int dx = x; dx < x + w; dx++)
            {
                for (int dy = y; dy < y + h; dy++)
                {
                    mapData[dx, dy] = 0;
                }
            }

            // Guardar el centro de la habitación
            roomCenters.Add(new Vector2Int(x + w / 2, y + h / 2));
        }

        // Conectar habitaciones con pasillos
        for (int i = 1; i < roomCenters.Count; i++)
        {
            ConnectRooms(mapData, roomCenters[i - 1], roomCenters[i]);
        }
    }

    bool IsAreaEmpty(int[,] mapData, int x, int y, int w, int h)
    {
        for (int dx = x - 1; dx <= x + w; dx++)
        {
            for (int dy = y - 1; dy <= y + h; dy++)
            {
                if (dx < 0 || dy < 0 || dx >= mapData.GetLength(0) || dy >= mapData.GetLength(1))
                    return false;

                if (mapData[dx, dy] == 0)
                    return false;
            }
        }
        return true;
    }

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

    void CreateHorizontalTunnel(int[,] mapData, int x1, int x2, int y)
    {
        for (int x = Mathf.Min(x1, x2); x <= Mathf.Max(x1, x2); x++)
        {
            if (x >= 0 && x < mapData.GetLength(0) && y >= 0 && y < mapData.GetLength(1))
                mapData[x, y] = 0;
        }
    }

    void CreateVerticalTunnel(int[,] mapData, int y1, int y2, int x)
    {
        for (int y = Mathf.Min(y1, y2); y <= Mathf.Max(y1, y2); y++)
        {
            if (x >= 0 && x < mapData.GetLength(0) && y >= 0 && y < mapData.GetLength(1))
                mapData[x, y] = 0;
        }
    }
}
