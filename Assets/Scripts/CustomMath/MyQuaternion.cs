using System;
using UnityEngine;

namespace CustomMath
{
    [Serializable]
    public class MyQuaternion : IEquatable<MyQuaternion>, IFormattable
    {
        #region Variables

        public float x;
        public float y;
        public float z;
        public float w;

        public Vec3 eulerAngles
        {
            get
            {
                // pointer to Cuaterniones_y_unity.pptm.pdf page 9-14

                float xValue = x * w - y * z;
                Vec3 result = Vec3.Zero;

                if (xValue > 0.4999f * sqrMagnitude)
                {
                    result.y = 2f * Mathf.Atan2(y, x);
                    result.x = Mathf.PI / 2;
                    result.z = 0;
                    return result * Mathf.Rad2Deg;
                }

                if (xValue < -0.4999f * sqrMagnitude)
                {
                    result.y = -2f * Mathf.Atan2(y, x);
                    result.x = -Mathf.PI / 2;
                    result.z = 0;
                    return result * Mathf.Rad2Deg;
                }

                MyQuaternion quaternion = new MyQuaternion(w, z, x, y);
                result.y = Mathf.Atan2(2f * quaternion.x * quaternion.w + 2f * quaternion.y * quaternion.z, 1f - 2f * (quaternion.z * quaternion.z + quaternion.w * quaternion.w));
                result.x = Mathf.Asin(2f * (quaternion.x * quaternion.z - quaternion.w * quaternion.y));
                result.z = Mathf.Atan2(2f * quaternion.x * quaternion.y + 2f * quaternion.z * quaternion.w, 1f - 2f * (quaternion.y * quaternion.y + quaternion.z * quaternion.z));
                return result * Mathf.Rad2Deg;
            }
            set
            {
                MyQuaternion q = Euler(value);
                this.Set(q.x, q.y, q.z, q.w);
            }
        }

        public float sqrMagnitude
        {
            get { return w * w + x * x + y * y + z * z; }
        }

        public float magnitude
        {
            get { return MathF.Sqrt(sqrMagnitude); }
        }

        public MyQuaternion normalized
        {
            get { return new MyQuaternion(x / magnitude, y / magnitude, z / magnitude, w / magnitude); }
        }

        public Quaternion toQuaternion
        {
            get { return new Quaternion(x, y, z, w); }
            set
            {
                x = value.x;
                y = value.y;
                z = value.z;
                w = value.w;
            }
        }

        #endregion

        #region Constants

        public const float kEpsilon = 1E-06F;

        #endregion

        #region DefaultValues

        public static MyQuaternion identity
        {
            get { return new MyQuaternion(0, 0, 0, 1); }
        }

        #endregion

        #region Constructors

        public MyQuaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public MyQuaternion(Quaternion q)
        {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;
        }

        #endregion

        #region Operators

        public static Vec3 operator *(MyQuaternion rotation, Vec3 point)
        {
            //https://automaticaddison.com/how-to-convert-a-quaternion-to-a-rotation-matrix/#Convert_a_Quaternion_to_a_Rotation_Matrix
            //https://en.wikipedia.org/wiki/Rotation_matrix

            MY4X4 myRotationMatrix = MY4X4.Rotate(rotation);
            Vector4 pointV4 = new Vector4(point.x, point.y, point.z, 1);

            Vector4 result = myRotationMatrix * pointV4;
            return new Vec3(result.x, result.y, result.z);
        }

        public static MyQuaternion operator *(MyQuaternion lhs, MyQuaternion rhs)
        {
            // https://stackoverflow.com/questions/19956555/how-to-multiply-two-quaternions
            //i^2 = -1
            //i^3 = -i
            //i^4 = 1

            //    mul reales| x1i*x2i  |  y1j*y2j  | z1k* z2k
            //    a.w* b.w - a.x * b.x - a.y * b.y - a.z * b.z,  // 1

            //    a.w* b.x + a.x * b.w + a.y * b.z - a.z * b.y,  // i
            //    a.w* b.y - a.x * b.z + a.y * b.w + a.z * b.x,  // j
            //    a.w* b.z + a.x * b.y - a.y * b.x + a.z * b.w   // k

            // rearrange it to fit into my format x y z w in this case i j k 1
            return new MyQuaternion(
            lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y,
            lhs.w * rhs.y - lhs.x * rhs.z + lhs.y * rhs.w + lhs.z * rhs.x,
            lhs.w * rhs.z + lhs.x * rhs.y - lhs.y * rhs.x + lhs.z * rhs.w,
            lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z);
        }

