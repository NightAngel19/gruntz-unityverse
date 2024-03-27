﻿using System.Collections;
using GruntzUnityverse.Actorz;
using GruntzUnityverse.Core;
using UnityEngine;

namespace GruntzUnityverse.Itemz.Base {
/// <summary>
/// The base class for Powerupz.
/// </summary>
public class LevelPowerup : LevelItem {
	/// <summary>
	/// The duration of the Powerup.
	/// </summary>
	public float duration;

	public EquippedPowerup equippedPowerup;

	protected override IEnumerator Pickup(Grunt targetGrunt) {
		yield return base.Pickup(targetGrunt);

		Level.instance.levelStatz.powerupzCollected++;
		GetComponent<SpriteRenderer>().enabled = false;
		GetComponent<CircleCollider2D>().enabled = false;

		targetGrunt.equippedPowerupz.Add(equippedPowerup);
		equippedPowerup.Equip(targetGrunt, duration);

		yield return new WaitForSeconds(0.5f);

		Destroy(gameObject);
	}
}
}
