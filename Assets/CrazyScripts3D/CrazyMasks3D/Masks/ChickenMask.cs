using Unity.Netcode;
using UnityEngine;

public class ChickenMask : Mask {
  public override void EquipMask(Collider collision) {
    base.EquipMask(collision);
    if (collision.gameObject.layer == LayerMask.NameToLayer("Player")) {
      collision.TryGetComponent(out VehicleController controller);
      controller.SetStats(stats);
      NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<VehicleBody3D>().camera.transform.localPosition = stats.cameraPosition;
    }
  }
  
  public override void HitEffect(GameObject hit) {
    hit.TryGetComponent(out Rigidbody rb);
    rb.AddExplosionForce(5, transform.position, 10, 10f, ForceMode.Impulse);
  }
}
