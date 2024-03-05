﻿using System.Collections.Generic;
using System.Linq;
using GruntzUnityverse.Actorz;
using GruntzUnityverse.Actorz.BehaviourManagement;
using GruntzUnityverse.Core;
using GruntzUnityverse.Utils.Extensionz;
using UnityEngine;

namespace GruntzUnityverse.Pathfinding {
/// <summary>
/// Custom representation of a node used for pathfinding.
/// </summary>
public class Node : MonoBehaviour {
	/// <summary>
	/// The location of this NodeV2 in 2D space.
	/// </summary>
	public Vector2Int location2D;

	/// <summary>
	/// The SpriteRenderer of this NodeV2.
	/// </summary>
	protected SpriteRenderer spriteRenderer;

	/// <summary>
	/// The collider used for checking interactions with this NodeV2.
	/// </summary>
	public CircleCollider2D circleCollider2D;

	public Grunt gruntOnNode;

	// --------------------------------------------------
	// Pathfinding
	// --------------------------------------------------

	#region Pathfinding
	/// <summary>
	/// The parent NodeV2 of this NodeV2, used for retracing paths.
	/// </summary>
	[Header("Pathfinding")]
	public Node parent;

	/// <summary>
	/// Cost from start to this node.
	/// </summary>
	public int g;

	/// <summary>
	/// Heuristic (cost from this NodeV2 to end).
	/// </summary>
	public int h;

	/// <summary>
	/// Total cost (g + h).
	/// </summary>
	public int F => g + h;

	/// <summary>
	/// The neighbours of this NodeV2.
	/// </summary>  
	public List<Node> neighbours;

	/// <summary>
	/// The diagonal neighbours of this NodeV2.
	/// </summary>
	public List<Node> diagonalNeighbours;

	/// <summary>
	/// The orthogonal neighbours of this NodeV2.
	/// </summary>
	public List<Node> orthogonalNeighbours;

	/// <summary>
	/// Serializable representation of the neighbours of this NodeV2.
	/// </summary>
	public NeighbourSet neighbourSet;
	#endregion

	// --------------------------------------------------
	// Flagz
	// --------------------------------------------------

	#region Flagz
	/// <summary>
	/// If true, this NodeV2 is occupied by an actor.
	/// </summary>
	[Header("Flagz")]
	public bool IsOccupied => GameManager.Instance.allGruntz.Any(gr => gr.node == this);

	/// <summary>
	/// If true, this NodeV2 is reserved by an actor for its next move.
	/// </summary>
	public bool isReserved;

	/// <summary>
	/// If true, this NodeV2 is blocked by a wall or other obstacle.
	/// </summary>
	public bool isBlocked;

	public bool isWater;
	public bool isFire;
	public bool isVoid;

	/// <summary>
	/// If true, this NodeV2 is walkable. This combines the 'occupied' and 'blocked' flags.
	/// </summary>
	public bool IsWalkable {
		get => !IsOccupied && !isBlocked && !isWater && !isFire && !isVoid && !isReserved;
	}

	/// <summary>
	/// If true, this NodeV2 is walkable with a Toob equipped.
	/// </summary>
	public bool IsWalkableWithToob {
		get => !IsOccupied && !isBlocked && !isFire && !isVoid && !isReserved;
	}

	/// <summary>
	/// If true, this NodeV2 is walkable with Wingz equipped.
	/// </summary>
	public bool IsWalkableWithWingz {
		get => !IsOccupied && !isReserved;
	}

	/// <summary>
	/// If true, this NodeV2 is a hard corner, meaning that it prevents
	/// diagonal movement to its neighbours.
	/// </summary>
	public bool hardCorner;
	#endregion

	public TileType tileType;

