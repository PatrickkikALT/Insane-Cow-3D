using System;
using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Mask : NetworkBehaviour {
  public GameObject meshToTransform;
  public MaskStat stats;
  public bool equipped;
  public string maskName;

  private Collider _collider;
  private Renderer _renderer;
  private void Start() {
    _collider = GetComponent<Collider>();
    _renderer = GetComponent<Renderer>();
    StartCoroutine(OnSpawnCoroutine());
  }

  public void OnTriggerEnter(Collider collision) {
    if (!IsServer) return;
    
    if (collision.gameObject.layer == LayerMask.NameToLayer("Player")) {
      EquipMask(collision);
    }
  }

  public virtual void EquipMask(Collider collision) {
    if (!IsServer) return;
    
    if (collision.gameObject.TryGetComponent(out VehicleBody3D body)) {
      EquipMaskClientRpc(body.NetworkObjectId, GetType().Name);
      GetComponent<NetworkObject>().Despawn();
    }
  }
  
  [ClientRpc]
  protected void EquipMaskClientRpc(ulong vehicleNetworkId, string maskTypeName) {
    if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(vehicleNetworkId, out NetworkObject vehicleNetObj)) {
      if (!vehicleNetObj.TryGetComponent(out VehicleBody3D body)) return;
      
      Vector3 pos = body.transform.position;
      Quaternion rot = body.transform.rotation;
      
      GameObject obj = Instantiate(meshToTransform, pos, rot);
      GameObject oldObj = body.mesh;
      
      obj.transform.SetParent(oldObj.transform.parent);
      obj.transform.localScale = oldObj.transform.localScale;
      Destroy(oldObj);
      body.mesh = obj;
      
      var component = body.gameObject.AddComponent(this.GetType());
      if (component is Mask mask) {
        mask.stats = this.stats;
        mask.equipped = true;
        mask.maskName = maskName;
      }
      
      if (body.IsOwner) {
        body.GetComponent<VehicleController>()?.SetStats(stats);
        body.camera.transform.localPosition = stats.cameraPosition;
      }
    }
  }
  
  public virtual void HitEffect(GameObject hit) {
    return;
  }

  public IEnumerator OnSpawnCoroutine() {
    if (equipped) {
      AudioSource source = gameObject.GetComponent<AudioSource>();
      source.clip = stats.clip;
      source.Play(0); 
      yield break;
    }
    _collider.enabled = false;
    yield return new WaitForSeconds(1f);
    _collider.enabled = true;
  }
}