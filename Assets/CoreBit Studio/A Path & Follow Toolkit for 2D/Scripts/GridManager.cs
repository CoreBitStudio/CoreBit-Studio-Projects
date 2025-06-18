using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a 2D grid of nodes used for pathfinding, including dynamic updates,
/// obstacle avoidance, and character padding. Responsible for grid generation,
/// node querying, and integration with pathfinding and movement agents.
/// </summary>
public class GridManager : MonoBehaviour
{
    [SerializeField, Tooltip("Transform representing the starting point for pathfinding.")]
    private Transform startTransform;

    [SerializeField, Tooltip("Transform representing the target point for pathfinding.")]
    private Transform targetTransform;

    [SerializeField, Tooltip("Layer mask defining what is considered unwalkable (e.g., walls or obstacles).")]
    private LayerMask unwalkableMask;

    [SerializeField, Tooltip("Size of the grid in world units (width, height).")]
    private Vector2 gridWorldSize;

    [SerializeField, Tooltip("Offset from transform.position to determine grid center.")]
    private Vector2 gridOffset;

    [SerializeField, Tooltip("Radius of each grid node. Node size is diameter = 2 * radius.")]
    private float nodeRadius;

    [SerializeField, Tooltip("Number of grid units to keep walkable nodes away from obstacles.")]
    private int obstaclePadding = 0;

    [SerializeField, Tooltip("Character size in world units (width, height). Used for grid walkability padding.")]
    private Vector2 characterSize = new Vector2(1f, 1f);

    [SerializeField, Tooltip("Reference to the pathfinding algorithm component.")]
    private Pathfinder pathfinder;

    [SerializeField, Tooltip("Reference to the agent that follows the computed path.")]
    private FollowerPathAgent followerPathAgent;

    [SerializeField, Tooltip("Whether to draw debug Gizmos for grid and path.")]
    private bool drawGizmos = true;


    private float nodeDiameter;
    private int gridSizeX, gridSizeY;

    private GridNode[,] grid;
    private List<GridNode> path;

    private Vector3 gridCenterPosition;

    public LayerMask UnwalkableMask => unwalkableMask;
    public float NodeRadius => nodeRadius;
    public Transform TargetTransform { get => targetTransform; set => targetTransform = value; }
    public Pathfinder Pathfinder { set => pathfinder = value; }
    public FollowerPathAgent FollowerPathAgent { set => followerPathAgent = value; }

    void Start()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        // Subscribe to directional change events
        followerPathAgent.OnChangeHorizontalSide.AddListener(OnChangeSide);
        followerPathAgent.OnChangeVerticalSide.AddListener(OnChangeSide);

        SetGridCenterPosition(); // Align grid center

