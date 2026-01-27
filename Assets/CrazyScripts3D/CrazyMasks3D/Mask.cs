using System;
using System.Collections;
using UnityEngine;

public class Mask : MonoBehaviour {
  public GameObject meshToTransform;
  public MaskStat stats;

  private void Start() {
    StartCoroutine(SpinCoroutine());
  }

  public void OnTriggerEnter(Collider collision) {
    if (collision.gameObject.layer == LayerMask.NameToLayer("Player")) {
      EquipMask(collision);
    }
  }

  public virtual void EquipMask(Collider collision) {
    Vector3 pos = collision.transform.position;
    Quaternion rot = collision.transform.rotation;
    
    GameObject obj = Instantiate(meshToTransform, pos, rot);
    VehicleBody3D body = collision.gameObject.GetComponent<VehicleBody3D>();
    GameObject oldObj = body.mesh;
    body.onHit += HitEffect;
    obj.transform.SetParent(oldObj.transform.parent);
    obj.transform.localScale = oldObj.transform.localScale;
    Destroy(oldObj);
    Destroy(GetComponent<BoxCollider>());
    GetComponent<MeshRenderer>().enabled = false;
  }
  public virtual void HitEffect(GameObject hit) {
    return;
  }

  public IEnumerator SpinCoroutine() {
    while (gameObject.activeSelf) {
      transform.Rotate(0, 1, 0);
      yield return null;
    }
  }
}
