using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour {
  public static GameManager Instance { get; private set; }

  [SerializeField] private GameObject playerPrefab;
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
    }
  }

  private void SpawnAllPlayers() {
    int spawnIndex = 0;

    foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
      Vector3 spawnPosition = spawnPoints[spawnIndex % spawnPoints.Length].position;

      GameObject playerObject = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
      playerObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

      spawnIndex++;
    }
  }
}