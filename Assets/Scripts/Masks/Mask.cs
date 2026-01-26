using System;
using System.Collections;
using UnityEngine;

public class Mask : MonoBehaviour, IMask {
  public GameObject meshToTransform;

  private void Start() {
    StartCoroutine(SpinCoroutine());
  }

  public void OnTriggerEnter(Collider collision) {
    if (collision.gameObject.layer == LayerMask.NameToLayer("Player")) {
      EquipMask();
    }
  }

  public virtual void EquipMask() {
    
  }

  public IEnumerator SpinCoroutine() {
    while (gameObject.activeSelf) {
      transform.Rotate(0, 100 * Time.deltaTime, 0);
      yield return null;
    }
  }
}
