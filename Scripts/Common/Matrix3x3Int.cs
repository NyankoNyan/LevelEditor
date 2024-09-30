using System;
using System.Collections.Generic;

using UnityEngine;

namespace Level.API
{
    public struct Matrix3x3Int : IEquatable<Matrix3x3Int>
    {
        public static readonly Matrix3x3Int U0 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.U0);
        public static readonly Matrix3x3Int U90 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.U90);
        public static readonly Matrix3x3Int U180 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.U180);
        public static readonly Matrix3x3Int U270 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.U270);

        public static readonly Matrix3x3Int L0 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.L0);
        public static readonly Matrix3x3Int L90 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.L90);
        public static readonly Matrix3x3Int L180 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.L180);
        public static readonly Matrix3x3Int L270 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.L270);

        public static readonly Matrix3x3Int R0 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.R0);
        public static readonly Matrix3x3Int R90 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.R90);
        public static readonly Matrix3x3Int R180 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.R180);
        public static readonly Matrix3x3Int R270 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.R270);

        public static readonly Matrix3x3Int F0 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.F0);
        public static readonly Matrix3x3Int F90 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.F90);
        public static readonly Matrix3x3Int F180 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.F180);
        public static readonly Matrix3x3Int F270 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.F270);

        public static readonly Matrix3x3Int B0 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.B0);
        public static readonly Matrix3x3Int B90 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.B90);
        public static readonly Matrix3x3Int B180 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.B180);
        public static readonly Matrix3x3Int B270 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.B270);

        public static readonly Matrix3x3Int D0 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.D0);
        public static readonly Matrix3x3Int D90 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.D90);
        public static readonly Matrix3x3Int D180 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.D180);
        public static readonly Matrix3x3Int D270 = Matrix3x3Int.CreateFromDiscreteAngle(DiscreteAngle.D270);

        private static Dictionary<int, DiscreteAngle> _hashes;

#pragma warning disable IDE1006 // Стили именования
        public sbyte[] m;
#pragma warning restore IDE1006 // Стили именования

        static Matrix3x3Int()
        {
            _hashes = new();
            foreach (DiscreteAngle angle in Enum.GetValues(typeof(DiscreteAngle))) {
                _hashes.Add(CreateFromDiscreteAngle(angle).GetHashCode(), angle);
            }
        }

        public Matrix3x3Int(sbyte[] m)
        {
            if (m is null || m.Length != 9) {
                throw new Exception("Wrong matrix members");
            }

            this.m = (sbyte[])m.Clone();
        }

        public Vector3Int Mult(Vector3Int v)
        {
            return new Vector3Int(
                v.x * m[0] + v.y * m[1] + v.z * m[2],
                v.x * m[3] + v.y * m[4] + v.z * m[5],
                v.x * m[6] + v.y * m[7] + v.z * m[8]);
        }

        public static Vector3Int operator *(Matrix3x3Int m, Vector3Int v)
        {
            return m.Mult(v);
        }

        public static Matrix3x3Int operator *(Matrix3x3Int m1, Matrix3x3Int m2)
        {
            var m3 = new sbyte[9];
            for (int i = 0; i < 9; i++) {
                m3[i] = 0;
                for (int j = 0; j < 3; j++) {
                    m3[i] += (sbyte)(m1.m[i - i % 3 + j] * m2.m[i % 3 + j * 3]);
                }
            }
            return new Matrix3x3Int(m3);
        }

        private static Matrix3x3Int CreateFromDiscreteAngle(DiscreteAngle angle)
        {
            int mainComp = (int)angle / 10;
            int secComp = (int)angle % 10;

            var mainM = mainComp switch {
                0 => new Matrix3x3Int(new sbyte[] {
                        1, 0, 0,
                        0, 1, 0,
                        0, 0, 1,
                    }),
                1 => new Matrix3x3Int(new sbyte[] {
                        0, -1, 0,
                        1, 0, 0,
                        0, 0, 1,
                    }),
                2 => new Matrix3x3Int(new sbyte[] {
                        0, 1, 0,
                        -1, 0, 0,
                        0, 0, 1,
                    }),
                3 => new Matrix3x3Int(new sbyte[] {
                        1, 0, 0,
                        0, 0, -1,
                        0, 1, 0,
                    }),
                4 => new Matrix3x3Int(new sbyte[] {
                        1, 0, 0,
                        0, 0, 1,
                        0, -1, 0,
                    }),
                5 => new Matrix3x3Int(new sbyte[] {
                        1, 0, 0,
                        0, -1, 0,
                        0, 0, -1,
                    }),
                _ => throw new ArgumentException(((int)angle).ToString()),
            };

            var secM = secComp switch {
                0 => new Matrix3x3Int(new sbyte[] {
                        1, 0, 0,
                        0, 1, 0,
                        0, 0, 1,
                    }),
                1 => new Matrix3x3Int(new sbyte[] {
                        0, 0, 1,
                        0, 1, 0,
                        -1, 0, 0,
                    }),
                2 => new Matrix3x3Int(new sbyte[] {
                        -1, 0, 0,
                        0, 1, 0,
                        0, 0, -1,
                    }),
                3 => new Matrix3x3Int(new sbyte[] {
                        0, 0, -1,
                        0, 1, 0,
                        1, 0, 0,
                    }),
                _ => throw new ArgumentException(((int)angle).ToString()),
            };
            return mainM * secM;
        }

        public static Matrix3x3Int FromDiscreteAngle(DiscreteAngle angle)
        {
            return angle switch {
                DiscreteAngle.U0 => U0,
                DiscreteAngle.U90 => U90,
                DiscreteAngle.U180 => U180,
                DiscreteAngle.U270 => U270,
                DiscreteAngle.L0 => L0,
                DiscreteAngle.L90 => L90,
                DiscreteAngle.L180 => L180,
                DiscreteAngle.L270 => L270,
                DiscreteAngle.R0 => R0,
                DiscreteAngle.R90 => R90,
                DiscreteAngle.R180 => R180,
                DiscreteAngle.R270 => R270,
                DiscreteAngle.F0 => F0,
                DiscreteAngle.F90 => F90,
                DiscreteAngle.F180 => F180,
                DiscreteAngle.F270 => F270,
                DiscreteAngle.B0 => B0,
                DiscreteAngle.B90 => B90,
                DiscreteAngle.B180 => B180,
                DiscreteAngle.B270 => B270,
                DiscreteAngle.D0 => D0,
                DiscreteAngle.D90 => D90,
                DiscreteAngle.D180 => D180,
                DiscreteAngle.D270 => D270,
                _ => throw new ArgumentException(((int)angle).ToString()),
            };
        }

        public bool Equals(Matrix3x3Int other)
        {
            for (int i = 0; i < 9; i++) {
                if (m[i] != other.m[i]) {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            int packed = 0;
            for (int i = 0; i < 9; i++) {
                packed = (packed << 2) | (m[i] + 1);
            }
            return packed.GetHashCode();
        }

        public DiscreteAngle Angle => _hashes[GetHashCode()];
    }
}