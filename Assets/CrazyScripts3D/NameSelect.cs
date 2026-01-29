using UnityEngine;

public class NameSelect : MonoBehaviour {
  public void OnEndEdit(string str) {
    if (str.Length != 0) {
      PlayerPrefs.SetString("PlayerName", str);
    }
  }
}
