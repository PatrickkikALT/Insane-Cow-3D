using System;
using UnityEngine;

//nobe script but edited
[Serializable]
[Tooltip("A box area in which items can be spawned")]
public class SpawnBox
{
  [Tooltip("The center position of the box relative to the origin")]
  public Vector3 BoxPos;

  [Tooltip("The size of the box in each dimension")]
  public Vector3 BoxSize;

  [Tooltip("The color of the gizmo when drawn in the editor")]
  public Color GizmoColor;

  [Tooltip("Returns a random location inside the SpawnBox")]
  public Vector3 GetRandomPosition(Vector3 origin) {
    //loop until valid position is found
    //while loop is cheaper then recursion
    while (true) {
      var randomPos = new Vector3(UnityEngine.Random.Range(BoxPos.x - BoxSize.x / 2, BoxPos.x + BoxSize.x / 2), UnityEngine.Random.Range(BoxPos.y - BoxSize.y / 2, BoxPos.y + BoxSize.y / 2), UnityEngine.Random.Range(BoxPos.z - BoxSize.z / 2, BoxPos.z + BoxSize.z / 2));

      //Position where the ray will be shot from
      var rayPos = randomPos + origin + Vector3.up * 5;

      //Shoots a ray to find any surface that matches the spawnLayer's type
      if (Physics.Raycast(rayPos, Vector3.down, out RaycastHit hit, rayPos.y + 10)) {
        return origin + randomPos;
      }

      return new Vector3(0, 0, 0);
    }
  }
}