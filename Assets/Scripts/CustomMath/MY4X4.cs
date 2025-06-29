using System;
using UnityEngine;

namespace CustomMath
{
    public class MY4X4 : IEquatable<MY4X4>
    {
        #region Variables

        public float m00;
        public float m33;
        public float m23;
        public float m13;
        public float m03;
        public float m32;
        public float m22;
        public float m02;
        public float m12;
        public float m21;
        public float m11;
        public float m01;
        public float m30;
        public float m20;
        public float m10;
        public float m31;

        //
        // Summary:
        //     Attempts to get a rotation quaternion from this matrix.
        public MyQuaternion rotation
        {
            get
            {
                MyQuaternion q = MyQuaternion.identity;
                MY4X4 m = new MY4X4(GetColumn(0), GetColumn(1), GetColumn(2), GetColumn(3));
                Vec3 scale = lossyScale;

                // Normalize Scale from Matrix4x4
                m[0, 0] /= scale.x;
                m[0, 1] /= scale.y;
                m[0, 2] /= scale.z;
                m[1, 0] /= scale.x;
                m[1, 1] /= scale.y;
                m[1, 2] /= scale.z;
                m[2, 0] /= scale.x;
                m[2, 1] /= scale.y;
                m[2, 2] /= scale.z;

                //Toma diagonal m00 m11 m22 (Escala)
                // Agarra y setea una suma o resta dependiendo del componente en el que estemos siendo 
                // w suma de todas las escalas
                // por cada eje suma su escala y resta la resto
                q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2; //Devuelve la raiz de un n�mero que debe ser al menos 0.
                q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2; //Por eso hace un min entre las posiciones de las diagonales.
                q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
                q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;

                // m[2, 1] - m[1, 2] son las posiciones de los senos de cada eje dentro de la matriz
                q.x *= Mathf.Sign(q.x * (m21 - m12));
                q.y *= Mathf.Sign(q.y * (m02 - m20)); //Son los valores de la matriz que se van a modificar
                q.z *= Mathf.Sign(q.z * (m10 - m01));

                return q.normalized;
            }
        }

        //
        // Summary:
        //     Attempts to get a scale value from the matrix. (Read Only)
        public Vec3 lossyScale
        {
            get { return new Vec3(GetColumn(0).magnitude, GetColumn(1).magnitude, GetColumn(2).magnitude); }
        }

        //
        // Summary:
        //     Checks whether this is an identity matrix. (Read Only)
        public bool isIdentity
        {
            get { return this == identity; }
        }

        //
        // Summary:
        //     The determinant of the matrix. (Read Only)
        public float determinant
        {
            get { return Determinant(this); }
        }

        //
        // Summary:
        //     Returns the transpose of this matrix (Read Only).
        public MY4X4 transpose
        {
            get { return Transpose(this); }
        }

        //
        // Summary:
        //     The inverse of this matrix. (Read Only)
        public MY4X4 inverse { get { return Inverse(this); } }

        #endregion

        #region Constructors

        public MY4X4(Vector4 column0, Vector4 column1, Vector4 column2, Vector4 column3)
        {
            m00 = column0.x;
            m10 = column0.y;
            m20 = column0.z;
            m30 = column0.w;

            m01 = column1.x;
            m11 = column1.y;
            m21 = column1.z;
            m31 = column1.w;

            m02 = column2.x;
            m12 = column2.y;
            m22 = column2.z;
            m32 = column2.w;

            m03 = column3.x;
            m13 = column3.y;
            m23 = column3.z;
            m33 = column3.w;
        }

        #endregion

        #region Constants

        public const float kEpsilon = 1E-25F;

        #endregion

        #region Defaults

        //
        // Summary:
        //     Returns a matrix with all elements set to zero (Read Only).
        public static MY4X4 zero
        {
            get { return new MY4X4(Vector4.zero, Vector4.zero, Vector4.zero, Vector4.zero); }
        }

