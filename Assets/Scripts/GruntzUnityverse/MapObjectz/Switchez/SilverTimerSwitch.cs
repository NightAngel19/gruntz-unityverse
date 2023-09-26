﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GruntzUnityverse.MapObjectz.BaseClasses;
using GruntzUnityverse.MapObjectz.Pyramidz;
using UnityEngine;

namespace GruntzUnityverse.MapObjectz.Switchez {
  public class SilverTimerSwitch : ObjectSwitch {
    private List<SilverPyramid> _pyramidz;
    private const float TimeStep = 0.1f;

    protected override void Start() {
      base.Start();

      _pyramidz = parent.GetComponentsInChildren<SilverPyramid>().ToList();

      if (_pyramidz.Count.Equals(0)) {
        WarnWithSpriteChange("There is no Silver Pyramid assigned to this Switch, this way the Switch won't work properly!");
      }
    }

    private void Update() {
      if (IsBeingPressed()) {
        if (hasBeenPressed) {
          return;
        }

        HandleSilverPyramidz();

        PressSwitch();
      } else if (!hasBeenReleased) {
        ReleaseSwitch();
      }
    }

    private void HandleSilverPyramidz() {
      foreach (SilverPyramid pyramid in _pyramidz) {
        StartCoroutine(HandleSilverPyramid(pyramid));
      }
    }

    private IEnumerator HandleSilverPyramid(SilverPyramid pyramid) {
      while (pyramid.Delay > 0) {
        pyramid.Delay -= TimeStep;

        yield return new WaitForSeconds(TimeStep);
      }

      pyramid.Toggle();

      while (pyramid.Duration > 0) {
        pyramid.Duration -= TimeStep;

        yield return new WaitForSeconds(TimeStep);
      }

      pyramid.Toggle();
    }
  }
}