        public static bool operator ==(MyQuaternion lhs, MyQuaternion rhs)
        {
            float diff_x = lhs.x - rhs.x;
            float diff_y = lhs.y - rhs.y;
            float diff_z = lhs.z - rhs.z;
            float diff_w = lhs.w - rhs.w;
            float sqrmag = diff_x * diff_x + diff_y * diff_y + diff_z * diff_z + diff_w * diff_w;
            //Checks if the difference between both vectors is close to zero
            return sqrmag < kEpsilon * kEpsilon;
        }

        public static bool operator !=(MyQuaternion lhs, MyQuaternion rhs)
        {
            return !(lhs == rhs);
        }

        #endregion

        #region Functions

        public static float Angle(MyQuaternion a, MyQuaternion b)
        {
            //  Pointer to Cuaterniones_y_unity.pptm.pdf Page 15
            float dotAbs = Mathf.Abs(Dot(a, b));
            return a == b ? 0.0f : Mathf.Acos(Mathf.Min(dotAbs, 1.0f)) * 2 * Mathf.Rad2Deg;
        }

        public static MyQuaternion AngleAxis(float angle, Vec3 axis)
        {
            axis.Normalize();
            // we divide the angle by the amount of axis we operate minus one because the angle gets distributed through all the axis we operate
            // this distribution takes place so the scalar is the amount of axis we can rotate around minus 1, making a 1D rotation not work by trying to divide by 0
            // 2D has the angle divided by 1 so it can be simplified
            // 3D has the angle divided by 2
            axis *= Mathf.Sin(angle * Mathf.Deg2Rad * 0.5f);
            return new MyQuaternion(axis.x, axis.y, axis.z, Mathf.Cos(angle * Mathf.Deg2Rad * 0.5f));
        }

        public static float Dot(MyQuaternion a, MyQuaternion b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
        }

        public static MyQuaternion Euler(Vec3 euler)
        {
            // pointer to Cuaterniones_y_unity.pptm.pdf page 8
            MyQuaternion qx = identity;
            MyQuaternion qy = identity;
            MyQuaternion qz = identity;

            float halfAngleToRadians = Mathf.Deg2Rad * 0.5f;
            qz.Set(0, 0, MathF.Sin(euler.z * halfAngleToRadians), MathF.Cos(euler.z * halfAngleToRadians));
            qx.Set(MathF.Sin(euler.x * halfAngleToRadians), 0, 0, MathF.Cos(euler.x * halfAngleToRadians));
            qy.Set(0, MathF.Sin(euler.y * halfAngleToRadians), 0, MathF.Cos(euler.y * halfAngleToRadians));

            return qy * qx * qz;
        }

        public static MyQuaternion Euler(float x, float y, float z)
        {
            MyQuaternion qx = identity;
            MyQuaternion qy = identity;
            MyQuaternion qz = identity;

            float halfAngleToRadians = Mathf.Deg2Rad * 0.5f;
            qz.Set(0, 0, MathF.Sin(z * halfAngleToRadians), MathF.Cos(z * halfAngleToRadians));
            qx.Set(MathF.Sin(x * halfAngleToRadians), 0, 0, MathF.Cos(x * halfAngleToRadians));
            qy.Set(0, MathF.Sin(y * halfAngleToRadians), 0, MathF.Cos(y * halfAngleToRadians));

            return qy * qx * qz;
        }

        public static MyQuaternion FromToRotation(Vec3 fromDirection, Vec3 toDirection)
        {
            //Creates the axis based on the perpendicular vector to both (We'll rotate around it)
            Vec3 axis = Vec3.Cross(fromDirection, toDirection);
            //Gets the angle between the vectors
            float angle = Vec3.Angle(fromDirection, toDirection);
            //Returns the rotation in angles around the previously calculated axis
            return AngleAxis(angle, axis.normalizedVec3);
        }

        public static MyQuaternion Inverse(MyQuaternion rotation)
        {
            return new MyQuaternion(-rotation.x, -rotation.y, -rotation.z, rotation.w);
        }

        public static MyQuaternion Lerp(MyQuaternion a, MyQuaternion b, float t)
        {
            MyQuaternion result = identity;
            t = Mathf.Clamp01(t);
            float timeLeft = 1 - t;
            if (Dot(a, b) >= 0)
            {
                result.x = (timeLeft * a.x) + (t * b.x);
                result.y = (timeLeft * a.y) + (t * b.y);
                result.z = (timeLeft * a.z) + (t * b.z);
                result.w = (timeLeft * a.w) + (t * b.w);
            }
            else
            {
                result.x = (timeLeft * a.x) - (t * b.x);
                result.y = (timeLeft * a.y) - (t * b.y);
                result.z = (timeLeft * a.z) - (t * b.z);
                result.w = (timeLeft * a.w) - (t * b.w);
            }

            result.Normalize();
            return result;
        }

