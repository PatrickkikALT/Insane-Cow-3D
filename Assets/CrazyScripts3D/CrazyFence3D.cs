using System;
using UnityEngine;
using Unity.Netcode;

public class CrazyFence3D : MonoBehaviour {
  public void OnCollisionEnter(Collision other) {
    if (other.gameObject.CompareTag("Entity")) {
      if (other.transform.TryGetComponent(out VehicleBody3D body)) {
        if (body.IsServer) {
          body.Death();
        }
      }
    }
  }
}