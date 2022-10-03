using System.Collections.Generic;
using System.Linq;

using Itemz;

using Singletonz;

using UnityEngine;

namespace Switchez {
  public class CheckpointSwitchTool : MonoBehaviour {
    public ToolType requirement;
    public bool isChecked;
    public List<Sprite> animFrames;
    public SpriteRenderer spriteRenderer;

    private void Update() {
      if (isChecked) {
        return;
      }

      if (MapManager.Instance.gruntz
        .Any(grunt => (Vector2)grunt.transform.position == (Vector2)transform.position
                      && grunt.tool == requirement)
      ) {
        isChecked = true;
        spriteRenderer.sprite = animFrames[1];
      }
    }
  }
}
