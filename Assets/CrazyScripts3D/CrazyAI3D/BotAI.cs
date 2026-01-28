using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(VehicleBody3D))]
public class BotAI : NetworkBehaviour {
  [Header("Raycast Settings")] public float rayDistance = 5f;
  public LayerMask detectionLayer = -1;

  [Header("Ray Angles (degrees)")] public float nearSideAngle = 15f;
  public float farSideAngle = 45f;

  [Header("Debug Visualization")] public bool showDebugRays = true;

  private VehicleBody3D _vehicle;

  private void Start() {
    _vehicle = GetComponent<VehicleBody3D>();
  }

  private void FixedUpdate() {
    // ONLY the server should process AI steering and physics application
    if (!IsServer || !IsSpawned) return;

    bool charge = CastRay(0f);
    bool left = CastRay(nearSideAngle);
    bool right = CastRay(-nearSideAngle);
    bool cleft = CastRay(farSideAngle);
    bool cright = CastRay(-farSideAngle);

    float steering;
    float engineForce = -1000f;

    if (charge) {
      steering = 0f;
    }
    else if (cleft) {
      steering = 0.5f;
    }
    else if (cright) {
      steering = -0.5f;
    }
    else if (left) {
      steering = 0.2f;
    }
    else if (right) {
      steering = -0.2f;
    }
    else {
      steering = 0f;
    }

    // AI communicates directly with the body on the server, 
    // bypassing the need for ServerRpcs used by players.
    _vehicle.Steering = steering;
    _vehicle.EngineForce = engineForce;
  }

  private bool CastRay(float angleOffset) {
    Vector3 direction = Quaternion.Euler(0, angleOffset, 0) * transform.forward;
    Ray ray = new Ray(transform.position, direction);

    if (showDebugRays) {
      Color rayColor = Physics.Raycast(ray, rayDistance, detectionLayer) ? Color.red : Color.green;
      Debug.DrawRay(transform.position, direction * rayDistance, rayColor);
    }

    return Physics.Raycast(ray, rayDistance, detectionLayer);
  }

  private void OnDrawGizmos() {
    if (!showDebugRays) return;

    Gizmos.color = Color.cyan;
    Gizmos.DrawWireSphere(transform.position, 0.2f);
  }
}