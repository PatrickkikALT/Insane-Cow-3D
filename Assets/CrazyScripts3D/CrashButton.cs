using UnityEngine;
using UnityEngine.Diagnostics;

public class CrashButton : MonoBehaviour {
  public void OnPress() {
    Utils.ForceCrash(ForcedCrashCategory.FatalError);
  }
}
