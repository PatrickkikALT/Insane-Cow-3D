using System;
using UnityEngine;

public class CrazyFence3D : MonoBehaviour {
  public void OnCollisionEnter(Collision other) {
    if (other.gameObject.CompareTag("Entity")) {
      other.transform.TryGetComponent(out VehicleBody3D body);
      body.Death();
    }
  }
}