        //
        // Summary:
        //     Returns the identity matrix (Read Only).
        public static MY4X4 identity
        {
            get
            {
                Vector4 col1 = new Vector4(1, 0);
                Vector4 col2 = new Vector4(0, 1);
                Vector4 col3 = new Vector4(0, 0, 1);
                Vector4 col4 = new Vector4(0, 0, 0, 1);
                return new MY4X4(col1, col2, col3, col4);
            }
        }

        #endregion

        #region Operators

        public static Vector4 operator *(MY4X4 lhs, Vector4 vector)
        {
            //each row times column (in this case always same column vector)
            float x = lhs.m00 * vector.x + lhs.m01 * vector.y + lhs.m02 * vector.z + lhs.m03 * vector.w;
            float y = lhs.m10 * vector.x + lhs.m11 * vector.y + lhs.m12 * vector.z + lhs.m13 * vector.w;
            float z = lhs.m20 * vector.x + lhs.m21 * vector.y + lhs.m22 * vector.z + lhs.m23 * vector.w;

            return new Vector4(x, y, z, vector.w);
        }

        public static MY4X4 operator *(MY4X4 lhs, MY4X4 rhs)
        {
            MY4X4 newMatrix = MY4X4.zero;
            newMatrix.m00 = lhs.m00 * rhs.m00 + lhs.m01 * rhs.m10 + lhs.m02 * rhs.m20 + lhs.m03 * rhs.m30;
            newMatrix.m01 = lhs.m00 * rhs.m01 + lhs.m01 * rhs.m11 + lhs.m02 * rhs.m21 + lhs.m03 * rhs.m31;
            newMatrix.m02 = lhs.m00 * rhs.m02 + lhs.m01 * rhs.m12 + lhs.m02 * rhs.m22 + lhs.m03 * rhs.m32;
            newMatrix.m03 = lhs.m00 * rhs.m03 + lhs.m01 * rhs.m13 + lhs.m02 * rhs.m23 + lhs.m03 * rhs.m33;

            newMatrix.m10 = lhs.m10 * rhs.m00 + lhs.m11 * rhs.m10 + lhs.m12 * rhs.m20 + lhs.m13 * rhs.m30;
            newMatrix.m11 = lhs.m10 * rhs.m01 + lhs.m11 * rhs.m11 + lhs.m12 * rhs.m21 + lhs.m13 * rhs.m31;
            newMatrix.m12 = lhs.m10 * rhs.m02 + lhs.m11 * rhs.m12 + lhs.m12 * rhs.m22 + lhs.m13 * rhs.m32;
            newMatrix.m13 = lhs.m10 * rhs.m03 + lhs.m11 * rhs.m13 + lhs.m12 * rhs.m23 + lhs.m13 * rhs.m33;

            newMatrix.m20 = lhs.m20 * rhs.m00 + lhs.m21 * rhs.m10 + lhs.m22 * rhs.m20 + lhs.m23 * rhs.m30;
            newMatrix.m21 = lhs.m20 * rhs.m01 + lhs.m21 * rhs.m11 + lhs.m22 * rhs.m21 + lhs.m23 * rhs.m31;
            newMatrix.m22 = lhs.m20 * rhs.m02 + lhs.m21 * rhs.m12 + lhs.m22 * rhs.m22 + lhs.m23 * rhs.m32;
            newMatrix.m23 = lhs.m20 * rhs.m03 + lhs.m21 * rhs.m13 + lhs.m22 * rhs.m23 + lhs.m23 * rhs.m33;

            newMatrix.m30 = lhs.m30 * rhs.m00 + lhs.m31 * rhs.m10 + lhs.m32 * rhs.m20 + lhs.m33 * rhs.m30;
            newMatrix.m31 = lhs.m30 * rhs.m01 + lhs.m31 * rhs.m11 + lhs.m32 * rhs.m21 + lhs.m33 * rhs.m31;
            newMatrix.m32 = lhs.m30 * rhs.m02 + lhs.m31 * rhs.m12 + lhs.m32 * rhs.m22 + lhs.m33 * rhs.m32;
            newMatrix.m33 = lhs.m30 * rhs.m03 + lhs.m31 * rhs.m13 + lhs.m32 * rhs.m23 + lhs.m33 * rhs.m33;

            return newMatrix;
        }

