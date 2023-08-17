﻿using System.Collections;
using GruntzUnityverse.Actorz;
using GruntzUnityverse.Enumz;
using UnityEngine;

namespace GruntzUnityverse.MapObjectz.Itemz.Toolz {
  public class Club : Tool {
    protected override void Start() {
      base.Start();

      toolName = ToolName.Club;
      rangeType = RangeType.Melee;
      damage = GlobalValuez.ClubDamage;
    }

    public override IEnumerator Use(Grunt grunt) {
      yield return null;
    }

    public override IEnumerator Attack(Grunt attackTarget) {
      Vector2Int diffVector = attackTarget.targetObject.location - attackTarget.navigator.ownLocation;
      attackTarget.isInterrupted = true;

      attackTarget.navigator.SetFacingDirection(new Vector3(diffVector.x, diffVector.y, 0));

      AnimationClip clipToPlay =
        attackTarget.animationPack.Item[$"{GetType().Name}Grunt_Item_{attackTarget.navigator.facingDirection}"];

      attackTarget.animancer.Play(clipToPlay);

      // Wait the amount of time it takes for the tool to get in contact with the target Grunt
      yield return new WaitForSeconds(1f);

      StartCoroutine(attackTarget.GetStruck());
    }
  }
}
