﻿using System.Collections.Generic;
using System.Linq;
using GruntzUnityverse.Enumz;
using GruntzUnityverse.Managerz;
using GruntzUnityverse.Pathfinding;
using GruntzUnityverse.Utility;
using UnityEngine;

namespace GruntzUnityverse.Actorz {
  /// <summary>
  /// The component describing the movement of a Grunt.
  /// </summary>
  public class Navigator : MonoBehaviour {
    #region Nodes & Locations
    public Vector2Int startingLocation;
    public Node startingNode;
    public Vector2Int ownLocation;
    public Node ownNode;
    public Vector2Int targetLocation;
    public Node targetNode;
    #endregion

    public bool haveMoveCommand;
    private bool _doFindPath;

    #region Pathfinding
    public Node pathStart;
    public Node pathEnd;
    public List<Node> path;
    #endregion

    #region Flags
    public bool isMoving;
    public bool isMoveForced;
    public bool movesDiagonally;
    #endregion

    public Vector3 moveVector;
    public Direction facingDirection;
    private const float StepThreshold = 0.1f;


    private void Start() {
      facingDirection = Direction.South;
      startingLocation = Vector2Int.RoundToInt(transform.position);
      startingNode = LevelManager.Instance.NodeAt(startingLocation);
      ownLocation = startingLocation;
      ownNode = startingNode;
      targetLocation = ownLocation;
      targetNode = ownNode;
      _doFindPath = true;
    }

    private void Update() {
      ownNode = LevelManager.Instance.NodeAt(ownLocation);

      SetDiagonalFlag();
    }

    /// <summary>
    /// Moves the <see cref="Grunt"/> towards its current target.
    /// </summary>
    public void MoveTowardsTarget() {
      pathStart = LevelManager.Instance.NodeAt(ownLocation);
      pathEnd = LevelManager.Instance.NodeAt(targetLocation);

      // This way path is only calculated only when it's needed
      if (!isMoving) {
        path = Pathfinder.PathBetween(pathStart, pathEnd, isMoveForced, movesDiagonally);
      }

      if (path is null) {
        return;
      }

      if (path.Count <= 1) {
        return;
      }

      Vector3 nextPosition = LocationAsPosition(path[1].location);

      if (Vector2.Distance(nextPosition, transform.position) > 0.1f) {
        isMoving = true;
        moveVector = (nextPosition - gameObject.transform.position).normalized;

        // Todo: Swap 0.6f to Grunt speed
        transform.position += moveVector * (Time.deltaTime / 0.6f);

        SetFacingDirection(moveVector);

        if (isMoveForced) {
          Grunt deadGrunt = LevelManager.Instance.allGruntz.FirstOrDefault(grunt => grunt.AtNode(targetNode));

          if (deadGrunt is not null) {
            StartCoroutine(deadGrunt.Death("Squash"));
          }

          isMoveForced = false;
        }
      } else {
        isMoving = false;

        ownLocation = path[1].location;

        path.RemoveAt(1);
      }
    }

    public void MoveTowardsTargetNode() {
      // This way path is only calculated only when it's needed
      if ((targetNode.IsOccupied() || targetNode.IsUnavailable()) && targetNode != ownNode) {
        ConditionalLogger.Log("Target node is occupied or unavailable, searching for new target.");
        SetTargetBesideNode(targetNode);
      }

      if (_doFindPath) {
        path = Pathfinder.PathBetween(ownNode, targetNode, isMoveForced, movesDiagonally);
      }

      // There's no path to target or Grunt has reached target
      if ((path is null) || (path.Count <= 1)) {
        isMoving = false;
        haveMoveCommand = false;
        isMoveForced = false;
        ConditionalLogger.Log("There's no path to target or Grunt has reached target.");

        return;
      }

      Vector3 nextPosition = LocationAsPosition(path[1].location);

      // Continuing only if the Grunt is not close enough to the target
      if (Vector2.Distance(nextPosition, transform.position) > StepThreshold) {
        MoveSomeTowards(nextPosition);
        SetFacingDirection(moveVector);
        HandleForcedMovement(isMoveForced);
      } else {
        FinishStep();
      }
    }

    private void FinishStep() {
      ownLocation = path[1].location;
      _doFindPath = true;

      path.RemoveAt(1);
    }

    private void MoveSomeTowards(Vector3 nextPosition) {
      _doFindPath = false;
      moveVector = (nextPosition - gameObject.transform.position).normalized;
      // Todo: Swap 0.6f to Grunt's moveSpeed
      gameObject.transform.position += moveVector * (Time.deltaTime / 0.6f);
    }

    private void HandleForcedMovement(bool isForced) {
      if (!isForced) {
        return;
      }

      Grunt deathMarkedGrunt =
        LevelManager.Instance.allGruntz.FirstOrDefault(grunt => grunt.AtNode(targetNode));

      // Killing the target if the Grunt was forced to move (e.g. by an Arrow or by teleporting)
      if (deathMarkedGrunt is not null) {
        StartCoroutine(deathMarkedGrunt.Death("Squash"));
      }

      isMoveForced = false;
    }

    public void SetTargetBesideNode(Node node) {
      List<Node> freeNeighbours = node.Neighbours.FindAll(node1 => !node1.IsUnavailable() && !node1.IsOccupied());

      // No path possible
      if (freeNeighbours.Count == 0) {
        path.Clear();
        haveMoveCommand = false;
        isMoveForced = false;

        // Todo: Play line that says that the Grunt can't move
        return;
      }

      List<Node> shortestPath = Pathfinder.PathBetween(ownNode, freeNeighbours[0], isMoveForced, movesDiagonally);

      // Iterate over free neighbours to find shortest path
      foreach (Node neighbour in freeNeighbours) {
        List<Node> pathToNode = Pathfinder.PathBetween(ownNode, neighbour, isMoveForced, movesDiagonally);

        // Check if current path is shorter than current shortest path
        if (pathToNode.Count != 0 && pathToNode.Count < shortestPath.Count) {
          shortestPath = pathToNode;
        }
      }

      if (shortestPath.Count != 0) {
        targetNode = shortestPath.Last();
      }
    }

    private Vector3 LocationAsPosition(Vector2Int location) {
      return new Vector3(location.x, location.y, transform.position.z);
    }

    public void SetFacingDirection(Vector3 facingVector) {
      Vector2Int directionVector = Vector2Int.RoundToInt(facingVector);

      facingDirection = directionVector switch {
        var vector when vector.Equals(Vector2Direction.north) => Direction.North,
        var vector when vector.Equals(Vector2Direction.northeast) => Direction.Northeast,
        var vector when vector.Equals(Vector2Direction.east) => Direction.East,
        var vector when vector.Equals(Vector2Direction.southeast) => Direction.Southeast,
        var vector when vector.Equals(Vector2Direction.south) => Direction.South,
        var vector when vector.Equals(Vector2Direction.southwest) => Direction.Southwest,
        var vector when vector.Equals(Vector2Direction.west) => Direction.West,
        var vector when vector.Equals(Vector2Direction.northwest) => Direction.Northwest,
        _ => facingDirection,
      };
    }

    private void SetDiagonalFlag() {
      movesDiagonally = facingDirection == Direction.Northeast
        || facingDirection == Direction.Southeast
        || facingDirection == Direction.Southwest
        || facingDirection == Direction.Northwest;
    }
  }
}