        public static bool operator ==(MY4X4 lhs, MY4X4 rhs)
        {
            float diff_m00 = lhs.m00 - rhs.m00;
            float diff_m01 = lhs.m01 - rhs.m01;
            float diff_m02 = lhs.m02 - rhs.m02;
            float diff_m03 = lhs.m03 - rhs.m03;

            float sqrRow0 = diff_m00 * diff_m00 + diff_m01 * diff_m01 + diff_m02 * diff_m02 + diff_m03 * diff_m03;

            float diff_m10 = lhs.m10 - rhs.m10;
            float diff_m11 = lhs.m11 - rhs.m11;
            float diff_m12 = lhs.m12 - rhs.m12;
            float diff_m13 = lhs.m13 - rhs.m13;
            float sqrRow1 = diff_m10 * diff_m10 + diff_m11 * diff_m11 + diff_m12 * diff_m12 + diff_m13 * diff_m13;

            float diff_m20 = lhs.m20 - rhs.m20;
            float diff_m21 = lhs.m21 - rhs.m21;
            float diff_m22 = lhs.m22 - rhs.m22;
            float diff_m23 = lhs.m23 - rhs.m23;
            float sqrRow2 = diff_m20 * diff_m20 + diff_m21 * diff_m21 + diff_m22 * diff_m22 + diff_m23 * diff_m23;

            float diff_m30 = lhs.m30 - rhs.m30;
            float diff_m31 = lhs.m31 - rhs.m31;
            float diff_m32 = lhs.m32 - rhs.m32;
            float diff_m33 = lhs.m33 - rhs.m33;
            float sqrRow3 = diff_m30 * diff_m30 + diff_m31 * diff_m31 + diff_m32 * diff_m32 + diff_m33 * diff_m33;

            float squares = sqrRow0 + sqrRow1 + sqrRow2 + sqrRow3;
            return squares < kEpsilon * kEpsilon;
            //Checks if the difference between both vectors is close to zero
            // return sqrmag < kEpsilon * kEpsilon;
        }

        public static bool operator !=(MY4X4 lhs, MY4X4 rhs)
        {
            return !(lhs == rhs);
        }

        #endregion

        #region Functions

        public static float Determinant(MY4X4 m)
        {
            float a = m.m00;
            float b = m.m01;
            float c = m.m02;
            float d = m.m03;

            //m00 m01 m02 m03
            //m10 m11 m12 m13
            //m20 m21 m22 m23
            //m30 m31 m32 m33

            // aDeterminant 
            // m11 m12 m13
            // m21 m22 m23
            // m31 m32 m33
            float aDeterminant = m.m11 * (m.m22 * m.m33 - m.m23 * m.m32) - m.m12 * (m.m21 * m.m33 - m.m23 * m.m31) + m.m13 * (m.m21 * m.m32 - m.m22 * m.m31);

            // bDeterminant 
            // m10 m12 m13
            // m20 m22 m23
            // m30 m32 m33
            float bDeterminant = m.m10 * (m.m22 * m.m33 - m.m23 * m.m32) - m.m12 * (m.m20 * m.m33 - m.m23 * m.m30) + m.m13 * (m.m20 * m.m32 - m.m22 * m.m30);

            // cDeterminant 
            // m10 m11 m13
            // m20 m21 m23
            // m30 m31 m33
            float cDeterminant = m.m10 * (m.m21 * m.m33 - m.m23 * m.m31) - m.m11 * (m.m20 * m.m33 - m.m23 * m.m30) + m.m13 * (m.m20 * m.m31 - m.m21 * m.m30);

            // dDeterminant 
            // m10 m11 m12
            // m20 m21 m22
            // m30 m31 m32
            float dDeterminant = m.m10 * (m.m21 * m.m32 - m.m22 * m.m31) - m.m11 * (m.m20 * m.m32 - m.m22 * m.m30) + m.m12 * (m.m20 * m.m31 - m.m21 * m.m30);

            return a * aDeterminant - b * bDeterminant + c * cDeterminant - d * dDeterminant;
        }

