using TMPro;
using UnityEngine;

public class PlayerListItem : MonoBehaviour {
  [SerializeField] private TextMeshProUGUI playerNameText;
  [SerializeField] private TextMeshProUGUI readyStatusText;

  public void SetPlayerData(string playerName, bool isReady, bool isHost) {
    playerNameText.text = playerName;
    if (isHost) {
      playerNameText.text += " (Host)";
    }

    if (isReady) {
      readyStatusText.text = "READY";
      readyStatusText.color = Color.green;
    }
    else {
      readyStatusText.text = "NOT READY";
      readyStatusText.color = Color.red;
    }
  }
}