        StartCoroutine(UpdateGridIEnumerator());
    }

    /// <summary>
    /// Coroutine that continuously updates the grid and path if needed.
    /// </summary>
    IEnumerator UpdateGridIEnumerator()
    {
        CreateGrid();
        path = pathfinder.FindPath(startTransform.position, targetTransform.position);

        if (path != null)
        {
            followerPathAgent.FollowPath(path);
        }

        while (true)
        {
            yield return new WaitForSeconds(0.25f);

            if (followerPathAgent.CanFollow)
            {
                if (Vector2.Distance(transform.position, targetTransform.position) > characterSize.x)
                {
                    StartCoroutine(UpdatGrid());
                }
            }
        }
    }

    /// <summary>
    /// Updates the grid and re-computes the path from current position to target.
    /// </summary>
    private IEnumerator UpdatGrid()
    {
        SetGridCenterPosition();
        CreateGrid();

        GridNode currentStartNode = NodeFromWorldPoint(transform.position);
        GridNode currentEndNode;

        if (!currentStartNode.walkable)
            currentStartNode = GetClosestWalkableNode(currentStartNode);

        // If the target is outside the grid, get closest walkable node instead
        if (!IsInsideGrid(targetTransform.position))
        {
            currentEndNode = GetClosestWalkableNodeToWorldPos(targetTransform.position);
        }
        else
        {
            currentEndNode = NodeFromWorldPoint(targetTransform.position);
            if (!currentEndNode.walkable)
                currentEndNode = GetClosestWalkableNode(currentEndNode);
        }

        List<GridNode> path = pathfinder.FindPath(currentStartNode.worldPosition, currentEndNode.worldPosition);

        if (path != null && path.Count > 0)
        {
            this.path = path;
            followerPathAgent.FollowPath(path);
        }

        yield return new WaitForSeconds(0.25f);
    }

    /// <summary>
    /// Returns true if a given world position is within the grid bounds.
    /// </summary>
    private bool IsInsideGrid(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - gridCenterPosition;
        return Mathf.Abs(localPos.x) <= gridWorldSize.x / 2f && Mathf.Abs(localPos.y) <= gridWorldSize.y / 2f;
    }

    /// <summary>
    /// Finds the closest walkable node to a world position.
    /// </summary>
    private GridNode GetClosestWalkableNodeToWorldPos(Vector3 worldPos)
    {
        GridNode closest = null;
        float closestDist = float.MaxValue;

        foreach (var node in grid)
        {
            if (!node.walkable) continue;

            float dist = Vector3.Distance(worldPos, node.worldPosition);
            if (dist < closestDist)
            {
                closest = node;
                closestDist = dist;
            }
        }

        return closest;
    }

    /// <summary>
    /// Finds the closest node on a given path to a world position.
    /// </summary>
    public GridNode FindClosestNodeOnPath(List<GridNode> path, Vector3 worldPos)
    {
        GridNode closest = null;
        float minDist = float.MaxValue;

        foreach (GridNode node in path)
        {
            float dist = Vector2.Distance(worldPos, node.worldPosition);
            if (dist < minDist)
            {
                minDist = dist;
                closest = node;
            }
        }

        return closest;
    }

    /// <summary>
    /// Searches for the nearest walkable node from a given starting node.
    /// </summary>
    public GridNode GetClosestWalkableNode(GridNode fromNode)
    {
        float closestDistance = Mathf.Infinity;
        GridNode closestNode = null;

        foreach (var node in grid)
        {
            if (node.walkable)
            {
                float dist = Vector3.Distance(fromNode.worldPosition, node.worldPosition);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestNode = node;
                }
            }
        }

        return closestNode;
    }

    /// <summary>
    /// Calculates and aligns the center of the grid based on agent direction and offset.
    /// </summary>
    private void SetGridCenterPosition()
    {
        float xAligned, yAligned;

        if (followerPathAgent.LastHorizontalDirection == HorizontalDirection.Right)
            xAligned = Mathf.Floor((transform.position.x + Mathf.Abs(gridOffset.x)) / nodeDiameter) * nodeDiameter;
        else
            xAligned = Mathf.Floor((transform.position.x - Mathf.Abs(gridOffset.x)) / nodeDiameter) * nodeDiameter;

        if (followerPathAgent.LastVerticalDirection == VerticalDirection.Up)
            yAligned = Mathf.Floor((transform.position.y + Mathf.Abs(gridOffset.y)) / nodeDiameter) * nodeDiameter;
        else
            yAligned = Mathf.Floor((transform.position.y - Mathf.Abs(gridOffset.y)) / nodeDiameter) * nodeDiameter;

        gridCenterPosition = new Vector3(xAligned, yAligned, transform.position.z);
    }

    /// <summary>
    /// Generates the grid and applies padding based on obstacle and character size.
    /// </summary>
    void CreateGrid()
    {
        grid = new GridNode[gridSizeX, gridSizeY];
        float offsetX = Mathf.Floor((gridCenterPosition.x - gridWorldSize.x / 2f) / nodeDiameter) * nodeDiameter;
        float offsetY = Mathf.Floor((gridCenterPosition.y - gridWorldSize.y / 2f) / nodeDiameter) * nodeDiameter;
        Vector3 worldBottomLeft = new Vector3(offsetX, offsetY, 0f);

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius)
                                                      + Vector3.up * (y * nodeDiameter + nodeRadius);
                bool walkable = !Physics2D.OverlapCircle(worldPoint, nodeRadius, unwalkableMask);
                grid[x, y] = new GridNode(walkable, worldPoint, x, y);
            }
        }

        if (obstaclePadding > 0)
        {
            List<GridNode> nodesToBlock = new();

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    if (!grid[x, y].walkable)
                    {
                        for (int dx = -obstaclePadding; dx <= obstaclePadding; dx++)
                        {
                            for (int dy = -obstaclePadding; dy <= obstaclePadding; dy++)
                            {
                                int checkX = x + dx;
                                int checkY = y + dy;

                                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                                {
                                    GridNode node = grid[checkX, checkY];
                                    if (node.walkable && !nodesToBlock.Contains(node))
                                        nodesToBlock.Add(node);
                                }
                            }
                        }
                    }
                }
            }

            foreach (var node in nodesToBlock)
                node.walkable = false;
        }

        ApplyCharacterPadding();
    }

    /// <summary>
    /// Converts a world position to its corresponding grid node.
    /// </summary>
    public GridNode NodeFromWorldPoint(Vector3 worldPosition)
    {
        Vector3 localPosition = worldPosition - gridCenterPosition;

        float percentX = Mathf.Clamp01((localPosition.x + gridWorldSize.x / 2) / gridWorldSize.x);
        float percentY = Mathf.Clamp01((localPosition.y + gridWorldSize.y / 2) / gridWorldSize.y);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    /// <summary>
    /// Clears cost and parent info on all grid nodes (used before pathfinding).
    /// </summary>
    public void ClearPathData()
    {
        foreach (var node in grid)
        {
            node.gCost = 0;
            node.hCost = 0;
            node.parent = null;
        }
    }

    /// <summary>
    /// Returns valid neighbors of a node, filtering out diagonals blocked by adjacent walls.
    /// </summary>
    public List<GridNode> GetNeighbours(GridNode node)
    {
        List<GridNode> neighbours = new();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    GridNode neighbour = grid[checkX, checkY];

                    if (x != 0 && y != 0)
                    {
                        GridNode node1 = grid[node.gridX + x, node.gridY];
                        GridNode node2 = grid[node.gridX, node.gridY + y];
                        if (!node1.walkable || !node2.walkable)
                            continue;
                    }

                    neighbours.Add(neighbour);
                }
            }
        }

        return neighbours;
    }

    public GridNode GetNode(int x, int y)
    {
        if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY)
        {
            return grid[x, y];
        }
        return null;
    }

    /// <summary>
    /// Checks if a diagonal move is valid (i.e., both adjacent sides are walkable).
    /// </summary>
    public bool IsDiagonalMoveValid(GridNode current, GridNode neighbour)
    {
        int dx = neighbour.gridX - current.gridX;
        int dy = neighbour.gridY - current.gridY;

        if (Mathf.Abs(dx) == 1 && Mathf.Abs(dy) == 1)
        {
            GridNode node1 = grid[current.gridX + dx, current.gridY];
            GridNode node2 = grid[current.gridX, current.gridY + dy];

            if (!node1.walkable || !node2.walkable)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Expands obstacle area to account for character size.
    /// </summary>
    void ApplyCharacterPadding()
    {
        int paddingX = Mathf.FloorToInt(characterSize.x / nodeDiameter / 2f);
        int paddingY = Mathf.FloorToInt(characterSize.y / nodeDiameter / 2f);

        List<GridNode> newUnwalkables = new();

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (!grid[x, y].walkable)
                {
                    for (int dx = -paddingX; dx <= paddingX; dx++)
                    {
                        for (int dy = -paddingY; dy <= paddingY; dy++)
                        {
                            int checkX = x + dx;
                            int checkY = y + dy;

                            if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                            {
                                GridNode node = grid[checkX, checkY];
                                if (node.walkable && !newUnwalkables.Contains(node))
                                {
                                    newUnwalkables.Add(node);
                                }
                            }
                        }
                    }
                }
            }
        }

        foreach (var node in newUnwalkables)
            node.walkable = false;
    }

    /// <summary>
    /// Called when the follower changes direction — refreshes the grid.
    /// </summary>
    private void OnChangeSide()
    {
        StartCoroutine(UpdatGrid());
    }

    public List<GridNode> GetAllNodes()
    {
        List<GridNode> allNodes = new List<GridNode>();
        foreach (GridNode node in grid)
        {
            allNodes.Add(node);
        }
        return allNodes;
    }


    /// <summary>
    /// Draws debug Gizmos for grid layout and path visualization.
    /// </summary>
    void OnDrawGizmos()
    {
        if (!drawGizmos)
        {
            return;
        }
        if (followerPathAgent == null)
        {
            return;
        }
        Gizmos.color = new Color(1f, 1f, 1f, 1f);

        Gizmos.DrawWireCube(transform.position, characterSize);

        float gizmoNodeDiameter = nodeRadius * 2f;



        Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
        if (grid != null && drawGizmos)
        {
            foreach (var node in grid)
            {
                // Default color with 0.4 alpha
                Gizmos.color = node.walkable ? new Color(1f, 1f, 1f, 0.4f) : new Color(1f, 0f, 0f, 0.4f);

                if (path != null && path.Contains(node))
                    Gizmos.color = new Color(0f, 1f, 1f, 0.4f); // Cyan with alpha 0.4

                Gizmos.DrawCube(node.worldPosition, Vector3.one * (gizmoNodeDiameter - 0.05f));
            }
        }
        else if (drawGizmos)
        {
            // Grid is not initialized yet — show placeholder cubes
            int gridSizeXPreview = Mathf.RoundToInt(gridWorldSize.x / gizmoNodeDiameter);
            int gridSizeYPreview = Mathf.RoundToInt(gridWorldSize.y / gizmoNodeDiameter);
            Vector3 center = transform.position + (followerPathAgent?.LastHorizontalDirection == HorizontalDirection.Right
                ? (Vector3)gridOffset
                : -(Vector3)gridOffset);

            Vector3 worldBottomLeft = center - new Vector3(gridWorldSize.x, gridWorldSize.y) / 2f;

            for (int x = 0; x < gridSizeXPreview; x++)
            {
                for (int y = 0; y < gridSizeYPreview; y++)
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * gizmoNodeDiameter + nodeRadius)
                                                      + Vector3.up * (y * gizmoNodeDiameter + nodeRadius);

                    Gizmos.color = new Color(1f, 1f, 1f, 0.4f); // white with 20% opacity
                    Gizmos.DrawCube(worldPoint, Vector3.one * (gizmoNodeDiameter - 0.05f));

                }
            }
        }

    }
}
