﻿using System.Collections;
using GruntzUnityverse.Actorz;
using GruntzUnityverse.Enumz;
using UnityEngine;

namespace GruntzUnityverse.Objectz.Itemz.Toolz {
  public class Shovel : Tool {
    protected override void Start() {
      Name = ToolName.Shovel;
    }

    public override IEnumerator Use(Grunt grunt) {
      Vector2Int diffVector = grunt.TargetObject.OwnLocation - grunt.Navigator.OwnLocation;
      grunt.IsInterrupted = true;

      grunt.Navigator.ChangeFacingDirection(new Vector3(diffVector.x, diffVector.y, 0));

      AnimationClip clipToPlay =
        // Todo: Replace with right animation
        grunt.AnimationPack.Item[$"{GetType().Name}Grunt_Item_{grunt.Navigator.FacingDirection}"];

      grunt.Animancer.Play(clipToPlay);

      StartCoroutine(grunt.TargetObject.BeUsed(grunt));

      yield return new WaitForSeconds(1.6f);

      grunt.IsInterrupted = false;

      if (grunt.TargetObject is null) {
        yield break;
      }
      // grunt.TargetObject = null;
    }

    public IEnumerator DigHole(Grunt grunt) {
      grunt.Animator.Play($"UseItem_{grunt.Navigator.FacingDirection}");
      // ((Hole)grunt.TargetObject).Animator.Play("DirtFlying");

      yield return new WaitForSeconds(2.1f);

      grunt.IsInterrupted = false;

      ((Hole)grunt.TargetObject).IsOpen = !((Hole)grunt.TargetObject).IsOpen;

      ((Hole)grunt.TargetObject).Renderer.sprite = ((Hole)grunt.TargetObject).IsOpen
        ? ((Hole)grunt.TargetObject).OpenSprite
        : ((Hole)grunt.TargetObject).FilledSprite;

      // if (grunt.TargetObject is null) {
      //   yield break;
      // }

      // StartCoroutine(((Hole)grunt.TargetObject).Dig());

      grunt.TargetObject = null;
    }
  }
}
