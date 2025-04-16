using UnityEngine;

public class RoomData
{
    public Vector2Int center;
    public RectInt bounds;

    public RoomData(Vector2Int center, RectInt bounds)
    {
        this.center = center;
        this.bounds = bounds;
    }
}
