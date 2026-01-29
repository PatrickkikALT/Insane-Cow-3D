using Unity.Netcode;
using UnityEngine;

public class MaskManager : NetworkBehaviour {
  
  [SerializeField] private GameObject[] maskPrefabs;

  public override void OnNetworkSpawn() {
    if (!IsServer) return;
    SpawnRandomMask(Vector3.zero);
  }
  
  private void SpawnRandomMask(Vector3 position) {
    if (!IsServer) return;
    
    GameObject maskPrefab = maskPrefabs[Random.Range(0, maskPrefabs.Length)];
    GameObject maskInstance = Instantiate(maskPrefab, position, Quaternion.identity);
    
    NetworkObject netObj = maskInstance.GetComponent<NetworkObject>();
    netObj.Spawn();

  }
}