        public static MY4X4 Inverse(MY4X4 m)
        {
            float detA = Determinant(m); //Debe tener determinante, de otra forma, no es inversible
            if (detA == 0)
                return zero;

            Vector4 row0;
            Vector4 row1;
            Vector4 row2;
            Vector4 row3;

            #region Row0

            float m00determine = m.m11 * m.m22 * m.m33 + m.m12 * m.m23 * m.m31 + m.m13 * m.m21 * m.m32 - m.m11 * m.m23 * m.m32 - m.m12 * m.m21 * m.m33 - m.m13 * m.m22 * m.m31;
            float m01determine = m.m01 * m.m23 * m.m32 + m.m02 * m.m21 * m.m33 + m.m03 * m.m22 * m.m31 - m.m01 * m.m22 * m.m33 - m.m02 * m.m23 * m.m31 - m.m03 * m.m21 * m.m32;
            float m02determine = m.m01 * m.m12 * m.m33 + m.m02 * m.m13 * m.m32 + m.m03 * m.m11 * m.m32 - m.m01 * m.m13 * m.m32 - m.m02 * m.m11 * m.m33 - m.m03 * m.m12 * m.m31;
            float m03determine = m.m01 * m.m13 * m.m22 + m.m02 * m.m11 * m.m23 + m.m03 * m.m12 * m.m21 - m.m01 * m.m12 * m.m23 - m.m02 * m.m13 * m.m21 - m.m03 * m.m11 * m.m22;
            row0 = new Vector4(m00determine, m01determine, m02determine, m03determine);

            #endregion

            #region Row1

            float m10determine = m.m10 * m.m23 * m.m32 + m.m12 * m.m20 * m.m33 + m.m13 * m.m22 * m.m30 - m.m10 * m.m22 * m.m33 - m.m12 * m.m23 * m.m30 - m.m13 * m.m20 * m.m32;
            float m11determine = m.m00 * m.m22 * m.m33 + m.m02 * m.m23 * m.m30 + m.m03 * m.m20 * m.m32 - m.m00 * m.m23 * m.m32 - m.m02 * m.m20 * m.m33 - m.m03 * m.m22 * m.m30;
            float m12determine = m.m00 * m.m13 * m.m32 + m.m02 * m.m10 * m.m33 + m.m03 * m.m12 * m.m30 - m.m00 * m.m12 * m.m33 - m.m02 * m.m13 * m.m30 - m.m03 * m.m10 * m.m32;
            float m13determine = m.m00 * m.m12 * m.m23 + m.m02 * m.m13 * m.m20 + m.m03 * m.m10 * m.m22 - m.m00 * m.m13 * m.m22 - m.m02 * m.m10 * m.m23 - m.m03 * m.m12 * m.m20;
            row1 = new Vector4(m10determine, m11determine, m12determine, m13determine);

            #endregion

            #region Row2

            float m20determine = m.m10 * m.m21 * m.m33 + m.m11 * m.m23 * m.m30 + m.m13 * m.m20 * m.m31 - m.m10 * m.m23 * m.m31 - m.m11 * m.m20 * m.m33 - m.m13 * m.m31 * m.m30;
            float m21determine = m.m00 * m.m23 * m.m31 + m.m01 * m.m20 * m.m33 + m.m03 * m.m21 * m.m30 - m.m00 * m.m21 * m.m33 - m.m01 * m.m23 * m.m30 - m.m03 * m.m20 * m.m31;
            float m22determine = m.m00 * m.m11 * m.m33 + m.m01 * m.m13 * m.m31 + m.m03 * m.m10 * m.m31 - m.m00 * m.m13 * m.m31 - m.m01 * m.m10 * m.m33 - m.m03 * m.m11 * m.m30;
            float m23determine = m.m00 * m.m13 * m.m21 + m.m01 * m.m10 * m.m23 + m.m03 * m.m11 * m.m31 - m.m00 * m.m11 * m.m23 - m.m01 * m.m13 * m.m20 - m.m03 * m.m10 * m.m21;
            row2 = new Vector4(m20determine, m21determine, m22determine, m23determine);

            #endregion

            #region Row3

            float m30determine = m.m10 * m.m22 * m.m31 + m.m11 * m.m20 * m.m32 + m.m12 * m.m21 * m.m30 - m.m00 * m.m21 * m.m32 - m.m11 * m.m22 * m.m30 - m.m12 * m.m20 * m.m31;
            float m31determine = m.m00 * m.m21 * m.m32 + m.m01 * m.m22 * m.m30 + m.m02 * m.m20 * m.m31 - m.m00 * m.m22 * m.m31 - m.m01 * m.m20 * m.m32 - m.m02 * m.m21 * m.m30;
            float m32determine = m.m00 * m.m12 * m.m31 + m.m01 * m.m10 * m.m32 + m.m02 * m.m11 * m.m30 - m.m00 * m.m11 * m.m32 - m.m01 * m.m12 * m.m30 - m.m02 * m.m10 * m.m31;
            float m33determine = m.m00 * m.m11 * m.m22 + m.m01 * m.m12 * m.m20 + m.m02 * m.m10 * m.m21 - m.m00 * m.m12 * m.m21 - m.m01 * m.m10 * m.m22 - m.m02 * m.m11 * m.m20;
            row3 = new Vector4(m30determine, m31determine, m32determine, m33determine);

            #endregion

            row0 /= detA;
            row1 /= detA;
            row2 /= detA;
            row3 /= detA;

            MY4X4 res = MY4X4.identity;

            res.SetRow(0, row0);
            res.SetRow(1, row1);
            res.SetRow(2, row2);
            res.SetRow(3, row3);

            return res;
        }

