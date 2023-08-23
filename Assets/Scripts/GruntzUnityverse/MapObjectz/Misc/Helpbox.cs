using System.Linq;
using GruntzUnityverse.Managerz;
using UnityEngine;

namespace GruntzUnityverse.MapObjectz.Misc {
  public class Helpbox : MapObject {
    public CameraMovement cameraMovement;
    public bool isUntouched;
    public string boxText;
    public static bool isTextShown;
    // ------------------------------------------------------------ //

    protected override void Start() {
      base.Start();

      isUntouched = true;
    }
    // ------------------------------------------------------------ //

    private void Update() {
      // Pausing the game when a Grunt steps onto a HelpBox and displaying the HelpBox text
      if (isUntouched && LevelManager.Instance.PlayerGruntz.Any(grunt => grunt.AtNode(ownNode))) {
        DisplayHelpbox();

        return;
      }

      // Resuming the game when user clicks the left or right mouse button while the game is paused
      if (!isUntouched && isTextShown && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))) {
        HideHelpbox();

        return;
      }

      if (LevelManager.Instance.PlayerGruntz.All(grunt => !grunt.AtNode(ownNode))) {
        isUntouched = true;
      }
    }
    // ------------------------------------------------------------ //

    private void DisplayHelpbox() {
      isUntouched = false;
      Time.timeScale = 0;
      LevelManager.Instance.helpBoxText.text = boxText;
      isTextShown = true;
      cameraMovement.areControlsDisabled = true;
    }

    private void HideHelpbox() {
      Time.timeScale = 1;
      LevelManager.Instance.helpBoxText.text = "";
      isTextShown = false;
      cameraMovement.areControlsDisabled = false;
    }
  }
}
