using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CustomMath
{
    [Serializable]
    public struct Vec3 : IEquatable<Vec3>
    {
        #region Variables

        public float x;
        public float y;
        public float z;

        public float sqrMagnitude
        {
            get { return (x * x + y * y + z * z); }
        }

        public Vector3 normalized
        {
            get { return new Vector3(x / magnitude, y / magnitude, z / magnitude); }
        }

        public Vec3 normalizedVec3
        {
            get
            {
                Vec3 normalizedVec = new Vec3(x, y, z);
                normalizedVec.Normalize();
                return normalizedVec;
            }
        }

        public float magnitude
        {
            get { return Mathf.Sqrt(sqrMagnitude); }
        }

        #endregion

        #region constants

        public const float epsilon = 1e-05f;

        #endregion

        #region Default Values

        public static Vec3 Zero
        {
            get { return new Vec3(0.0f, 0.0f, 0.0f); }
        }

        public static Vec3 One
        {
            get { return new Vec3(1.0f, 1.0f, 1.0f); }
        }

        public static Vec3 Forward
        {
            get { return new Vec3(0.0f, 0.0f, 1.0f); }
        }

        public static Vec3 Back
        {
            get { return new Vec3(0.0f, 0.0f, -1.0f); }
        }

        public static Vec3 Right
        {
            get { return new Vec3(1.0f, 0.0f, 0.0f); }
        }

        public static Vec3 Left
        {
            get { return new Vec3(-1.0f, 0.0f, 0.0f); }
        }

        public static Vec3 Up
        {
            get { return new Vec3(0.0f, 1.0f, 0.0f); }
        }

        public static Vec3 Down
        {
            get { return new Vec3(0.0f, -1.0f, 0.0f); }
        }

        public static Vec3 PositiveInfinity
        {
            get { return new Vec3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity); }
        }

        public static Vec3 NegativeInfinity
        {
            get { return new Vec3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity); }
        }

        #endregion

        #region Constructors

        public Vec3(float x, float y)
        {
            this.x = x;
            this.y = y;
            this.z = 0.0f;
        }

        public Vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vec3(Vec3 v3)
        {
            this.x = v3.x;
            this.y = v3.y;
            this.z = v3.z;
        }

        public Vec3(Vector3 v3)
        {
            this.x = v3.x;
            this.y = v3.y;
            this.z = v3.z;
        }

        public Vec3(Vector2 v2)
        {
            this.x = v2.x;
            this.y = v2.y;
            this.z = 0.0f;
        }

        #endregion

        #region Operators

        public static bool operator ==(Vec3 left, Vec3 right)
        {
            float diff_x = left.x - right.x;
            float diff_y = left.y - right.y;
            float diff_z = left.z - right.z;
            float sqrmag = diff_x * diff_x + diff_y * diff_y + diff_z * diff_z;
            //Checks if the difference between both vectors is close to zero
            return sqrmag < epsilon * epsilon;
        }

        public static bool operator !=(Vec3 left, Vec3 right)
        {
            return !(left == right);
        }

        public static Vec3 operator +(Vec3 leftV3, Vec3 rightV3)
        {
            return new Vec3(leftV3.x + rightV3.x, leftV3.y + rightV3.y, leftV3.z + rightV3.z);
        }

        public static Vec3 operator -(Vec3 leftV3, Vec3 rightV3)
        {
            return new Vec3(leftV3.x - rightV3.x, leftV3.y - rightV3.y, leftV3.z - rightV3.z);
        }

        public static Vec3 operator -(Vec3 v3)
        {
            return new Vec3(v3.x * -1, v3.y * -1, v3.y * -1);
        }

        public static Vec3 operator *(Vec3 v3, float scalar)
        {
            return new Vec3(v3.x * scalar, v3.y * scalar, v3.z * scalar);
        }

        public static Vec3 operator *(float scalar, Vec3 v3)
        {
            return new Vec3(v3.x * scalar, v3.y * scalar, v3.z * scalar);
        }

        public static Vec3 operator /(Vec3 v3, float scalar)
        {
            return new Vec3(v3.x / scalar, v3.y / scalar, v3.z / scalar);
        }

        public static implicit operator Vector3(Vec3 v3)
        {
            return new Vector3(v3.x, v3.y, v3.z);
        }

        public static implicit operator Vector2(Vec3 v2)
        {
            return new Vector2(v2.x, v2.y);
        }

        #endregion

        #region Functions

        public override string ToString()
        {
            return "X = " + x.ToString() + "   Y = " + y.ToString() + "   Z = " + z.ToString();
        }

        public static float Angle(Vec3 from, Vec3 to)
        {
            // by definition dot product a.b = |a||b| cos(O) disclaimer: O represents the symbol theta NOT A ZERO
            // divide each side by |a||b| and cos = dot / |a||b| 
            if (from.magnitude == 0 || to.magnitude == 0)
                return 0;

            return Mathf.Acos(Vec3.Dot(from, to) / (from.magnitude * to.magnitude)) * Mathf.Rad2Deg;
        }

        public static Vec3 ClampMagnitude(Vec3 vector, float maxLength)
        {
            if (vector.magnitude > maxLength)
            {
                vector.Normalize();
                Debug.Log(vector.magnitude);
                return vector * maxLength;
            }

            return vector;
        }

        public static float Magnitude(Vec3 vector)
        {
            return Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
        }

        public static Vec3 Cross(Vec3 a, Vec3 b)
        {
            Vec3 normal;

            normal.x = a.y * b.z - a.z * b.y;
            // for the "y" axis, we change the order of substraction to invert the value (since x and z remain positive while y is negative due to matrix sign) 
            normal.y = a.z * b.x - a.x * b.z;
            normal.z = a.x * b.y - a.y * b.x;

            return normal;
        }

        public static float Distance(Vec3 a, Vec3 b)
        {
            return (a - b).magnitude;
        }

        public static float Dot(Vec3 a, Vec3 b)
        {
            return (a.x * b.x) + (a.y * b.y) + (a.z * b.z);
        }

        public static Vec3 Lerp(Vec3 a, Vec3 b, float t)
        {
            t = Mathf.Clamp(t, 0, 1);
            // the first vector plus the distance of itself to the second vector multiplied by t (0 returns a, 1 returns b)
            return a + (b - a) * t;
        }

        public static Vec3 LerpUnclamped(Vec3 a, Vec3 b, float t)
        {
            return a + (b - a) * t;
        }

        public static Vec3 Max(Vec3 a, Vec3 b)
        {
            Vec3 max;
            max.x = a.x > b.x ? a.x : b.x;
            max.y = a.y > b.y ? a.y : b.y;
            max.z = a.z > b.z ? a.z : b.z;

            return max;
        }

        public static Vec3 Min(Vec3 a, Vec3 b)
        {
            Vec3 min;
            min.x = a.x < b.x ? a.x : b.x;
            min.y = a.y < b.y ? a.y : b.y;
            min.z = a.z < b.z ? a.z : b.z;

            return min;
        }

        public static float SqrMagnitude(Vec3 vector)
        {
            return (vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
        }

        public static Vec3 Project(Vec3 vector, Vec3 onNormal)
        {
            if (onNormal.magnitude == 0)
                return Vec3.Zero;

            // https://www.geogebra.org/m/arPXpSet search why dotProduct(a,b) / b equals project
            onNormal.Normalize();
            onNormal *= (Dot(vector, onNormal) / Mathf.Sqrt(MathF.Pow(onNormal.x, 2) + MathF.Pow(onNormal.y, 2) + MathF.Pow(onNormal.z, 2)));
            return onNormal;
        }

        public static Vec3 Reflect(Vec3 inDirection, Vec3 inNormal)
        {
            return inDirection - 2 * inNormal * Dot(inDirection, inNormal);
        }

        public void Set(float newX, float newY, float newZ)
        {
            x = newX;
            y = newY;
            z = newZ;
        }

        public void Scale(Vec3 scale)
        {
            x *= scale.x;
            y *= scale.y;
            z *= scale.z;
        }

        public void Normalize()
        {
            if (magnitude == 0)
                return;

            float newX = x / magnitude;
            float newY = y / magnitude;
            float newZ = z / magnitude;
            x = newX;
            y = newY;
            z = newZ;
        }

        public static Vec3 Normalize(Vec3 value)
        {
            float magnitude = value.magnitude;
            if (magnitude == 0)
                return new Vec3(0, 0, 0);

            float newX = value.x / magnitude;
            float newY = value.y / magnitude;
            float newZ = value.z / magnitude;
            return new Vec3(newX, newY, newZ);
        }

        #endregion

        #region Internals

        public override bool Equals(object other)
        {
            if (!(other is Vec3)) return false;
            return Equals((Vec3)other);
        }

        public bool Equals(Vec3 other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
        }

        #endregion
    }
}