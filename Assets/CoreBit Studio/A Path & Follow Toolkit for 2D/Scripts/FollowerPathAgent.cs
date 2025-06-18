using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// FollowerPathAgent handles movement along a given path of GridNodes,
/// triggering UnityEvents based on movement direction and state changes.
/// Supports directional events, movement state tracking, and integration with GridManager and Pathfinder.
/// </summary>
public class FollowerPathAgent : MonoBehaviour
{
    [SerializeField, Tooltip("Speed at which the object follows the path.")]
    private float moveSpeed = 3f;

    [SerializeField, Tooltip("Whether the agent is allowed to follow the path")]
    private bool canFollow = true;

    [Header("Movement Events")]
    [SerializeField, Tooltip("Invoked when movement starts.")]
    private UnityEvent onStartMove;

    [SerializeField, Tooltip("Invoked when movement ends.")]
    private UnityEvent onEndMove;

    [SerializeField, Tooltip("Invoked when moving to the left.")]
    private UnityEvent onMovedLeft;

    [SerializeField, Tooltip("Invoked when moving to the right.")]
    private UnityEvent onMovedRight;

    [SerializeField, Tooltip("Invoked when the horizontal direction changes.")]
    private UnityEvent onChangeHorizontalSide;

    [SerializeField, Tooltip("Invoked when moving up.")]
    private UnityEvent onMovedUp;

    [SerializeField, Tooltip("Invoked when moving down.")]
    private UnityEvent onMovedDown;

    [SerializeField, Tooltip("Invoked when the vertical direction changes.")]
    private UnityEvent onChangeVerticalSide;

    [SerializeField, HideInInspector]
    private Pathfinder pathfinder; // Pathfinder Reference

    private const float reachThreshold = 0.25f; // Distance threshold to consider a node as reached
    private const float directionThreshold = 0.05f; // Minimal directional change to consider a new direction

    private int currentIndex = 0; // Current index in the path

    private bool isFollowing = false; // Whether the agent is currently following a path
    private bool isMoving = false; // Whether the agent is currently moving

    private Vector3 lastPosition; // Last recorded position for movement comparison
    private List<GridNode> path; // Current path to follow
    private HorizontalDirection lastHorizontal = HorizontalDirection.Right; // Last known horizontal direction
    private VerticalDirection lastVertical = VerticalDirection.Up; // Last known vertical direction
 



    // Public accessors
    public bool CanFollow { get { return canFollow; } set { canFollow = value; } }
    public List<GridNode> Path { get { return path; } set { path = value; } }
    public UnityEvent OnStartMove { get { return onStartMove; } set { onStartMove = value; } }
    public UnityEvent OnEndMove { get { return onEndMove; } set { onEndMove = value; } }
    public UnityEvent OnMovedLeft { get { return onMovedLeft; } set { onMovedLeft = value; } }
    public UnityEvent OnMovedRight { get { return onMovedRight; } set { onMovedRight = value; } }
    public UnityEvent OnMovedUp { get { return onMovedUp; } set { onMovedUp = value; } }
    public UnityEvent OnMovedDown { get { return onMovedDown; } set { onMovedDown = value; } }
    public UnityEvent OnChangeHorizontalSide { get { return onChangeHorizontalSide; } set { onChangeHorizontalSide = value; } }
    public UnityEvent OnChangeVerticalSide { get { return onChangeVerticalSide; } set { onChangeVerticalSide = value; } }
    public HorizontalDirection LastHorizontalDirection { get { return lastHorizontal; } set { lastHorizontal = value; } }
    public VerticalDirection LastVerticalDirection { get { return lastVertical; } set { lastVertical = value; } }

   
    private void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        if (!canFollow)
        {
            return;
        }
        Vector3 direction = (transform.position - lastPosition).normalized;
        CheckMovementState();
        HandleHorizontalDirection(direction);
        HandleVerticalDirection(direction);

        lastPosition = transform.position;
        if (!isFollowing || path == null || currentIndex >= path.Count)
            return;


        Vector3 targetPos = path[currentIndex].worldPosition;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        float currentReachThreshold;
        if (pathfinder.EnablePathSmoothing)
            currentReachThreshold = 1;
        else
            currentReachThreshold = reachThreshold;

