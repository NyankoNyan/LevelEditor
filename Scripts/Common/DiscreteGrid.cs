using UnityEngine;

namespace Level.API
{
    public static class DiscreteGrid
    {
        public static Vector3Int RotateVector(Vector3Int vector, DiscreteAngle angle) =>
            Matrix3x3Int.FromDiscreteAngle(angle) * vector;

        public static Vector3Int RotateOffset(Vector3Int point, Vector3Int offset, DiscreteAngle angle) =>
            point + RotateVector(offset, angle);
    }
}