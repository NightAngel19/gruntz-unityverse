using System.Collections;
using System.Linq;
using GruntzUnityverse.MapObjectz.BaseClasses;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

namespace GruntzUnityverse.MapObjectz.Interactablez {
  public class Rock : MapObject, IBreakable {
    public AnimationClip BreakAnimation { get; set; }
    private Vector3 _brokenScale;
    private Quaternion _brokenRotation;
    public MapObject hiddenItem;

    // ------------------------------------------------------------ //
    // OVERRIDES
    // ------------------------------------------------------------ //
    public override void Setup() {
      base.Setup();

      isTargetable = true;
      ownNode.isBlocked = true;
      ownNode.isHardTurn = true;
      _brokenScale = new Vector3(0.7f, 0.7f, 0.7f);
      _brokenRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

      if (hiddenItem == null) {
        hiddenItem = transform.GetComponentsInChildren<MapObject>()
          .FirstOrDefault(mo => mo != this);
      }

      hiddenItem?.SetRendererEnabled(false);
    }

    protected override void LoadAnimationz() {
      Addressables.LoadAssetAsync<AnimationClip>($"{abbreviatedArea}_RockBreak.anim").Completed += handle => {
        BreakAnimation = handle.Result;
      };
    }

    public IEnumerator Break(float contactDelay) {
      yield return new WaitForSeconds(contactDelay);

      animancer.Play(BreakAnimation);

      Addressables.LoadAssetAsync<AudioClip>($"{abbreviatedArea}_RockBreak.wav").Completed += handle => {
        GameManager.Instance.audioSource.PlayOneShot(handle.Result);
      };

      transform.localScale = _brokenScale;
      transform.localRotation = _brokenRotation;

      GameManager.Instance.currentLevelManager.Rockz.Remove(this);
      GameManager.Instance.currentLevelManager.SetBlockedAt(location, false);
      GameManager.Instance.currentLevelManager.SetHardTurnAt(location, false);
      hiddenItem?.SetRendererEnabled(true);

      yield return new WaitForSeconds(BreakAnimation.length);

      spriteRenderer.sortingLayerName = "AlwaysBottom";
      spriteRenderer.sortingOrder = 10;
      enabled = false;
    }
  }
}
