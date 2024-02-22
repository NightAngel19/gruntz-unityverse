﻿using System.Linq;
using GruntzUnityverse.Pathfinding;
using GruntzUnityverse.Utils.Extensionz;
using UnityEngine;

namespace GruntzUnityverse.Core {
public class Selector : MonoBehaviour {
	public Vector2Int location2D;
	public Node node;
	public Camera mainCamera;

	private void Start() {
		mainCamera = Camera.main;
	}

	private void Update() {
		transform.position = mainCamera.ScreenToWorldPoint(Input.mousePosition).RoundedToInt(z: 15f);
		location2D = Vector2Int.RoundToInt(transform.position);
		node = Level.Instance.levelNodes.FirstOrDefault(n => n.location2D == location2D);
	}
}
}