        //
        // Summary:
        //     Create a "look at" matrix.
        //
        // Parameters:
        //   from:
        //     The source point.
        //
        //   to:
        //     The target point.
        //
        //   up:
        //     The vector describing the up direction (typically Vec3.up).
        //
        // Returns:
        //     The resulting transformation matrix.
        public static MY4X4 LookAt(Vec3 from, Vec3 to, Vec3 up)
        {
            return TRS(from, MyQuaternion.LookRotation(to - from, up), Vec3.One);
        }

        //
        // Summary:
        //     Creates a rotation matrix.
        //
        // Parameters:
        //   q:
        public static MY4X4 Rotate(MyQuaternion q)
        {
            MyQuaternion rotation = q;
            rotation.Normalize();

            Vector4 firstColumn = new Vector4(2 * (rotation.w * rotation.w + rotation.x * rotation.x) - 1,
            2 * (rotation.x * rotation.y + rotation.w * rotation.z),
            2 * (rotation.x * rotation.z - rotation.w * rotation.y),
            0);

            Vector4 secondColumn = new Vector4(2 * (rotation.x * rotation.y - rotation.w * rotation.z),
            2 * (rotation.w * rotation.w + rotation.y * rotation.y) - 1,
            2 * (rotation.y * rotation.z + rotation.w * rotation.x),
            0);

            Vector4 thirdColumn = new Vector4(2 * (rotation.x * rotation.z + rotation.w * rotation.y),
            2 * (rotation.y * rotation.z - rotation.w * rotation.x),
            2 * (rotation.w * rotation.w + rotation.z * rotation.z) - 1,
            0);

            Vector4 fourthColumn = new Vector4(0, 0, 0, 1);

            return new MY4X4(firstColumn, secondColumn, thirdColumn, fourthColumn);
        }

