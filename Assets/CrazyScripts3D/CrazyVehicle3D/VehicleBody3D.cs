using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class VehicleBody3D : NetworkBehaviour {
  [Header("Motion Controls")] 
  [SerializeField] private float engineForce;
  [SerializeField] private float brake;
  [SerializeField] private float steering;

  [Header("Physics Settings")] 
  [SerializeField] private LayerMask collisionMask = -1;

  private Rigidbody _rigidBody;
  private Collider _collider;
  public List<VehicleWheel3D> wheels = new();

  private List<Vector3> _forwardWS = new();
  private List<Vector3> _axle = new();
  private List<float> _forwardImpulse = new();
  private List<float> _sideImpulse = new();
  private const float SideFrictionStiffness = 1.0f;
  
  public GameObject mesh;
  
  public GameObject dead;
  public Camera camera;

  private GameObject _lastHitPlayer;
  private int _kills;
  public void AddKill() => _kills++;
  public int GetKills => _kills;
  private bool _isDead;

  [Header("Defaults")] 
  public MaskStat defaultStats;
  public GameObject defaultMesh;
  
  private VehicleController _vehicleController;

  public override void OnNetworkSpawn() {
    _rigidBody = GetComponent<Rigidbody>();
    _rigidBody.isKinematic = !IsServer && !IsOwner;
    _collider = GetComponent<Collider>();
    _vehicleController = GetComponent<VehicleController>();
    foreach (VehicleWheel3D wheel in wheels) {
      if (wheel) wheel.Initialize(this);
    }

    if (camera) {
      camera.gameObject.SetActive(IsOwner);
    }
  }

  public void OnCollisionEnter(Collision other) {
    if (!IsServer || _isDead) return;
    if (other.gameObject.CompareTag("Entity") || other.gameObject != gameObject) {
      if (TryGetComponent(out Mask mask)) {
        if (other.gameObject.TryGetComponent(out NetworkObject hitNetObj)) {
          mask.HitEffect(other.gameObject);
          
          ApplyMaskHitEffectClientRpc(mask.maskName, hitNetObj.NetworkObjectId, transform.position);
        }
      }
    }
    _lastHitPlayer = other.gameObject;
  }
  
  [ClientRpc]
  private void ApplyMaskHitEffectClientRpc(string maskType, ulong hitObjectId, Vector3 hitPosition) {
    if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(hitObjectId, out NetworkObject networkObject)) {
      if (networkObject.TryGetComponent(out Rigidbody rb)) {
        switch (maskType) {
          case "TheLionDoesNotCareAboutGameBalance":
            rb.AddExplosionForce(50000, hitPosition, 10, 10f, ForceMode.Impulse);
            break;
        }
      }
    }
  }

  public void Death() {
    if (!IsServer || _isDead) return;
    _isDead = true;
    Instantiate(dead, transform.position + transform.up, transform.rotation);
    MaskManager.Instance.SpawnRandomMask(transform.position + transform.up);
    StartCoroutine(RespawnCoroutine());
    if (_lastHitPlayer.TryGetComponent(out VehicleBody3D vehicleBody)) {
      vehicleBody.AddKill();
    }
    print("Dead");
  }

  private IEnumerator RespawnCoroutine() {
    _collider.enabled = false;
    mesh.SetActive(false);
    _rigidBody.isKinematic = true;
    //move body away so it doesnt do weird physics bug
    transform.position = new Vector3(0, 100, 0);
    
    
    yield return new WaitForSeconds(5f);
    var pos = CrazyGameManager3D.Instance.spawnBox.GetRandomPosition(CrazyGameManager3D.Instance.transform.position);
    pos.y = 1;
    transform.position = pos; 
    transform.rotation = Quaternion.identity;
    ReplaceMesh();
    _rigidBody.isKinematic = false;
    _collider.enabled = true;
    _isDead = false;
  }

  private void ReplaceMesh() {
    Vector3 pos = transform.position;
    Quaternion rot = transform.rotation;
      
    GameObject obj = Instantiate(defaultMesh, pos, rot);
    GameObject oldObj = mesh;
      
    obj.transform.SetParent(oldObj.transform.parent);
    obj.transform.localScale = oldObj.transform.localScale;
    Destroy(oldObj);
    mesh = obj;
    if (_vehicleController) {
      _vehicleController.SetStats(defaultStats);
    }

    if (camera) {
      camera.transform.localPosition = defaultStats.cameraPosition;
    }
  }

  private void FixedUpdate() {
    if (!IsSpawned || (!IsServer && !IsOwner) || _isDead) return;

    float zRotation = transform.eulerAngles.z;
    if (zRotation > 180f) {
      zRotation -= 360f;  
    }

    if (Mathf.Abs(zRotation) >= 89f) {
      Death();
      return;
    }
    float deltaTime = Time.fixedDeltaTime;

    for (int i = 0; i < wheels.Count; i++) {
      UpdateWheel(i);
    }

    for (int i = 0; i < wheels.Count; i++) {
      RayCast(i);
    }

    UpdateSuspension(deltaTime);

    foreach (VehicleWheel3D wheel in wheels) {
      float suspensionForce = wheel.WheelsSuspensionForce;

      if (suspensionForce > wheel.MaxSuspensionForce) {
        suspensionForce = wheel.MaxSuspensionForce;
      }

      Vector3 impulse = wheel.ContactNormalWS * (suspensionForce * deltaTime);
      Vector3 relativePosition = wheel.ContactPointWS - _rigidBody.worldCenterOfMass;

      _rigidBody.AddForceAtPosition(impulse, wheel.ContactPointWS, ForceMode.Impulse);
    }

    UpdateFriction(deltaTime);

    foreach (VehicleWheel3D wheel in wheels) {
      Vector3 relpos = wheel.HardPointWS - _rigidBody.worldCenterOfMass;
      Vector3 vel = _rigidBody.linearVelocity + Vector3.Cross(_rigidBody.angularVelocity, relpos);

      if (wheel.IsInContact) {
        Vector3 fwd = transform.forward;
        if (wheel.UseAsSteering) {
          Quaternion steeringRot = Quaternion.AngleAxis(wheel.Steering * Mathf.Rad2Deg, Vector3.up);
          fwd = steeringRot * fwd;
        }

        float proj = Vector3.Dot(fwd, wheel.ContactNormalWS);
        fwd -= wheel.ContactNormalWS * proj;
        fwd.Normalize();

        float proj2 = Vector3.Dot(fwd, vel);

        wheel.DeltaRotation = (proj2 * deltaTime) / wheel.Radius;
      }

      wheel.Rotation += wheel.DeltaRotation;
      wheel.RPM = ((wheel.DeltaRotation / deltaTime) * 60f) / (2f * Mathf.PI);
      wheel.DeltaRotation *= 0.99f;

      wheel.UpdateVisualTransform();
    }
  }

  private void UpdateWheel(int wheelIndex) {
    VehicleWheel3D wheel = wheels[wheelIndex];

    Vector3 chassisConnectionPoint = transform.TransformPoint(wheel.LocalPosition);
    Vector3 wheelDirection = -transform.up;
    Vector3 wheelAxle = transform.right;

    wheel.HardPointWS = chassisConnectionPoint;
    wheel.WheelDirectionWS = wheelDirection;
    wheel.WheelAxleWS = wheelAxle;
  }

  private void RayCast(int wheelIndex) {
    VehicleWheel3D wheel = wheels[wheelIndex];

    float rayLength = wheel.SuspensionRestLength + wheel.Radius;
    Vector3 rayVector = wheel.WheelDirectionWS * rayLength;
    Vector3 source = wheel.HardPointWS - wheel.Radius * wheel.WheelDirectionWS;
    Vector3 target = source + rayVector;

    RaycastHit hit;
    wheel.IsInContact = false;
    wheel.GroundObject = null;

    if (Physics.Raycast(source, rayVector.normalized, out hit, rayLength, collisionMask)) {
      float param = hit.distance / rayLength;
      float depth = rayLength * param;

      wheel.ContactNormalWS = hit.normal;
      wheel.IsInContact = true;
      wheel.GroundObject = hit.rigidbody;

      float hitDistance = param * rayLength;
      wheel.SuspensionLength = hitDistance - wheel.Radius;

      float minSuspensionLength = wheel.SuspensionRestLength - wheel.MaxSuspensionTravel;
      float maxSuspensionLength = wheel.SuspensionRestLength + wheel.MaxSuspensionTravel;
      wheel.SuspensionLength = Mathf.Clamp(wheel.SuspensionLength, minSuspensionLength, maxSuspensionLength);

      wheel.ContactPointWS = hit.point;

      float denominator = Vector3.Dot(wheel.ContactNormalWS, wheel.WheelDirectionWS);

      Vector3 chassisVelocityAtContactPoint = _rigidBody.linearVelocity +
                                              Vector3.Cross(_rigidBody.angularVelocity,
                                                wheel.ContactPointWS - _rigidBody.worldCenterOfMass);

      float projVel = Vector3.Dot(wheel.ContactNormalWS, chassisVelocityAtContactPoint);

      if (denominator >= -0.1f) {
        wheel.SuspensionRelativeVelocity = 0f;
        wheel.ClippedInvContactDotSuspension = 1f / 0.1f;
      }
      else {
        float inv = -1f / denominator;
        wheel.SuspensionRelativeVelocity = projVel * inv;
        wheel.ClippedInvContactDotSuspension = inv;
      }
    }
    else {
      wheel.IsInContact = false;
      wheel.SuspensionLength = wheel.SuspensionRestLength;
      wheel.SuspensionRelativeVelocity = 0f;
      wheel.ContactNormalWS = -wheel.WheelDirectionWS;
      wheel.ClippedInvContactDotSuspension = 1f;
    }
  }

  private void UpdateSuspension(float deltaTime) {
    float chassisMass = _rigidBody.mass;

    foreach (VehicleWheel3D wheel in wheels) {
      if (wheel.IsInContact) {
        float force;

        float suspLength = wheel.SuspensionRestLength;
        float currentLength = wheel.SuspensionLength;
        float lengthDiff = suspLength - currentLength;

        force = wheel.SuspensionStiffness * lengthDiff * wheel.ClippedInvContactDotSuspension;

        float projectedRelVel = wheel.SuspensionRelativeVelocity;
        float suspDamping;

        suspDamping = projectedRelVel < 0f ? wheel.WheelsDampingCompression : wheel.WheelsDampingRelaxation;

        force -= suspDamping * projectedRelVel;

        wheel.WheelsSuspensionForce = force * chassisMass;
        if (wheel.WheelsSuspensionForce < 0f) {
          wheel.WheelsSuspensionForce = 0f;
        }
      }
      else {
        wheel.WheelsSuspensionForce = 0f;
      }
    }
  }

  private void UpdateFriction(float deltaTime) {
    int numWheels = wheels.Count;
    if (numWheels == 0) return;

    _forwardWS.Clear();
    _axle.Clear();
    _forwardImpulse.Clear();
    _sideImpulse.Clear();

    for (int i = 0; i < numWheels; i++) {
      _forwardWS.Add(Vector3.zero);
      _axle.Add(Vector3.zero);
      _forwardImpulse.Add(0f);
      _sideImpulse.Add(0f);
    }

    for (int i = 0; i < numWheels; i++) {
      VehicleWheel3D wheel = wheels[i];

      if (wheel.IsInContact) {
        Vector3 axleVec;

        if (wheel.UseAsSteering) {
          Quaternion steeringRot = Quaternion.AngleAxis(wheel.Steering * Mathf.Rad2Deg, transform.up);
          axleVec = steeringRot * transform.right;
        }
        else {
          axleVec = transform.right;
        }

        Vector3 surfNormalWS = wheel.ContactNormalWS;
        float proj = Vector3.Dot(axleVec, surfNormalWS);
        axleVec -= surfNormalWS * proj;
        axleVec.Normalize();

        _axle[i] = axleVec;
        _forwardWS[i] = Vector3.Cross(surfNormalWS, axleVec).normalized;

        float sideImpulseValue = 0f;
        ResolveSingleBilateral(wheel.ContactPointWS, wheel.GroundObject, wheel.ContactPointWS,
          axleVec, ref sideImpulseValue, wheel.RollInfluence, deltaTime);

        _sideImpulse[i] = sideImpulseValue * SideFrictionStiffness;
      }
    }

    float sideFactor = 1f;
    float fwdFactor = 0.5f;
    bool sliding = false;

    for (int i = 0; i < numWheels; i++) {
      VehicleWheel3D wheel = wheels[i];
      float rollingFriction = 0f;

      if (wheel.IsInContact) {
        if (wheel.EngineForce != 0f) {
          rollingFriction = wheel.EngineForce * deltaTime;
        }
        else {
          float maxImpulse = wheel.Brake != 0f ? wheel.Brake : 0f;
          VehicleWheelContactPoint contactPt = new VehicleWheelContactPoint(
            _rigidBody, wheel.GroundObject, wheel.ContactPointWS, _forwardWS[i], maxImpulse);
          rollingFriction = CalcRollingFriction(contactPt);
        }
      }

      _forwardImpulse[i] = 0f;
      wheel.SkidInfo = 1f;

      if (wheel.IsInContact) {
        wheel.SkidInfo = 1f;

        float maximp = wheel.WheelsSuspensionForce * deltaTime * wheel.FrictionSlip;
        float maximpSide = maximp;
        float maximpSquared = maximp * maximpSide;

        _forwardImpulse[i] = rollingFriction;

        float x = _forwardImpulse[i] * fwdFactor;
        float y = _sideImpulse[i] * sideFactor;

        float impulseSquared = x * x + y * y;

        if (impulseSquared > maximpSquared) {
          sliding = true;
          float factor = maximp / Mathf.Sqrt(impulseSquared);
          wheel.SkidInfo *= factor;
        }
      }
    }

    if (sliding) {
      for (int i = 0; i < numWheels; i++) {
        if (_sideImpulse[i] != 0f) {
          if (wheels[i].SkidInfo < 1f) {
            _forwardImpulse[i] *= wheels[i].SkidInfo;
            _sideImpulse[i] *= wheels[i].SkidInfo;
          }
        }
      }
    }

    for (int i = 0; i < numWheels; i++) {
      VehicleWheel3D wheel = wheels[i];
      Vector3 relPos = wheel.ContactPointWS - _rigidBody.worldCenterOfMass;

      if (_forwardImpulse[i] != 0f) {
        _rigidBody.AddForceAtPosition(_forwardWS[i] * _forwardImpulse[i], wheel.ContactPointWS, ForceMode.Impulse);
      }

      if (_sideImpulse[i] != 0f) {
        Vector3 sideImp = _axle[i] * _sideImpulse[i];
        Vector3 vChassisWorldUp = transform.up;
        relPos -= vChassisWorldUp * (Vector3.Dot(vChassisWorldUp, relPos) * (1f - wheel.RollInfluence));

        _rigidBody.AddForceAtPosition(sideImp, wheel.ContactPointWS + relPos, ForceMode.Impulse);
      }
    }
  }

  private void ResolveSingleBilateral(Vector3 pos1, Rigidbody body2, Vector3 pos2,
    Vector3 normal, ref float impulse, float rollInfluence, float deltaTime) {
    float normalLenSqr = normal.sqrMagnitude;

    if (normalLenSqr > 1.1f) {
      impulse = 0f;
      return;
    }

    Vector3 relPos1 = pos1 - _rigidBody.worldCenterOfMass;
    Vector3 relPos2 = body2 ? pos2 - body2.worldCenterOfMass : Vector3.zero;

    Vector3 vel1 = _rigidBody.linearVelocity + Vector3.Cross(_rigidBody.angularVelocity, relPos1);
    Vector3 vel2 = body2 ? body2.linearVelocity + Vector3.Cross(body2.angularVelocity, relPos2) : Vector3.zero;
    Vector3 vel = vel1 - vel2;

    float relVel = Vector3.Dot(normal, vel);
    float contactDamping = 0.2f;

    if (rollInfluence > 0f) {
      contactDamping = Mathf.Min(contactDamping, deltaTime / rollInfluence);
    }

    float mass1 = _rigidBody.mass > 0 ? _rigidBody.mass : 0f;
    float mass2 = body2 && body2.mass > 0 ? body2.mass : 0f;
    float invMass1 = mass1 > 0 ? 1f / mass1 : 0f;
    float invMass2 = mass2 > 0 ? 1f / mass2 : 0f;

    float massTerm = 1f / (invMass1 + invMass2);
    impulse = -contactDamping * relVel * massTerm;
  }

  private float CalcRollingFriction(VehicleWheelContactPoint contactPoint) {
    Vector3 contactPosWorld = contactPoint.frictionPositionWorld;
    Vector3 relPos1 = contactPosWorld - _rigidBody.worldCenterOfMass;
    Vector3 relPos2 = contactPoint.body1
      ? contactPosWorld - contactPoint.body1.worldCenterOfMass
      : Vector3.zero;

    float maxImpulse = contactPoint.maxImpulse;

    Vector3 vel1 = _rigidBody.linearVelocity + Vector3.Cross(_rigidBody.angularVelocity, relPos1);
    Vector3 vel2 = contactPoint.body1
      ? contactPoint.body1.linearVelocity + Vector3.Cross(contactPoint.body1.angularVelocity, relPos2)
      : Vector3.zero;
    Vector3 vel = vel1 - vel2;

    float vrel = Vector3.Dot(contactPoint.frictionDirectionWorld, vel);
    float j1 = -vrel * contactPoint.jacDiagABInv;

    return Mathf.Clamp(j1, -maxImpulse, maxImpulse);
  }

  public void SetEngineForce(float force) {
    engineForce = force;
    foreach (VehicleWheel3D wheel in wheels) {
      wheel.EngineForce = force;
    }
  }

  public void SetBrake(float brakeForce) {
    brake = brakeForce;
    foreach (var wheel in wheels) {
      wheel.Brake = brakeForce;
    }
  }

  public void SetSteering(float steerAngle) {
    steering = steerAngle;
    foreach (VehicleWheel3D wheel in wheels) {
      wheel.Steering = steerAngle;
    }
  }

  public float EngineForce {
    get => engineForce;
    set => SetEngineForce(value);
  }

  public float Brake {
    get => brake;
    set => SetBrake(value);
  }

  public float Steering {
    get => steering;
    set => SetSteering(value);
  }

  public Rigidbody RigidBody => _rigidBody;

  private void OnDrawGizmos() {
    if (!Application.isPlaying || wheels == null) return;

    foreach (VehicleWheel3D wheel in wheels) {
      if (!wheel) continue;

      Vector3 source = wheel.HardPointWS - wheel.Radius * wheel.WheelDirectionWS;
      float rayLength = wheel.SuspensionRestLength + wheel.Radius;
      Vector3 target = source + wheel.WheelDirectionWS * rayLength;

      Gizmos.color = wheel.IsInContact ? Color.green : Color.red;
      Gizmos.DrawLine(source, target);
      Gizmos.DrawWireSphere(source, 0.05f);

      if (wheel.IsInContact) {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(wheel.ContactPointWS, 0.1f);
      }
    }
  }
}