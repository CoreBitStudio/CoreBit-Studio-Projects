using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pathfinder class responsible for finding optimal paths on a grid using the A* algorithm.
/// Includes optional path smoothing and diagonal movement handling.
/// </summary>
public class Pathfinder : MonoBehaviour
{
    [Header("References")]
    [SerializeField, Tooltip("Reference to the GridManager that holds the node data.")]
    private GridManager gridManager;

    [Header("Pathfinding Settings")]
    [SerializeField, Tooltip("Enable or disable optional path smoothing.")]
    private bool enablePathSmoothing = false;

    /// <summary>
    /// Allows external classes to set the GridManager reference.
    /// </summary>
    public GridManager GridManager { set { gridManager = value; } }
    public bool EnablePathSmoothing { set { enablePathSmoothing = value; } get => enablePathSmoothing; }

    /// <summary>
    /// Finds a path from startPos to targetPos using A* pathfinding.
    /// </summary>
    /// <param name="startPos">World position to start from.</param>
    /// <param name="targetPos">World position to reach.</param>
    /// <returns>A list of nodes representing the path, or null if no path was found.</returns>
    public List<GridNode> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        gridManager.ClearPathData(); // Reset previous path data

        GridNode startNode = gridManager.NodeFromWorldPoint(startPos);
        GridNode targetNode = gridManager.NodeFromWorldPoint(targetPos);

        if (!startNode.walkable || !targetNode.walkable)
            return null;

        List<GridNode> openSet = new List<GridNode>();
        HashSet<GridNode> closedSet = new HashSet<GridNode>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // Find the node with the lowest F cost
            GridNode currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < currentNode.FCost ||
                    (openSet[i].FCost == currentNode.FCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            // Reached the target
            if (currentNode == targetNode)
            {
                List<GridNode> finalPath = RetracePath(startNode, targetNode);
                if (enablePathSmoothing)
                    finalPath = SmoothPath(finalPath);
                return finalPath;
            }

            // Check neighbors
            foreach (GridNode neighbour in gridManager.GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                    continue;

                // Prevent invalid diagonal movement
                bool isDiagonal = currentNode.gridX != neighbour.gridX && currentNode.gridY != neighbour.gridY;
                if (isDiagonal)
                {
                    GridNode nextH = gridManager.GetNode(currentNode.gridX + (neighbour.gridX - currentNode.gridX), currentNode.gridY);
                    GridNode nextV = gridManager.GetNode(currentNode.gridX, currentNode.gridY + (neighbour.gridY - currentNode.gridY));

                    if ((nextH == null || !nextH.walkable) && (nextV == null || !nextV.walkable))
                    {
                        continue; // Diagonal is blocked by obstacles
                    }
                }

                // Update cost and add to open set if needed
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }

        return null; // No path found
    }

    /// <summary>
    /// Applies line-of-sight based path smoothing to reduce unnecessary waypoints.
    /// </summary>
    private List<GridNode> SmoothPath(List<GridNode> path)
    {
        if (path == null || path.Count < 3)
            return path;

        List<GridNode> smoothPath = new List<GridNode>();
        smoothPath.Add(path[0]);

        int index = 0;
        while (index < path.Count - 1)
        {
            int nextIndex = index + 1;
            for (int i = path.Count - 1; i > nextIndex; i--)
            {
                if (HasClearLineOfSight(path[index], path[i]))
                {
                    nextIndex = i;
                    break;
                }
            }
            smoothPath.Add(path[nextIndex]);
            index = nextIndex;
        }

        return smoothPath;
    }

    /// <summary>
    /// Checks if there's a clear line of sight between two nodes.
    /// </summary>
    private bool HasClearLineOfSight(GridNode a, GridNode b)
    {
        Vector2 dir = b.worldPosition - a.worldPosition;
        float dist = Vector2.Distance(a.worldPosition, b.worldPosition);
        return !Physics2D.Raycast(a.worldPosition, dir.normalized, dist, gridManager.UnwalkableMask);
    }

    /// <summary>
    /// Rebuilds the path from end node to start node by following parent references.
    /// </summary>
    private List<GridNode> RetracePath(GridNode startNode, GridNode endNode)
    {
        List<GridNode> path = new List<GridNode>();
        GridNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    /// <summary>
    /// Calculates movement cost between two nodes, considering diagonal penalties.
    /// </summary>
    private int GetDistance(GridNode a, GridNode b, GridNode fromParent = null)
    {
        int dstX = Mathf.Abs(a.gridX - b.gridX);
        int dstY = Mathf.Abs(a.gridY - b.gridY);

        int baseCost = 10;
        int diagonalCost = 14;

        // Penalty for breaking a diagonal chain
        if (fromParent != null)
        {
            bool fromWasDiagonal = Mathf.Abs(fromParent.gridX - a.gridX) == 1 &&
                                   Mathf.Abs(fromParent.gridY - a.gridY) == 1;

            if (fromWasDiagonal && (a.gridX == fromParent.gridX || a.gridY == fromParent.gridY))
            {
                return diagonalCost * 2 + baseCost * (Mathf.Abs(dstX - dstY));
            }
        }

        return dstX > dstY
            ? diagonalCost * dstY + baseCost * (dstX - dstY)
            : diagonalCost * dstX + baseCost * (dstY - dstX);
    }
}