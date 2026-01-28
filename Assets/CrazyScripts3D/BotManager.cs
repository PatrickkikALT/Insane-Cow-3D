using System;
using UnityEngine;

public class BotManager : MonoBehaviour {
  [Range(0, 100)]
  [SerializeField] private int amountOfBots;

  [SerializeField] private GameObject botPrefab;
  [SerializeField] private SpawnBox spawnBox;

  public void Start() {
    for (int i = 0; i <= amountOfBots; i++) {
      GameObject obj = Instantiate(botPrefab, spawnBox.GetRandomPosition(transform.position), Quaternion.identity.Rand(false, true, false));
      obj.name = $"Bot_{i}";
    }
  }

  private void OnDrawGizmos() {
    Gizmos.color = spawnBox.GizmoColor;
    Gizmos.DrawWireCube(spawnBox.BoxPos + transform.position, spawnBox.BoxSize);
  }
}
