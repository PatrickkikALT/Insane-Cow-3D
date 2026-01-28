using UnityEngine;

[RequireComponent(typeof(VehicleBody3D))]
public class BotAI : MonoBehaviour {
  [Header("Raycast Settings")] public float rayDistance = 5f;
  public LayerMask detectionLayer = -1;
  public LayerMask obstacleLayer; // Add a specific layer for Fences

  [Header("Ray Angles (degrees)")] public float nearSideAngle = 15f;
  public float farSideAngle = 45f;

  [Header("Debug Visualization")] public bool showDebugRays = true;

  private VehicleBody3D _vehicle;

  private void Start() {
    _vehicle = GetComponent<VehicleBody3D>();
  }

  private void FixedUpdate() {
    bool obstacleLeft = CastRay(nearSideAngle, obstacleLayer);
    bool obstacleRight = CastRay(-nearSideAngle, obstacleLayer);
    bool obstacleFarLeft = CastRay(farSideAngle, obstacleLayer);
    bool obstacleFarRight = CastRay(-farSideAngle, obstacleLayer);
    bool obstacleCenter = CastRay(0f, obstacleLayer);
    
    bool charge = CastRay(0f, detectionLayer);
    bool left = CastRay(nearSideAngle, detectionLayer);
    bool right = CastRay(-nearSideAngle, detectionLayer);

    float steering = 0f;
    float engineForce = -1500f;
    
    if (obstacleCenter || obstacleLeft || obstacleFarLeft) {
      steering = -0.5f;
    }
    else if (obstacleRight || obstacleFarRight) {
      steering = 0.5f;
    }
    else if (charge) {
      steering = 0f;
    }
    else if (left) {
      steering = 0.2f;
    }
    else if (right) {
      steering = -0.2f;
    }
    else {
      steering = Mathf.Sin(Time.time * 0.01f) * 0.2f;
    }

    _vehicle.Steering = steering;
    _vehicle.EngineForce = engineForce;
  }

  private bool CastRay(float angleOffset, LayerMask layer) {
    Vector3 direction = Quaternion.Euler(0, angleOffset, 0) * transform.forward;
    Ray ray = new Ray(transform.position + Vector3.up * 0.5f, direction);

    if (showDebugRays) {
      Color rayColor = Physics.Raycast(ray, rayDistance, layer) ? Color.red : Color.green;
      Debug.DrawRay(transform.position + Vector3.up * 0.5f, direction * rayDistance, rayColor);
    }

    return Physics.Raycast(ray, rayDistance, layer);
  }
  
  private bool CastRay(float angleOffset) {
    return CastRay(angleOffset, detectionLayer);
  }

  private void OnDrawGizmos() {
    if (!showDebugRays) return;

    Gizmos.color = Color.cyan;
    Gizmos.DrawWireSphere(transform.position, 0.2f);
  }
}