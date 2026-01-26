using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(VehicleBody3D))]
public class VehicleController : MonoBehaviour
{
  [Header("Input Settings")]
  [SerializeField] private float maxEngineForce = 2000f;
  [SerializeField] private float maxBrakeForce = 100f;
  [SerializeField] private float maxSteeringAngle = 30f;
  private float _throttle;
  private float _steer;
  private bool _handbrake;
  private VehicleBody3D vehicle;

  private void Start() {
    vehicle = GetComponent<VehicleBody3D>();
  }

  public void Throttle(InputAction.CallbackContext ctx) {
    _throttle = -ctx.ReadValue<float>();
  }

  public void Brake(InputAction.CallbackContext ctx) {
    _handbrake = ctx.performed;
  }

  public void Steer(InputAction.CallbackContext ctx) {
    _steer = ctx.ReadValue<float>();
  }

  private void Update() {
    float engineForce = _throttle * maxEngineForce;
    vehicle.SetEngineForce(engineForce);
    
    float steeringAngle = _steer * maxSteeringAngle * Mathf.Deg2Rad;
    vehicle.SetSteering(steeringAngle);
    
    float brakeForce = _handbrake ? maxBrakeForce : 0f;
    vehicle.SetBrake(brakeForce);
  }
}