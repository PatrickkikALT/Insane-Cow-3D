using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using FirePixel.Networking;
using Unity.Mathematics;

public class CrazyGameManager3D : NetworkBehaviour {
  public static CrazyGameManager3D Instance { get; private set; }

  [SerializeField] private GameObject playerPrefab;
  public SpawnBox spawnBox;
  [SerializeField] private List<Transform> spawnPoints;

  private void Awake() {
    if (Instance && Instance != this) {
      Destroy(gameObject);
      return;
    }
    Instance = this;
  }

  public override void OnNetworkSpawn() {
    if (IsServer) {
      var obj = new GameObject();
      for (int i = 0; i < NetworkManager.ConnectedClients.Count; i++) {
        var trans = Instantiate(obj, spawnBox.GetRandomPosition(new Vector3(0, 0, 0)), Quaternion.identity).transform;
        spawnPoints.Add(trans);
      }
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
    int spawnIndex = (int)(clientId % (ulong)spawnPoints.Count);
    Vector3 spawnPosition = spawnPoints[spawnIndex].position;

    GameObject playerObject = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    NetworkObject netObj = playerObject.GetComponent<NetworkObject>();
    netObj.SpawnAsPlayerObject(clientId);
  }
}