        public static MyQuaternion LerpUnclamped(MyQuaternion a, MyQuaternion b, float t)
        {
            MyQuaternion result = identity;

            float timeLeft = 1 - t;
            if (Dot(a, b) >= 0)
            {
                result.x = (timeLeft * a.x) + (t * b.x);
                result.y = (timeLeft * a.y) + (t * b.y);
                result.z = (timeLeft * a.z) + (t * b.z);
                result.w = (timeLeft * a.w) + (t * b.w);
            }
            else
            {
                result.x = (timeLeft * a.x) - (t * b.x);
                result.y = (timeLeft * a.y) - (t * b.y);
                result.z = (timeLeft * a.z) - (t * b.z);
                result.w = (timeLeft * a.w) - (t * b.w);
            }

            result.Normalize();
            return result;
        }

        public static MyQuaternion LookRotation(Vec3 forward)
        {
            forward.Normalize();
            Vec3 right = Vec3.Normalize(Vec3.Cross(Vec3.Up, forward));
            Vec3 up = Vec3.Normalize(Vec3.Cross(forward, right));

            float m00 = right.x;
            float m01 = right.y;
            float m02 = right.z;
            float m10 = up.x;
            float m11 = up.y;
            float m12 = up.z;
            float m20 = forward.x;
            float m21 = forward.y;
            float m22 = forward.z;

            float diagonals = m00 + m11 + m22;

            MyQuaternion q = MyQuaternion.identity;

            if (diagonals > 0f)
            {
                float num = MathF.Sqrt(diagonals + 1);
                q.w = num * 0.5f;
                num = 0.5f / num;
                q.x = (m12 - m21) * num;
                q.y = (m20 - m02) * num;
                q.z = (m01 - m10) * num;
                return q;
            }

            if (m00 >= m11 && m00 >= m22)
            {
                float num = MathF.Sqrt(1 + m00 - m11 - m22);
                q.x = num * 0.5f;
                num = 0.5f / num;
                q.y = (m01 + m10) * num;
                q.z = (m20 + m02) * num;
                q.w = (m12 - m21) * num;
                return q;
            }

            if (m11 > m22)
            {
                float num = MathF.Sqrt(1 + m11 - m00 - m22);
                q.y = num * 0.5f;
                num = 0.5f / num;
                q.x = (m01 + m10) * num;
                q.z = (m12 + m21) * num;
                q.w = (m20 - m02) * num;
                return q;
            }

            float num1 = MathF.Sqrt(1 + m22 - m00 - m11);
            q.z = num1 * 0.5f;
            num1 = 0.5f / num1;
            q.x = (m20 + m02) * num1;
            q.y = (m21 + m12) * num1;
            q.w = (m01 - m10) * num1;
            return q;
        }

        public static MyQuaternion LookRotation(Vec3 forward, [UnityEngine.Internal.DefaultValue("Vec3.up")] Vec3 upwards)
        {
            forward.Normalize();
            Vec3 right = Vec3.Normalize(Vec3.Cross(upwards, forward));
            upwards = Vec3.Normalize(Vec3.Cross(forward, right));
            //Crea una matriz rotacion en base a los ejes y la devuelve a rotacion
            float m00 = right.x;
            float m01 = right.y;
            float m02 = right.z;
            float m10 = upwards.x;
            float m11 = upwards.y;
            float m12 = upwards.z;
            float m20 = forward.x;
            float m21 = forward.y;
            float m22 = forward.z;
            //Tambien se podr�a tratar mediante matriz 4x4, seteando una matriz en base a nuestros ejes y pedirle la rotation

            float diagonals = m00 + m11 + m22;

            MyQuaternion q = MyQuaternion.identity;

            if (diagonals > 0f)
            {
                float num = MathF.Sqrt(diagonals + 1);
                q.w = num * 0.5f;
                num = 0.5f / num;
                q.x = (m12 - m21) * num;
                q.y = (m20 - m02) * num;
                q.z = (m01 - m10) * num;
                return q;
            }

            if (m00 >= m11 && m00 >= m22)
            {
                float num = MathF.Sqrt(1 + m00 - m11 - m22);
                q.x = num * 0.5f;
                num = 0.5f / num;
                q.y = (m01 + m10) * num;
                q.z = (m20 + m02) * num;
                q.w = (m12 - m21) * num;
                return q;
            }

            if (m11 > m22)
            {
                float num = MathF.Sqrt(1 + m11 - m00 - m22);
                q.y = num * 0.5f;
                num = 0.5f / num;
                q.x = (m01 + m10) * num;
                q.z = (m12 + m21) * num;
                q.w = (m20 - m02) * num;
                return q;
            }

            float num1 = MathF.Sqrt(1 + m22 - m00 - m11);
            q.z = num1 * 0.5f;
            num1 = 0.5f / num1;
            q.x = (m20 + m02) * num1;
            q.y = (m21 + m12) * num1;
            q.w = (m01 - m10) * num1;
            return q;
        }

