using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class MaskManager : NetworkBehaviour {
  
  [SerializeField] private GameObject[] maskPrefabs;
  public static MaskManager Instance;
  public void Awake() {
    if (Instance && Instance != this) {
      Destroy(gameObject);
      return;
    }
    Instance = this;
  }
  
  public void SpawnRandomMask(Vector3 position) {
    if (!IsServer) return;
    
    GameObject maskPrefab = maskPrefabs[Random.Range(0, maskPrefabs.Length)];
    GameObject maskInstance = Instantiate(maskPrefab, position, Quaternion.identity);
    
    NetworkObject netObj = maskInstance.GetComponent<NetworkObject>();
    netObj.Spawn();

  }
}
