using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class CrazyExplosion3D : NetworkBehaviour {
  private Animator _animator;
  public GameObject player;

  public override void OnNetworkSpawn() {
    Destroy(gameObject, 0.9585f);
  }

  public override void OnDestroy() {
    base.OnDestroy();
    GetComponent<NetworkObject>().Despawn();
  }

  private void FixedUpdate() {
    transform.LookAt(player.transform.position);
  }
}
