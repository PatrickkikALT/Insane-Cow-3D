using UnityEngine;

[CreateAssetMenu(fileName = "MaskStat", menuName = "ScriptableObjects/MaskStat", order = 1)]
public class MaskStat : ScriptableObject {
  public float maxEngineForce = 2000f;
  public float maxBrakeForce = 100f;
  public float maxSteeringAngle = 30f;
  public Vector3 cameraPosition = new Vector3(0, 2.7f, -2.5f);
  public float rigidbodyMass = 500;
  public AudioClip clip;
}