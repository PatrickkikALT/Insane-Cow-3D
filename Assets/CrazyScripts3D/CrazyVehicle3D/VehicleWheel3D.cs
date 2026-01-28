using System;
using Unity.Netcode;
using UnityEngine;

public class VehicleWheel3D : MonoBehaviour {
  [Header("Wheel Properties")] [SerializeField]
  private float radius = 0.5f;

  [SerializeField] private float suspensionRestLength = 0.15f;
  [SerializeField] private float maxSuspensionTravel = 0.2f;

  [Header("Suspension")] [SerializeField]
  private float suspensionStiffness = 5.88f;

  [SerializeField] private float wheelsDampingCompression = 0.83f;
  [SerializeField] private float wheelsDampingRelaxation = 0.88f;
  [SerializeField] private float maxSuspensionForce = 6000f;

  [Header("Friction")] [SerializeField] private float frictionSlip = 10.5f;
  [SerializeField] private float rollInfluence = 0.1f;

  [Header("Wheel Settings")] [SerializeField]
  private bool useAsTraction = false;

  [SerializeField] private bool useAsSteering = false;

  [Header("Visual")] [SerializeField] private Transform wheelMesh;
  
  private VehicleBody3D _body;
  private Vector3 _localPosition;

  private float _steering;
  private float _rotation;
  private float _deltaRotation;
  private float _rpm;
  private float _engineForce;
  private float _brake;

  private float _clippedInvContactDotSuspension = 1f;
  private float _suspensionRelativeVelocity;
  private float _wheelsSuspensionForce;
  private float _skidInfo;
  
  private Vector3 _contactNormalWs;
  private Vector3 _contactPointWs;
  private float _suspensionLength;
  private Vector3 _hardPointWs;
  private Vector3 _wheelDirectionWs;
  private Vector3 _wheelAxleWs;
  private bool _isInContact;
  private Rigidbody _groundObject;

  public void Initialize(VehicleBody3D vehicleBody) {
    _body = vehicleBody;
    _localPosition = vehicleBody.transform.InverseTransformPoint(transform.position);
  }
  

  public void UpdateVisualTransform() {
    if (!wheelMesh) return;

    Vector3 up = -_wheelDirectionWs;
    Vector3 right = _wheelAxleWs;
    Vector3 forward = Vector3.Cross(up, right).normalized;

    Vector3 origin = _hardPointWs + _wheelDirectionWs * _suspensionLength;

    Quaternion steeringRot = Quaternion.AngleAxis(_steering * Mathf.Rad2Deg, up);
    Quaternion rotatingRot = Quaternion.AngleAxis(_rotation * Mathf.Rad2Deg, right);

    Matrix4x4 basis = new Matrix4x4();
    basis.SetColumn(0, right);
    basis.SetColumn(1, up);
    basis.SetColumn(2, forward);
    basis.SetColumn(3, new Vector4(0, 0, 0, 1));

    Quaternion basisRot = basis.rotation;

    wheelMesh.position = origin;
    wheelMesh.rotation = steeringRot * rotatingRot * basisRot;
  }

  // Properties
  public float Radius {
    get => radius;
    set => radius = value;
  }

  public float SuspensionRestLength {
    get => suspensionRestLength;
    set => suspensionRestLength = value;
  }

  public float MaxSuspensionTravel {
    get => maxSuspensionTravel;
    set => maxSuspensionTravel = value;
  }

  public float SuspensionStiffness {
    get => suspensionStiffness;
    set => suspensionStiffness = value;
  }

  public float MaxSuspensionForce {
    get => maxSuspensionForce;
    set => maxSuspensionForce = value;
  }

  public float WheelsDampingCompression {
    get => wheelsDampingCompression;
    set => wheelsDampingCompression = value;
  }

  public float WheelsDampingRelaxation {
    get => wheelsDampingRelaxation;
    set => wheelsDampingRelaxation = value;
  }

  public float FrictionSlip {
    get => frictionSlip;
    set => frictionSlip = value;
  }

  public float RollInfluence {
    get => rollInfluence;
    set => rollInfluence = value;
  }

  public bool UseAsTraction {
    get => useAsTraction;
    set => useAsTraction = value;
  }

  public bool UseAsSteering {
    get => useAsSteering;
    set => useAsSteering = value;
  }

  public float EngineForce {
    get => _engineForce;
    set => _engineForce = value;
  }

  public float Brake {
    get => _brake;
    set => _brake = value;
  }

  public float Steering {
    get => _steering;
    set => _steering = value;
  }

  public float Rotation {
    get => _rotation;
    set => _rotation = value;
  }

  public float DeltaRotation {
    get => _deltaRotation;
    set => _deltaRotation = value;
  }

  public Vector3 ContactPoint => _contactPointWs;
  public Vector3 ContactNormal => _contactNormalWs;

  public Vector3 LocalPosition => _localPosition;

  public Vector3 HardPointWS {
    get => _hardPointWs;
    set => _hardPointWs = value;
  }

  public Vector3 WheelDirectionWS {
    get => _wheelDirectionWs;
    set => _wheelDirectionWs = value;
  }

  public Vector3 WheelAxleWS {
    get => _wheelAxleWs;
    set => _wheelAxleWs = value;
  }

  public Vector3 ContactNormalWS {
    get => _contactNormalWs;
    set => _contactNormalWs = value;
  }

  public Vector3 ContactPointWS {
    get => _contactPointWs;
    set => _contactPointWs = value;
  }

  public float SuspensionLength {
    get => _suspensionLength;
    set => _suspensionLength = value;
  }

  public float SuspensionRelativeVelocity {
    get => _suspensionRelativeVelocity;
    set => _suspensionRelativeVelocity = value;
  }

  public float ClippedInvContactDotSuspension {
    get => _clippedInvContactDotSuspension;
    set => _clippedInvContactDotSuspension = value;
  }

  public float WheelsSuspensionForce {
    get => _wheelsSuspensionForce;
    set => _wheelsSuspensionForce = value;
  }

  public bool IsInContact {
    get => _isInContact;
    set => _isInContact = value;
  }

  public Rigidbody GroundObject {
    get => _groundObject;
    set => _groundObject = value;
  }

  public float SkidInfo {
    get => _skidInfo;
    set => _skidInfo = value;
  }

  public float RPM {
    get => _rpm;
    set => _rpm = value;
  }
}