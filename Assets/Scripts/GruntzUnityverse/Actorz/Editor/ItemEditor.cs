﻿using GruntzUnityverse.MapObjectz.BaseClasses;
using UnityEditor;

namespace GruntzUnityverse.Actorz.Editor {
  [CustomEditor(typeof(MapItem), true), CanEditMultipleObjects]
  public class ItemEditor : UnityEditor.Editor {
    public override void OnInspectorGUI() { }
  }
}
