﻿using System.Collections;
using GruntzUnityverse.V2.Core;
using GruntzUnityverse.V2.Grunt;
using GruntzUnityverse.V2.Itemz.Toolz;

namespace GruntzUnityverse.V2.Itemz {
/// <summary>
/// The base class for all Toolz.
/// </summary>
public abstract class Tool : ItemV2 {
	public EquippedTool equippedTool;

	/// <summary>
	/// The base damage this Tool applies, without any modifiers.
	/// </summary>
	public int damage;

	/// <summary>
	/// The range of this Tool in tilez.
	/// </summary>
	public int range;

	/// <summary>
	/// The time needed in secondz for a <see cref="GruntV2"/> with this Tool to move one tile.
	/// </summary>
	public float moveSpeed;

	public float rechargeTime;

	/// <summary>
	/// The time needed in secondz before this Tool's effect is applied (e.g. before a rock is thrown).
	/// </summary>
	// Todo: Maybe not needed -> use animation length instead
	public float useTime;

	// Todo: Maybe not needed -> use animation length instead
	public float attackTime;

	protected override IEnumerator Pickup(GruntV2 target) {
		yield return base.Pickup(target);

		target.equippedTool = equippedTool;
		target.animationPack = equippedTool.animationPack;
		GM.Instance.levelStatz.toolz++;
	}
}
}
