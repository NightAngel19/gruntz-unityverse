﻿using System.Collections;
using GruntzUnityverse.Actorz;
using UnityEngine;

namespace GruntzUnityverse.Objectz.Itemz.Toolz {
  public class Shovel : Tool {
    public  IEnumerator DigHole(Grunt grunt) {
      grunt.Animator.Play($"UseItem_{grunt.Navigator.FacingDirection}");
      grunt.IsInterrupted = true;

      // Todo: Wait for the exact time needed for digging Holez
      yield return new WaitForSeconds(1);

      grunt.IsInterrupted = false;

      StartCoroutine(((Hole)grunt.TargetObject).Dig());
      grunt.TargetObject = null;
    }
  }
}
