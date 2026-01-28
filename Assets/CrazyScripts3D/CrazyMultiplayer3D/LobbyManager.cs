using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkBehaviour {
  public static LobbyManager Instance { get; private set; }

  [SerializeField] private int maxPlayers = 4;

  private NetworkList<LobbyPlayerData> _lobbyPlayers;

  public System.Action onLobbyPlayersChanged;

  private void Awake() {
    if (Instance && Instance != this) {
      Destroy(gameObject);
      return;
    }
    Instance = this;

    _lobbyPlayers = new NetworkList<LobbyPlayerData>();
  }

  public override void OnNetworkSpawn() {
    if (IsServer) {
      NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
      NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
      
      AddPlayer(NetworkManager.Singleton.LocalClientId, "Host");
    }

    _lobbyPlayers.OnListChanged += OnLobbyPlayersListChanged;
  }

  public override void OnNetworkDespawn() {
    if (IsServer) {
      NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
      NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
    }

    _lobbyPlayers.OnListChanged -= OnLobbyPlayersListChanged;
  }

  private void OnClientConnectedCallback(ulong clientId) {
    if (!IsServer) return;

    if (_lobbyPlayers.Count >= maxPlayers) {
      NetworkManager.Singleton.DisconnectClient(clientId);
      return;
    }
    
    RequestPlayerNameClientRpc(new ClientRpcParams {
      Send = new ClientRpcSendParams {
        TargetClientIds = new ulong[] { clientId }
      }
    });
  }

  private void OnClientDisconnectCallback(ulong clientId) {
    if (!IsServer) return;
    RemovePlayer(clientId);
  }

  [ClientRpc]
  private void RequestPlayerNameClientRpc(ClientRpcParams clientRpcParams = default) {
    string playerName = PlayerPrefs.GetString("PlayerName", $"Player_{Random.Range(1000, 9999)}");
    SubmitPlayerNameServerRpc(playerName);
  }

  [ServerRpc(RequireOwnership = false)]
  private void SubmitPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default) {
    ulong clientId = serverRpcParams.Receive.SenderClientId;
    AddPlayer(clientId, playerName);
  }

  private void AddPlayer(ulong clientId, string playerName) {
    if (!IsServer) return;

    LobbyPlayerData newPlayer = new LobbyPlayerData {
      clientId = clientId,
      playerName = playerName,
      isReady = false
    };

    _lobbyPlayers.Add(newPlayer);
  }

  public void RemovePlayer(ulong clientId) {
    if (!IsServer) return;

    for (int i = 0; i < _lobbyPlayers.Count; i++) {
      if (_lobbyPlayers[i].clientId == clientId) {
        _lobbyPlayers.RemoveAt(i);
        break;
      }
    }
  }

  [ServerRpc(RequireOwnership = false)]
  public void SetPlayerReadyServerRpc(bool isReady, ServerRpcParams serverRpcParams = default) {
    ulong clientId = serverRpcParams.Receive.SenderClientId;

    for (int i = 0; i < _lobbyPlayers.Count; i++) {
      if (_lobbyPlayers[i].clientId == clientId) {
        LobbyPlayerData updatedPlayer = _lobbyPlayers[i];
        updatedPlayer.isReady = isReady;
        _lobbyPlayers[i] = updatedPlayer;
        break;
      }
    }
  }

  public bool AreAllPlayersReady() {
    if (_lobbyPlayers.Count < 2) return false;

    foreach (var player in _lobbyPlayers) {
      if (!player.isReady)
        return false;
    }

    return true;
  }

  public void StartGame() {
    if (!IsServer) return;
    if (!AreAllPlayersReady()) return;

    LobbyNetworkManager.Instance.LoadGameScene();
  }

  public List<LobbyPlayerData> GetPlayers() {
    List<LobbyPlayerData> players = new List<LobbyPlayerData>();
    foreach (var player in _lobbyPlayers) {
      players.Add(player);
    }

    return players;
  }

  private void OnLobbyPlayersListChanged(NetworkListEvent<LobbyPlayerData> changeEvent) {
    onLobbyPlayersChanged?.Invoke();
  }
}

public struct LobbyPlayerData : INetworkSerializable, System.IEquatable<LobbyPlayerData> {
  public ulong clientId;
  public FixedString64Bytes playerName;
  public bool isReady;

  public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
    serializer.SerializeValue(ref clientId);
    serializer.SerializeValue(ref playerName);
    serializer.SerializeValue(ref isReady);
  }

  public bool Equals(LobbyPlayerData other) {
    return clientId == other.clientId &&
           playerName.Equals(other.playerName) &&
           isReady == other.isReady;
  }
}