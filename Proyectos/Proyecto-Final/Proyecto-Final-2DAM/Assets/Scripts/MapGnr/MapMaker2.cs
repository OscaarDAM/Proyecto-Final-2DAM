using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapMaker2 : MonoBehaviour
{
    public Tilemap tilemap;
    public Tile wallTile;
    public Tile floorTile;

    public int mapWidth = 32;
    public int mapHeight = 32;

    private int[,] mapData;
    public RoomGenerator roomGenerator;

    void Start()
    {
        this.mapData= new int[mapWidth, mapHeight];
        roomGenerator.GenerateRooms(mapData, mapWidth, mapHeight);
        PrintMap();
        GeneratedTiles();
    }

    void PrintMap()
{
    for (int i = 0; i < mapWidth; i++)
    {
        string row = "";
        for (int j = 0; j < mapHeight; j++)
        {
            row += mapData[i, j] + " ";
        }
        Debug.Log(row);
    }
}

    void GeneratedTiles()
    {
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                if (mapData[i, j] == 1)
                {
                    tilemap.SetTile(new Vector3Int(i, j, 0), wallTile);
                }
                else
                {
                    tilemap.SetTile(new Vector3Int(i, j, 0), floorTile);
                }
            }
        }
    }
}
