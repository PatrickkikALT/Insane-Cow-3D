using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour {
  [SerializeField] private Button hostButton;
  [SerializeField] private Button joinButton;
  [SerializeField] private TMP_InputField playerNameInput;
  [SerializeField] private TMP_InputField ipAddressInput;
  [SerializeField] private GameObject connectionPanel;
  [SerializeField] private TextMeshProUGUI statusText;

  private void Start() {
    hostButton.onClick.AddListener(OnHostClicked);
    joinButton.onClick.AddListener(OnJoinClicked);
    
    string savedName = PlayerPrefs.GetString("PlayerName", "");
    if (string.IsNullOrEmpty(savedName)) {
      savedName = $"Player_{Random.Range(1000, 9999)}";
    }
    playerNameInput.text = savedName;
    
    ipAddressInput.text = "127.0.0.1";
  }

  private void OnHostClicked() {
    SavePlayerName();
    statusText.text = "Starting Host...";
    LobbyNetworkManager.Instance.StartHost();
  }

  private void OnJoinClicked() {
    SavePlayerName();

    string ipAddress = ipAddressInput.text;
    if (string.IsNullOrEmpty(ipAddress)) {
      ipAddress = "127.0.0.1";
    }

    Unity.Netcode.NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>()
      .SetConnectionData(ipAddress, 7777);

    statusText.text = $"Connecting to {ipAddress}...";
    LobbyNetworkManager.Instance.StartClient();
  }

  private void SavePlayerName() {
    string playerName = playerNameInput.text;
    if (string.IsNullOrEmpty(playerName)) {
      playerName = $"Player_{Random.Range(1000, 9999)}";
    }

    PlayerPrefs.SetString("PlayerName", playerName);
    PlayerPrefs.Save();
  }
}