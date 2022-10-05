using System.Collections;
using System.Linq;

using Singletonz;

using UnityEngine;

namespace Switchez {
  public class SecretSwitch : MonoBehaviour {
    public SpriteRenderer spriteRenderer;
    public bool isUntouched;
    public Sprite[] animFrames;

    private void Start() {
      isUntouched = true;
    }

    private void Update() {
      if (isUntouched) {
        if (
          MapManager.Instance.gruntz.Any(grunt1 => (Vector2)grunt1.transform.position == (Vector2)transform.position)
        ) {
          spriteRenderer.sprite = animFrames[1];
          isUntouched = false;

          foreach (SecretTile secretTile in MapManager.Instance.secretTilez) {
            StartCoroutine(HandleSecretTile(secretTile));
          }
        }
      }
    }

    private IEnumerator HandleSecretTile(SecretTile secretTile) {
      while (secretTile.delay > 0) {
        secretTile.delay -= 0.5f;
        yield return new WaitForSeconds(0.5f);
      }
      
      secretTile.GetComponent<SpriteRenderer>().enabled = true;

      if (secretTile.isWalkable) {
        MapManager.Instance.AddNavTileAt(secretTile.transform.position);
        Debug.Log("Walkable");
      }

      while (secretTile.duration > 0) {
        secretTile.duration -= 0.5f;
        yield return new WaitForSeconds(0.5f);
      }

      secretTile.GetComponent<SpriteRenderer>().enabled = false;

      if (secretTile.isWalkable) {
        MapManager.Instance.RemoveNavTileAt(secretTile.transform.position);
        Debug.Log("Not Walkable");
      }
    }
  }
}
