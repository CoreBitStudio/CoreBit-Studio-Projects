using UnityEngine;

/// <summary>
/// Represents a single node in the grid used for pathfinding.
/// Stores position, walkability, and cost values for A* calculations.
/// </summary>
public class GridNode
{
    /// <summary>
    /// Indicates whether the node is traversable (true) or blocked (false).
    /// </summary>
    public bool walkable;

    /// <summary>
    /// The world position of this node in 3D space.
    /// </summary>
    public Vector3 worldPosition;

    /// <summary>
    /// The X index of this node in the grid.
    /// </summary>
    public int gridX;

    /// <summary>
    /// The Y index of this node in the grid.
    /// </summary>
    public int gridY;

    /// <summary>
    /// Cost from the start node to this node.
    /// </summary>
    public int gCost;

    /// <summary>
    /// Heuristic cost from this node to the target node.
    /// </summary>
    public int hCost;

    /// <summary>
    /// The node that comes before this one in the current path.
    /// </summary>
    public GridNode parent;

    /// <summary>
    /// Total cost function for A* (f = g + h).
    /// </summary>
    public int FCost => gCost + hCost;

    /// <summary>
    /// Constructs a new GridNode with walkability, world position, and grid indices.
    /// </summary>
    public GridNode(bool walkable, Vector3 worldPosition, int gridX, int gridY)
    {
        this.walkable = walkable;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
    }
}
