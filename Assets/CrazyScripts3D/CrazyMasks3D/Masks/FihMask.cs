using System.Collections;
using UnityEngine;

public class FihMask : Mask {
  public AudioSource audio;
  public int timeTillDrown;
  private VehicleBody3D _body;
  public override void EquipMask(Collider collision) {
    base.EquipMask(collision);
    _body = GetComponent<VehicleBody3D>();
    audio.Play();
    StartCoroutine(DrownCoroutine());
  }

  public IEnumerator DrownCoroutine() {
    yield return new WaitForSeconds(timeTillDrown);
    _body.Death();
  }
}
