using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[RequireComponent(typeof(VehicleBody3D))]
public class VehicleController : NetworkBehaviour {
  [Header("Input Settings")] 
  private float _maxEngineForce = 2000f;
  private float _maxBrakeForce = 100f;
  private float _maxSteeringAngle = 30f;
  private float _throttle;
  private float _steer;
  private bool _handbrake;
  private VehicleBody3D _vehicle;

  private void Start() {
    _vehicle = GetComponent<VehicleBody3D>();
  }

  public void SetStats(MaskStat stats) {
    _maxEngineForce = stats.maxEngineForce;
    _maxBrakeForce = stats.maxBrakeForce;
    _maxSteeringAngle = stats.maxSteeringAngle;
    GetComponent<Rigidbody>().mass = stats.rigidbodyMass;
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
    if (!IsOwner) return;

    UpdateInputServerRpc(_throttle, _steer, _handbrake);
  }

  [ServerRpc]
  private void UpdateInputServerRpc(float throttle, float steer, bool handbrake) {
    float engineForce = throttle * _maxEngineForce;
    _vehicle.SetEngineForce(engineForce);

    float steeringAngle = steer * _maxSteeringAngle * Mathf.Deg2Rad;
    _vehicle.SetSteering(steeringAngle);

    float brakeForce = handbrake ? _maxBrakeForce : 0f;
    _vehicle.SetBrake(brakeForce);
  }
}