	/// <summary>
	/// Sets up this NodeV2 with the given parameters.
	/// </summary>
	/// <param name="position">The position of this NodeV2 in the grid. </param>
	/// <param name="tileName">The name of the tile this NodeV2 is based on.</param>
	/// <param name="nodes">The list of all nodes in the grid.</param>
	public void SetupNode(Vector3Int position, string tileName, List<Node> nodes) {
		transform.position = position - Vector3Int.one / 2;
		location2D = new Vector2Int(position.x, position.y);
		AssignTileType(tileName);
		nodes.Add(this);
	}

	/// <summary>
	/// Assigns the tile type of this NodeV2 based on the given tile name.
	/// </summary>
	/// <param name="tileName"></param>
	private void AssignTileType(string tileName) {
		if (tileName.Contains("Ground")) {
			tileType = TileType.Ground;
		} else if (tileName.Contains("Collision")) {
			tileType = TileType.Collision;
			isBlocked = true;
		} else if (tileName.Contains("Water")) {
			tileType = TileType.Water;
			isWater = true;
		} else if (tileName.Contains("Fire")) {
			tileType = TileType.Fire;
			isFire = true;
		} else if (tileName.Contains("Void")) {
			tileType = TileType.Void;
			isVoid = true;
		} else {
			tileType = TileType.Unknown;
		}
	}

	/// <summary>
	/// Returns whether this NodeV2 can be reached diagonally from the given NodeV2.
	/// </summary>
	/// <param name="toCheck"></param>
	/// <returns></returns>
	public bool CanReachDiagonally(Node toCheck) {
		return !diagonalNeighbours.Contains(toCheck)
			|| !neighbours.Any(n => n.hardCorner && toCheck.neighbours.Contains(n));
	}

	/// <summary>
	/// Assigns the neighbours of this NodeV2 based from the given list of nodes.
	/// </summary>
	/// <param name="nodes"></param>
	public void AssignNeighbours(List<Node> nodes) {
		neighbourSet = new NeighbourSet {
			up = nodes.FirstOrDefault(n => n.location2D.Equals(location2D.Up())),
			upRight = nodes.FirstOrDefault(n => n.location2D.Equals(location2D.UpRight())),
			right = nodes.FirstOrDefault(n => n.location2D.Equals(location2D.Right())),
			downRight = nodes.FirstOrDefault(n => n.location2D.Equals(location2D.DownRight())),
			down = nodes.FirstOrDefault(n => n.location2D.Equals(location2D.Down())),
			downLeft = nodes.FirstOrDefault(n => n.location2D.Equals(location2D.DownLeft())),
			left = nodes.FirstOrDefault(n => n.location2D.Equals(location2D.Left())),
			upLeft = nodes.FirstOrDefault(n => n.location2D.Equals(location2D.UpLeft())),
		};

		neighbours = neighbourSet.AsList();

		diagonalNeighbours = new List<Node> {
			neighbourSet.upRight,
			neighbourSet.downRight,
			neighbourSet.downLeft,
			neighbourSet.upLeft,
		};

		orthogonalNeighbours = new List<Node> {
			neighbourSet.up,
			neighbourSet.right,
			neighbourSet.down,
			neighbourSet.left,
		};
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.TryGetComponent(out Grunt grunt)) {
			grunt.node.isReserved = false;
			grunt.node = this;
			this.isReserved = false;
			grunt.transform.position = this.transform.position;

			if (grunt.attackTarget != null) {
				grunt.HandleActionCommand(grunt.attackTarget.node, Intent.ToAttack);

				return;
			}

			grunt.EvaluateState();

			if (gruntOnNode != null && gruntOnNode != grunt) {
				gruntOnNode.Die(AnimationManager.Instance.squashDeathAnimation);
			}

			gruntOnNode = grunt;
			grunt.spriteRenderer.sortingOrder = 10;
		}

		// if (other.TryGetComponent(out RollingBall ball)) {
		// 	
		// }
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (other.TryGetComponent(out Grunt grunt)) {
			grunt.spriteRenderer.sortingOrder = 12;
		}
	}
}
}
