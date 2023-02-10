﻿using System.Collections.Generic;
using System.Linq;
using GruntzUnityverse.Enumz;
using GruntzUnityverse.Managerz;
using GruntzUnityverse.Pathfinding;
using GruntzUnityverse.Utility;
using UnityEngine;

namespace GruntzUnityverse.Actorz {
  public class NavComponent : MonoBehaviour {
    [field: SerializeField] public Vector2Int OwnLocation { get; set; }
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
    [field: SerializeField] public bool ProhibitPathRecalculation { get; set; }
    [field: SerializeField] public Vector3 MoveVector { get; set; }
    [field: SerializeField] public CompassDirection FacingDirection { get; set; }

    private void Start() {
      FacingDirection = CompassDirection.South;
      OwnLocation = Vector2Int.FloorToInt(transform.position);
      TargetLocation = OwnLocation;
    }

    public void MoveTowardsTarget() {
      PathStart = LevelManager.Instance.nodeList.First(node => node.GridLocation.Equals(OwnLocation));

      PathEnd = LevelManager.Instance.nodeList.First(
        node => node.GridLocation.Equals(TargetLocation) && !node.isBlocked
      );

      if (!ProhibitPathRecalculation) {
        Debug.Log("Changing path");
        Path = Pathfinder.PathBetween(PathStart, PathEnd);
      }

      if (Path == null) {
        return;
      }

      if (Path.Count <= 1) {
        return;
      }

      PreviousLocation = Path[0]
        .GridLocation;

      Vector3 nextPosition = LocationAsPosition(
        Path[1]
          .GridLocation
      );

      // Todo: Handle here disallowing move commands while moving
      if (Vector2.Distance(nextPosition, gameObject.transform.position) > 0.1f) {
        IsMoving = true;
        ProhibitPathRecalculation = true;

        MoveVector = (nextPosition - gameObject.transform.position).normalized;

        DetermineFacingDirection(MoveVector);

        // 0.3f is hardcoded only for ease of testing, remove after not needed
        transform.position += MoveVector * (Time.deltaTime / 0.3f);
      } else {
        IsMoving = false;
        ProhibitPathRecalculation = false;

        OwnLocation = Path[1]
          .GridLocation;

        Path.RemoveAt(1);
      }
    }

    private Vector3 LocationAsPosition(Vector2Int location) {
      return new Vector3(location.x + 0.5f, location.y + 0.5f, -15f);
    }

    private void DetermineFacingDirection(Vector3 moveVector) {
      Vector2Int directionVector = Vector2Int.RoundToInt(moveVector);

      FacingDirection = directionVector switch {
        var vector when vector.Equals(Vector2IntC.North) => CompassDirection.North,
        var vector when vector.Equals(Vector2IntC.NorthEast) => CompassDirection.NorthEast,
        var vector when vector.Equals(Vector2IntC.East) => CompassDirection.East,
        var vector when vector.Equals(Vector2IntC.SouthEast) => CompassDirection.SouthEast,
        var vector when vector.Equals(Vector2IntC.South) => CompassDirection.South,
        var vector when vector.Equals(Vector2IntC.SouthWest) => CompassDirection.SouthWest,
        var vector when vector.Equals(Vector2IntC.West) => CompassDirection.West,
        var vector when vector.Equals(Vector2IntC.NorthWest) => CompassDirection.NorthWest,
        var _ => FacingDirection,
      };
    }
  }
}
