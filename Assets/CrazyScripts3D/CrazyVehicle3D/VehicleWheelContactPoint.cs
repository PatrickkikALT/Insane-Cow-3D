using UnityEngine;

public struct VehicleWheelContactPoint {
  public Rigidbody body1;
  public Vector3 frictionPositionWorld;
  public Vector3 frictionDirectionWorld;
  public float maxImpulse;
  public float jacDiagABInv;

  public VehicleWheelContactPoint(Rigidbody vehicleBody, Rigidbody groundBody, Vector3 frictionPosWorld,
    Vector3 frictionDirWorld, float maxImp) {
    body1 = groundBody;
    frictionPositionWorld = frictionPosWorld;
    frictionDirectionWorld = frictionDirWorld;
    maxImpulse = maxImp;

    float denom0 = 0;
    float denom1 = 0;

    if (vehicleBody) {
      Vector3 r0 = frictionPosWorld - vehicleBody.worldCenterOfMass;
      Vector3 c0 = Vector3.Cross(r0, frictionDirWorld);
      Vector3 vec = vehicleBody.inertiaTensorRotation * Vector3.Scale(vehicleBody.inertiaTensor,
        Quaternion.Inverse(vehicleBody.inertiaTensorRotation) * c0);
      vec = Vector3.Cross(vec, r0);
      denom0 = vehicleBody.mass > 0 ? (1f / vehicleBody.mass) : 0;
      denom0 += Vector3.Dot(frictionDirWorld, vec);
    }

    float relaxation = 1f;
    jacDiagABInv = relaxation / (denom0 + denom1);
  }
}