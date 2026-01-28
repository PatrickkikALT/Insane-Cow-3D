using JetBrains.Annotations;
using UnityEngine;

public static class Extensions {
  public static Quaternion Rand(this Quaternion q, bool x, bool y, bool z) {
    Quaternion def = Quaternion.identity;
    int x1 = x ? Random.Range(0, 360) : (int)def.x;
    int y1 = y ? Random.Range(0, 360) : (int)def.y;
    int z1 = z ? Random.Range(0, 360) : (int)def.z;
    return new Quaternion(x1, y1, z1, 1);
  }
}
