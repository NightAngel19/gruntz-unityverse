﻿using System.Collections.Generic;
using System.Linq;
using GruntzUnityverse.Objectz.Switchez;
using UnityEngine;

namespace GruntzUnityverse.Objectz.Pyramidz {
  public class CheckpointPyramid : Pyramid {
    [field: SerializeField] public List<CheckpointSwitch> Switchez { get; set; }
    [field: SerializeField] public bool HasChanged { get; set; }

    protected override void Update() {
      base.Update();

      if (Switchez.Any(checkpointSwitch => !checkpointSwitch.IsPressed)) {
        return;
      }

      TogglePyramid();
      
      HasChanged = true;
      enabled = false;
    }
  }
}
