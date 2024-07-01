﻿using System.Linq;
using Animancer;
using Cysharp.Threading.Tasks;
using GruntzUnityverse.Actorz;
using GruntzUnityverse.Objectz.Interactablez;
using GruntzUnityverse.Utils;
using UnityEngine;

namespace GruntzUnityverse.Itemz.Base {
public abstract class LevelItem : MonoBehaviour {
	/// <summary>
	/// The display name of the item.
	/// </summary>
	public string displayName;

	/// <summary>
	/// The code-name of the item.
	/// </summary>
	public string codeName;

	/// <summary>
	/// The animation clip used to display the item on the level.
	/// </summary>
	public AnimationClip rotatingAnim;

	/// <summary>
	/// The animation clip used by a Grunt picking up the item.
	/// </summary>
	public AnimationClip pickupAnim;

	public bool hideUnderMound;

	public AnimancerComponent animancer;

	public void Setup() {
		animancer ??= GetComponent<AnimancerComponent>();

		Rock rock = FindObjectsByType<Rock>(FindObjectsSortMode.None)
			.FirstOrDefault(r => Vector2Int.RoundToInt(r.transform.position) == Vector2Int.RoundToInt(transform.position));

		if (rock != null) {
			rock.heldItem = this;
			GetComponent<SpriteRenderer>().enabled = false;
			GetComponent<CircleCollider2D>().isTrigger = false;

			return;
		}

		Hole hole = FindObjectsByType<Hole>(FindObjectsSortMode.None)
			.FirstOrDefault(r => Vector2Int.RoundToInt(r.transform.position) == Vector2Int.RoundToInt(transform.position));

		if (hole != null && hideUnderMound) {
			hole.heldItem = this;
			GetComponent<SpriteRenderer>().enabled = false;
			GetComponent<CircleCollider2D>().isTrigger = false;
		}
	}

	protected virtual void Start() {
		animancer.Play(rotatingAnim);
	}

	/// <summary>
	/// Called when a <see cref="Grunt"/> picks up this item.
	/// (Provides no implementation since child classes need to modify
	/// different properties of the Grunt picking up the item.)
	/// </summary>
	protected virtual async void Pickup(Grunt targetGrunt) {
		targetGrunt.GoToState(StateHandler.State.Committed);
		targetGrunt.enabled = false;

		targetGrunt.animancer.Play(pickupAnim);
		await UniTask.WaitForSeconds(pickupAnim.length);

		targetGrunt.enabled = true;
		targetGrunt.GoToState(StateHandler.State.Walking);
	}

	/// <summary>
	/// Called when an <see cref="Grunt"/> moves onto this Item.
	/// Other than RollingBallz, only Gruntz have the ability to collide with Items.
	/// This is checked inside the method, so there is no need to expose this method to child classes.
	/// </summary>
	/// <param name="other">The collider of the colliding object.</param>
	private void OnTriggerEnter2D(Collider2D other) {
		if (other.TryGetComponent(out Grunt grunt)) {
			GetComponent<SpriteRenderer>().enabled = false;

			Pickup(grunt);
		}
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (other.TryGetComponent(out Grunt _)) {
			GetComponent<SpriteRenderer>().enabled = true;
		}
	}

	public void LocalizeName(string newDisplayName) {
		displayName = newDisplayName;
	}

	#if UNITY_EDITOR
	private void OnValidate() {
		GetComponent<Animator>().hideFlags = HideFlags.HideInInspector;
		animancer.hideFlags = HideFlags.HideInInspector;

		GetComponent<TrimName>().hideFlags = HideFlags.HideInInspector;
	}
	#endif
}
}
