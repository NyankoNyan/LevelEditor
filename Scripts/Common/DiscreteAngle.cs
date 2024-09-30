using System;

using UnityEngine;

namespace Level.API
{
    public enum DiscreteAngle
    {
        // Up vector around (default)

        U0 = 0,
        U90 = 1,
        U180 = 2,
        U270 = 3,

        // Other sides (for full 3d rotation)

        L0 = 10,
        L90 = 11,
        L180 = 12,
        L270 = 13,

        R0 = 20,
        R90 = 21,
        R180 = 22,
        R270 = 23,

        F0 = 30,
        F90 = 31,
        F180 = 32,
        F270 = 33,

        B0 = 40,
        B90 = 41,
        B180 = 42,
        B270 = 43,

        D0 = 50,
        D90 = 51,
        D180 = 52,
        D270 = 53,
    }

    public static class DiscreteAngleExtension
    {
        public static byte Compress(this DiscreteAngle angle)
        {
            int mainComp = (int)angle / 10;
            int secComp = (int)angle % 10;

            return (byte)((secComp << 3) + mainComp);
        }

        public static DiscreteAngle ToDiscreteAngle(this byte v)
        {
            return (DiscreteAngle)((v & 7) * 10 + ((v & 24) >> 3));
        }

        public static Quaternion ToQuaternion(this DiscreteAngle angle)
        {
            int mainComp = (int)angle / 10;
            int secComp = (int)angle % 10;

            Quaternion mainRot = mainComp switch {
                0 => Quaternion.identity,
                1 => Quaternion.AngleAxis(90, Vector3.forward),
                2 => Quaternion.AngleAxis(-90, Vector3.forward),
                3 => Quaternion.AngleAxis(90, Vector3.right),
                4 => Quaternion.AngleAxis(-90, Vector3.right),
                5 => Quaternion.AngleAxis(180, Vector3.right),
                _ => throw new ArgumentException(((int)angle).ToString()),
            };

            Quaternion secRot = secComp switch {
                0 => Quaternion.identity,
                1 => Quaternion.AngleAxis(90, Vector3.up),
                2 => Quaternion.AngleAxis(180, Vector3.up),
                3 => Quaternion.AngleAxis(270, Vector3.up),
                _ => throw new ArgumentException(((int)angle).ToString()),
            };

            return mainRot * secRot;
        }

        public static DiscreteAngle Mult(this DiscreteAngle v1, DiscreteAngle v2) => 
            (Matrix3x3Int.FromDiscreteAngle(v1) * Matrix3x3Int.FromDiscreteAngle(v2)).Angle;
    }
}