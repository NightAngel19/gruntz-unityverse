﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Animancer;
using Cysharp.Threading.Tasks;
using GruntzUnityverse.Actorz.BehaviourManagement;
using GruntzUnityverse.Actorz.Data;
using GruntzUnityverse.Actorz.UI;
using GruntzUnityverse.Animation;
using GruntzUnityverse.Core;
using GruntzUnityverse.DataPersistence;
using GruntzUnityverse.Editor.PropertyDrawers;
using GruntzUnityverse.Itemz.Base;
using GruntzUnityverse.Objectz;
using GruntzUnityverse.Objectz.Interactablez;
using GruntzUnityverse.Objectz.Interfacez;
using GruntzUnityverse.Objectz.Secretz;
using GruntzUnityverse.Pathfinding;
using GruntzUnityverse.UI;
using GruntzUnityverse.Utils.Extensionz;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace GruntzUnityverse.Actorz {
/// <summary>
/// The class representing a Grunt in the game.
/// </summary>
public class Grunt : MonoBehaviour, IDataPersistence, IAnimatable {
	/// <summary>
	/// The name of this Grunt.
	/// </summary>
	[Header("Statz")]
	public int gruntId;

	public string gruntName;

	public GruntColor gruntColor;

	public Statz statz;

	[Range(0, 5)]
	public float moveSpeed;

	[Header("State Handling")]
	public State state;

	public Intent intent;

	[Header("Flagz")]
	public bool selected;

	public bool waiting;

	public bool committed;

	public bool BetweenNodes => transform.position != node.transform.position;

	// --------------------------------------------------
	// Equipment
	// --------------------------------------------------

	#region Equipment
	/// <summary>
	/// The tool currently equipped by this Grunt.
	/// </summary>
	[Header("Equipment")]
	public EquippedTool equippedTool;

	/// <summary>
	/// The toy currently equipped by this Grunt.
	/// </summary>
	public EquippedToy equippedToy;

	/// <summary>
	/// The powerup currently active on this Grunt.
	/// </summary>
	public List<EquippedPowerup> equippedPowerupz;
	#endregion

	// --------------------------------------------------
	// Animation
	// --------------------------------------------------

	#region Animation
	/// <summary>
	/// The direction this Grunt is facing.
	/// </summary>
	[Header("Animation")]
	public Direction facingDirection;

	/// <summary>
	/// The animation pack this Grunt uses.
	/// </summary>
	public AnimationPack animationPack;

	// -------------------------
	// IAnimatable
	// -------------------------

	[field: SerializeField] public Animator Animator { get; set; }
	[field: SerializeField] public AnimancerComponent Animancer { get; set; }
	#endregion

	// -------------------------------------------------- //
	// Pathfinding
	// -------------------------------------------------- //

	#region Pathfinding
	/// <summary>
	/// The location of this Grunt in 2D space.
	/// </summary>
	[Header("Pathfinding")]
	public Vector2 Location2D => node.location2D;

	/// <summary>
	/// The node this Grunt is currently on.
	/// </summary>
	public Node node;

	/// <summary>
	/// The node the Grunt is moving towards.
	/// </summary>
	public Node travelGoal;

	/// <summary>
	/// The next node the Grunt will move to.
	/// </summary>
	public Node next;
	#endregion

	// --------------------------------------------------
	// Interaction
	// --------------------------------------------------

	#region Interaction
	/// <summary>
	/// The target the Grunt will try to interact with.
	/// </summary>
	[Header("Interaction")]
	public GridObject interactionTarget;

	/// <summary>
	/// The target the Grunt will try to attack.
	/// </summary>
	public Grunt attackTarget;
	#endregion

	// --------------------------------------------------
	// UI
	// --------------------------------------------------

	#region UI
	[Header("UI")]
	public GruntEntry gruntEntry;
	#endregion

	// --------------------------------------------------
	// Eventz
	// --------------------------------------------------

	#region Eventz
	[Header("Eventz")]
	[HideInNormalInspector]
	public UnityEvent onStaminaDrained;

	[HideInNormalInspector]
	public UnityEvent onStaminaRegenerated;

	[HideInNormalInspector]
	public UnityEvent onHit;

	public UnityEvent onDeath;
	#endregion

	// --------------------------------------------------
	// Componentz
	// --------------------------------------------------

	#region Componentz
	[Header("Componentz")]
	public SpriteRenderer spriteRenderer;

	public CircleCollider2D circleCollider2D;

	public GameObject selectionMarker;
	#endregion

	public Barz barz;

	// --------------------------------------------------
	// Lifecycle
	// --------------------------------------------------

	#region Lifecycle
	private void Awake() {
		spriteRenderer = GetComponent<SpriteRenderer>();
		circleCollider2D = GetComponent<CircleCollider2D>();
	}

	private void Start() {
		if (!gameObject.CompareTag("Dizgruntled")) {
			gruntId = GameManager.Instance.playerGruntz.ToList().IndexOf(this) + 1;

			gruntEntry = FindObjectsByType<GruntEntry>(FindObjectsSortMode.None)
				.First(entry => entry.EntryId == gruntId);

			gruntEntry.SetHealth(statz.health);
			gruntEntry.SetStamina(statz.stamina);

			Debug.Log(gruntEntry.name);

			if (equippedTool != null) {
				gruntEntry.SetTool(equippedTool.toolName.Replace(" ", ""));
			}

			if (equippedToy != null) {
				gruntEntry.SetToy(equippedToy.toyName.Replace(" ", ""));
			}
		}

		node = Level.Instance.levelNodes.First(n => n.location2D == Vector2Int.RoundToInt(transform.position));

		Animancer.Play(AnimationPack.GetRandomClip(facingDirection, animationPack.idle));
	}

	private void Update() {
		if (state == State.Idle) {
			Animancer.Play(AnimationPack.GetRandomClip(facingDirection, animationPack.idle));
		} else if (state == State.Moving || BetweenNodes) {
			ChangePosition();
		} else if (waiting) {
			Animancer.Play(AnimationPack.GetRandomClip(facingDirection, animationPack.hostileIdle));
		}
	}
	#endregion

	// --------------------------------------------------
	// Input Actions
	// --------------------------------------------------

	#region Input Actions
	#region Selection
	/// <summary>
	/// Try to select the Grunt if he is under the selector.
	/// </summary>
	private void OnSelect() {
		if (GameManager.Instance.selector.location2D == node.location2D) {
			Select();
		} else {
			Deselect();
		}
	}

	/// <summary>
	/// Try to select the Grunt if he is under the selector in addition to any already selected Grunt(z).
	/// If he is already selected, deselect him.
	/// </summary>
	private void OnAdditionalSelect() {
		if (GameManager.Instance.selector.location2D != Location2D) {
			return;
		}

		selected = !selected;
		selectionMarker.SetActive(!selectionMarker.activeSelf);
	}

	/// <summary>
	/// Try selecting the Grunt even if it's not under the selector.
	/// </summary>
	private void OnSelectAll() {
		Select();
	}
	#endregion

	/// <summary>
	/// Set the Grunt's travel goal to the selector's location, then evaluate the Grunt's state if possible.
	/// </summary>
	private void OnMove() {
		if (!selected) {
			return;
		}

		travelGoal = Level.Instance.levelNodes
			.First(n => n == GameManager.Instance.selector.node);

		intent = Intent.ToMove;
		EvaluateState(whenFalse: (BetweenNodes || committed));
	}

	/// <summary>
	/// Search for an interaction or attack target under the selector, set the appropriate target, then evaluate the Grunt's state if possible.
	/// </summary>
	private void OnAction() {
		if (!selected) {
			return;
		}

		interactionTarget =
			FindObjectsByType<GridObject>(FindObjectsSortMode.None)
				.FirstOrDefault(
					go => {
						IInteractable interactable = go as IInteractable;

						return go.enabled
							&& go.node == GameManager.Instance.selector.node
							&& interactable != null
							&& interactable.CompatibleItemz.Contains(equippedTool.toolName);
					}
				);

		if (interactionTarget != null) {
			HandleActionCommand(interactionTarget.node, Intent.ToInteract);

			return;
		}

		attackTarget = GameManager.Instance.allGruntz
			.FirstOrDefault(g => g.node == GameManager.Instance.selector.node && g.enabled && g != this);

		if (attackTarget != null) {
			HandleActionCommand(attackTarget.node, Intent.ToAttack);

			return;
		}

		// Todo: Play voice line for having an incompatible tool

		intent = Intent.ToIdle;
		EvaluateState(whenFalse: BetweenNodes);
	}

	/// <summary>
	/// Take appropriate action according to the interaction/attack target's node and intent.
	/// </summary>
	/// <param name="targetNode">The node of the target. </param>
	/// <param name="newIntent">The intent against the target.</param>
	public void HandleActionCommand(Node targetNode, Intent newIntent) {
		travelGoal = InRange(targetNode) ? node : targetNode;

		if (travelGoal == null) {
			travelGoal = node;
			intent = Intent.ToIdle;
			EvaluateState(whenFalse: BetweenNodes);
		}

		intent = newIntent;
		EvaluateState(whenFalse: BetweenNodes || committed);
	}

	/// <summary>
	/// Force the Grunt into an idle state.
	/// </summary>
	private void OnForceIdle() {
		if (!selected) {
			return;
		}

		intent = Intent.ToIdle;
		state = State.Idle;
	}

	/// <summary>
	/// Todo
	/// </summary>
	private void OnGive() {
		if (!selected) {
			return;
		}
	}
	#endregion

	/// <summary>
	/// Evaluate the Grunt's current state, then take appropriate action.
	/// </summary>
	/// <param name="whenFalse">A condition potentially blocking evaluation.</param>
	public async void EvaluateState(bool whenFalse = false) {
		if (whenFalse) {
			return;
		}

		if (node == travelGoal) {
			if (waiting) {
				await UniTask.WaitWhile(() => statz.stamina < Statz.MaxValue);
				waiting = false;

				switch (intent) {
					case Intent.ToInteract:
						state = State.Interacting;
						StartCoroutine(Interact(interactionTarget));

						return;
					case Intent.ToAttack:
						state = State.Attacking;
						Attack(attackTarget);

						return;
				}
			}

			switch (intent) {
				case Intent.ToMove or Intent.ToIdle:
					intent = Intent.ToIdle;
					state = State.Idle;

					return;
				case Intent.ToInteract:
					state = State.Interacting;
					StartCoroutine(Interact(interactionTarget));

					return;
				case Intent.ToAttack:
					state = State.Attacking;
					Attack(attackTarget);

					return;
			}
		} else {
			Move(travelGoal);
		}
	}

	private void Select() {
		selected = true;
		selectionMarker.SetActive(true);
	}

	private void Deselect() {
		selected = false;
		selectionMarker.SetActive(false);
	}

	private void Move(Node target) {
		if (target == node) {
			GoToIdleIfNotActing();

			return;
		}

		if (target == null) {
			return;
		}

		// When target is unreachable, search for a new target adjacent to it
		if (!target.IsWalkable() || (interactionTarget != null && interactionTarget.node == target)) {
			if (node.neighbours.Contains(target)) {
				travelGoal = node;

				GoToIdleIfNotActing();

				return;
			}

			List<Node> freeNeighbours = target.neighbours.Where(n => n.IsWalkable()).ToList();

			if (freeNeighbours.Count == 0) {
				travelGoal = node;

				GoToIdleIfNotActing();

				return;
			}

			target = freeNeighbours.OrderBy(n => Pathfinder.CalculateHeuristic(node, n)).First();
		}

		// Search for path to target
		// List<NodeV2> newPath = Pathfinder.AstarSearch(node, target, Level.Instance.levelNodes.ToHashSet());
		List<Node> newPath = Pathfinder.AstarSearch(node, target, Level.Instance.levelNodes.ToHashSet());

		// When no path found, evaluate state
		if (newPath.Count <= 0) {
			travelGoal = node;

			GoToIdleIfNotActing();

			return;
		}

		if (newPath[0].ReservedBy == null) {
			state = State.Moving;
			next = newPath[0];
			FaceTowardsNode(next);
		}
	}

	private void GoToIdleIfNotActing() {
		if (intent is Intent.ToMove or Intent.ToIdle) {
			intent = Intent.ToIdle;
		}

		EvaluateState(whenFalse: BetweenNodes);
	}

	private IEnumerator Interact(GridObject target) {
		if (!InRange(target.node)) {
			travelGoal = target.node;
			EvaluateState(whenFalse: BetweenNodes);

			yield break;
		}

		if (statz.stamina < Statz.MaxValue) {
			FaceTowardsNode(target.node);

			waiting = true;
			intent = Intent.ToInteract;
			EvaluateState(whenFalse: BetweenNodes);

			yield break;
		}

		FaceTowardsNode(target.node);

		committed = true;

		if (target is Rock rock) {
			yield return BreakRock(rock);
		} else if (target is Hole hole) {
			yield return Dig(hole);
		}

		committed = false;

		DrainStamina();

		intent = Intent.ToIdle;
		EvaluateState(whenFalse: BetweenNodes);
	}

	private IEnumerator Dig(Hole toDig) {
		AnimationClip toPlay = AnimationPack.GetRandomClip(facingDirection, animationPack.interact);

		toDig.dirt.GetComponent<AnimancerComponent>().Play(AnimationManager.Instance.dirtEffect);
		Animancer.Play(toPlay);

		yield return new WaitForSeconds(toPlay.length * 1.5f);

		toDig.Interact();

		yield return new WaitForSeconds(toPlay.length * 0.5f);

		interactionTarget = null;
	}

	private IEnumerator BreakRock(Rock toBreak) {
		AnimationClip toPlay = AnimationPack.GetRandomClip(facingDirection, animationPack.interact);

		Animancer.Play(toPlay);

		yield return new WaitForSeconds(toPlay.length / 2);

		toBreak.Interact();

		yield return new WaitForSeconds(toPlay.length / 2);

		interactionTarget = null;
	}

	private async void Attack(Grunt target) {
		if (!target.enabled) {
			// Todo: Play voice line for being unable to interact

			intent = Intent.ToIdle;
			EvaluateState();

			return;
		}

		if (!InRange(target.node)) {
			travelGoal = target.node;
			EvaluateState(whenFalse: BetweenNodes);

			return;
		}

		if (statz.stamina < Statz.MaxValue) {
			FaceTowardsNode(target.node);

			waiting = true;
			intent = Intent.ToAttack;
			EvaluateState(whenFalse: BetweenNodes);

			return;
		}

		FaceTowardsNode(target.node);

		AnimationClip toPlay = AnimationPack.GetRandomClip(facingDirection, animationPack.attack);
		Animancer.Play(toPlay);

		committed = true;
		await UniTask.WaitForSeconds(toPlay.length / 2);

		attackTarget.TakeDamage(equippedTool.damage);
		attackTarget.onHit.Invoke();

		await UniTask.WaitForSeconds(toPlay.length / 2);
		committed = false;

		DrainStamina();

		if (attackTarget.statz.health <= 0) {
			attackTarget = null;

			intent = Intent.ToIdle;
		}

		EvaluateState(whenFalse: BetweenNodes);
	}

	public void RegenerateStamina() {
		if (statz.stamina >= Statz.MaxValue) {
			CancelInvoke(nameof(RegenerateStamina));

			statz.stamina = Statz.MaxValue;
			onStaminaRegenerated.Invoke();

			return;
		}

		statz.stamina++;
		barz.staminaBar.Adjust(statz.stamina);
		gruntEntry.SetStamina(statz.stamina);
	}

	public void DrainStamina() {
		statz.stamina = 0;
		onStaminaDrained.Invoke();
		InvokeRepeating(nameof(RegenerateStamina), 0, 0.2f);
	}

	/// <summary>
	/// Continually moves the Grunt's physical position between the current and next node.
	/// </summary>
	private void ChangePosition() {
		Vector3 moveVector = (next.transform.position - transform.position).normalized;
		gameObject.transform.position += moveVector * (Time.deltaTime / moveSpeed);

		Animancer.Play(AnimationPack.GetRandomClip(facingDirection, animationPack.walk));
	}

	/// <summary>
	/// Faces the Grunt towards the given node.
	/// </summary>
	/// <param name="toFace">The node to face towards.</param>
	private void FaceTowardsNode(Node toFace) {
		Vector2 direction = (toFace.location2D - node.location2D);

		facingDirection = direction switch {
			_ when direction == Vector2.up => facingDirection = Direction.Up,
			_ when direction == Vector2.down => facingDirection = Direction.Down,
			_ when direction == Vector2.left => facingDirection = Direction.Left,
			_ when direction == Vector2.right => facingDirection = Direction.Right,
			_ when direction == (Vector2.up + Vector2.right) => facingDirection = Direction.UpRight,
			_ when direction == (Vector2.up + Vector2.left) => facingDirection = Direction.UpLeft,
			_ when direction == (Vector2.down + Vector2.right) => facingDirection = Direction.DownRight,
			_ when direction == (Vector2.down + Vector2.left) => facingDirection = Direction.DownLeft,
			_ => facingDirection,
		};
	}

	/// <summary>
	/// Checks whether the given node is in range of the Grunt's equipped tool.
	/// </summary>
	/// <param name="otherNode">The node to check.</param>
	/// <returns>True when the node is in range, false otherwise.</returns>
	private bool InRange(Node otherNode) {
		return Mathf.Abs(node.location2D.x - otherNode.location2D.x) <= equippedTool.range
			&& Mathf.Abs(node.location2D.y - otherNode.location2D.y) <= equippedTool.range;
	}

	/// <summary>
	/// Damage the grunt, adjusting its health bar as well.
	/// </summary>
	/// <param name="damage">The amount of damage to be dealt.</param>
	private void TakeDamage(int damage) {
		statz.health = Math.Clamp(statz.health - damage, 0, Statz.MaxValue);
		barz.healthBar.Adjust(statz.health);
		gruntEntry.SetHealth(statz.health);

		if (statz.health <= 0) {
			onDeath.Invoke();
		}
	}

	/// <summary>
	/// Kills the Grunt. (duh)
	/// </summary>
	public async void Die() {
		enabled = false;

		CancelInvoke(nameof(RegenerateStamina));
		DeactivateBarz();

		Addressables.InstantiateAsync($"GruntPuddle_{gruntColor.ToString()}.prefab", GameObject.Find("Puddlez").transform).Completed += handle => {
			GruntPuddle puddle = handle.Result.GetComponent<GruntPuddle>();
			puddle.transform.position = transform.position;
		};

		spriteRenderer.sortingLayerName = "AlwaysBottom";
		spriteRenderer.sortingOrder = 0;

		await Animancer.Play(animationPack.deathAnimation);

		gruntEntry.SetHealth(0);
		gruntEntry.SetStamina(0);
		gruntEntry.headSlot.sprite = gruntEntry.blankSlotIcon;
		gruntEntry.toolSlot.sprite = gruntEntry.blankSlotIcon;
		gruntEntry.toySlot.sprite = gruntEntry.blankSlotIcon;
		gruntEntry.powerupSlot.sprite = gruntEntry.blankSlotIcon;
		gruntEntry.healSlot.sprite = gruntEntry.blankSlotIcon;

		spriteRenderer.enabled = false;
		GameManager.Instance.allGruntz.Remove(this);
		Level.Instance.levelStatz.deathz++;
		Destroy(gameObject);
	}

	/// <summary>
	/// Kills the Grunt. (duh)
	/// </summary>
	public async void Die(AnimationClip toPlay, bool leavePuddle = true, bool playBelow = true) {
		enabled = false;

		CancelInvoke(nameof(RegenerateStamina));
		DeactivateBarz();

		if (leavePuddle) {
			Addressables.InstantiateAsync($"GruntPuddle_{gruntColor.ToString()}.prefab", GameObject.Find("Puddlez").transform).Completed += handle => {
				GruntPuddle puddle = handle.Result.GetComponent<GruntPuddle>();
				puddle.transform.position = transform.position;
			};
		}

		if (playBelow) {
			spriteRenderer.sortingLayerName = "AlwaysBottom";
			spriteRenderer.sortingOrder = 0;
		}

		await Animancer.Play(toPlay);

		spriteRenderer.enabled = false;
		GameManager.Instance.allGruntz.Remove(this);
		Level.Instance.levelStatz.deathz++;
		Destroy(gameObject);
	}

	public IEnumerator Teleport(Transform destination, Warp fromWarp) {
		enabled = false;

		// The first part of the animation plays where the Grunt is sucked into the warp
		Animancer.Play(AnimationManager.Instance.gruntWarpOutEndAnimation);

		yield return new WaitForSeconds(AnimationManager.Instance.gruntWarpOutEndAnimation.length);

		Camera.main.transform.position = new Vector3(destination.position.x, destination.position.y, -10);
		transform.position = destination.position;
		node = Level.Instance.levelNodes.First(n => n.location2D == Vector2Int.RoundToInt(transform.position));

		// The second part of the animation plays where the Grunt is spat out of the warp
		Animancer.Play(AnimationManager.Instance.gruntWarpEnterAnimation);

		yield return new WaitForSeconds(AnimationManager.Instance.gruntWarpEnterAnimation.length);

		enabled = true;
		intent = Intent.ToIdle;
		EvaluateState();

		fromWarp.Deactivate();
	}

	private void DeactivateBarz() {
		barz.healthBar.gameObject.SetActive(false);
		barz.staminaBar.gameObject.SetActive(false);
		barz.toyTimeBar.gameObject.SetActive(false);
		barz.wingzTimeBar.gameObject.SetActive(false);
	}

// --------------------------------------------------
// IDataPersistence
// --------------------------------------------------

	#region IDataPersistence
	public string Guid { get; set; }

	/// <summary>
	/// Saves the data to a GruntDataV2 object.
	/// </summary>
	/// <param name="data"></param>
	public void Save(ref GameData data) {
		GruntDataV2 saveData = new GruntDataV2 {
			guid = Guid,
			gruntName = gruntName,
			position = transform.position,
		};

		data.gruntData.InitializeListAdd(saveData);

		Debug.Log($"Saving {gruntName} at {transform.position} with GUID {Guid}");
	}

	public void Load(GameData data) {
		// GruntDataV2 loadData = data.gruntData.First(); // Remove the data from the list so it doesn't get loaded again
		//
		// Guid = loadData.guid;
		// gruntName = loadData.gruntName;
		// transform.position = loadData.position;
	}

	/// <summary>
	/// Loads the data from a GruntDataV2 object.
	/// </summary>
	/// <param name="data"></param>
	public void Load(GruntDataV2 data) {
		Guid = data.guid;
		gruntName = data.gruntName;
		// transform.position = data.position;
	}

	public void GenerateGuid() {
		Guid = System.Guid.NewGuid().ToString();
	}
	#endregion

}

}
