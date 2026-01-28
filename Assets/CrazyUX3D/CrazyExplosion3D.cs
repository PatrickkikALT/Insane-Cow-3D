using System;
using System.Collections;
using UnityEngine;

public class CrazyExplosion3D : MonoBehaviour {
  private Animator _animator;
  public GameObject player;

  private void Start() {
    Destroy(gameObject, 0.9585f);
  }

  private void FixedUpdate() {
    transform.LookAt(player.transform.position);
  }
}