        //
        // Summary:
        //     Creates a scaling matrix.
        //
        // Parameters:
        //   vector:
        public static MY4X4 Scale(Vec3 vector)
        {
            Vector4 col1 = new Vector4(vector.x, 0);
            Vector4 col2 = new Vector4(0, vector.y);
            Vector4 col3 = new Vector4(0, 0, vector.z);
            Vector4 col4 = new Vector4(0, 0, 0, 1);
            return new MY4X4(col1, col2, col3, col4);
        }

        //
        // Summary:
        //     Creates a translation matrix.
        //
        // Parameters:
        //   vector:
        public static MY4X4 Translate(Vec3 vector)
        {
            Vector4 col1 = new Vector4(1, 0);
            Vector4 col2 = new Vector4(0, 1);
            Vector4 col3 = new Vector4(0, 0, 1);
            Vector4 col4 = new Vector4(vector.x, vector.y, vector.z, 1);
            return new MY4X4(col1, col2, col3, col4);
        }

        public static MY4X4 Transpose(MY4X4 m)
        {
            Vector4 row0 = m.GetRow(0);
            Vector4 row1 = m.GetRow(1);
            Vector4 row2 = m.GetRow(2);
            Vector4 row3 = m.GetRow(3);

            return new MY4X4(row0, row1, row2, row3);
        }

        //
        // Summary:
        //     Creates a translation, rotation and scaling matrix.
        //
        // Parameters:
        //   pos:
        //
        //   q:
        //
        //   s:
        public static MY4X4 TRS(Vec3 pos, MyQuaternion q, Vec3 s)
        {
            return Translate(pos) * Rotate(q) * Scale(s);
        }

        //
        // Summary:
        //     Get a column of the matrix.
        //
        // Parameters:
        //   index:
        public Vector4 GetColumn(int index)
        {
            switch (index)
            {
                case 0:
                    return new Vector4(m00, m10, m20, m30);
                case 1:
                    return new Vector4(m01, m11, m21, m31);
                case 2:
                    return new Vector4(m02, m12, m22, m32);
                case 3:
                    return new Vector4(m03, m13, m23, m33);
                default:
                    throw new Exception("Invalid Index");
            }
        }

        //
        // Summary:
        //     Get position vector from the matrix.
        public Vec3 GetPosition()
        {
            return new Vec3(m03, m13, m23);
        }

        //
        // Summary:
        //     Returns a row of the matrix.
        //
        // Parameters:
        //   index:
        public Vector4 GetRow(int index)
        {
            switch (index)
            {
                case 0:
                    return new Vector4(m00, m01, m02, m03);
                case 1:
                    return new Vector4(m10, m11, m12, m13);
                case 2:
                    return new Vector4(m20, m21, m22, m23);
                case 3:
                    return new Vector4(m30, m31, m32, m33);
                default:
                    throw new Exception("Invalid Index");
            }
        }

        //
        // Summary:
        //     Transforms a position by this matrix (generic).
        //
        // Parameters:
        //   point:
        public Vec3 MultiplyPoint(Vec3 point)
        {
            Vector4 v4Point = new Vector4(point.x, point.y, point.z, 1);
            v4Point = this * v4Point;

            return new Vec3(v4Point.x, v4Point.y, v4Point.z);
        }

        //
        // Summary:
        //     Transforms a position by this matrix (fast).
        //
        // Parameters:
        //   point:
        public Vec3 MultiplyPoint3x4(Vec3 point)
        {
            Vec3 res;
            res.x = (m00 * point.x + m01 * point.y + m02 * point.z) + m03;
            res.y = (m10 * point.x + m11 * point.y + m12 * point.z) + m13;
            res.z = (m20 * point.x + m21 * point.y + m22 * point.z) + m23;
            return res;
        }

