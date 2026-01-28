using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUI : MonoBehaviour {
  [SerializeField] private Transform playerListContainer;
  [SerializeField] private GameObject playerListItemPrefab;
  [SerializeField] private Button readyButton;
  [SerializeField] private Button startGameButton;
  [SerializeField] private Button leaveButton;
  [SerializeField] private TextMeshProUGUI readyButtonText;
  [SerializeField] private TextMeshProUGUI lobbyStatusText;

  private bool isReady = false;
  private List<GameObject> playerListItems = new List<GameObject>();

  private void Start() {
    readyButton.onClick.AddListener(OnReadyButtonClicked);
    startGameButton.onClick.AddListener(OnStartGameClicked);
    leaveButton.onClick.AddListener(OnLeaveButtonClicked);
    
    startGameButton.gameObject.SetActive(NetworkManager.Singleton.IsHost);

    if (LobbyManager.Instance != null) {
      LobbyManager.Instance.onLobbyPlayersChanged += UpdatePlayerList;
      UpdatePlayerList();
    }
  }

  private void OnDestroy() {
    if (LobbyManager.Instance != null) {
      LobbyManager.Instance.onLobbyPlayersChanged -= UpdatePlayerList;
    }
  }

  private void UpdatePlayerList() {
    foreach (var item in playerListItems) {
      Destroy(item);
    }

    playerListItems.Clear();
    
    List<LobbyPlayerData> players = LobbyManager.Instance.GetPlayers();
    
    foreach (var player in players) {
      GameObject item = Instantiate(playerListItemPrefab, playerListContainer);
      playerListItems.Add(item);

      TextMeshProUGUI nameText = item.transform.Find("PlayerName").GetComponent<TextMeshProUGUI>();
      nameText.text = player.playerName.ToString();

      TextMeshProUGUI statusText = item.transform.Find("ReadyStatus").GetComponent<TextMeshProUGUI>();
      if (player.isReady) {
        statusText.text = "READY";
        statusText.color = Color.green;
      }
      else {
        statusText.text = "NOT READY";
        statusText.color = Color.red;
      }
    }
    lobbyStatusText.text = $"Players: {players.Count}/4";
    
    if (NetworkManager.Singleton.IsHost) {
      startGameButton.interactable = LobbyManager.Instance.AreAllPlayersReady();
    }
  }

  private void OnReadyButtonClicked() {
    isReady = !isReady;
    LobbyManager.Instance.SetPlayerReadyServerRpc(isReady);

    if (isReady) {
      readyButtonText.text = "Not Ready";
      readyButton.GetComponent<Image>().color = Color.red;
    }
    else {
      readyButtonText.text = "Ready";
      readyButton.GetComponent<Image>().color = Color.green;
    }
  }

  private void OnStartGameClicked() {
    if (NetworkManager.Singleton.IsHost) {
      LobbyManager.Instance.StartGame();
    }
  }

  private void OnLeaveButtonClicked() {
    LobbyNetworkManager.Instance.Disconnect();
  }
}