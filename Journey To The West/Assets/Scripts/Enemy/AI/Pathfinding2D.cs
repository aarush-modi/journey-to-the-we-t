using UnityEngine;
using System.Collections.Generic;

public class Pathfinding2D : MonoBehaviour
{
    public static Pathfinding2D Instance { get; private set; }

    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector2Int gridSize = new Vector2Int(50, 50);
    [SerializeField] private Vector2 gridOrigin;

    private PathNode[,] grid;

    private void Awake()
    {
        Instance = this;
        BakeGrid();
    }

    public void SetBounds(Vector2 origin, Vector2Int size)
    {
        gridOrigin = origin;
        gridSize = size;
    }

    public void BakeGrid()
    {
        grid = new PathNode[gridSize.x, gridSize.y];

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2 worldPos = GridToWorld(x, y);
                bool walkable = Physics2D.OverlapBox(
                    worldPos,
                    Vector2.one * cellSize * 0.9f,
                    0f,
                    obstacleLayers
                ) == null;

                grid[x, y] = new PathNode(new Vector2Int(x, y), worldPos, walkable);
            }
        }
    }

    public bool IsWalkable(Vector2 worldPos)
    {
        Vector2Int gridPos = WorldToGrid(worldPos);
        if (!InBounds(gridPos))
            return false;
        return grid[gridPos.x, gridPos.y].Walkable;
    }

    public List<Vector2> FindPath(Vector2 start, Vector2 end)
    {
        Vector2Int startGrid = WorldToGrid(start);
        Vector2Int endGrid = WorldToGrid(end);

        if (!InBounds(startGrid) || !InBounds(endGrid))
            return null;

        PathNode startNode = grid[startGrid.x, startGrid.y];
        PathNode endNode = grid[endGrid.x, endGrid.y];

        if (!startNode.Walkable || !endNode.Walkable)
            return null;

        // Reset costs for all nodes
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                grid[x, y].GCost = int.MaxValue;
                grid[x, y].Parent = null;
            }
        }

        List<PathNode> openList = new List<PathNode>();
        HashSet<PathNode> closedSet = new HashSet<PathNode>();

        startNode.GCost = 0;
        startNode.HCost = OctileDistance(startGrid, endGrid);
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            // Find node with lowest FCost (tie-break on HCost)
            PathNode current = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].FCost < current.FCost ||
                    (openList[i].FCost == current.FCost && openList[i].HCost < current.HCost))
                {
                    current = openList[i];
                }
            }

            openList.Remove(current);
            closedSet.Add(current);

            if (current == endNode)
            {
                List<Vector2> path = RetracePath(startNode, endNode);
                return SmoothPath(path);
            }

            foreach (PathNode neighbor in GetNeighbors(current))
            {
                if (!neighbor.Walkable || closedSet.Contains(neighbor))
                    continue;

                int dx = Mathf.Abs(neighbor.GridPosition.x - current.GridPosition.x);
                int dy = Mathf.Abs(neighbor.GridPosition.y - current.GridPosition.y);
                int moveCost = (dx == 1 && dy == 1) ? 14 : 10;

                int tentativeGCost = current.GCost + moveCost;

                if (tentativeGCost < neighbor.GCost)
                {
                    neighbor.GCost = tentativeGCost;
                    neighbor.HCost = OctileDistance(neighbor.GridPosition, endGrid);
                    neighbor.Parent = current;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        return null;
    }

    private List<PathNode> GetNeighbors(PathNode node)
    {
        List<PathNode> neighbors = new List<PathNode>();
        int x = node.GridPosition.x;
        int y = node.GridPosition.y;

        // Cardinal directions
        bool canUp = InBounds(x, y + 1) && grid[x, y + 1].Walkable;
        bool canDown = InBounds(x, y - 1) && grid[x, y - 1].Walkable;
        bool canLeft = InBounds(x - 1, y) && grid[x - 1, y].Walkable;
        bool canRight = InBounds(x + 1, y) && grid[x + 1, y].Walkable;

        // Add cardinal neighbors (even if not walkable, the main loop filters them)
        if (InBounds(x, y + 1)) neighbors.Add(grid[x, y + 1]);
        if (InBounds(x, y - 1)) neighbors.Add(grid[x, y - 1]);
        if (InBounds(x - 1, y)) neighbors.Add(grid[x - 1, y]);
        if (InBounds(x + 1, y)) neighbors.Add(grid[x + 1, y]);

        // Diagonal neighbors: only if BOTH adjacent cardinals are walkable (no corner cutting)
        if (canUp && canRight && InBounds(x + 1, y + 1))
            neighbors.Add(grid[x + 1, y + 1]);
        if (canUp && canLeft && InBounds(x - 1, y + 1))
            neighbors.Add(grid[x - 1, y + 1]);
        if (canDown && canRight && InBounds(x + 1, y - 1))
            neighbors.Add(grid[x + 1, y - 1]);
        if (canDown && canLeft && InBounds(x - 1, y - 1))
            neighbors.Add(grid[x - 1, y - 1]);

        return neighbors;
    }

    private List<Vector2> RetracePath(PathNode startNode, PathNode endNode)
    {
        List<Vector2> path = new List<Vector2>();
        PathNode current = endNode;

        while (current != startNode)
        {
            path.Add(current.WorldPosition);
            current = current.Parent;
        }

        path.Add(startNode.WorldPosition);
        path.Reverse();
        return path;
    }

    private List<Vector2> SmoothPath(List<Vector2> path)
    {
        if (path == null || path.Count <= 2)
            return path;

        List<Vector2> smoothed = new List<Vector2>();
        smoothed.Add(path[0]);

        int current = 0;
        while (current < path.Count - 1)
        {
            int farthest = current + 1;

            // Find the farthest point we can reach with direct line-of-sight
            for (int i = path.Count - 1; i > current + 1; i--)
            {
                RaycastHit2D hit = Physics2D.Linecast(path[current], path[i], obstacleLayers);
                if (hit.collider == null)
                {
                    farthest = i;
                    break;
                }
            }

            smoothed.Add(path[farthest]);
            current = farthest;
        }

        return smoothed;
    }

    private int OctileDistance(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return 10 * (dx + dy) + (14 - 2 * 10) * Mathf.Min(dx, dy);
    }

    private Vector2 GridToWorld(int x, int y)
    {
        return gridOrigin + new Vector2(
            x * cellSize + cellSize * 0.5f,
            y * cellSize + cellSize * 0.5f
        );
    }

    private Vector2Int WorldToGrid(Vector2 worldPos)
    {
        Vector2 local = worldPos - gridOrigin;
        int x = Mathf.FloorToInt(local.x / cellSize);
        int y = Mathf.FloorToInt(local.y / cellSize);
        return new Vector2Int(x, y);
    }

    private bool InBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridSize.x && pos.y >= 0 && pos.y < gridSize.y;
    }

    private bool InBounds(int x, int y)
    {
        return x >= 0 && x < gridSize.x && y >= 0 && y < gridSize.y;
    }

    private void OnDrawGizmosSelected()
    {
        if (grid == null)
            return;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                PathNode node = grid[x, y];
                Gizmos.color = node.Walkable
                    ? new Color(0f, 1f, 0f, 0.2f)
                    : new Color(1f, 0f, 0f, 0.2f);
                Gizmos.DrawCube(node.WorldPosition, Vector3.one * cellSize * 0.9f);
            }
        }
    }
}
