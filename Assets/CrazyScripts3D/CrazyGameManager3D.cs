using Unity.Netcode;
using UnityEngine;
using FirePixel.Networking;

public class GameManager : NetworkBehaviour {
  public static GameManager Instance { get; private set; }

  [SerializeField] private GameObject playerPrefab;
  [SerializeField] private SpawnBox spawnBox;
  [SerializeField] private Transform[] spawnPoints;

  private void Awake() {
    if (Instance && Instance != this) {
      Destroy(gameObject);
      return;
    }
    Instance = this;
  }

  public override void OnNetworkSpawn() {
    if (IsServer) {
      
      SpawnAllPlayers();
      
      NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }
  }

  public override void OnNetworkDespawn() {
    if (IsServer && NetworkManager.Singleton != null) {
      NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }
  }

  private void OnClientConnected(ulong clientId) {
    SpawnPlayer(clientId);
  }

  private void SpawnAllPlayers() {
    foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
      SpawnPlayer(clientId);
    }
  }

  private void SpawnPlayer(ulong clientId) {
    int spawnIndex = (int)(clientId % (ulong)spawnPoints.Length);
    Vector3 spawnPosition = spawnPoints[spawnIndex].position;

    GameObject playerObject = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    NetworkObject netObj = playerObject.GetComponent<NetworkObject>();
    netObj.SpawnAsPlayerObject(clientId);
  }
}