        //
        // Summary:
        //     Transforms a direction by this matrix.
        //
        // Parameters:
        //   vector:
        public Vec3 MultiplyVector(Vec3 vector)
        {
            Vec3 res;
            res.x = m00 * vector.x + m01 * vector.y + m02 * vector.z;
            res.y = m10 * vector.x + m11 * vector.y + m12 * vector.z;
            res.z = m20 * vector.x + m21 * vector.y + m22 * vector.z;
            return res;
        }

        //
        // Summary:
        //     Sets a column of the matrix.
        //
        // Parameters:
        //   index:
        //
        //   column:
        public void SetColumn(int index, Vector4 column)
        {
            this[0, index] = column.x;
            this[1, index] = column.y;
            this[2, index] = column.z;
            this[3, index] = column.w;
        }

        //
        // Summary:
        //     Sets a row of the matrix.
        //
        // Parameters:
        //   index:
        //
        //   row:
        public void SetRow(int index, Vector4 row)
        {
            this[index, 0] = row.x;
            this[index, 1] = row.y;
            this[index, 2] = row.z;
            this[index, 3] = row.w;
        }

        //
        // Summary:
        //     Sets this matrix to a translation, rotation and scaling matrix.
        //
        // Parameters:
        //   pos:
        //
        //   q:
        //
        //   s:
        public void SetTRS(Vec3 pos, MyQuaternion q, Vec3 s)
        {
            MY4X4 trs = TRS(pos, q, s);

            for (int i = 0; i < 4; i++)
            {
                SetColumn(i, trs.GetColumn(i));
            }
        }
        //
        // Summary:
        //     Returns a formatted string for this matrix.
        //
        // Parameters:
        //   format:
        //     A numeric format string.
        //
        //   formatProvider:
        //     An object that specifies culture-specific formatting.

        //
        // Summary:
        //     Checks if this matrix is a valid transform matrix.
        public bool ValidTRS()
        {
            //Checks if every axis is orthogonal (aka everyone of them are perpendicular between them)

            Vec3 column0 = new Vec3(m00, m10, m20);
            Vec3 column1 = new Vec3(m01, m11, m21);
            Vec3 column2 = new Vec3(m02, m12, m22);

            return Vec3.Dot(column0, column1) <= kEpsilon &&
                   Vec3.Dot(column0, column2) <= kEpsilon &&
                   Vec3.Dot(column1, column2) <= kEpsilon;
        }

        #endregion

        #region Internals

        public override string ToString()
        {
            return $"{m00}\t {m01}\t {m02}\t {m03}\n" +
                   $"{m10}\t {m11}\t {m12}\t {m13}\n" +
                   $"{m20}\t {m21}\t {m22}\t {m23}\n" +
                   $"{m30}\t {m31}\t {m32}\t {m33}";
        }

        public float this[int index]
        {
            get
            {
                float[] values = { m00, m01, m02, m03, m10, m11, m12, m13, m20, m21, m22, m23, m30, m31, m32, m33 };
                return values[index];
            }
            set
            {
                float[] values = { m00, m01, m02, m03, m10, m11, m12, m13, m20, m21, m22, m23, m30, m31, m32, m33 };
                values[index] = value;

                m00 = values[0];
                m01 = values[1];
                m02 = values[2];
                m03 = values[3];
                m10 = values[4];
                m11 = values[5];
                m12 = values[6];
                m13 = values[7];
                m20 = values[8];
                m21 = values[9];
                m22 = values[10];
                m23 = values[11];
                m30 = values[12];
                m31 = values[13];
                m32 = values[14];
                m33 = values[15];
            }
        }

        public float this[int row, int column]
        {
            get { return this[column + row * 4]; }
            set { this[column + row * 4] = value; }
        }

        public override bool Equals(object other)
        {
            throw new NotImplementedException();
        }

        public bool Equals(MY4X4 other)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}