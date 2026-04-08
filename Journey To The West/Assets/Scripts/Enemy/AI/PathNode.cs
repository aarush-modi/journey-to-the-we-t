using UnityEngine;

public class PathNode
{
    public Vector2Int GridPosition;
    public Vector2 WorldPosition;
    public bool Walkable;
    public int GCost;
    public int HCost;
    public PathNode Parent;

    public int FCost => GCost + HCost;

    public PathNode(Vector2Int gridPosition, Vector2 worldPosition, bool walkable)
    {
        GridPosition = gridPosition;
        WorldPosition = worldPosition;
        Walkable = walkable;
        GCost = int.MaxValue;
        HCost = 0;
        Parent = null;
    }
}
