using UnityEngine;

[RequireComponent(typeof(VehicleBody3D))]
public class VehicleAI : MonoBehaviour {
  [Header("AI Behavior")] [SerializeField]
  private float moveForwardChance = 0.7f;

  [SerializeField] private float minThrottle = 0.3f;
  [SerializeField] private float maxThrottle = 1.0f;
  [SerializeField] private float maxEngineForce = 2000f;

  [Header("Steering Behavior")] [SerializeField]
  private float steeringChangeInterval = 1f;

  [SerializeField] private float maxSteeringAngle = 30f;

  [Header("Randomness")] [SerializeField]
  private float throttleChangeInterval = 2f;

  [SerializeField] private float stopDuration = 1f;

  private VehicleBody3D _vehicle;
  private float _currentThrottle;
  private float _currentSteering;

  private float _steeringTimer;
  private float _throttleTimer;
  private void Start() {
    _vehicle = GetComponent<VehicleBody3D>();

    // Start with random values
    RandomizeSteering();
    RandomizeThrottle();
  }

  private void Update() {
    _steeringTimer += Time.deltaTime;
    if (_steeringTimer >= steeringChangeInterval) {
      _steeringTimer = 0f;
      RandomizeSteering();
    }
    
    _throttleTimer += Time.deltaTime;
    if (_throttleTimer >= throttleChangeInterval) {
      _throttleTimer = 0f;
      RandomizeThrottle();
    }
    
    float engineForce = _currentThrottle * maxEngineForce;
    _vehicle.SetEngineForce(engineForce);

    float steeringAngle = _currentSteering * maxSteeringAngle * Mathf.Deg2Rad;
    _vehicle.SetSteering(steeringAngle);

    _vehicle.SetBrake(0f);
  }

  private void RandomizeSteering() {
    _currentSteering = Random.Range(-1f, 1f);
    
  }

  private void RandomizeThrottle() {
    if (Random.value < moveForwardChance) {
      _currentThrottle = Random.Range(minThrottle, maxThrottle);
      _throttleTimer = -throttleChangeInterval;
    }
    else {
      _currentThrottle = 0f;
      _throttleTimer = -stopDuration;
    }
  }
}