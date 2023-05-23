﻿using System.Collections;
using GruntzUnityverse.Actorz;
using UnityEngine;

namespace GruntzUnityverse.Objectz.Itemz.Toolz {
  public class Gauntletz : Tool {
    public IEnumerator BreakRock(Grunt grunt) {
      grunt.Animator.Play($"UseItem_{grunt.Navigator.FacingDirection}");
      grunt.IsInterrupted = true;

      // Todo: Wait for the exact time needed for breaking Rockz
      yield return new WaitForSeconds(0.5f);

      grunt.IsInterrupted = false;

      StartCoroutine(((Rock)grunt.TargetObject).Break());
      grunt.TargetObject = null;
    }
  }
}
