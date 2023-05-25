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
    [field: SerializeField] public Vector2Int OwnLocation { get; set; }
    [field: SerializeField] public Node OwnNode { get; set; }
    [field: SerializeField] public Vector2Int PreviousLocation { get; set; }
    [field: SerializeField] public Vector2Int TargetLocation { get; set; }
    [field: SerializeField] public Vector2Int SavedTargetLocation { get; set; }
    [field: SerializeField] public bool HaveSavedTarget { get; set; }


    #region Pathfinding

    [field: SerializeField] public Node PathStart { get; set; }
    [field: SerializeField] public Node PathEnd { get; set; }
    [field: SerializeField] public List<Node> Path { get; set; }

    #endregion


    [field: SerializeField] public bool IsMoving { get; set; }
    [field: SerializeField] public bool IsMovementForced { get; set; }
    [field: SerializeField] public Vector3 MoveVector { get; set; }
    [field: SerializeField] public CompassDirection FacingDirection { get; set; }


    private void Start() {
      FacingDirection = CompassDirection.South;
      OwnLocation = Vector2Int.FloorToInt(transform.position);
      OwnNode = LevelManager.Instance.NodeAt(OwnLocation);
      TargetLocation = OwnLocation;
    }

    private void Update() {
      // Todo: Maybe not calculate it every frame?
      OwnNode = LevelManager.Instance.NodeAt(OwnLocation);
    }

    /// <summary>
    /// Moves the <see cref="Grunt"/> towards its current target.
    /// </summary>
    public void MoveTowardsTarget() {
      PathStart = LevelManager.Instance.NodeAt(OwnLocation);
      PathEnd = LevelManager.Instance.NodeAt(TargetLocation);

      if (!IsMoving) {
        Path = Pathfinder.PathBetween(PathStart, PathEnd, IsMovementForced);
      }

      if (Path == null) {
        return;
      }

      if (Path.Count <= 1) {
        return;
      }

      PreviousLocation = Path[0].OwnLocation;

      Vector3 nextPosition = LocationAsPosition(Path[1].OwnLocation);

      // Todo: Handle here disallowing move commands while moving
      if (Vector2.Distance(nextPosition, gameObject.transform.position) > 0.1f) {
        IsMoving = true;
        MoveVector = (nextPosition - gameObject.transform.position).normalized;

        // 0.3f is hardcoded only for ease of testing, remove after not needed
        transform.position += MoveVector * (Time.deltaTime / 0.6f);

        DetermineFacingDirection(MoveVector);

        if (IsMovementForced) {
          StartCoroutine(
            LevelManager.Instance.AllGruntz.First(grunt => grunt.IsOnLocation(TargetLocation))
              .Die(DeathType.GetSquashed)
          );

          IsMovementForced = false;
        }
      } else {
        IsMoving = false;

        OwnLocation = Path[1].OwnLocation;

        Path.RemoveAt(1);
      }
    }

    public void SetTargetBeside(Node node) {
      List<Node> freeNeighbours = node.Neighbours.FindAll(node1 => !node1.isBlocked);

      // No path possible
      if (freeNeighbours.Count == 0) {
        // Todo: Play line that says that the Grunt can't move
        return;
      }

      List<Node> shortestPath = Pathfinder.PathBetween(OwnNode, freeNeighbours[0], IsMovementForced);

      bool hasShortestPathPossible = false;

      // Iterate over neighbours to find shortest path
      foreach (Node neighbour in freeNeighbours) {
        if (shortestPath.Count == 1) {
          // There is no possible shorter way, set target to shortest path
          TargetLocation = shortestPath[0].OwnLocation;

          hasShortestPathPossible = true;

          break;
        }

        List<Node> pathToNode = Pathfinder.PathBetween(OwnNode, neighbour, IsMovementForced);

        // Check if current path is shorter than current shortest path
        if (pathToNode.Count != 0 && pathToNode.Count < shortestPath.Count) {
          shortestPath = pathToNode;
        }
      }

      if (!hasShortestPathPossible) {
        TargetLocation = shortestPath.Last().OwnLocation;
      }
    }

    private Vector3 LocationAsPosition(Vector2Int location) {
      return new Vector3(location.x + 0.5f, location.y + 0.5f, -15f);
    }

    public void DetermineFacingDirection(Vector3 moveVector) {
      Vector2Int directionVector = Vector2Int.RoundToInt(moveVector);

      FacingDirection = directionVector switch {
        var vector when vector.Equals(Vector2IntCustom.North()) => CompassDirection.North,
        var vector when vector.Equals(Vector2IntCustom.NorthEast()) => CompassDirection.NorthEast,
        var vector when vector.Equals(Vector2IntCustom.East()) => CompassDirection.East,
        var vector when vector.Equals(Vector2IntCustom.SouthEast()) => CompassDirection.SouthEast,
        var vector when vector.Equals(Vector2IntCustom.South()) => CompassDirection.South,
        var vector when vector.Equals(Vector2IntCustom.SouthWest()) => CompassDirection.SouthWest,
        var vector when vector.Equals(Vector2IntCustom.West()) => CompassDirection.West,
        var vector when vector.Equals(Vector2IntCustom.NorthWest()) => CompassDirection.NorthWest,
        _ => FacingDirection,
      };
    }
  }
}