        if (Vector3.Distance(transform.position, targetPos) < currentReachThreshold)
        {
            currentIndex++;

            if (currentIndex >= path.Count)
            {
                isFollowing = false;
            }
        }
    }

    /// <summary>
    /// Begins following a new path.
    /// If Pathfinder.enablePathSmoothing is true, starts exactly at the closest node.
    /// Otherwise, starts a few nodes ahead (lookAhead).
    /// </summary>
    /// <param name="newPath">The new path to follow.</param>
    public void FollowPath(List<GridNode> newPath)
    {
        if (newPath == null || newPath.Count == 0 || IsSamePath(newPath))
        {
            isFollowing = false;
            return;
        }

        path = newPath;

        int closestIndex = FindClosestIndexOnPath(transform.position, path);
        int lookAhead = 2;

        if (pathfinder != null && pathfinder.EnablePathSmoothing)
        {
            currentIndex = Mathf.Clamp(closestIndex, 0, path.Count - 1);
        }
        else
        {
            currentIndex = Mathf.Min(closestIndex + lookAhead, path.Count - 1);
        }

        isFollowing = true;
    }

    /// <summary>
    /// Finds the index of the closest node to the given position on the path.
    /// </summary>
    private int FindClosestIndexOnPath(Vector3 position, List<GridNode> path)
    {
        int closestIndex = 0;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < path.Count; i++)
        {
            float dist = Vector3.Distance(position, path[i].worldPosition);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    /// <summary>
    /// Checks and triggers start/end movement events based on position change.
    /// </summary>
    private void CheckMovementState()
    {
        if (lastPosition == transform.position)
        {
            if (isMoving)
                onEndMove?.Invoke();
            isMoving = false;
        }
        else
        {
            if (!isMoving)
                onStartMove?.Invoke();
            isMoving = true;
        }
    }

    /// <summary>
    /// Handles horizontal movement direction changes and triggers corresponding events.
    /// </summary>
    private void HandleHorizontalDirection(Vector3 direction)
    {
        if (Mathf.Abs(direction.x) < directionThreshold)
            return;

        if (direction.x > 0 && lastHorizontal != HorizontalDirection.Right)
        {
            lastHorizontal = HorizontalDirection.Right;
            onMovedRight?.Invoke();
            onChangeHorizontalSide?.Invoke();
        }
        else if (direction.x < 0 && lastHorizontal != HorizontalDirection.Left)
        {
            lastHorizontal = HorizontalDirection.Left;
            onMovedLeft?.Invoke();
            onChangeHorizontalSide?.Invoke();
        }
    }

    /// <summary>
    /// Handles vertical movement direction changes and triggers corresponding events.
    /// </summary>
    private void HandleVerticalDirection(Vector3 direction)
    {
        if (Mathf.Abs(direction.y) < directionThreshold)
            return;

        if (direction.y > 0 && lastVertical != VerticalDirection.Up)
        {
            lastVertical = VerticalDirection.Up;
            onMovedUp?.Invoke();
            onChangeVerticalSide?.Invoke();
        }
        else if (direction.y < 0 && lastVertical != VerticalDirection.Down)
        {
            lastVertical = VerticalDirection.Down;
            onMovedDown?.Invoke();
            onChangeVerticalSide?.Invoke();
        }
    }

    /// <summary>
    /// Checks whether the new path is the same as the current one.
    /// </summary>
    private bool IsSamePath(List<GridNode> newPath)
    {
        if (path == null || newPath == null || path.Count != newPath.Count)
            return false;

        for (int i = 0; i < path.Count; i++)
        {
            if (path[i] != newPath[i])
                return false;
        }

        return true;
    }

#if UNITY_EDITOR
    // Utility for creating the GridManager and Pathfinder components during development
    [ContextMenu("Create Grid Manager")]
    public void CreateGridManager()
    {
        if (GetComponentInChildren<GridManager>() != null)
        {
            Debug.LogWarning("GridManager and Pathfinder already exist.");
            return;
        }

        GameObject gridObj = new GameObject("GridManager");
        gridObj.transform.SetParent(transform);
        gridObj.transform.localPosition = Vector3.zero;

        GridManager gridManager = gridObj.AddComponent<GridManager>();
        Pathfinder pathfinder = gridObj.AddComponent<Pathfinder>();

        gridManager.Pathfinder = pathfinder;
        gridManager.FollowerPathAgent = this;
        pathfinder.GridManager = gridManager;
        this.pathfinder = pathfinder;

        Debug.Log("GridManager and Pathfinder created successfully!");
    }
#endif
}

// Enum representing possible horizontal directions
public enum HorizontalDirection { None, Left, Right }

// Enum representing possible vertical directions
public enum VerticalDirection { None, Up, Down }
