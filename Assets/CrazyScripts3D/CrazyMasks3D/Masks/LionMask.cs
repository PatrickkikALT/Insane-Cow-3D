using Unity.Netcode;
using UnityEngine;

public class LionMask : Mask {
  public override void HitEffect(GameObject hit) {
    if (hit.TryGetComponent(out NetworkObject netObj)) {
      if (hit.TryGetComponent(out Rigidbody rb)) {
        print("test");
        rb.AddExplosionForce(50000, hit.transform.position, 10, 10f, ForceMode.Impulse);
      }
    }
  }
}