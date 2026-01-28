using System;
using UnityEngine;
using Unity.Netcode;

public class BotManager : NetworkBehaviour {
  [Range(0, 100)]
  [SerializeField] private int amountOfBots;

  [SerializeField] private GameObject botPrefab;
  [SerializeField] private SpawnBox spawnBox;

  public override void OnNetworkSpawn() {
    if (!IsServer) return;

    for (int i = 0; i < amountOfBots; i++) {
      SpawnBot(i);
    }
  }

  private void SpawnBot(int index) {
    Vector3 pos = spawnBox.GetRandomPosition(transform.position);
    Quaternion rot = Quaternion.identity.Rand(false, true, false);
    
    GameObject obj = Instantiate(botPrefab, pos, rot);
    obj.name = $"Bot_{index}";
    NetworkObject netObj = obj.GetComponent<NetworkObject>();
    if (netObj) {
      netObj.Spawn();
    }
  }

  private void OnDrawGizmos() {
    Gizmos.color = spawnBox.GizmoColor;
    Gizmos.DrawWireCube(spawnBox.BoxPos + transform.position, spawnBox.BoxSize);
  }
}
