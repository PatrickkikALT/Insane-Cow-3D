using UnityEngine;

public interface IMask {
  public void OnTriggerEnter(Collider collision);
  public void EquipMask();
}