        public static MyQuaternion Normalize(MyQuaternion q)
        {
            return new MyQuaternion(q.x / q.magnitude, q.y / q.magnitude, q.z / q.magnitude, q.w / q.magnitude);
        }

        public static MyQuaternion RotateTowards(MyQuaternion from, MyQuaternion to, float maxDegreesDelta)
        {
            if (Dot(from.normalized, to.normalized) >= 1 - kEpsilon || Dot(from.normalized, to.normalized) <= -1 + kEpsilon)
            {
                return to;
            }

            float angle = Angle(from, to);
            return LerpUnclamped(from, to, maxDegreesDelta / angle);
        }

        public static MyQuaternion Slerp(MyQuaternion a, MyQuaternion b, float t)
        {
            t = Mathf.Clamp01(t);
            return SlerpUnclamped(a, b, t);
        }

        public static MyQuaternion SlerpUnclamped(MyQuaternion a, MyQuaternion b, float t)
        {
            float cosAngle = Dot(a, b);

            float angle = Mathf.Acos(Mathf.Abs(cosAngle));
            float sinAngle = Mathf.Sin(angle);
            float quatAWeight = Mathf.Sin(angle * (1.0f - t)) / sinAngle;
            float quatBWeight = Mathf.Sin(angle * t) / sinAngle;

            if (cosAngle < 0)
                quatAWeight = -quatAWeight;

            MyQuaternion res = identity;

            res.x = quatAWeight * a.x + quatBWeight * b.x;
            res.y = quatAWeight * a.y + quatBWeight * b.y;
            res.z = quatAWeight * a.z + quatBWeight * b.z;
            res.w = quatAWeight * a.w + quatBWeight * b.w;

            return res;
        }

        public void Normalize()
        {
            float originalMagnitude = magnitude;

            x /= originalMagnitude;
            y /= originalMagnitude;
            z /= originalMagnitude;
            w /= originalMagnitude;
        }

        public void Set(float newX, float newY, float newZ, float newW)
        {
            x = newX;
            y = newY;
            z = newZ;
            w = newW;
        }

        public void SetFromToRotation(Vec3 fromDirection, Vec3 toDirection)
        {
            Vec3 axis = Vec3.Cross(fromDirection, toDirection);
            float angle = Vec3.Angle(fromDirection, toDirection);
            MyQuaternion result = AngleAxis(angle, axis.normalizedVec3);
            x = result.x;
            y = result.y;
            z = result.z;
            w = result.w;
        }

        public void SetLookRotation(Vec3 view)
        {
            MyQuaternion q = LookRotation(view);

            this.x = q.x;
            this.y = q.y;
            this.z = q.z;
            this.w = q.w;
        }

        public void SetLookRotation(Vec3 view, [UnityEngine.Internal.DefaultValue("Vec3.up")] Vec3 up)
        {
            MyQuaternion q = LookRotation(view, up);

            this.x = q.x;
            this.y = q.y;
            this.z = q.z;
            this.w = q.w;
        }

        public void ToAngleAxis(out float angle, out Vec3 axis)
        {
            Normalize();
            angle = 2.0f * Mathf.Acos(w) * Mathf.Rad2Deg;
            float mag = Mathf.Sqrt(1.0f - w * w);
            if (mag > 0.0001f)
            {
                axis = new Vec3(x, y, z) / mag;
            }
            else
            {
                axis = new Vec3(1, 0, 0);
            }
        }

        #endregion

        #region Internals

        public float this[int index]
        {
            get
            {
                float[] values = { x, y, z, w };
                return values[index];
            }
            set
            {
                float[] values = { x, y, z, w };
                values[index] = value;

                this.x = values[0];
                this.y = values[1];
                this.z = values[2];
                this.w = values[3];
            }
        }

        public bool Equals(MyQuaternion other)
        {
            return x == other.x && y == other.y && z == other.z && w == other.w;
        }

        public override bool Equals(object other)
        {
            if (!(other is MyQuaternion)) return false;
            return Equals((MyQuaternion)other);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return $"{x}, {y}, {z}, {w}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z, w, eulerAngles, normalized);
        }

        #endregion
    }
}