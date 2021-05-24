//#define MATX_SIMD
using System;
using System.Diagnostics;

namespace Droid.Core
{
    public static partial class Matrix_
    {
        public const float MATRIX_INVERSE_EPSILON = 1e-14F;
        public const float MATRIX_EPSILON = 1e-6F;
    }

    public struct Matrix2x2
    {
        public Matrix2x2(Vector2 x, Vector2 y)
        {
            mat0.x = x.x; mat0.y = x.y;
            mat1.x = y.x; mat1.y = y.y;
        }
        public Matrix2x2(float xx, float xy, float yx, float yy)
        {
            mat0.x = xx; mat0.y = xy;
            mat1.x = yx; mat1.y = yy;
        }
        public Matrix2x2(float[][] src)
        {
            mat0 = new Vector2(src[0][0], src[0][1]);
            mat1 = new Vector2(src[1][0], src[1][1]);
        }

        public unsafe ref Vector2 this[int index]
        {
            get
            {
                fixed (Vector2* mat = &mat0)
                    return ref mat[index];
            }
        }

        public static Matrix2x2 operator -(Matrix2x2 _)
            => new(
            -_.mat0.x, -_.mat0.y,
            -_.mat1.x, -_.mat1.y);
        public static Matrix2x2 operator *(Matrix2x2 _, float a)
            => new(
            _.mat0.x * a, _.mat0.y * a,
            _.mat1.x * a, _.mat1.y * a);
        public static Vector2 operator *(Matrix2x2 _, Vector2 vec)
            => new(
            _.mat0.x * vec.x + _.mat0.y * vec.y,
            _.mat1.x * vec.x + _.mat1.y * vec.y);
        public static Matrix2x2 operator *(Matrix2x2 _, Matrix2x2 a)
            => new(
            _.mat0.x * a.mat0.x + _.mat0.y * a.mat1.x,
            _.mat0.x * a.mat0.y + _.mat0.y * a.mat1.y,
            _.mat1.x * a.mat0.x + _.mat1.y * a.mat1.x,
            _.mat1.x * a.mat0.y + _.mat1.y * a.mat1.y);
        public static Matrix2x2 operator +(Matrix2x2 _, Matrix2x2 a)
            => new(
            _.mat0.x + a.mat0.x, _.mat0.y + a.mat0.y,
            _.mat1.x + a.mat1.x, _.mat1.y + a.mat1.y);
        public static Matrix2x2 operator -(Matrix2x2 _, Matrix2x2 a)
            => new(
            _.mat0.x - a.mat0.x, _.mat0.y - a.mat0.y,
            _.mat1.x - a.mat1.x, _.mat1.y - a.mat1.y);

        public static Matrix2x2 operator *(float a, Matrix2x2 mat)
            => mat * a;
        public static Vector2 operator *(Vector2 vec, Matrix2x2 mat)
            => mat * vec;

        public bool Compare(Matrix2x2 a)                        // exact compare, no epsilon
        {
            if (mat0.Compare(a.mat0) &&
                mat1.Compare(a.mat1))
                return true;
            return false;
        }
        public bool Compare(Matrix2x2 a, float epsilon)   // compare with epsilon
        {
            if (mat0.Compare(a.mat0, epsilon) &&
                mat1.Compare(a.mat1, epsilon))
                return true;
            return false;
        }
        public static bool operator ==(Matrix2x2 _, Matrix2x2 a)                 // exact compare, no epsilon
            => _.Compare(a);
        public static bool operator !=(Matrix2x2 _, Matrix2x2 a)                 // exact compare, no epsilon
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Matrix2x2 q && Compare(q);
        public override int GetHashCode()
            => mat0.GetHashCode();

        public void Zero()
        {
            mat0.Zero();
            mat1.Zero();
        }
        public void Identity()
            => this = identity;
        public bool IsIdentity(float epsilon = Matrix_.MATRIX_EPSILON)
            => Compare(identity, epsilon);
        public bool IsSymmetric(float epsilon = Matrix_.MATRIX_EPSILON)
            => MathX.Fabs(mat0.y - mat1.x) < epsilon;
        public bool IsDiagonal(float epsilon = Matrix_.MATRIX_EPSILON)
        {
            if (MathX.Fabs(mat0.y) > epsilon ||
                MathX.Fabs(mat1.x) > epsilon)
                return false;
            return true;
        }

        public float Trace()
            => mat0.x + mat1.y;
        public float Determinant()
            => mat0.x * mat1.y - mat0.y * mat1.x;
        public Matrix2x2 Transpose()   // returns transpose
            => new(
            mat0.x, mat1.x,
            mat0.y, mat1.y);
        public Matrix2x2 TransposeSelf()
        {
            var tmp = mat0.y;
            mat0.y = mat1.x;
            mat1.x = tmp;
            return this;
        }
        public Matrix2x2 Inverse()      // returns the inverse ( m * m.Inverse() = identity )
        {
            var invMat = this;
            var r = invMat.InverseSelf();
            Debug.Assert(r);
            return invMat;
        }
        public bool InverseSelf()     // returns false if determinant is zero
        {
            // 2+4 = 6 multiplications
            //		 1 division
            var det = mat0.x * mat1.y - mat0.y * mat1.x;
            if (MathX.Fabs(det) < Matrix_.MATRIX_INVERSE_EPSILON)
                return false;

            var invDet = 1.0f / det;
            var a = mat0.x;
            mat0.x = mat1.y * invDet;
            mat0.y = -mat0.y * invDet;
            mat1.x = -mat1.x * invDet;
            mat1.y = a * invDet;
            return true;
        }
        public Matrix2x2 InverseFast()  // returns the inverse ( m * m.Inverse() = identity )
        {
            var invMat = this;
            var r = invMat.InverseFastSelf();
            Debug.Assert(r);
            return invMat;
        }
        public bool InverseFastSelf()    // returns false if determinant is zero
        {
            // 2+4 = 6 multiplications
            //		 1 division
            var det = mat0.x * mat1.y - mat0.y * mat1.x;
            if (MathX.Fabs(det) < Matrix_.MATRIX_INVERSE_EPSILON)
                return false;

            var invDet = 1.0f / det;
            var a = mat0.x;
            mat0.x = mat1.y * invDet;
            mat0.y = -mat0.y * invDet;
            mat1.x = -mat1.x * invDet;
            mat1.y = a * invDet;
            return true;
        }

        public static int Dimension
            => 4;

        public unsafe T ToFloatPtr<T>(FloatPtr<T> callback)
            => mat0.ToFloatPtr(callback);
        public unsafe string ToString(int precision = 2)
            => ToFloatPtr(array => StringX.FloatArrayToString(array, Dimension, precision));

        internal Vector2 mat0;
        internal Vector2 mat1;

        public static Matrix2x2 zero = new(new Vector2(0, 0), new Vector2(0, 0));
        public static Matrix2x2 identity = new(new Vector2(1, 0), new Vector2(0, 1));
        //#define default	identity
    }

    public struct Matrix3x3
    {
        public Matrix3x3(Vector3 x, Vector3 y, Vector3 z)
        {
            mat0.x = x.x; mat0.y = x.y; mat0.z = x.z;
            mat1.x = y.x; mat1.y = y.y; mat1.z = y.z;
            mat2.x = z.x; mat2.y = z.y; mat2.z = z.z;
        }
        public Matrix3x3(float xx, float xy, float xz, float yx, float yy, float yz, float zx, float zy, float zz)
        {
            mat0.x = xx; mat0.y = xy; mat0.z = xz;
            mat1.x = yx; mat1.y = yy; mat1.z = yz;
            mat2.x = zx; mat2.y = zy; mat2.z = zz;
        }
        public unsafe Matrix3x3(float[][] src)
        {
            mat0 = new Vector3(src[0][0], src[0][1], src[0][2]);
            mat1 = new Vector3(src[1][0], src[1][1], src[1][2]);
            mat2 = new Vector3(src[2][0], src[2][1], src[2][2]);
        }

        public unsafe ref Vector3 this[int index]
        {
            get
            {
                fixed (Vector3* mat = &mat0)
                    return ref mat[index];
            }
        }

        public static Matrix3x3 operator -(Matrix3x3 _)
            => new(
            -_.mat0.x, -_.mat0.y, -_.mat0.z,
            -_.mat1.x, -_.mat1.y, -_.mat1.z,
            -_.mat2.x, -_.mat2.y, -_.mat2.z);
        public static Matrix3x3 operator *(Matrix3x3 _, float a)
            => new(
            _.mat0.x * a, _.mat0.y * a, _.mat0.z * a,
            _.mat1.x * a, _.mat1.y * a, _.mat1.z * a,
            _.mat2.x * a, _.mat2.y * a, _.mat2.z * a);
        public static Vector3 operator *(Matrix3x3 _, Vector3 vec)
            => new(
            _.mat0.x * vec.x + _.mat1.x * vec.y + _.mat2.x * vec.z,
            _.mat0.y * vec.x + _.mat1.y * vec.y + _.mat2.y * vec.z,
            _.mat0.z * vec.x + _.mat1.z * vec.y + _.mat2.z * vec.z);

        public static unsafe Matrix3x3 operator *(Matrix3x3 _, Matrix3x3 a)
        {
            Matrix3x3 dst;
            var m1Ptr = (float*)&_;
            var m2Ptr = (float*)&a;
            var dstPtr = (float*)&dst;
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    *dstPtr = m1Ptr[0] * m2Ptr[0 * 3 + j]
                            + m1Ptr[1] * m2Ptr[1 * 3 + j]
                            + m1Ptr[2] * m2Ptr[2 * 3 + j];
                    dstPtr++;
                }
                m1Ptr += 3;
            }
            return dst;
        }

        public static Matrix3x3 operator +(Matrix3x3 _, Matrix3x3 a)
            => new(
            _.mat0.x + a.mat0.x, _.mat0.y + a.mat0.y, _.mat0.z + a.mat0.z,
            _.mat1.x + a.mat1.x, _.mat1.y + a.mat1.y, _.mat1.z + a.mat1.z,
            _.mat2.x + a.mat2.x, _.mat2.y + a.mat2.y, _.mat2.z + a.mat2.z);
        public static Matrix3x3 operator -(Matrix3x3 _, Matrix3x3 a)
            => new(
            _.mat0.x - a.mat0.x, _.mat0.y - a.mat0.y, _.mat0.z - a.mat0.z,
            _.mat1.x - a.mat1.x, _.mat1.y - a.mat1.y, _.mat1.z - a.mat1.z,
            _.mat2.x - a.mat2.x, _.mat2.y - a.mat2.y, _.mat2.z - a.mat2.z);

        public static Matrix3x3 operator *(float a, Matrix3x3 mat)
            => mat * a;
        public static Vector3 operator *(Vector3 vec, Matrix3x3 mat)
            => mat * vec;

        public bool Compare(Matrix3x3 a)                       // exact compare, no epsilon
        {
            if (mat0.Compare(a.mat0) &&
                mat1.Compare(a.mat1) &&
                mat2.Compare(a.mat2))
                return true;
            return false;
        }
        public bool Compare(Matrix3x3 a, float epsilon)  // compare with epsilon
        {
            if (mat0.Compare(a.mat0, epsilon) &&
                mat1.Compare(a.mat1, epsilon) &&
                mat2.Compare(a.mat2, epsilon))
                return true;
            return false;
        }
        public static bool operator ==(Matrix3x3 _, Matrix3x3 a)                   // exact compare, no epsilon
            => _.Compare(a);
        public static bool operator !=(Matrix3x3 _, Matrix3x3 a)                   // exact compare, no epsilon
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Matrix3x3 q && Compare(q);
        public override int GetHashCode()
            => mat0.GetHashCode();

        public unsafe void Zero()
        {
            fixed (void* p = &this)
                U.memset(p, 0, sizeof(Matrix3x3));
        }
        public void Identity()
            => this = identity;
        public bool IsIdentity(float epsilon = Matrix_.MATRIX_EPSILON)
            => Compare(identity, epsilon);
        public bool IsSymmetric(float epsilon = Matrix_.MATRIX_EPSILON)
        {
            if (MathX.Fabs(mat0.y - mat1.x) > epsilon) return false;
            if (MathX.Fabs(mat0.z - mat2.x) > epsilon) return false;
            if (MathX.Fabs(mat1.z - mat2.y) > epsilon) return false;
            return true;
        }
        public bool IsDiagonal(float epsilon = Matrix_.MATRIX_EPSILON)
        {
            if (MathX.Fabs(mat0.y) > epsilon ||
                MathX.Fabs(mat0.z) > epsilon ||
                MathX.Fabs(mat1.x) > epsilon ||
                MathX.Fabs(mat1.z) > epsilon ||
                MathX.Fabs(mat2.x) > epsilon ||
                MathX.Fabs(mat2.y) > epsilon)
                return false;
            return true;
        }
        public bool IsRotated()
            => !Compare(identity);

        public void ProjectVector(Vector3 src, out Vector3 dst)
        {
            dst.x = src * mat0;
            dst.y = src * mat1;
            dst.z = src * mat2;
        }
        public void UnprojectVector(Vector3 src, out Vector3 dst)
            => dst = mat0 * src.x + mat1 * src.y + mat2 * src.z;

        public bool FixDegeneracies()    // fix degenerate axial cases
        {
            var r = mat0.FixDegenerateNormal();
            r |= mat1.FixDegenerateNormal();
            r |= mat2.FixDegenerateNormal();
            return r;
        }
        public bool FixDenormals()       // change tiny numbers to zero
        {
            var r = mat0.FixDenormals();
            r |= mat1.FixDenormals();
            r |= mat2.FixDenormals();
            return r;
        }

        public float Trace()
            => mat0.x + mat1.y + mat2.z;
        public float Determinant()
        {
            var det2_12_01 = mat1.x * mat2.y - mat1.y * mat2.x;
            var det2_12_02 = mat1.x * mat2.z - mat1.z * mat2.x;
            var det2_12_12 = mat1.y * mat2.z - mat1.z * mat2.y;
            return mat0.x * det2_12_12 - mat0.y * det2_12_02 + mat0.z * det2_12_01;
        }
        public Matrix3x3 OrthoNormalize()
        {
            var ortho = this;
            ortho.mat0.Normalize();
            ortho.mat2.Cross(mat0, mat1); ortho.mat2.Normalize();
            ortho.mat1.Cross(mat2, mat0); ortho.mat1.Normalize();
            return ortho;
        }
        public Matrix3x3 OrthoNormalizeSelf()
        {
            mat0.Normalize();
            mat2.Cross(mat0, mat1); mat2.Normalize();
            mat1.Cross(mat2, mat0); mat1.Normalize();
            return this;
        }
        public Matrix3x3 Transpose()   // returns transpose
            => new(
            mat0.x, mat1.x, mat2.x,
            mat0.y, mat1.y, mat2.y,
            mat0.z, mat1.z, mat2.z);
        public Matrix3x3 TransposeSelf()
        {
            var tmp0 = mat0.y; mat0.y = mat1.x; mat1.x = tmp0;
            var tmp1 = mat0.z; mat0.z = mat2.x; mat2.x = tmp1;
            var tmp2 = mat1.z; mat1.z = mat2.y; mat2.y = tmp2;
            return this;
        }

        public Matrix3x3 Inverse()     // returns the inverse ( m * m.Inverse() = identity )
        {
            var invMat = this;
            var r = invMat.InverseSelf();
            Debug.Assert(r);
            return invMat;
        }
        public bool InverseSelf()        // returns false if determinant is zero
        {
            // 18+3+9 = 30 multiplications
            //			 1 division
            Matrix3x3 inverse;
            inverse.mat0.x = mat1.y * mat2.z - mat1.z * mat2.y;
            inverse.mat1.x = mat1.z * mat2.x - mat1.x * mat2.z;
            inverse.mat2.x = mat1.x * mat2.y - mat1.y * mat2.x;

            var det = mat0.x * inverse.mat0.x + mat0.y * inverse.mat1.x + mat0.z * inverse.mat2.x;
            if (MathX.Fabs(det) < Matrix_.MATRIX_INVERSE_EPSILON)
                return false;

            inverse.mat0.y = mat0.z * mat2.y - mat0.y * mat2.z;
            inverse.mat0.z = mat0.y * mat1.z - mat0.z * mat1.y;
            inverse.mat1.y = mat0.x * mat2.z - mat0.z * mat2.x;
            inverse.mat1.z = mat0.z * mat1.x - mat0.x * mat1.z;
            inverse.mat2.y = mat0.y * mat2.x - mat0.x * mat2.y;
            inverse.mat2.z = mat0.x * mat1.y - mat0.y * mat1.x;

            var invDet = 1.0f / det;
            mat0.x = inverse.mat0.x * invDet; mat0.y = inverse.mat0.y * invDet; mat0.z = inverse.mat0.z * invDet;
            mat1.x = inverse.mat1.x * invDet; mat1.y = inverse.mat1.y * invDet; mat1.z = inverse.mat1.z * invDet;
            mat2.x = inverse.mat2.x * invDet; mat2.y = inverse.mat2.y * invDet; mat2.z = inverse.mat2.z * invDet;

            return true;
        }
        public Matrix3x3 InverseFast() // returns the inverse ( m * m.Inverse() = identity )
        {
            var invMat = this;
            var r = invMat.InverseFastSelf();
            Debug.Assert(r);
            return invMat;
        }
        public bool InverseFastSelf()    // returns false if determinant is zero
        {
            // 18+3+9 = 30 multiplications
            //			 1 division
            Matrix3x3 inverse;
            inverse.mat0.x = mat1.y * mat2.z - mat1.z * mat2.y;
            inverse.mat1.x = mat1.z * mat2.x - mat1.x * mat2.z;
            inverse.mat2.x = mat1.x * mat2.y - mat1.y * mat2.x;

            var det = mat0.x * inverse.mat0.x + mat0.y * inverse.mat1.x + mat0.z * inverse.mat2.x;
            if (MathX.Fabs(det) < Matrix_.MATRIX_INVERSE_EPSILON)
                return false;

            inverse.mat0.y = mat0.z * mat2.y - mat0.y * mat2.z;
            inverse.mat0.z = mat0.y * mat1.z - mat0.z * mat1.y;
            inverse.mat1.y = mat0.x * mat2.z - mat0.z * mat2.x;
            inverse.mat1.z = mat0.z * mat1.x - mat0.x * mat1.z;
            inverse.mat2.y = mat0.y * mat2.x - mat0.x * mat2.y;
            inverse.mat2.z = mat0.x * mat1.y - mat0.y * mat1.x;

            var invDet = 1.0f / det;
            mat0.x = inverse.mat0.x * invDet; mat0.y = inverse.mat0.y * invDet; mat0.z = inverse.mat0.z * invDet;
            mat1.x = inverse.mat1.x * invDet; mat1.y = inverse.mat1.y * invDet; mat1.z = inverse.mat1.z * invDet;
            mat2.x = inverse.mat2.x * invDet; mat2.y = inverse.mat2.y * invDet; mat2.z = inverse.mat2.z * invDet;

            return true;
        }
        public Matrix3x3 TransposeMultiply(Matrix3x3 b)
            => new(
            mat0.x * b.mat0.x + mat1.x * b.mat1.x + mat2.x * b.mat2.x,
            mat0.x * b.mat0.y + mat1.x * b.mat1.y + mat2.x * b.mat2.y,
            mat0.x * b.mat0.z + mat1.x * b.mat1.z + mat2.x * b.mat2.z,
            mat0.y * b.mat0.x + mat1.y * b.mat1.x + mat2.y * b.mat2.x,
            mat0.y * b.mat0.y + mat1.y * b.mat1.y + mat2.y * b.mat2.y,
            mat0.y * b.mat0.z + mat1.y * b.mat1.z + mat2.y * b.mat2.z,
            mat0.z * b.mat0.x + mat1.z * b.mat1.x + mat2.z * b.mat2.x,
            mat0.z * b.mat0.y + mat1.z * b.mat1.y + mat2.z * b.mat2.y,
            mat0.z * b.mat0.z + mat1.z * b.mat1.z + mat2.z * b.mat2.z);

        public Matrix3x3 InertiaTranslate(float mass, Vector3 centerOfMass, Vector3 translation)
        {
            var newCenter = centerOfMass + translation;

            Matrix3x3 m;
            m.mat0.x = mass * ((centerOfMass.y * centerOfMass.y + centerOfMass.z * centerOfMass.z) - (newCenter.y * newCenter.y + newCenter.z * newCenter.z));
            m.mat1.y = mass * ((centerOfMass.x * centerOfMass.x + centerOfMass.z * centerOfMass.z) - (newCenter.x * newCenter.x + newCenter.z * newCenter.z));
            m.mat2.z = mass * ((centerOfMass.x * centerOfMass.x + centerOfMass.y * centerOfMass.y) - (newCenter.x * newCenter.x + newCenter.y * newCenter.y));

            m.mat0.y = m.mat1.x = mass * (newCenter.x * newCenter.y - centerOfMass.x * centerOfMass.y);
            m.mat1.z = m.mat2.y = mass * (newCenter.y * newCenter.z - centerOfMass.y * centerOfMass.z);
            m.mat0.z = m.mat2.x = mass * (newCenter.x * newCenter.z - centerOfMass.x * centerOfMass.z);

            return this + m;
        }
        public Matrix3x3 InertiaTranslateSelf(float mass, Vector3 centerOfMass, Vector3 translation)
        {
            var newCenter = centerOfMass + translation;

            Matrix3x3 m;
            m.mat0.x = mass * ((centerOfMass.y * centerOfMass.y + centerOfMass.z * centerOfMass.z) - (newCenter.y * newCenter.y + newCenter.z * newCenter.z));
            m.mat1.y = mass * ((centerOfMass.x * centerOfMass.x + centerOfMass.z * centerOfMass.z) - (newCenter.x * newCenter.x + newCenter.z * newCenter.z));
            m.mat2.z = mass * ((centerOfMass.x * centerOfMass.x + centerOfMass.y * centerOfMass.y) - (newCenter.x * newCenter.x + newCenter.y * newCenter.y));

            m.mat0.y = m.mat1.x = mass * (newCenter.x * newCenter.y - centerOfMass.x * centerOfMass.y);
            m.mat1.z = m.mat2.y = mass * (newCenter.y * newCenter.z - centerOfMass.y * centerOfMass.z);
            m.mat0.z = m.mat2.x = mass * (newCenter.x * newCenter.z - centerOfMass.x * centerOfMass.z);

            this += m;

            return this;
        }
        public Matrix3x3 InertiaRotate(Matrix3x3 rotation)
            // NOTE: the rotation matrix is stored column-major
            => rotation.Transpose() * this * rotation;
        public Matrix3x3 InertiaRotateSelf(Matrix3x3 rotation)
        {
            // NOTE: the rotation matrix is stored column-major
            this = rotation.Transpose() * this * rotation;
            return this;
        }

        public static int Dimension
            => 9;

        public Angles ToAngles()
        {
            var sp = mat0.z;
            // cap off our sin value so that we don't get any NANs
            if (sp > 1.0f) sp = 1.0f;
            else if (sp < -1.0f) sp = -1.0f;

            var theta = -(float)Math.Asin(sp);
            var cp = (float)Math.Cos(theta);

            Angles angles;
            if (cp > 8192.0f * MathX.FLT_EPSILON)
            {
                angles.pitch = MathX.RAD2DEG(theta);
                angles.yaw = MathX.RAD2DEG((float)Math.Atan2(mat0.y, mat0.x));
                angles.roll = MathX.RAD2DEG((float)Math.Atan2(mat1.z, mat2.z));
            }
            else
            {
                angles.pitch = MathX.RAD2DEG(theta);
                angles.yaw = MathX.RAD2DEG(-(float)Math.Atan2(mat1.x, mat1.y));
                angles.roll = 0;
            }
            return angles;
        }
        static int[] _ToQuat_next = { 1, 2, 0 };
        public Quat ToQuat()
        {
            float t, s;
            var q = new Quat();

            var trace = mat0.x + mat1.y + mat2.z;
            if (trace > 0.0f)
            {
                t = trace + 1.0f;
                s = MathX.InvSqrt(t) * 0.5f;

                q.w = s * t;
                q.x = (mat2.y - mat1.z) * s;
                q.y = (mat0.z - mat2.x) * s;
                q.z = (mat1.x - mat0.y) * s;
            }
            else
            {
                var i = 0;
                if (mat1.y > mat0.x) i = 1;
                if (mat2.z > this[i][i]) i = 2;
                var j = _ToQuat_next[i];
                var k = _ToQuat_next[j];

                t = this[i][i] - (this[j][j] + this[k][k]) + 1.0f;
                s = MathX.InvSqrt(t) * 0.5f;

                q[i] = s * t;
                q.w = (this[k][j] - this[j][k]) * s;
                q[j] = (this[j][i] + this[i][j]) * s;
                q[k] = (this[k][i] + this[i][k]) * s;
            }
            return q;
        }
        public CQuat ToCQuat()
        {
            var q = ToQuat();
            return q.w < 0.0f
                ? new CQuat(-q.x, -q.y, -q.z)
                : new CQuat(q.x, q.y, q.z);
        }
        static int[] _ToRotation_next = { 1, 2, 0 };
        public Rotation ToRotation()
        {
            var r = new Rotation();
            float t, s;

            var trace = mat0.x + mat1.y + mat2.z;
            if (trace > 0.0f)
            {
                t = trace + 1.0f;
                s = MathX.InvSqrt(t) * 0.5f;

                r.angle = s * t;
                r.vec.x = (mat2.y - mat1.z) * s;
                r.vec.y = (mat0.z - mat2.x) * s;
                r.vec.z = (mat1.x - mat0.y) * s;
            }
            else
            {
                var i = 0;
                if (mat1.y > mat0.x) i = 1;
                if (mat2.z > this[i][i]) i = 2;
                var j = _ToRotation_next[i];
                var k = _ToRotation_next[j];

                t = (this[i][i] - (this[j][j] + this[k][k])) + 1.0f;
                s = MathX.InvSqrt(t) * 0.5f;

                r.vec[i] = s * t;
                r.angle = (this[k][j] - this[j][k]) * s;
                r.vec[j] = (this[j][i] + this[i][j]) * s;
                r.vec[k] = (this[k][i] + this[i][k]) * s;
            }
            r.angle = MathX.ACos(r.angle);
            if (MathX.Fabs(r.angle) < 1e-10f)
            {
                r.vec.Set(0.0f, 0.0f, 1.0f);
                r.angle = 0.0f;
            }
            else
            {
                r.vec.Normalize();
                r.vec.FixDegenerateNormal();
                r.angle *= 2.0f * MathX.M_RAD2DEG;
            }

            r.origin.Zero();
            r.axis = this;
            r.axisValid = true;
            return r;
        }
        public Matrix4x4 ToMat4()
            // NOTE: Matrix3x3 is transposed because it is column-major
            => new(
                mat0.x, mat1.x, mat2.x, 0.0f,
                mat0.y, mat1.y, mat2.y, 0.0f,
                mat0.z, mat1.z, mat2.z, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f);
        public Vector3 ToAngularVelocity()
        {
            var rotation = ToRotation();
            return rotation.GetVec() * MathX.DEG2RAD(rotation.GetAngle());
        }
        public unsafe T ToFloatPtr<T>(FloatPtr<T> callback)
            => mat0.ToFloatPtr(callback);
        public unsafe string ToString(int precision = 2)
            => ToFloatPtr(array => StringX.FloatArrayToString(array, Dimension, precision));

        public void TransposeMultiply(Matrix3x3 inv, Matrix3x3 b, out Matrix3x3 dst)
        {
            dst.mat0.x = inv.mat0.x * b.mat0.x + inv.mat1.x * b.mat1.x + inv.mat2.x * b.mat2.x;
            dst.mat0.y = inv.mat0.x * b.mat0.y + inv.mat1.x * b.mat1.y + inv.mat2.x * b.mat2.y;
            dst.mat0.z = inv.mat0.x * b.mat0.z + inv.mat1.x * b.mat1.z + inv.mat2.x * b.mat2.z;
            dst.mat1.x = inv.mat0.y * b.mat0.x + inv.mat1.y * b.mat1.x + inv.mat2.y * b.mat2.x;
            dst.mat1.y = inv.mat0.y * b.mat0.y + inv.mat1.y * b.mat1.y + inv.mat2.y * b.mat2.y;
            dst.mat1.z = inv.mat0.y * b.mat0.z + inv.mat1.y * b.mat1.z + inv.mat2.y * b.mat2.z;
            dst.mat2.x = inv.mat0.z * b.mat0.x + inv.mat1.z * b.mat1.x + inv.mat2.z * b.mat2.x;
            dst.mat2.y = inv.mat0.z * b.mat0.y + inv.mat1.z * b.mat1.y + inv.mat2.z * b.mat2.y;
            dst.mat2.z = inv.mat0.z * b.mat0.z + inv.mat1.z * b.mat1.z + inv.mat2.z * b.mat2.z;
        }
        public Matrix3x3 SkewSymmetric(Vector3 src)
            => new(0.0f, -src.z, src.y, src.z, 0.0f, -src.x, -src.y, src.x, 0.0f);

        internal Vector3 mat0;
        internal Vector3 mat1;
        internal Vector3 mat2;

        public static Matrix3x3 zero = new(new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        public static Matrix3x3 identity = new(new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1));
        //#define default identity
    }

    public struct Matrix4x4
    {
        public Matrix4x4(Vector4 x, Vector4 y, Vector4 z, Vector4 w)
        {
            mat0 = x;
            mat1 = y;
            mat2 = z;
            mat3 = w;
        }
        public Matrix4x4(float xx, float xy, float xz, float xw,
            float yx, float yy, float yz, float yw,
            float zx, float zy, float zz, float zw,
            float wx, float wy, float wz, float ww)
        {
            mat0.x = xx; mat0.y = xy; mat0.z = xz; mat0.w = xw;
            mat1.x = yx; mat1.y = yy; mat1.z = yz; mat1.w = yw;
            mat2.x = zx; mat2.y = zy; mat2.z = zz; mat2.w = zw;
            mat3.x = wx; mat3.y = wy; mat3.z = wz; mat3.w = ww;
        }
        public Matrix4x4(Matrix3x3 rotation, Vector3 translation)
        {
            // NOTE: Matrix3x3 is transposed because it is column-major
            mat0.x = rotation.mat0.x;
            mat0.y = rotation.mat1.x;
            mat0.z = rotation.mat2.x;
            mat0.w = translation.x;
            mat1.x = rotation.mat0.y;
            mat1.y = rotation.mat1.y;
            mat1.z = rotation.mat2.y;
            mat1.w = translation.y;
            mat2.x = rotation.mat0.z;
            mat2.y = rotation.mat1.z;
            mat2.z = rotation.mat2.z;
            mat2.w = translation.z;
            mat3.x = 0.0f;
            mat3.y = 0.0f;
            mat3.z = 0.0f;
            mat3.w = 1.0f;
        }
        public Matrix4x4(float[][] src)
        {
            mat0 = new Vector4(src[0][0], src[0][1], src[0][2], src[0][3]);
            mat1 = new Vector4(src[1][0], src[1][1], src[1][2], src[1][3]);
            mat2 = new Vector4(src[2][0], src[2][1], src[2][2], src[2][3]);
            mat3 = new Vector4(src[3][0], src[3][1], src[3][2], src[3][3]);
        }

        public unsafe ref Vector4 this[int index]
        {
            get
            {
                fixed (Vector4* mat = &mat0)
                    return ref mat[index];
            }
        }

        public static Matrix4x4 operator *(Matrix4x4 _, float a)
            => new(
            _.mat0.x * a, _.mat0.y * a, _.mat0.z * a, _.mat0.w * a,
            _.mat1.x * a, _.mat1.y * a, _.mat1.z * a, _.mat1.w * a,
            _.mat2.x * a, _.mat2.y * a, _.mat2.z * a, _.mat2.w * a,
            _.mat3.x * a, _.mat3.y * a, _.mat3.z * a, _.mat3.w * a);
        public static Vector4 operator *(Matrix4x4 _, Vector4 vec)
            => new(
            _.mat0.x * vec.x + _.mat0.y * vec.y + _.mat0.z * vec.z + _.mat0.w * vec.w,
            _.mat1.x * vec.x + _.mat1.y * vec.y + _.mat1.z * vec.z + _.mat1.w * vec.w,
            _.mat2.x * vec.x + _.mat2.y * vec.y + _.mat2.z * vec.z + _.mat2.w * vec.w,
            _.mat3.x * vec.x + _.mat3.y * vec.y + _.mat3.z * vec.z + _.mat3.w * vec.w);
        public static Vector3 operator *(Matrix4x4 _, Vector3 vec)
        {
            var s = _.mat3.x * vec.x + _.mat3.y * vec.y + _.mat3.z * vec.z + _.mat3.w;
            if (s == 0.0f)
                return new(0.0f, 0.0f, 0.0f);
            if (s == 1.0f)
                return new(
                _.mat0.x * vec.x + _.mat0.y * vec.y + _.mat0.z * vec.z + _.mat0.w,
                _.mat1.x * vec.x + _.mat1.y * vec.y + _.mat1.z * vec.z + _.mat1.w,
                _.mat2.x * vec.x + _.mat2.y * vec.y + _.mat2.z * vec.z + _.mat2.w);
            else
            {
                var invS = 1.0f / s;
                return new(
                (_.mat0.x * vec.x + _.mat0.y * vec.y + _.mat0.z * vec.z + _.mat0.w) * invS,
                (_.mat1.x * vec.x + _.mat1.y * vec.y + _.mat1.z * vec.z + _.mat1.w) * invS,
                (_.mat2.x * vec.x + _.mat2.y * vec.y + _.mat2.z * vec.z + _.mat2.w) * invS);
            }
        }
        public static unsafe Matrix4x4 operator *(Matrix4x4 _, Matrix4x4 a)
        {
            Matrix4x4 dst;
            var m1Ptr = (float*)&_;
            var m2Ptr = (float*)&a;
            var dstPtr = (float*)&dst;
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    *dstPtr = m1Ptr[0] * m2Ptr[0 * 4 + j]
                            + m1Ptr[1] * m2Ptr[1 * 4 + j]
                            + m1Ptr[2] * m2Ptr[2 * 4 + j]
                            + m1Ptr[3] * m2Ptr[3 * 4 + j];
                    dstPtr++;
                }
                m1Ptr += 4;
            }
            return dst;
        }

        public static Matrix4x4 operator +(Matrix4x4 _, Matrix4x4 a)
            => new(
            _.mat0.x + a.mat0.x, _.mat0.y + a.mat0.y, _.mat0.z + a.mat0.z, _.mat0.w + a.mat0.w,
            _.mat1.x + a.mat1.x, _.mat1.y + a.mat1.y, _.mat1.z + a.mat1.z, _.mat1.w + a.mat1.w,
            _.mat2.x + a.mat2.x, _.mat2.y + a.mat2.y, _.mat2.z + a.mat2.z, _.mat2.w + a.mat2.w,
            _.mat3.x + a.mat3.x, _.mat3.y + a.mat3.y, _.mat3.z + a.mat3.z, _.mat3.w + a.mat3.w);
        public static Matrix4x4 operator -(Matrix4x4 _, Matrix4x4 a)
            => new(
            _.mat0.x - a.mat0.x, _.mat0.y - a.mat0.y, _.mat0.z - a.mat0.z, _.mat0.w - a.mat0.w,
            _.mat1.x - a.mat1.x, _.mat1.y - a.mat1.y, _.mat1.z - a.mat1.z, _.mat1.w - a.mat1.w,
            _.mat2.x - a.mat2.x, _.mat2.y - a.mat2.y, _.mat2.z - a.mat2.z, _.mat2.w - a.mat2.w,
            _.mat3.x - a.mat3.x, _.mat3.y - a.mat3.y, _.mat3.z - a.mat3.z, _.mat3.w - a.mat3.w);

        public static Matrix4x4 operator *(float a, Matrix4x4 mat)
            => mat * a;
        public static Vector4 operator *(Vector4 vec, Matrix4x4 mat)
            => mat * vec;
        public static Vector3 operator *(Vector3 vec, Matrix4x4 mat)
            => mat * vec;

        public unsafe bool Compare(Matrix4x4 a)                       // exact compare, no epsilon
        {
            fixed (void* mat = &mat0)
            {
                var ptr1 = (float*)&mat;
                var ptr2 = (float*)&a.mat0;
                for (var i = 0; i < 4 * 4; i++)
                    if (ptr1[i] != ptr2[i])
                        return false;
                return true;
            }
        }
        public unsafe bool Compare(Matrix4x4 a, float epsilon)  // compare with epsilon
        {
            fixed (void* mat = &mat0)
            {
                var ptr1 = (float*)&mat;
                var ptr2 = (float*)&a.mat0;
                for (var i = 0; i < 4 * 4; i++)
                    if (MathX.Fabs(ptr1[i] - ptr2[i]) > epsilon)
                        return false;
                return true;
            }
        }
        public static bool operator ==(Matrix4x4 _, Matrix4x4 a)                   // exact compare, no epsilon
            => _.Compare(a);
        public static bool operator !=(Matrix4x4 _, Matrix4x4 a)                   // exact compare, no epsilon
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Matrix4x4 q && Compare(q);
        public override int GetHashCode()
            => mat0.GetHashCode();

        public unsafe void Zero()
        {
            fixed (void* p = &this)
                U.memset(p, 0, sizeof(Matrix4x4));
        }
        public void Identity()
            => this = identity;
        public bool IsIdentity(float epsilon = Matrix_.MATRIX_EPSILON)
            => Compare(identity, epsilon);
        public bool IsSymmetric(float epsilon = Matrix_.MATRIX_EPSILON)
        {
            for (var i = 1; i < 4; i++)
                for (var j = 0; j < i; j++)
                    if (MathX.Fabs(this[i][j] - this[j][i]) > epsilon)
                        return false;
            return true;
        }
        public bool IsDiagonal(float epsilon = Matrix_.MATRIX_EPSILON)
        {
            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 4; j++)
                    if (i != j && MathX.Fabs(this[i][j]) > epsilon)
                        return false;
            return true;
        }
        public bool IsRotated()
        {
            if (mat0.y == 0 && mat0.z == 0 &&
                mat1.x == 0 && mat1.z == 0 &&
                mat2.x == 0 && mat2.y == 0)
                return false;
            return true;
        }

        public void ProjectVector(Vector4 src, out Vector4 dst)
        {
            dst.x = src * mat0;
            dst.y = src * mat1;
            dst.z = src * mat2;
            dst.w = src * mat3;
        }
        public void UnprojectVector(Vector4 src, out Vector4 dst)
            => dst = mat0 * src.x + mat1 * src.y + mat2 * src.z + mat3 * src.w;

        public float Trace()
            => mat0.x + mat1.y + mat2.z + mat3.w;
        public float Determinant()
        {
            // 2x2 sub-determinants
            var det2_01_01 = mat0.x * mat1.y - mat0.y * mat1.x;
            var det2_01_02 = mat0.x * mat1.z - mat0.z * mat1.x;
            var det2_01_03 = mat0.x * mat1.w - mat0.w * mat1.x;
            var det2_01_12 = mat0.y * mat1.z - mat0.z * mat1.y;
            var det2_01_13 = mat0.y * mat1.w - mat0.w * mat1.y;
            var det2_01_23 = mat0.z * mat1.w - mat0.w * mat1.z;

            // 3x3 sub-determinants
            var det3_201_012 = mat2.x * det2_01_12 - mat2.y * det2_01_02 + mat2.z * det2_01_01;
            var det3_201_013 = mat2.x * det2_01_13 - mat2.y * det2_01_03 + mat2.w * det2_01_01;
            var det3_201_023 = mat2.x * det2_01_23 - mat2.z * det2_01_03 + mat2.w * det2_01_02;
            var det3_201_123 = mat2.y * det2_01_23 - mat2.z * det2_01_13 + mat2.w * det2_01_12;

            return -det3_201_123 * mat3.x + det3_201_023 * mat3.y - det3_201_013 * mat3.z + det3_201_012 * mat3.w;
        }
        public Matrix4x4 Transpose()   // returns transpose
        {
            var transpose = new Matrix4x4();
            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 4; j++)
                    transpose[i][j] = this[j][i];
            return transpose;
        }
        public Matrix4x4 TransposeSelf()
        {
            for (var i = 0; i < 4; i++)
                for (var j = i + 1; j < 4; j++)
                {
                    var temp = this[i][j];
                    this[i][j] = this[j][i];
                    this[j][i] = temp;
                }
            return this;
        }
        public Matrix4x4 Inverse()     // returns the inverse ( m * m.Inverse() = identity )
        {
            var invMat = this;
            var r = invMat.InverseSelf();
            Debug.Assert(r);
            return invMat;
        }
        public bool InverseSelf()        // returns false if determinant is zero
        {
            // 84+4+16 = 104 multiplications
            //			   1 division
            // 2x2 sub-determinants required to calculate 4x4 determinant
            var det2_01_01 = mat0.x * mat1.y - mat0.y * mat1.x;
            var det2_01_02 = mat0.x * mat1.z - mat0.z * mat1.x;
            var det2_01_03 = mat0.x * mat1.w - mat0.w * mat1.x;
            var det2_01_12 = mat0.y * mat1.z - mat0.z * mat1.y;
            var det2_01_13 = mat0.y * mat1.w - mat0.w * mat1.y;
            var det2_01_23 = mat0.z * mat1.w - mat0.w * mat1.z;

            // 3x3 sub-determinants required to calculate 4x4 determinant
            var det3_201_012 = mat2.x * det2_01_12 - mat2.y * det2_01_02 + mat2.z * det2_01_01;
            var det3_201_013 = mat2.x * det2_01_13 - mat2.y * det2_01_03 + mat2.w * det2_01_01;
            var det3_201_023 = mat2.x * det2_01_23 - mat2.z * det2_01_03 + mat2.w * det2_01_02;
            var det3_201_123 = mat2.y * det2_01_23 - mat2.z * det2_01_13 + mat2.w * det2_01_12;

            var det = -det3_201_123 * mat3.x + det3_201_023 * mat3.y - det3_201_013 * mat3.z + det3_201_012 * mat3.w;
            if (MathX.Fabs(det) < Matrix_.MATRIX_INVERSE_EPSILON)
                return false;

            var invDet = 1.0f / det;

            // remaining 2x2 sub-determinants
            var det2_03_01 = mat0.x * mat3.y - mat0.y * mat3.x;
            var det2_03_02 = mat0.x * mat3.z - mat0.z * mat3.x;
            var det2_03_03 = mat0.x * mat3.w - mat0.w * mat3.x;
            var det2_03_12 = mat0.y * mat3.z - mat0.y * mat3.y;
            var det2_03_13 = mat0.y * mat3.z - mat0.z * mat3.y;
            var det2_03_23 = mat0.z * mat3.z - mat0.z * mat3.z;

            var det2_13_01 = mat1.x * mat3.y - mat1.y * mat3.x;
            var det2_13_02 = mat1.x * mat3.z - mat1.z * mat3.x;
            var det2_13_03 = mat1.x * mat3.w - mat1.w * mat3.x;
            var det2_13_12 = mat1.y * mat3.z - mat1.z * mat3.y;
            var det2_13_13 = mat1.y * mat3.w - mat1.w * mat3.y;
            var det2_13_23 = mat1.z * mat3.w - mat1.w * mat3.z;

            // remaining 3x3 sub-determinants
            var det3_203_012 = mat2.x * det2_03_12 - mat2.y * det2_03_02 + mat2.z * det2_03_01;
            var det3_203_013 = mat2.x * det2_03_13 - mat2.y * det2_03_03 + mat2.w * det2_03_01;
            var det3_203_023 = mat2.x * det2_03_23 - mat2.z * det2_03_03 + mat2.w * det2_03_02;
            var det3_203_123 = mat2.y * det2_03_23 - mat2.z * det2_03_13 + mat2.w * det2_03_12;

            var det3_213_012 = mat2.x * det2_13_12 - mat2.y * det2_13_02 + mat2.z * det2_13_01;
            var det3_213_013 = mat2.x * det2_13_13 - mat2.y * det2_13_03 + mat2.w * det2_13_01;
            var det3_213_023 = mat2.x * det2_13_23 - mat2.z * det2_13_03 + mat2.w * det2_13_02;
            var det3_213_123 = mat2.y * det2_13_23 - mat2.z * det2_13_13 + mat2.w * det2_13_12;

            var det3_301_012 = mat3.x * det2_01_12 - mat3.y * det2_01_02 + mat3.z * det2_01_01;
            var det3_301_013 = mat3.x * det2_01_13 - mat3.y * det2_01_03 + mat3.w * det2_01_01;
            var det3_301_023 = mat3.x * det2_01_23 - mat3.z * det2_01_03 + mat3.w * det2_01_02;
            var det3_301_123 = mat3.y * det2_01_23 - mat3.z * det2_01_13 + mat3.w * det2_01_12;

            mat0.x = -det3_213_123 * invDet; mat1.x = +det3_213_023 * invDet; mat2.x = -det3_213_013 * invDet; mat3.x = +det3_213_012 * invDet;
            mat0.y = +det3_203_123 * invDet; mat1.y = -det3_203_023 * invDet; mat2.y = +det3_203_013 * invDet; mat3.y = -det3_203_012 * invDet;
            mat0.z = +det3_301_123 * invDet; mat1.z = -det3_301_023 * invDet; mat2.z = +det3_301_013 * invDet; mat3.z = -det3_301_012 * invDet;
            mat0.w = -det3_201_123 * invDet; mat1.w = +det3_201_023 * invDet; mat2.w = -det3_201_013 * invDet; mat3.w = +det3_201_012 * invDet;

            return true;
        }
        public Matrix4x4 InverseFast() // returns the inverse ( m * m.Inverse() = identity )
        {
            var invMat = this;
            var r = invMat.InverseFastSelf();
            Debug.Assert(r);
            return invMat;
        }
        public bool InverseFastSelf()    // returns false if determinant is zero
        {
            // 84+4+16 = 104 multiplications
            //			   1 division
            // 2x2 sub-determinants required to calculate 4x4 determinant
            var det2_01_01 = mat0.x * mat1.y - mat0.y * mat1.x;
            var det2_01_02 = mat0.x * mat1.z - mat0.z * mat1.x;
            var det2_01_03 = mat0.x * mat1.w - mat0.w * mat1.x;
            var det2_01_12 = mat0.y * mat1.z - mat0.z * mat1.y;
            var det2_01_13 = mat0.y * mat1.w - mat0.w * mat1.y;
            var det2_01_23 = mat0.z * mat1.w - mat0.w * mat1.z;

            // 3x3 sub-determinants required to calculate 4x4 determinant
            var det3_201_012 = mat2.x * det2_01_12 - mat2.y * det2_01_02 + mat2.z * det2_01_01;
            var det3_201_013 = mat2.x * det2_01_13 - mat2.y * det2_01_03 + mat2.w * det2_01_01;
            var det3_201_023 = mat2.x * det2_01_23 - mat2.z * det2_01_03 + mat2.w * det2_01_02;
            var det3_201_123 = mat2.y * det2_01_23 - mat2.z * det2_01_13 + mat2.w * det2_01_12;

            var det = -det3_201_123 * mat3.x + det3_201_023 * mat3.y - det3_201_013 * mat3.z + det3_201_012 * mat3.w;
            if (MathX.Fabs(det) < Matrix_.MATRIX_INVERSE_EPSILON)
                return false;

            var invDet = 1.0f / det;

            // remaining 2x2 sub-determinants
            var det2_03_01 = mat0.x * mat3.y - mat0.y * mat3.x;
            var det2_03_02 = mat0.x * mat3.z - mat0.z * mat3.x;
            var det2_03_03 = mat0.x * mat3.w - mat0.w * mat3.x;
            var det2_03_12 = mat0.y * mat3.z - mat0.z * mat3.y;
            var det2_03_13 = mat0.y * mat3.w - mat0.w * mat3.y;
            var det2_03_23 = mat0.z * mat3.w - mat0.w * mat3.z;

            var det2_13_01 = mat1.x * mat3.y - mat1.y * mat3.x;
            var det2_13_02 = mat1.x * mat3.z - mat1.z * mat3.x;
            var det2_13_03 = mat1.x * mat3.w - mat1.w * mat3.x;
            var det2_13_12 = mat1.y * mat3.z - mat1.z * mat3.y;
            var det2_13_13 = mat1.y * mat3.w - mat1.w * mat3.y;
            var det2_13_23 = mat1.z * mat3.w - mat1.w * mat3.z;

            // remaining 3x3 sub-determinants
            var det3_203_012 = mat2.x * det2_03_12 - mat2.y * det2_03_02 + mat2.y * det2_03_01;
            var det3_203_013 = mat2.x * det2_03_13 - mat2.y * det2_03_03 + mat2.z * det2_03_01;
            var det3_203_023 = mat2.x * det2_03_23 - mat2.z * det2_03_03 + mat2.z * det2_03_02;
            var det3_203_123 = mat2.y * det2_03_23 - mat2.z * det2_03_13 + mat2.z * det2_03_12;

            var det3_213_012 = mat2.x * det2_13_12 - mat2.y * det2_13_02 + mat2.y * det2_13_01;
            var det3_213_013 = mat2.x * det2_13_13 - mat2.y * det2_13_03 + mat2.z * det2_13_01;
            var det3_213_023 = mat2.x * det2_13_23 - mat2.z * det2_13_03 + mat2.z * det2_13_02;
            var det3_213_123 = mat2.y * det2_13_23 - mat2.z * det2_13_13 + mat2.z * det2_13_12;

            var det3_301_012 = mat3.x * det2_01_12 - mat3.y * det2_01_02 + mat3.y * det2_01_01;
            var det3_301_013 = mat3.x * det2_01_13 - mat3.y * det2_01_03 + mat3.z * det2_01_01;
            var det3_301_023 = mat3.x * det2_01_23 - mat3.z * det2_01_03 + mat3.z * det2_01_02;
            var det3_301_123 = mat3.y * det2_01_23 - mat3.z * det2_01_13 + mat3.z * det2_01_12;

            mat0.x = -det3_213_123 * invDet; mat1.x = +det3_213_023 * invDet; mat2.x = -det3_213_013 * invDet; mat3.x = +det3_213_012 * invDet;
            mat0.y = +det3_203_123 * invDet; mat1.y = -det3_203_023 * invDet; mat2.y = +det3_203_013 * invDet; mat3.y = -det3_203_012 * invDet;
            mat0.z = +det3_301_123 * invDet; mat1.z = -det3_301_023 * invDet; mat2.z = +det3_301_013 * invDet; mat3.z = -det3_301_012 * invDet;
            mat0.w = -det3_201_123 * invDet; mat1.w = +det3_201_023 * invDet; mat2.w = -det3_201_013 * invDet; mat3.w = +det3_201_012 * invDet;

            return true;
        }
        public Matrix4x4 TransposeMultiply(Matrix4x4 b)
            => throw new NotSupportedException();

        public static int Dimension
            => 16;

        public unsafe T ToFloatPtr<T>(FloatPtr<T> callback)
            => mat0.ToFloatPtr(callback);
        public unsafe string ToString(int precision = 2)
            => ToFloatPtr(array => StringX.FloatArrayToString(array, Dimension, precision));

        internal Vector4 mat0;
        internal Vector4 mat1;
        internal Vector4 mat2;
        internal Vector4 mat3;

        public static Matrix4x4 zero = new(new Vector4(0, 0, 0, 0), new Vector4(0, 0, 0, 0), new Vector4(0, 0, 0, 0), new Vector4(0, 0, 0, 0));
        public static Matrix4x4 identity = new(new Vector4(1, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1));
        //#define default	identity
    }

    public struct Matrix5x5
    {
        public Matrix5x5(Vector5 v0, Vector5 v1, Vector5 v2, Vector5 v3, Vector5 v4)
        {
            mat0 = v0;
            mat1 = v1;
            mat2 = v2;
            mat3 = v3;
            mat4 = v4;
        }
        public Matrix5x5(float[][] src)
        {
            mat0 = new Vector5(src[0][0], src[0][1], src[0][2], src[0][3], src[0][4]);
            mat1 = new Vector5(src[1][0], src[1][1], src[1][2], src[1][3], src[1][4]);
            mat2 = new Vector5(src[2][0], src[2][1], src[2][2], src[2][3], src[2][4]);
            mat3 = new Vector5(src[3][0], src[3][1], src[3][2], src[3][3], src[3][4]);
            mat4 = new Vector5(src[4][0], src[4][1], src[4][2], src[4][3], src[4][4]);
        }

        public unsafe ref Vector5 this[int index]
        {
            get
            {
                fixed (Vector5* mat = &mat0)
                    return ref mat[index];
            }
        }

        public static Matrix5x5 operator *(Matrix5x5 _, float a)
            => new(
            new Vector5(_.mat0.x * a, _.mat0.y * a, _.mat0.z * a, _.mat0.s * a, _.mat0.t * a),
            new Vector5(_.mat1.x * a, _.mat1.y * a, _.mat1.z * a, _.mat1.s * a, _.mat1.t * a),
            new Vector5(_.mat2.x * a, _.mat2.y * a, _.mat2.z * a, _.mat2.s * a, _.mat2.t * a),
            new Vector5(_.mat3.x * a, _.mat3.y * a, _.mat3.z * a, _.mat3.s * a, _.mat3.t * a),
            new Vector5(_.mat4.x * a, _.mat4.y * a, _.mat4.z * a, _.mat4.s * a, _.mat4.t * a));
        public static Vector5 operator *(Matrix5x5 _, Vector5 vec)
            => new(
            _.mat0.x * vec.x + _.mat0.y * vec.y + _.mat0.z * vec.z + _.mat0.s * vec.s + _.mat0.t * vec.t,
            _.mat1.x * vec.x + _.mat1.y * vec.y + _.mat1.z * vec.z + _.mat1.s * vec.s + _.mat1.t * vec.t,
            _.mat2.x * vec.x + _.mat2.y * vec.y + _.mat2.z * vec.z + _.mat2.s * vec.s + _.mat2.t * vec.t,
            _.mat3.x * vec.x + _.mat3.y * vec.y + _.mat3.z * vec.z + _.mat3.s * vec.s + _.mat3.t * vec.t,
            _.mat4.x * vec.x + _.mat4.y * vec.y + _.mat4.z * vec.z + _.mat4.s * vec.s + _.mat4.t * vec.t);
        public static unsafe Matrix5x5 operator *(Matrix5x5 _, Matrix5x5 a)
        {
            Matrix5x5 dst;
            var m1Ptr = (float*)&_;
            var m2Ptr = (float*)&a;
            var dstPtr = (float*)&dst;
            for (var i = 0; i < 5; i++)
            {
                for (var j = 0; j < 5; j++)
                {
                    *dstPtr = m1Ptr[0] * m2Ptr[0 * 5 + j]
                        + m1Ptr[1] * m2Ptr[1 * 5 + j]
                        + m1Ptr[2] * m2Ptr[2 * 5 + j]
                        + m1Ptr[3] * m2Ptr[3 * 5 + j]
                        + m1Ptr[4] * m2Ptr[4 * 5 + j];
                    dstPtr++;
                }
                m1Ptr += 5;
            }
            return dst;
        }
        public static Matrix5x5 operator +(Matrix5x5 _, Matrix5x5 a)
            => new(
            new Vector5(_.mat0.x + a.mat0.x, _.mat0.y + a.mat0.y, _.mat0.z + a.mat0.z, _.mat0.s + a.mat0.s, _.mat0.t + a.mat0.t),
            new Vector5(_.mat1.x + a.mat1.x, _.mat1.y + a.mat1.y, _.mat1.z + a.mat1.z, _.mat1.s + a.mat1.s, _.mat1.t + a.mat1.t),
            new Vector5(_.mat2.x + a.mat2.x, _.mat2.y + a.mat2.y, _.mat2.z + a.mat2.z, _.mat2.s + a.mat2.s, _.mat2.t + a.mat2.t),
            new Vector5(_.mat3.x + a.mat3.x, _.mat3.y + a.mat3.y, _.mat3.z + a.mat3.z, _.mat3.s + a.mat3.s, _.mat3.t + a.mat3.t),
            new Vector5(_.mat4.x + a.mat4.x, _.mat4.y + a.mat4.y, _.mat4.z + a.mat4.z, _.mat4.s + a.mat4.s, _.mat4.t + a.mat4.t));
        public static Matrix5x5 operator -(Matrix5x5 _, Matrix5x5 a)
            => new(
            new Vector5(_.mat0.x - a.mat0.x, _.mat0.y - a.mat0.y, _.mat0.z - a.mat0.z, _.mat0.s - a.mat0.s, _.mat0.t - a.mat0.t),
            new Vector5(_.mat1.x - a.mat1.x, _.mat1.y - a.mat1.y, _.mat1.z - a.mat1.z, _.mat1.s - a.mat1.s, _.mat1.t - a.mat1.t),
            new Vector5(_.mat2.x - a.mat2.x, _.mat2.y - a.mat2.y, _.mat2.z - a.mat2.z, _.mat2.s - a.mat2.s, _.mat2.t - a.mat2.t),
            new Vector5(_.mat3.x - a.mat3.x, _.mat3.y - a.mat3.y, _.mat3.z - a.mat3.z, _.mat3.s - a.mat3.s, _.mat3.t - a.mat3.t),
            new Vector5(_.mat4.x - a.mat4.x, _.mat4.y - a.mat4.y, _.mat4.z - a.mat4.z, _.mat4.s - a.mat4.s, _.mat4.t - a.mat4.t));

        public static Matrix5x5 operator *(float a, Matrix5x5 mat)
            => mat * a;
        public static Vector5 operator *(Vector5 vec, Matrix5x5 mat)
            => mat * vec;

        public unsafe bool Compare(Matrix5x5 a)                       // exact compare, no epsilon
        {
            fixed (void* mat = &mat0)
            {
                var ptr1 = (float*)&mat;
                var ptr2 = (float*)&a.mat0;
                for (var i = 0; i < 5 * 5; i++)
                    if (ptr1[i] != ptr2[i])
                        return false;
                return true;
            }
        }
        public unsafe bool Compare(Matrix5x5 a, float epsilon)  // compare with epsilon
        {
            fixed (void* mat = &mat0)
            {
                var ptr1 = (float*)&mat;
                var ptr2 = (float*)&a.mat0;
                for (var i = 0; i < 5 * 5; i++)
                    if (MathX.Fabs(ptr1[i] - ptr2[i]) > epsilon)
                        return false;
                return true;
            }
        }
        public static bool operator ==(Matrix5x5 _, Matrix5x5 a)                   // exact compare, no epsilon
            => _.Compare(a);
        public static bool operator !=(Matrix5x5 _, Matrix5x5 a)                   // exact compare, no epsilon
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Matrix5x5 q && Compare(q);
        public override int GetHashCode()
            => mat0.GetHashCode();

        public unsafe void Zero()
        {
            fixed (void* p = &this)
                U.memset(p, 0, sizeof(Matrix5x5));
        }
        public void Identity()
            => this = identity;
        public bool IsIdentity(float epsilon = Matrix_.MATRIX_EPSILON)
            => Compare(identity, epsilon);
        public bool IsSymmetric(float epsilon = Matrix_.MATRIX_EPSILON)
        {
            for (var i = 1; i < 5; i++)
                for (var j = 0; j < i; j++)
                    if (MathX.Fabs(this[i][j] - this[j][i]) > epsilon)
                        return false;
            return true;
        }
        public bool IsDiagonal(float epsilon = Matrix_.MATRIX_EPSILON)
        {
            for (var i = 0; i < 5; i++)
                for (var j = 0; j < 5; j++)
                    if (i != j && MathX.Fabs(this[i][j]) > epsilon)
                        return false;
            return true;
        }

        public float Trace()
            => mat0.x + mat1.y + mat2.z + mat3.s + mat4.t;
        public float Determinant()
        {
            // 2x2 sub-determinants required to calculate 5x5 determinant
            var det2_34_01 = mat3.x * mat4.y - mat3.y * mat4.x;
            var det2_34_02 = mat3.x * mat4.z - mat3.z * mat4.x;
            var det2_34_03 = mat3.x * mat4.s - mat3.s * mat4.x;
            var det2_34_04 = mat3.x * mat4.t - mat3.t * mat4.x;
            var det2_34_12 = mat3.y * mat4.z - mat3.z * mat4.y;
            var det2_34_13 = mat3.y * mat4.s - mat3.s * mat4.y;
            var det2_34_14 = mat3.y * mat4.t - mat3.t * mat4.y;
            var det2_34_23 = mat3.z * mat4.s - mat3.s * mat4.z;
            var det2_34_24 = mat3.z * mat4.t - mat3.t * mat4.z;
            var det2_34_34 = mat3.s * mat4.t - mat3.t * mat4.s;

            // 3x3 sub-determinants required to calculate 5x5 determinant
            var det3_234_012 = mat2.x * det2_34_12 - mat2.y * det2_34_02 + mat2.z * det2_34_01;
            var det3_234_013 = mat2.x * det2_34_13 - mat2.y * det2_34_03 + mat2.s * det2_34_01;
            var det3_234_014 = mat2.x * det2_34_14 - mat2.y * det2_34_04 + mat2.t * det2_34_01;
            var det3_234_023 = mat2.x * det2_34_23 - mat2.z * det2_34_03 + mat2.s * det2_34_02;
            var det3_234_024 = mat2.x * det2_34_24 - mat2.z * det2_34_04 + mat2.t * det2_34_02;
            var det3_234_034 = mat2.x * det2_34_34 - mat2.s * det2_34_04 + mat2.t * det2_34_03;
            var det3_234_123 = mat2.y * det2_34_23 - mat2.z * det2_34_13 + mat2.s * det2_34_12;
            var det3_234_124 = mat2.y * det2_34_24 - mat2.z * det2_34_14 + mat2.t * det2_34_12;
            var det3_234_134 = mat2.y * det2_34_34 - mat2.s * det2_34_14 + mat2.t * det2_34_13;
            var det3_234_234 = mat2.z * det2_34_34 - mat2.s * det2_34_24 + mat2.t * det2_34_23;

            // 4x4 sub-determinants required to calculate 5x5 determinant
            var det4_1234_0123 = mat1.x * det3_234_123 - mat1.y * det3_234_023 + mat1.z * det3_234_013 - mat1.s * det3_234_012;
            var det4_1234_0124 = mat1.x * det3_234_124 - mat1.y * det3_234_024 + mat1.z * det3_234_014 - mat1.t * det3_234_012;
            var det4_1234_0134 = mat1.x * det3_234_134 - mat1.y * det3_234_034 + mat1.s * det3_234_014 - mat1.t * det3_234_013;
            var det4_1234_0234 = mat1.x * det3_234_234 - mat1.z * det3_234_034 + mat1.s * det3_234_024 - mat1.t * det3_234_023;
            var det4_1234_1234 = mat1.y * det3_234_234 - mat1.z * det3_234_134 + mat1.s * det3_234_124 - mat1.t * det3_234_123;

            // determinant of 5x5 matrix
            return mat0.x * det4_1234_1234 - mat0.y * det4_1234_0234 + mat0.z * det4_1234_0134 - mat0.s * det4_1234_0124 + mat0.t * det4_1234_0123;
        }
        public Matrix5x5 Transpose()   // returns transpose
        {
            var transpose = new Matrix5x5();
            for (var i = 0; i < 5; i++)
                for (var j = 0; j < 5; j++)
                    transpose[i][j] = this[j][i];
            return transpose;
        }
        public Matrix5x5 TransposeSelf()
        {
            for (var i = 0; i < 5; i++)
                for (var j = i + 1; j < 5; j++)
                {
                    var temp = this[i][j];
                    this[i][j] = this[j][i];
                    this[j][i] = temp;
                }
            return this;
        }
        public Matrix5x5 Inverse()     // returns the inverse ( m * m.Inverse() = identity )
        {
            var invMat = this;
            var r = invMat.InverseSelf();
            Debug.Assert(r);
            return invMat;
        }
        public bool InverseSelf()        // returns false if determinant is zero
        {
            // 280+5+25 = 310 multiplications
            //				1 division
            // 2x2 sub-determinants required to calculate 5x5 determinant
            var det2_34_01 = mat3.x * mat4.y - mat3.y * mat4.x;
            var det2_34_02 = mat3.x * mat4.z - mat3.z * mat4.x;
            var det2_34_03 = mat3.x * mat4.s - mat3.s * mat4.x;
            var det2_34_04 = mat3.x * mat4.t - mat3.t * mat4.x;
            var det2_34_12 = mat3.y * mat4.y - mat3.z * mat4.y;
            var det2_34_13 = mat3.y * mat4.s - mat3.s * mat4.y;
            var det2_34_14 = mat3.y * mat4.t - mat3.t * mat4.y;
            var det2_34_23 = mat3.z * mat4.s - mat3.s * mat4.z;
            var det2_34_24 = mat3.z * mat4.t - mat3.t * mat4.z;
            var det2_34_34 = mat3.s * mat4.t - mat3.t * mat4.s;

            // 3x3 sub-determinants required to calculate 5x5 determinant
            var det3_234_012 = mat2.x * det2_34_12 - mat2.y * det2_34_02 + mat2.z * det2_34_01;
            var det3_234_013 = mat2.x * det2_34_13 - mat2.y * det2_34_03 + mat2.s * det2_34_01;
            var det3_234_014 = mat2.x * det2_34_14 - mat2.y * det2_34_04 + mat2.t * det2_34_01;
            var det3_234_023 = mat2.x * det2_34_23 - mat2.z * det2_34_03 + mat2.s * det2_34_02;
            var det3_234_024 = mat2.x * det2_34_24 - mat2.z * det2_34_04 + mat2.t * det2_34_02;
            var det3_234_034 = mat2.x * det2_34_34 - mat2.s * det2_34_04 + mat2.t * det2_34_03;
            var det3_234_123 = mat2.y * det2_34_23 - mat2.z * det2_34_13 + mat2.s * det2_34_12;
            var det3_234_124 = mat2.y * det2_34_24 - mat2.z * det2_34_14 + mat2.t * det2_34_12;
            var det3_234_134 = mat2.y * det2_34_34 - mat2.s * det2_34_14 + mat2.t * det2_34_13;
            var det3_234_234 = mat2.z * det2_34_34 - mat2.s * det2_34_24 + mat2.t * det2_34_23;

            // 4x4 sub-determinants required to calculate 5x5 determinant
            var det4_1234_0123 = mat1.x * det3_234_123 - mat1.y * det3_234_023 + mat1.z * det3_234_013 - mat1.s * det3_234_012;
            var det4_1234_0124 = mat1.x * det3_234_124 - mat1.y * det3_234_024 + mat1.z * det3_234_014 - mat1.t * det3_234_012;
            var det4_1234_0134 = mat1.x * det3_234_134 - mat1.y * det3_234_034 + mat1.s * det3_234_014 - mat1.t * det3_234_013;
            var det4_1234_0234 = mat1.x * det3_234_234 - mat1.z * det3_234_034 + mat1.s * det3_234_024 - mat1.t * det3_234_023;
            var det4_1234_1234 = mat1.y * det3_234_234 - mat1.z * det3_234_134 + mat1.s * det3_234_124 - mat1.t * det3_234_123;

            // determinant of 5x5 matrix
            var det = mat0.x * det4_1234_1234 - mat0.y * det4_1234_0234 + mat0.z * det4_1234_0134 - mat0.s * det4_1234_0124 + mat0.t * det4_1234_0123;
            if (MathX.Fabs(det) < Matrix_.MATRIX_INVERSE_EPSILON)
                return false;

            var invDet = 1.0f / det;

            // remaining 2x2 sub-determinants
            var det2_23_01 = mat2.x * mat3.y - mat2.y * mat3.x;
            var det2_23_02 = mat2.x * mat3.z - mat2.z * mat3.x;
            var det2_23_03 = mat2.x * mat3.s - mat2.s * mat3.x;
            var det2_23_04 = mat2.x * mat3.t - mat2.t * mat3.x;
            var det2_23_12 = mat2.y * mat3.z - mat2.z * mat3.y;
            var det2_23_13 = mat2.y * mat3.s - mat2.s * mat3.y;
            var det2_23_14 = mat2.y * mat3.t - mat2.t * mat3.y;
            var det2_23_23 = mat2.z * mat3.s - mat2.s * mat3.z;
            var det2_23_24 = mat2.z * mat3.t - mat2.t * mat3.z;
            var det2_23_34 = mat2.s * mat3.t - mat2.t * mat3.s;
            var det2_24_01 = mat2.x * mat4.y - mat2.y * mat4.x;
            var det2_24_02 = mat2.x * mat4.z - mat2.z * mat4.x;
            var det2_24_03 = mat2.x * mat4.s - mat2.s * mat4.x;
            var det2_24_04 = mat2.x * mat4.t - mat2.t * mat4.x;
            var det2_24_12 = mat2.y * mat4.z - mat2.z * mat4.y;
            var det2_24_13 = mat2.y * mat4.s - mat2.s * mat4.y;
            var det2_24_14 = mat2.y * mat4.t - mat2.t * mat4.y;
            var det2_24_23 = mat2.z * mat4.s - mat2.s * mat4.z;
            var det2_24_24 = mat2.z * mat4.t - mat2.t * mat4.z;
            var det2_24_34 = mat2.s * mat4.t - mat2.t * mat4.s;

            // remaining 3x3 sub-determinants
            var det3_123_012 = mat1.x * det2_23_12 - mat1.y * det2_23_02 + mat1.z * det2_23_01;
            var det3_123_013 = mat1.x * det2_23_13 - mat1.y * det2_23_03 + mat1.s * det2_23_01;
            var det3_123_014 = mat1.x * det2_23_14 - mat1.y * det2_23_04 + mat1.t * det2_23_01;
            var det3_123_023 = mat1.x * det2_23_23 - mat1.z * det2_23_03 + mat1.s * det2_23_02;
            var det3_123_024 = mat1.x * det2_23_24 - mat1.z * det2_23_04 + mat1.t * det2_23_02;
            var det3_123_034 = mat1.x * det2_23_34 - mat1.s * det2_23_04 + mat1.t * det2_23_03;
            var det3_123_123 = mat1.y * det2_23_23 - mat1.z * det2_23_13 + mat1.s * det2_23_12;
            var det3_123_124 = mat1.y * det2_23_24 - mat1.z * det2_23_14 + mat1.t * det2_23_12;
            var det3_123_134 = mat1.y * det2_23_34 - mat1.s * det2_23_14 + mat1.t * det2_23_13;
            var det3_123_234 = mat1.z * det2_23_34 - mat1.s * det2_23_24 + mat1.t * det2_23_23;
            var det3_124_012 = mat1.x * det2_24_12 - mat1.y * det2_24_02 + mat1.z * det2_24_01;
            var det3_124_013 = mat1.x * det2_24_13 - mat1.y * det2_24_03 + mat1.s * det2_24_01;
            var det3_124_014 = mat1.x * det2_24_14 - mat1.y * det2_24_04 + mat1.t * det2_24_01;
            var det3_124_023 = mat1.x * det2_24_23 - mat1.z * det2_24_03 + mat1.s * det2_24_02;
            var det3_124_024 = mat1.x * det2_24_24 - mat1.z * det2_24_04 + mat1.t * det2_24_02;
            var det3_124_034 = mat1.x * det2_24_34 - mat1.s * det2_24_04 + mat1.t * det2_24_03;
            var det3_124_123 = mat1.y * det2_24_23 - mat1.z * det2_24_13 + mat1.s * det2_24_12;
            var det3_124_124 = mat1.y * det2_24_24 - mat1.z * det2_24_14 + mat1.t * det2_24_12;
            var det3_124_134 = mat1.y * det2_24_34 - mat1.s * det2_24_14 + mat1.t * det2_24_13;
            var det3_124_234 = mat1.z * det2_24_34 - mat1.s * det2_24_24 + mat1.t * det2_24_23;
            var det3_134_012 = mat1.x * det2_34_12 - mat1.y * det2_34_02 + mat1.z * det2_34_01;
            var det3_134_013 = mat1.x * det2_34_13 - mat1.y * det2_34_03 + mat1.s * det2_34_01;
            var det3_134_014 = mat1.x * det2_34_14 - mat1.y * det2_34_04 + mat1.t * det2_34_01;
            var det3_134_023 = mat1.x * det2_34_23 - mat1.z * det2_34_03 + mat1.s * det2_34_02;
            var det3_134_024 = mat1.x * det2_34_24 - mat1.z * det2_34_04 + mat1.t * det2_34_02;
            var det3_134_034 = mat1.x * det2_34_34 - mat1.s * det2_34_04 + mat1.t * det2_34_03;
            var det3_134_123 = mat1.y * det2_34_23 - mat1.z * det2_34_13 + mat1.s * det2_34_12;
            var det3_134_124 = mat1.y * det2_34_24 - mat1.z * det2_34_14 + mat1.t * det2_34_12;
            var det3_134_134 = mat1.y * det2_34_34 - mat1.s * det2_34_14 + mat1.t * det2_34_13;
            var det3_134_234 = mat1.z * det2_34_34 - mat1.s * det2_34_24 + mat1.t * det2_34_23;

            // remaining 4x4 sub-determinants
            var det4_0123_0123 = mat0.x * det3_123_123 - mat0.y * det3_123_023 + mat0.z * det3_123_013 - mat0.s * det3_123_012;
            var det4_0123_0124 = mat0.x * det3_123_124 - mat0.y * det3_123_024 + mat0.z * det3_123_014 - mat0.t * det3_123_012;
            var det4_0123_0134 = mat0.x * det3_123_134 - mat0.y * det3_123_034 + mat0.s * det3_123_014 - mat0.t * det3_123_013;
            var det4_0123_0234 = mat0.x * det3_123_234 - mat0.z * det3_123_034 + mat0.s * det3_123_024 - mat0.t * det3_123_023;
            var det4_0123_1234 = mat0.y * det3_123_234 - mat0.z * det3_123_134 + mat0.s * det3_123_124 - mat0.t * det3_123_123;
            var det4_0124_0123 = mat0.x * det3_124_123 - mat0.y * det3_124_023 + mat0.z * det3_124_013 - mat0.s * det3_124_012;
            var det4_0124_0124 = mat0.x * det3_124_124 - mat0.y * det3_124_024 + mat0.z * det3_124_014 - mat0.t * det3_124_012;
            var det4_0124_0134 = mat0.x * det3_124_134 - mat0.y * det3_124_034 + mat0.s * det3_124_014 - mat0.t * det3_124_013;
            var det4_0124_0234 = mat0.x * det3_124_234 - mat0.z * det3_124_034 + mat0.s * det3_124_024 - mat0.t * det3_124_023;
            var det4_0124_1234 = mat0.y * det3_124_234 - mat0.z * det3_124_134 + mat0.s * det3_124_124 - mat0.t * det3_124_123;
            var det4_0134_0123 = mat0.x * det3_134_123 - mat0.y * det3_134_023 + mat0.z * det3_134_013 - mat0.s * det3_134_012;
            var det4_0134_0124 = mat0.x * det3_134_124 - mat0.y * det3_134_024 + mat0.z * det3_134_014 - mat0.t * det3_134_012;
            var det4_0134_0134 = mat0.x * det3_134_134 - mat0.y * det3_134_034 + mat0.s * det3_134_014 - mat0.t * det3_134_013;
            var det4_0134_0234 = mat0.x * det3_134_234 - mat0.z * det3_134_034 + mat0.s * det3_134_024 - mat0.t * det3_134_023;
            var det4_0134_1234 = mat0.y * det3_134_234 - mat0.z * det3_134_134 + mat0.s * det3_134_124 - mat0.t * det3_134_123;
            var det4_0234_0123 = mat0.x * det3_234_123 - mat0.y * det3_234_023 + mat0.z * det3_234_013 - mat0.s * det3_234_012;
            var det4_0234_0124 = mat0.x * det3_234_124 - mat0.y * det3_234_024 + mat0.z * det3_234_014 - mat0.t * det3_234_012;
            var det4_0234_0134 = mat0.x * det3_234_134 - mat0.y * det3_234_034 + mat0.s * det3_234_014 - mat0.t * det3_234_013;
            var det4_0234_0234 = mat0.x * det3_234_234 - mat0.z * det3_234_034 + mat0.s * det3_234_024 - mat0.t * det3_234_023;
            var det4_0234_1234 = mat0.y * det3_234_234 - mat0.z * det3_234_134 + mat0.s * det3_234_124 - mat0.t * det3_234_123;

            mat0.x = det4_1234_1234 * invDet; mat0.y = -det4_0234_1234 * invDet; mat0.z = det4_0134_1234 * invDet; mat0.s = -det4_0124_1234 * invDet; mat0.t = det4_0123_1234 * invDet;
            mat1.x = -det4_1234_0234 * invDet; mat1.y = det4_0234_0234 * invDet; mat1.z = -det4_0134_0234 * invDet; mat1.s = det4_0124_0234 * invDet; mat1.t = -det4_0123_0234 * invDet;
            mat2.x = det4_1234_0134 * invDet; mat2.y = -det4_0234_0134 * invDet; mat2.z = det4_0134_0134 * invDet; mat2.s = -det4_0124_0134 * invDet; mat2.t = det4_0123_0134 * invDet;
            mat3.x = -det4_1234_0124 * invDet; mat3.y = det4_0234_0124 * invDet; mat3.z = -det4_0134_0124 * invDet; mat3.s = det4_0124_0124 * invDet; mat3.t = -det4_0123_0124 * invDet;
            mat4.x = det4_1234_0123 * invDet; mat4.y = -det4_0234_0123 * invDet; mat4.z = det4_0134_0123 * invDet; mat4.s = -det4_0124_0123 * invDet; mat4.t = det4_0123_0123 * invDet;

            return true;
        }
        public Matrix5x5 InverseFast() // returns the inverse ( m * m.Inverse() = identity )
        {
            var invMat = this;
            var r = invMat.InverseFastSelf();
            Debug.Assert(r);
            return invMat;
        }
        public unsafe bool InverseFastSelf()    // returns false if determinant is zero
        {
            // 86+30+6 = 122 multiplications
            //	  2*1  =   2 divisions
            fixed (float* mat = &mat0.x)
            {
                //r0 = m0.Inverse();	// 3x3
                var c0 = mat[1 * 5 + 1] * mat[2 * 5 + 2] - mat[1 * 5 + 2] * mat[2 * 5 + 1];
                var c1 = mat[1 * 5 + 2] * mat[2 * 5 + 0] - mat[1 * 5 + 0] * mat[2 * 5 + 2];
                var c2 = mat[1 * 5 + 0] * mat[2 * 5 + 1] - mat[1 * 5 + 1] * mat[2 * 5 + 0];

                var det = mat[0 * 5 + 0] * c0 + mat[0 * 5 + 1] * c1 + mat[0 * 5 + 2] * c2;
                if (MathX.Fabs(det) < Matrix_.MATRIX_INVERSE_EPSILON)
                    return false;

                var invDet = 1.0f / det;

                var r0 = new Matrix3x3();
                r0.mat0.x = c0 * invDet;
                r0.mat0.y = (mat[0 * 5 + 2] * mat[2 * 5 + 1] - mat[0 * 5 + 1] * mat[2 * 5 + 2]) * invDet;
                r0.mat0.z = (mat[0 * 5 + 1] * mat[1 * 5 + 2] - mat[0 * 5 + 2] * mat[1 * 5 + 1]) * invDet;
                r0.mat1.x = c1 * invDet;
                r0.mat1.y = (mat[0 * 5 + 0] * mat[2 * 5 + 2] - mat[0 * 5 + 2] * mat[2 * 5 + 0]) * invDet;
                r0.mat1.z = (mat[0 * 5 + 2] * mat[1 * 5 + 0] - mat[0 * 5 + 0] * mat[1 * 5 + 2]) * invDet;
                r0.mat2.x = c2 * invDet;
                r0.mat2.y = (mat[0 * 5 + 1] * mat[2 * 5 + 0] - mat[0 * 5 + 0] * mat[2 * 5 + 1]) * invDet;
                r0.mat2.z = (mat[0 * 5 + 0] * mat[1 * 5 + 1] - mat[0 * 5 + 1] * mat[1 * 5 + 0]) * invDet;

                // r1 = r0 * m1;		// 3x2 = 3x3 * 3x2
                var r1 = new Matrix3x3();
                r1.mat0.x = r0.mat0.x * mat[0 * 5 + 3] + r0.mat0.y * mat[1 * 5 + 3] + r0.mat0.z * mat[2 * 5 + 3];
                r1.mat0.y = r0.mat0.x * mat[0 * 5 + 4] + r0.mat0.y * mat[1 * 5 + 4] + r0.mat0.z * mat[2 * 5 + 4];
                r1.mat1.x = r0.mat1.x * mat[0 * 5 + 3] + r0.mat1.y * mat[1 * 5 + 3] + r0.mat1.z * mat[2 * 5 + 3];
                r1.mat1.y = r0.mat1.x * mat[0 * 5 + 4] + r0.mat1.y * mat[1 * 5 + 4] + r0.mat1.z * mat[2 * 5 + 4];
                r1.mat2.x = r0.mat2.x * mat[0 * 5 + 3] + r0.mat2.y * mat[1 * 5 + 3] + r0.mat2.z * mat[2 * 5 + 3];
                r1.mat2.y = r0.mat2.x * mat[0 * 5 + 4] + r0.mat2.y * mat[1 * 5 + 4] + r0.mat2.z * mat[2 * 5 + 4];

                // r2 = m2 * r1;		// 2x2 = 2x3 * 3x2
                var r2 = new Matrix3x3();
                r2.mat0.x = mat[3 * 5 + 0] * r1.mat0.x + mat[3 * 5 + 1] * r1.mat1.x + mat[3 * 5 + 2] * r1.mat2.x;
                r2.mat0.y = mat[3 * 5 + 0] * r1.mat0.y + mat[3 * 5 + 1] * r1.mat1.y + mat[3 * 5 + 2] * r1.mat2.y;
                r2.mat1.x = mat[4 * 5 + 0] * r1.mat0.x + mat[4 * 5 + 1] * r1.mat1.x + mat[4 * 5 + 2] * r1.mat2.x;
                r2.mat1.y = mat[4 * 5 + 0] * r1.mat0.y + mat[4 * 5 + 1] * r1.mat1.y + mat[4 * 5 + 2] * r1.mat2.y;

                // r3 = r2 - m3;		// 2x2 = 2x2 - 2x2
                var r3 = new Matrix3x3();
                r3.mat0.x = r2.mat0.x - mat[3 * 5 + 3];
                r3.mat0.y = r2.mat0.y - mat[3 * 5 + 4];
                r3.mat1.x = r2.mat1.x - mat[4 * 5 + 3];
                r3.mat1.y = r2.mat1.y - mat[4 * 5 + 4];

                // r3.InverseSelf();	// 2x2
                det = r3.mat0.x * r3.mat1.y - r3.mat0.y * r3.mat1.x;
                if (MathX.Fabs(det) < Matrix_.MATRIX_INVERSE_EPSILON)
                    return false;

                invDet = 1.0f / det;

                c0 = r3.mat0.x;
                r3.mat0[0] = r3.mat1.y * invDet;
                r3.mat0[1] = -r3.mat0.y * invDet;
                r3.mat1[0] = -r3.mat1.x * invDet;
                r3.mat1[1] = c0 * invDet;

                // r2 = m2 * r0;		// 2x3 = 2x3 * 3x3
                r2.mat0[0] = mat[3 * 5 + 0] * r0.mat0.x + mat[3 * 5 + 1] * r0.mat1.x + mat[3 * 5 + 2] * r0.mat2.x;
                r2.mat0[1] = mat[3 * 5 + 0] * r0.mat0.y + mat[3 * 5 + 1] * r0.mat1.y + mat[3 * 5 + 2] * r0.mat2.y;
                r2.mat0[2] = mat[3 * 5 + 0] * r0.mat0.z + mat[3 * 5 + 1] * r0.mat1.z + mat[3 * 5 + 2] * r0.mat2.z;
                r2.mat1[0] = mat[4 * 5 + 0] * r0.mat0.x + mat[4 * 5 + 1] * r0.mat1.x + mat[4 * 5 + 2] * r0.mat2.x;
                r2.mat1[1] = mat[4 * 5 + 0] * r0.mat0.y + mat[4 * 5 + 1] * r0.mat1.y + mat[4 * 5 + 2] * r0.mat2.y;
                r2.mat1[2] = mat[4 * 5 + 0] * r0.mat0.z + mat[4 * 5 + 1] * r0.mat1.z + mat[4 * 5 + 2] * r0.mat2.z;

                // m2 = r3 * r2;		// 2x3 = 2x2 * 2x3
                mat[3 * 5 + 0] = r3.mat0.x * r2.mat0.x + r3.mat0.y * r2.mat1.x;
                mat[3 * 5 + 1] = r3.mat0.x * r2.mat0.y + r3.mat0.y * r2.mat1.y;
                mat[3 * 5 + 2] = r3.mat0.x * r2.mat0.z + r3.mat0.y * r2.mat1.z;
                mat[4 * 5 + 0] = r3.mat1.x * r2.mat0.x + r3.mat1.y * r2.mat1.x;
                mat[4 * 5 + 1] = r3.mat1.x * r2.mat0.y + r3.mat1.y * r2.mat1.y;
                mat[4 * 5 + 2] = r3.mat1.x * r2.mat0.z + r3.mat1.y * r2.mat1.z;

                // m0 = r0 - r1 * m2;	// 3x3 = 3x3 - 3x2 * 2x3
                mat[0 * 5 + 0] = r0.mat0.x - r1.mat0[0] * mat[3 * 5 + 0] - r1.mat0.y * mat[4 * 5 + 0];
                mat[0 * 5 + 1] = r0.mat0.y - r1.mat0[0] * mat[3 * 5 + 1] - r1.mat0.y * mat[4 * 5 + 1];
                mat[0 * 5 + 2] = r0.mat0.z - r1.mat0[0] * mat[3 * 5 + 2] - r1.mat0.y * mat[4 * 5 + 2];
                mat[1 * 5 + 0] = r0.mat1.x - r1.mat1[0] * mat[3 * 5 + 0] - r1.mat1.y * mat[4 * 5 + 0];
                mat[1 * 5 + 1] = r0.mat1.y - r1.mat1[0] * mat[3 * 5 + 1] - r1.mat1.y * mat[4 * 5 + 1];
                mat[1 * 5 + 2] = r0.mat1.z - r1.mat1[0] * mat[3 * 5 + 2] - r1.mat1.y * mat[4 * 5 + 2];
                mat[2 * 5 + 0] = r0.mat2.x - r1.mat2[0] * mat[3 * 5 + 0] - r1.mat2.y * mat[4 * 5 + 0];
                mat[2 * 5 + 1] = r0.mat2.y - r1.mat2[0] * mat[3 * 5 + 1] - r1.mat2.y * mat[4 * 5 + 1];
                mat[2 * 5 + 2] = r0.mat2.z - r1.mat2[0] * mat[3 * 5 + 2] - r1.mat2.y * mat[4 * 5 + 2];

                // m1 = r1 * r3;		// 3x2 = 3x2 * 2x2
                mat[0 * 5 + 3] = r1.mat0.x * r3.mat0.x + r1.mat0.y * r3.mat1.x;
                mat[0 * 5 + 4] = r1.mat0.x * r3.mat0.y + r1.mat0.y * r3.mat1.y;
                mat[1 * 5 + 3] = r1.mat1.x * r3.mat0.x + r1.mat1.y * r3.mat1.x;
                mat[1 * 5 + 4] = r1.mat1.x * r3.mat0.y + r1.mat1.y * r3.mat1.y;
                mat[2 * 5 + 3] = r1.mat2.x * r3.mat0.x + r1.mat2.y * r3.mat1.x;
                mat[2 * 5 + 4] = r1.mat2.x * r3.mat0.y + r1.mat2.y * r3.mat1.y;

                // m3 = -r3;			// 2x2 = - 2x2
                mat[3 * 5 + 3] = -r3.mat0.x;
                mat[3 * 5 + 4] = -r3.mat0.y;
                mat[4 * 5 + 3] = -r3.mat1.x;
                mat[4 * 5 + 4] = -r3.mat1.y;
            }

            return true;
        }

        public static int Dimension
            => 25;

        public unsafe T ToFloatPtr<T>(FloatPtr<T> callback)
            => mat0.ToFloatPtr(callback);
        public unsafe string ToString(int precision = 2)
            => ToFloatPtr(array => StringX.FloatArrayToString(array, Dimension, precision));

        internal Vector5 mat0;
        internal Vector5 mat1;
        internal Vector5 mat2;
        internal Vector5 mat3;
        internal Vector5 mat4;

        public static Matrix5x5 zero = new(new Vector5(0, 0, 0, 0, 0), new Vector5(0, 0, 0, 0, 0), new Vector5(0, 0, 0, 0, 0), new Vector5(0, 0, 0, 0, 0), new Vector5(0, 0, 0, 0, 0));
        public static Matrix5x5 identity = new(new Vector5(1, 0, 0, 0, 0), new Vector5(0, 1, 0, 0, 0), new Vector5(0, 0, 1, 0, 0), new Vector5(0, 0, 0, 1, 0), new Vector5(0, 0, 0, 0, 1));
        //#define default	identity
    }

    public struct Matrix6x6
    {
        public Matrix6x6(Vector6 v0, Vector6 v1, Vector6 v2, Vector6 v3, Vector6 v4, Vector6 v5)
        {
            mat0 = v0;
            mat1 = v1;
            mat2 = v2;
            mat3 = v3;
            mat4 = v4;
            mat5 = v5;
        }
        public Matrix6x6(Matrix3x3 m0, Matrix3x3 m1, Matrix3x3 m2, Matrix3x3 m3)
        {
            mat0 = new Vector6(m0[0].x, m0[0].y, m0[0].z, m1[0].x, m1[0].y, m1[0].z);
            mat1 = new Vector6(m0[1].x, m0[1].y, m0[1].z, m1[1].x, m1[1].y, m1[1].z);
            mat2 = new Vector6(m0[2].x, m0[2].y, m0[2].z, m1[2].x, m1[2].y, m1[2].z);
            mat3 = new Vector6(m2[0].x, m2[0].y, m2[0].z, m3[0].x, m3[0].y, m3[0].z);
            mat4 = new Vector6(m2[1].x, m2[1].y, m2[1].z, m3[1].x, m3[1].y, m3[1].z);
            mat5 = new Vector6(m2[2].x, m2[2].y, m2[2].z, m3[2].x, m3[2].y, m3[2].z);
        }

        public Matrix6x6(float[][] src)
        {
            mat0 = new Vector6(src[0][0], src[0][1], src[0][2], src[0][3], src[0][4], src[0][5]);
            mat1 = new Vector6(src[1][0], src[1][1], src[1][2], src[1][3], src[1][4], src[1][5]);
            mat2 = new Vector6(src[2][0], src[2][1], src[2][2], src[2][3], src[2][4], src[2][5]);
            mat3 = new Vector6(src[3][0], src[3][1], src[3][2], src[3][3], src[3][4], src[3][5]);
            mat4 = new Vector6(src[4][0], src[4][1], src[4][2], src[4][3], src[4][4], src[4][5]);
            mat5 = new Vector6(src[5][0], src[5][1], src[5][2], src[5][3], src[5][4], src[5][5]);
        }

        public unsafe ref Vector6 this[int index]
        {
            get
            {
                fixed (Vector6* mat = &mat0)
                    return ref mat[index];
            }
        }

        public static Matrix6x6 operator *(Matrix6x6 _, float a)
            => new(
            new Vector6(_.mat0[0] * a, _.mat0[1] * a, _.mat0[2] * a, _.mat0[3] * a, _.mat0[4] * a, _.mat0[5] * a),
            new Vector6(_.mat1[0] * a, _.mat1[1] * a, _.mat1[2] * a, _.mat1[3] * a, _.mat1[4] * a, _.mat1[5] * a),
            new Vector6(_.mat2[0] * a, _.mat2[1] * a, _.mat2[2] * a, _.mat2[3] * a, _.mat2[4] * a, _.mat2[5] * a),
            new Vector6(_.mat3[0] * a, _.mat3[1] * a, _.mat3[2] * a, _.mat3[3] * a, _.mat3[4] * a, _.mat3[5] * a),
            new Vector6(_.mat4[0] * a, _.mat4[1] * a, _.mat4[2] * a, _.mat4[3] * a, _.mat4[4] * a, _.mat4[5] * a),
            new Vector6(_.mat5[0] * a, _.mat5[1] * a, _.mat5[2] * a, _.mat5[3] * a, _.mat5[4] * a, _.mat5[5] * a));

        public static Vector6 operator *(Matrix6x6 _, Vector6 vec)
            => new(
            _.mat0[0] * vec[0] + _.mat0[1] * vec[1] + _.mat0[2] * vec[2] + _.mat0[3] * vec[3] + _.mat0[4] * vec[4] + _.mat0[5] * vec[5],
            _.mat1[0] * vec[0] + _.mat1[1] * vec[1] + _.mat1[2] * vec[2] + _.mat1[3] * vec[3] + _.mat1[4] * vec[4] + _.mat1[5] * vec[5],
            _.mat2[0] * vec[0] + _.mat2[1] * vec[1] + _.mat2[2] * vec[2] + _.mat2[3] * vec[3] + _.mat2[4] * vec[4] + _.mat2[5] * vec[5],
            _.mat3[0] * vec[0] + _.mat3[1] * vec[1] + _.mat3[2] * vec[2] + _.mat3[3] * vec[3] + _.mat3[4] * vec[4] + _.mat3[5] * vec[5],
            _.mat4[0] * vec[0] + _.mat4[1] * vec[1] + _.mat4[2] * vec[2] + _.mat4[3] * vec[3] + _.mat4[4] * vec[4] + _.mat4[5] * vec[5],
            _.mat5[0] * vec[0] + _.mat5[1] * vec[1] + _.mat5[2] * vec[2] + _.mat5[3] * vec[3] + _.mat5[4] * vec[4] + _.mat5[5] * vec[5]);
        public static unsafe Matrix6x6 operator *(Matrix6x6 _, Matrix6x6 a)
        {
            Matrix6x6 dst;
            var m1Ptr = (float*)&_;
            var m2Ptr = (float*)&a;
            var dstPtr = (float*)&dst;
            for (var i = 0; i < 6; i++)
            {
                for (var j = 0; j < 6; j++)
                {
                    *dstPtr = m1Ptr[0] * m2Ptr[0 * 6 + j]
                            + m1Ptr[1] * m2Ptr[1 * 6 + j]
                            + m1Ptr[2] * m2Ptr[2 * 6 + j]
                            + m1Ptr[3] * m2Ptr[3 * 6 + j]
                            + m1Ptr[4] * m2Ptr[4 * 6 + j]
                            + m1Ptr[5] * m2Ptr[5 * 6 + j];
                    dstPtr++;
                }
                m1Ptr += 6;
            }
            return dst;
        }
        public static Matrix6x6 operator +(Matrix6x6 _, Matrix6x6 a)
            => new(
            new Vector6(_.mat0[0] + a.mat0[0], _.mat0[1] + a.mat0[1], _.mat0[2] + a.mat0[2], _.mat0[3] + a.mat0[3], _.mat0[4] + a.mat0[4], _.mat0[5] + a.mat0[5]),
            new Vector6(_.mat1[0] + a.mat1[0], _.mat1[1] + a.mat1[1], _.mat1[2] + a.mat1[2], _.mat1[3] + a.mat1[3], _.mat1[4] + a.mat1[4], _.mat1[5] + a.mat1[5]),
            new Vector6(_.mat2[0] + a.mat2[0], _.mat2[1] + a.mat2[1], _.mat2[2] + a.mat2[2], _.mat2[3] + a.mat2[3], _.mat2[4] + a.mat2[4], _.mat2[5] + a.mat2[5]),
            new Vector6(_.mat3[0] + a.mat3[0], _.mat3[1] + a.mat3[1], _.mat3[2] + a.mat3[2], _.mat3[3] + a.mat3[3], _.mat3[4] + a.mat3[4], _.mat3[5] + a.mat3[5]),
            new Vector6(_.mat4[0] + a.mat4[0], _.mat4[1] + a.mat4[1], _.mat4[2] + a.mat4[2], _.mat4[3] + a.mat4[3], _.mat4[4] + a.mat4[4], _.mat4[5] + a.mat4[5]),
            new Vector6(_.mat5[0] + a[5][0], _.mat5[1] + a[5][1], _.mat5[2] + a[5][2], _.mat5[3] + a[5][3], _.mat5[4] + a[5][4], _.mat5[5] + a[5][5]));
        public static Matrix6x6 operator -(Matrix6x6 _, Matrix6x6 a)
            => new(
            new Vector6(_.mat0[0] - a.mat0[0], _.mat0[1] - a.mat0[1], _.mat0[2] - a.mat0[2], _.mat0[3] - a.mat0[3], _.mat0[4] - a.mat0[4], _.mat0[5] - a.mat0[5]),
            new Vector6(_.mat1[0] - a.mat1[0], _.mat1[1] - a.mat1[1], _.mat1[2] - a.mat1[2], _.mat1[3] - a.mat1[3], _.mat1[4] - a.mat1[4], _.mat1[5] - a.mat1[5]),
            new Vector6(_.mat2[0] - a.mat2[0], _.mat2[1] - a.mat2[1], _.mat2[2] - a.mat2[2], _.mat2[3] - a.mat2[3], _.mat2[4] - a.mat2[4], _.mat2[5] - a.mat2[5]),
            new Vector6(_.mat3[0] - a.mat3[0], _.mat3[1] - a.mat3[1], _.mat3[2] - a.mat3[2], _.mat3[3] - a.mat3[3], _.mat3[4] - a.mat3[4], _.mat3[5] - a.mat3[5]),
            new Vector6(_.mat4[0] - a.mat4[0], _.mat4[1] - a.mat4[1], _.mat4[2] - a.mat4[2], _.mat4[3] - a.mat4[3], _.mat4[4] - a.mat4[4], _.mat4[5] - a.mat4[5]),
            new Vector6(_.mat5[0] - a[5][0], _.mat5[1] - a[5][1], _.mat5[2] - a[5][2], _.mat5[3] - a[5][3], _.mat5[4] - a[5][4], _.mat5[5] - a[5][5]));

        public static Matrix6x6 operator *(float a, Matrix6x6 mat)
            => mat * a;
        public static Vector6 operator *(Vector6 vec, Matrix6x6 mat)
            => mat * vec;

        public unsafe bool Compare(Matrix6x6 a)                       // exact compare, no epsilon
        {
            fixed (void* mat = &mat0)
            {
                var ptr1 = (float*)&mat;
                var ptr2 = (float*)&a.mat0;
                for (var i = 0; i < 6 * 6; i++)
                    if (ptr1[i] != ptr2[i])
                        return false;
                return true;
            }
        }
        public unsafe bool Compare(Matrix6x6 a, float epsilon)  // compare with epsilon
        {
            fixed (void* mat = &mat0)
            {
                var ptr1 = (float*)&mat;
                var ptr2 = (float*)&a.mat0;
                for (var i = 0; i < 6 * 6; i++)
                    if (MathX.Fabs(ptr1[i] - ptr2[i]) > epsilon)
                        return false;
                return true;
            }
        }
        public static bool operator ==(Matrix6x6 _, Matrix6x6 a)                   // exact compare, no epsilon
                => _.Compare(a);
        public static bool operator !=(Matrix6x6 _, Matrix6x6 a)                   // exact compare, no epsilon
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Matrix6x6 q && Compare(q);
        public override int GetHashCode()
            => mat0.GetHashCode();

        public unsafe void Zero()
        {
            fixed (void* p = &this)
                U.memset(p, 0, sizeof(Matrix6x6));
        }
        public void Identity()
            => this = identity;
        public bool IsIdentity(float epsilon = Matrix_.MATRIX_EPSILON)
            => Compare(identity, epsilon);
        public bool IsSymmetric(float epsilon = Matrix_.MATRIX_EPSILON)
        {
            for (var i = 1; i < 6; i++)
                for (var j = 0; j < i; j++)
                    if (MathX.Fabs(this[i][j] - this[j][i]) > epsilon)
                        return false;
            return true;
        }
        public bool IsDiagonal(float epsilon = Matrix_.MATRIX_EPSILON)
        {
            for (var i = 0; i < 6; i++)
                for (var j = 0; j < 6; j++)
                    if (i != j && MathX.Fabs(this[i][j]) > epsilon)
                        return false;
            return true;
        }

        public Matrix3x3 SubMat3(int n)
        {
            Debug.Assert(n >= 0 && n < 4);
            var b0 = ((n & 2) >> 1) * 3;
            var b1 = (n & 1) * 3;
            return new(
                this[b0 + 0][b1 + 0], this[b0 + 0][b1 + 1], this[b0 + 0][b1 + 2],
                this[b0 + 1][b1 + 0], this[b0 + 1][b1 + 1], this[b0 + 1][b1 + 2],
                this[b0 + 2][b1 + 0], this[b0 + 2][b1 + 1], this[b0 + 2][b1 + 2]);
        }
        public float Trace()
            => mat0[0] + mat1[1] + mat2[2] + mat3[3] + mat4[4] + mat5[5];
        public float Determinant()
        {
            // 2x2 sub-determinants required to calculate 6x6 determinant
            var det2_45_01 = mat4[0] * mat5[1] - mat4[1] * mat5[0];
            var det2_45_02 = mat4[0] * mat5[2] - mat4[2] * mat5[0];
            var det2_45_03 = mat4[0] * mat5[3] - mat4[3] * mat5[0];
            var det2_45_04 = mat4[0] * mat5[4] - mat4[4] * mat5[0];
            var det2_45_05 = mat4[0] * mat5[5] - mat4[5] * mat5[0];
            var det2_45_12 = mat4[1] * mat5[2] - mat4[2] * mat5[1];
            var det2_45_13 = mat4[1] * mat5[3] - mat4[3] * mat5[1];
            var det2_45_14 = mat4[1] * mat5[4] - mat4[4] * mat5[1];
            var det2_45_15 = mat4[1] * mat5[5] - mat4[5] * mat5[1];
            var det2_45_23 = mat4[2] * mat5[3] - mat4[3] * mat5[2];
            var det2_45_24 = mat4[2] * mat5[4] - mat4[4] * mat5[2];
            var det2_45_25 = mat4[2] * mat5[5] - mat4[5] * mat5[2];
            var det2_45_34 = mat4[3] * mat5[4] - mat4[4] * mat5[3];
            var det2_45_35 = mat4[3] * mat5[5] - mat4[5] * mat5[3];
            var det2_45_45 = mat4[4] * mat5[5] - mat4[5] * mat5[4];

            // 3x3 sub-determinants required to calculate 6x6 determinant
            var det3_345_012 = mat3[0] * det2_45_12 - mat3[1] * det2_45_02 + mat3[2] * det2_45_01;
            var det3_345_013 = mat3[0] * det2_45_13 - mat3[1] * det2_45_03 + mat3[3] * det2_45_01;
            var det3_345_014 = mat3[0] * det2_45_14 - mat3[1] * det2_45_04 + mat3[4] * det2_45_01;
            var det3_345_015 = mat3[0] * det2_45_15 - mat3[1] * det2_45_05 + mat3[5] * det2_45_01;
            var det3_345_023 = mat3[0] * det2_45_23 - mat3[2] * det2_45_03 + mat3[3] * det2_45_02;
            var det3_345_024 = mat3[0] * det2_45_24 - mat3[2] * det2_45_04 + mat3[4] * det2_45_02;
            var det3_345_025 = mat3[0] * det2_45_25 - mat3[2] * det2_45_05 + mat3[5] * det2_45_02;
            var det3_345_034 = mat3[0] * det2_45_34 - mat3[3] * det2_45_04 + mat3[4] * det2_45_03;
            var det3_345_035 = mat3[0] * det2_45_35 - mat3[3] * det2_45_05 + mat3[5] * det2_45_03;
            var det3_345_045 = mat3[0] * det2_45_45 - mat3[4] * det2_45_05 + mat3[5] * det2_45_04;
            var det3_345_123 = mat3[1] * det2_45_23 - mat3[2] * det2_45_13 + mat3[3] * det2_45_12;
            var det3_345_124 = mat3[1] * det2_45_24 - mat3[2] * det2_45_14 + mat3[4] * det2_45_12;
            var det3_345_125 = mat3[1] * det2_45_25 - mat3[2] * det2_45_15 + mat3[5] * det2_45_12;
            var det3_345_134 = mat3[1] * det2_45_34 - mat3[3] * det2_45_14 + mat3[4] * det2_45_13;
            var det3_345_135 = mat3[1] * det2_45_35 - mat3[3] * det2_45_15 + mat3[5] * det2_45_13;
            var det3_345_145 = mat3[1] * det2_45_45 - mat3[4] * det2_45_15 + mat3[5] * det2_45_14;
            var det3_345_234 = mat3[2] * det2_45_34 - mat3[3] * det2_45_24 + mat3[4] * det2_45_23;
            var det3_345_235 = mat3[2] * det2_45_35 - mat3[3] * det2_45_25 + mat3[5] * det2_45_23;
            var det3_345_245 = mat3[2] * det2_45_45 - mat3[4] * det2_45_25 + mat3[5] * det2_45_24;
            var det3_345_345 = mat3[3] * det2_45_45 - mat3[4] * det2_45_35 + mat3[5] * det2_45_34;

            // 4x4 sub-determinants required to calculate 6x6 determinant
            var det4_2345_0123 = mat2[0] * det3_345_123 - mat2[1] * det3_345_023 + mat2[2] * det3_345_013 - mat2[3] * det3_345_012;
            var det4_2345_0124 = mat2[0] * det3_345_124 - mat2[1] * det3_345_024 + mat2[2] * det3_345_014 - mat2[4] * det3_345_012;
            var det4_2345_0125 = mat2[0] * det3_345_125 - mat2[1] * det3_345_025 + mat2[2] * det3_345_015 - mat2[5] * det3_345_012;
            var det4_2345_0134 = mat2[0] * det3_345_134 - mat2[1] * det3_345_034 + mat2[3] * det3_345_014 - mat2[4] * det3_345_013;
            var det4_2345_0135 = mat2[0] * det3_345_135 - mat2[1] * det3_345_035 + mat2[3] * det3_345_015 - mat2[5] * det3_345_013;
            var det4_2345_0145 = mat2[0] * det3_345_145 - mat2[1] * det3_345_045 + mat2[4] * det3_345_015 - mat2[5] * det3_345_014;
            var det4_2345_0234 = mat2[0] * det3_345_234 - mat2[2] * det3_345_034 + mat2[3] * det3_345_024 - mat2[4] * det3_345_023;
            var det4_2345_0235 = mat2[0] * det3_345_235 - mat2[2] * det3_345_035 + mat2[3] * det3_345_025 - mat2[5] * det3_345_023;
            var det4_2345_0245 = mat2[0] * det3_345_245 - mat2[2] * det3_345_045 + mat2[4] * det3_345_025 - mat2[5] * det3_345_024;
            var det4_2345_0345 = mat2[0] * det3_345_345 - mat2[3] * det3_345_045 + mat2[4] * det3_345_035 - mat2[5] * det3_345_034;
            var det4_2345_1234 = mat2[1] * det3_345_234 - mat2[2] * det3_345_134 + mat2[3] * det3_345_124 - mat2[4] * det3_345_123;
            var det4_2345_1235 = mat2[1] * det3_345_235 - mat2[2] * det3_345_135 + mat2[3] * det3_345_125 - mat2[5] * det3_345_123;
            var det4_2345_1245 = mat2[1] * det3_345_245 - mat2[2] * det3_345_145 + mat2[4] * det3_345_125 - mat2[5] * det3_345_124;
            var det4_2345_1345 = mat2[1] * det3_345_345 - mat2[3] * det3_345_145 + mat2[4] * det3_345_135 - mat2[5] * det3_345_134;
            var det4_2345_2345 = mat2[2] * det3_345_345 - mat2[3] * det3_345_245 + mat2[4] * det3_345_235 - mat2[5] * det3_345_234;

            // 5x5 sub-determinants required to calculate 6x6 determinant
            var det5_12345_01234 = mat1[0] * det4_2345_1234 - mat1[1] * det4_2345_0234 + mat1[2] * det4_2345_0134 - mat1[3] * det4_2345_0124 + mat1[4] * det4_2345_0123;
            var det5_12345_01235 = mat1[0] * det4_2345_1235 - mat1[1] * det4_2345_0235 + mat1[2] * det4_2345_0135 - mat1[3] * det4_2345_0125 + mat1[5] * det4_2345_0123;
            var det5_12345_01245 = mat1[0] * det4_2345_1245 - mat1[1] * det4_2345_0245 + mat1[2] * det4_2345_0145 - mat1[4] * det4_2345_0125 + mat1[5] * det4_2345_0124;
            var det5_12345_01345 = mat1[0] * det4_2345_1345 - mat1[1] * det4_2345_0345 + mat1[3] * det4_2345_0145 - mat1[4] * det4_2345_0135 + mat1[5] * det4_2345_0134;
            var det5_12345_02345 = mat1[0] * det4_2345_2345 - mat1[2] * det4_2345_0345 + mat1[3] * det4_2345_0245 - mat1[4] * det4_2345_0235 + mat1[5] * det4_2345_0234;
            var det5_12345_12345 = mat1[1] * det4_2345_2345 - mat1[2] * det4_2345_1345 + mat1[3] * det4_2345_1245 - mat1[4] * det4_2345_1235 + mat1[5] * det4_2345_1234;

            // determinant of 6x6 matrix
            return mat0[0] * det5_12345_12345 - mat0[1] * det5_12345_02345 + mat0[2] * det5_12345_01345 -
                    mat0[3] * det5_12345_01245 + mat0[4] * det5_12345_01235 - mat0[5] * det5_12345_01234;
        }

        public Matrix6x6 Transpose()   // returns transpose
        {
            var transpose = new Matrix6x6();
            for (var i = 0; i < 6; i++)
                for (var j = 0; j < 6; j++)
                    transpose[i][j] = this[j][i];
            return transpose;
        }
        public Matrix6x6 TransposeSelf()
        {
            for (var i = 0; i < 6; i++)
                for (var j = i + 1; j < 6; j++)
                {
                    var temp = this[i][j];
                    this[i][j] = this[j][i];
                    this[j][i] = temp;
                }
            return this;
        }
        public Matrix6x6 Inverse()     // returns the inverse ( m * m.Inverse() = identity )
        {
            var invMat = this;
            var r = invMat.InverseSelf();
            Debug.Assert(r);
            return invMat;
        }
        public bool InverseSelf()        // returns false if determinant is zero
        {
            // 810+6+36 = 852 multiplications
            //				1 division

            // 2x2 sub-determinants required to calculate 6x6 determinant
            var det2_45_01 = mat4[0] * mat5[1] - mat4[1] * mat5[0];
            var det2_45_02 = mat4[0] * mat5[2] - mat4[2] * mat5[0];
            var det2_45_03 = mat4[0] * mat5[3] - mat4[3] * mat5[0];
            var det2_45_04 = mat4[0] * mat5[4] - mat4[4] * mat5[0];
            var det2_45_05 = mat4[0] * mat5[5] - mat4[5] * mat5[0];
            var det2_45_12 = mat4[1] * mat5[2] - mat4[2] * mat5[1];
            var det2_45_13 = mat4[1] * mat5[3] - mat4[3] * mat5[1];
            var det2_45_14 = mat4[1] * mat5[4] - mat4[4] * mat5[1];
            var det2_45_15 = mat4[1] * mat5[5] - mat4[5] * mat5[1];
            var det2_45_23 = mat4[2] * mat5[3] - mat4[3] * mat5[2];
            var det2_45_24 = mat4[2] * mat5[4] - mat4[4] * mat5[2];
            var det2_45_25 = mat4[2] * mat5[5] - mat4[5] * mat5[2];
            var det2_45_34 = mat4[3] * mat5[4] - mat4[4] * mat5[3];
            var det2_45_35 = mat4[3] * mat5[5] - mat4[5] * mat5[3];
            var det2_45_45 = mat4[4] * mat5[5] - mat4[5] * mat5[4];

            // 3x3 sub-determinants required to calculate 6x6 determinant
            var det3_345_012 = mat3[0] * det2_45_12 - mat3[1] * det2_45_02 + mat3[2] * det2_45_01;
            var det3_345_013 = mat3[0] * det2_45_13 - mat3[1] * det2_45_03 + mat3[3] * det2_45_01;
            var det3_345_014 = mat3[0] * det2_45_14 - mat3[1] * det2_45_04 + mat3[4] * det2_45_01;
            var det3_345_015 = mat3[0] * det2_45_15 - mat3[1] * det2_45_05 + mat3[5] * det2_45_01;
            var det3_345_023 = mat3[0] * det2_45_23 - mat3[2] * det2_45_03 + mat3[3] * det2_45_02;
            var det3_345_024 = mat3[0] * det2_45_24 - mat3[2] * det2_45_04 + mat3[4] * det2_45_02;
            var det3_345_025 = mat3[0] * det2_45_25 - mat3[2] * det2_45_05 + mat3[5] * det2_45_02;
            var det3_345_034 = mat3[0] * det2_45_34 - mat3[3] * det2_45_04 + mat3[4] * det2_45_03;
            var det3_345_035 = mat3[0] * det2_45_35 - mat3[3] * det2_45_05 + mat3[5] * det2_45_03;
            var det3_345_045 = mat3[0] * det2_45_45 - mat3[4] * det2_45_05 + mat3[5] * det2_45_04;
            var det3_345_123 = mat3[1] * det2_45_23 - mat3[2] * det2_45_13 + mat3[3] * det2_45_12;
            var det3_345_124 = mat3[1] * det2_45_24 - mat3[2] * det2_45_14 + mat3[4] * det2_45_12;
            var det3_345_125 = mat3[1] * det2_45_25 - mat3[2] * det2_45_15 + mat3[5] * det2_45_12;
            var det3_345_134 = mat3[1] * det2_45_34 - mat3[3] * det2_45_14 + mat3[4] * det2_45_13;
            var det3_345_135 = mat3[1] * det2_45_35 - mat3[3] * det2_45_15 + mat3[5] * det2_45_13;
            var det3_345_145 = mat3[1] * det2_45_45 - mat3[4] * det2_45_15 + mat3[5] * det2_45_14;
            var det3_345_234 = mat3[2] * det2_45_34 - mat3[3] * det2_45_24 + mat3[4] * det2_45_23;
            var det3_345_235 = mat3[2] * det2_45_35 - mat3[3] * det2_45_25 + mat3[5] * det2_45_23;
            var det3_345_245 = mat3[2] * det2_45_45 - mat3[4] * det2_45_25 + mat3[5] * det2_45_24;
            var det3_345_345 = mat3[3] * det2_45_45 - mat3[4] * det2_45_35 + mat3[5] * det2_45_34;

            // 4x4 sub-determinants required to calculate 6x6 determinant
            var det4_2345_0123 = mat2[0] * det3_345_123 - mat2[1] * det3_345_023 + mat2[2] * det3_345_013 - mat2[3] * det3_345_012;
            var det4_2345_0124 = mat2[0] * det3_345_124 - mat2[1] * det3_345_024 + mat2[2] * det3_345_014 - mat2[4] * det3_345_012;
            var det4_2345_0125 = mat2[0] * det3_345_125 - mat2[1] * det3_345_025 + mat2[2] * det3_345_015 - mat2[5] * det3_345_012;
            var det4_2345_0134 = mat2[0] * det3_345_134 - mat2[1] * det3_345_034 + mat2[3] * det3_345_014 - mat2[4] * det3_345_013;
            var det4_2345_0135 = mat2[0] * det3_345_135 - mat2[1] * det3_345_035 + mat2[3] * det3_345_015 - mat2[5] * det3_345_013;
            var det4_2345_0145 = mat2[0] * det3_345_145 - mat2[1] * det3_345_045 + mat2[4] * det3_345_015 - mat2[5] * det3_345_014;
            var det4_2345_0234 = mat2[0] * det3_345_234 - mat2[2] * det3_345_034 + mat2[3] * det3_345_024 - mat2[4] * det3_345_023;
            var det4_2345_0235 = mat2[0] * det3_345_235 - mat2[2] * det3_345_035 + mat2[3] * det3_345_025 - mat2[5] * det3_345_023;
            var det4_2345_0245 = mat2[0] * det3_345_245 - mat2[2] * det3_345_045 + mat2[4] * det3_345_025 - mat2[5] * det3_345_024;
            var det4_2345_0345 = mat2[0] * det3_345_345 - mat2[3] * det3_345_045 + mat2[4] * det3_345_035 - mat2[5] * det3_345_034;
            var det4_2345_1234 = mat2[1] * det3_345_234 - mat2[2] * det3_345_134 + mat2[3] * det3_345_124 - mat2[4] * det3_345_123;
            var det4_2345_1235 = mat2[1] * det3_345_235 - mat2[2] * det3_345_135 + mat2[3] * det3_345_125 - mat2[5] * det3_345_123;
            var det4_2345_1245 = mat2[1] * det3_345_245 - mat2[2] * det3_345_145 + mat2[4] * det3_345_125 - mat2[5] * det3_345_124;
            var det4_2345_1345 = mat2[1] * det3_345_345 - mat2[3] * det3_345_145 + mat2[4] * det3_345_135 - mat2[5] * det3_345_134;
            var det4_2345_2345 = mat2[2] * det3_345_345 - mat2[3] * det3_345_245 + mat2[4] * det3_345_235 - mat2[5] * det3_345_234;

            // 5x5 sub-determinants required to calculate 6x6 determinant
            var det5_12345_01234 = mat1[0] * det4_2345_1234 - mat1[1] * det4_2345_0234 + mat1[2] * det4_2345_0134 - mat1[3] * det4_2345_0124 + mat1[4] * det4_2345_0123;
            var det5_12345_01235 = mat1[0] * det4_2345_1235 - mat1[1] * det4_2345_0235 + mat1[2] * det4_2345_0135 - mat1[3] * det4_2345_0125 + mat1[5] * det4_2345_0123;
            var det5_12345_01245 = mat1[0] * det4_2345_1245 - mat1[1] * det4_2345_0245 + mat1[2] * det4_2345_0145 - mat1[4] * det4_2345_0125 + mat1[5] * det4_2345_0124;
            var det5_12345_01345 = mat1[0] * det4_2345_1345 - mat1[1] * det4_2345_0345 + mat1[3] * det4_2345_0145 - mat1[4] * det4_2345_0135 + mat1[5] * det4_2345_0134;
            var det5_12345_02345 = mat1[0] * det4_2345_2345 - mat1[2] * det4_2345_0345 + mat1[3] * det4_2345_0245 - mat1[4] * det4_2345_0235 + mat1[5] * det4_2345_0234;
            var det5_12345_12345 = mat1[1] * det4_2345_2345 - mat1[2] * det4_2345_1345 + mat1[3] * det4_2345_1245 - mat1[4] * det4_2345_1235 + mat1[5] * det4_2345_1234;

            // determinant of 6x6 matrix
            var det = mat0[0] * det5_12345_12345 - mat0[1] * det5_12345_02345 + mat0[2] * det5_12345_01345 - mat0[3] * det5_12345_01245 + mat0[4] * det5_12345_01235 - mat0[5] * det5_12345_01234;
            if (MathX.Fabs(det) < Matrix_.MATRIX_INVERSE_EPSILON)
                return false;

            var invDet = 1.0f / det;

            // remaining 2x2 sub-determinants
            var det2_34_01 = mat3[0] * mat4[1] - mat3[1] * mat4[0];
            var det2_34_02 = mat3[0] * mat4[2] - mat3[2] * mat4[0];
            var det2_34_03 = mat3[0] * mat4[3] - mat3[3] * mat4[0];
            var det2_34_04 = mat3[0] * mat4[4] - mat3[4] * mat4[0];
            var det2_34_05 = mat3[0] * mat4[5] - mat3[5] * mat4[0];
            var det2_34_12 = mat3[1] * mat4[2] - mat3[2] * mat4[1];
            var det2_34_13 = mat3[1] * mat4[3] - mat3[3] * mat4[1];
            var det2_34_14 = mat3[1] * mat4[4] - mat3[4] * mat4[1];
            var det2_34_15 = mat3[1] * mat4[5] - mat3[5] * mat4[1];
            var det2_34_23 = mat3[2] * mat4[3] - mat3[3] * mat4[2];
            var det2_34_24 = mat3[2] * mat4[4] - mat3[4] * mat4[2];
            var det2_34_25 = mat3[2] * mat4[5] - mat3[5] * mat4[2];
            var det2_34_34 = mat3[3] * mat4[4] - mat3[4] * mat4[3];
            var det2_34_35 = mat3[3] * mat4[5] - mat3[5] * mat4[3];
            var det2_34_45 = mat3[4] * mat4[5] - mat3[5] * mat4[4];
            var det2_35_01 = mat3[0] * mat5[1] - mat3[1] * mat5[0];
            var det2_35_02 = mat3[0] * mat5[2] - mat3[2] * mat5[0];
            var det2_35_03 = mat3[0] * mat5[3] - mat3[3] * mat5[0];
            var det2_35_04 = mat3[0] * mat5[4] - mat3[4] * mat5[0];
            var det2_35_05 = mat3[0] * mat5[5] - mat3[5] * mat5[0];
            var det2_35_12 = mat3[1] * mat5[2] - mat3[2] * mat5[1];
            var det2_35_13 = mat3[1] * mat5[3] - mat3[3] * mat5[1];
            var det2_35_14 = mat3[1] * mat5[4] - mat3[4] * mat5[1];
            var det2_35_15 = mat3[1] * mat5[5] - mat3[5] * mat5[1];
            var det2_35_23 = mat3[2] * mat5[3] - mat3[3] * mat5[2];
            var det2_35_24 = mat3[2] * mat5[4] - mat3[4] * mat5[2];
            var det2_35_25 = mat3[2] * mat5[5] - mat3[5] * mat5[2];
            var det2_35_34 = mat3[3] * mat5[4] - mat3[4] * mat5[3];
            var det2_35_35 = mat3[3] * mat5[5] - mat3[5] * mat5[3];
            var det2_35_45 = mat3[4] * mat5[5] - mat3[5] * mat5[4];

            // remaining 3x3 sub-determinants
            var det3_234_012 = mat2[0] * det2_34_12 - mat2[1] * det2_34_02 + mat2[2] * det2_34_01;
            var det3_234_013 = mat2[0] * det2_34_13 - mat2[1] * det2_34_03 + mat2[3] * det2_34_01;
            var det3_234_014 = mat2[0] * det2_34_14 - mat2[1] * det2_34_04 + mat2[4] * det2_34_01;
            var det3_234_015 = mat2[0] * det2_34_15 - mat2[1] * det2_34_05 + mat2[5] * det2_34_01;
            var det3_234_023 = mat2[0] * det2_34_23 - mat2[2] * det2_34_03 + mat2[3] * det2_34_02;
            var det3_234_024 = mat2[0] * det2_34_24 - mat2[2] * det2_34_04 + mat2[4] * det2_34_02;
            var det3_234_025 = mat2[0] * det2_34_25 - mat2[2] * det2_34_05 + mat2[5] * det2_34_02;
            var det3_234_034 = mat2[0] * det2_34_34 - mat2[3] * det2_34_04 + mat2[4] * det2_34_03;
            var det3_234_035 = mat2[0] * det2_34_35 - mat2[3] * det2_34_05 + mat2[5] * det2_34_03;
            var det3_234_045 = mat2[0] * det2_34_45 - mat2[4] * det2_34_05 + mat2[5] * det2_34_04;
            var det3_234_123 = mat2[1] * det2_34_23 - mat2[2] * det2_34_13 + mat2[3] * det2_34_12;
            var det3_234_124 = mat2[1] * det2_34_24 - mat2[2] * det2_34_14 + mat2[4] * det2_34_12;
            var det3_234_125 = mat2[1] * det2_34_25 - mat2[2] * det2_34_15 + mat2[5] * det2_34_12;
            var det3_234_134 = mat2[1] * det2_34_34 - mat2[3] * det2_34_14 + mat2[4] * det2_34_13;
            var det3_234_135 = mat2[1] * det2_34_35 - mat2[3] * det2_34_15 + mat2[5] * det2_34_13;
            var det3_234_145 = mat2[1] * det2_34_45 - mat2[4] * det2_34_15 + mat2[5] * det2_34_14;
            var det3_234_234 = mat2[2] * det2_34_34 - mat2[3] * det2_34_24 + mat2[4] * det2_34_23;
            var det3_234_235 = mat2[2] * det2_34_35 - mat2[3] * det2_34_25 + mat2[5] * det2_34_23;
            var det3_234_245 = mat2[2] * det2_34_45 - mat2[4] * det2_34_25 + mat2[5] * det2_34_24;
            var det3_234_345 = mat2[3] * det2_34_45 - mat2[4] * det2_34_35 + mat2[5] * det2_34_34;
            var det3_235_012 = mat2[0] * det2_35_12 - mat2[1] * det2_35_02 + mat2[2] * det2_35_01;
            var det3_235_013 = mat2[0] * det2_35_13 - mat2[1] * det2_35_03 + mat2[3] * det2_35_01;
            var det3_235_014 = mat2[0] * det2_35_14 - mat2[1] * det2_35_04 + mat2[4] * det2_35_01;
            var det3_235_015 = mat2[0] * det2_35_15 - mat2[1] * det2_35_05 + mat2[5] * det2_35_01;
            var det3_235_023 = mat2[0] * det2_35_23 - mat2[2] * det2_35_03 + mat2[3] * det2_35_02;
            var det3_235_024 = mat2[0] * det2_35_24 - mat2[2] * det2_35_04 + mat2[4] * det2_35_02;
            var det3_235_025 = mat2[0] * det2_35_25 - mat2[2] * det2_35_05 + mat2[5] * det2_35_02;
            var det3_235_034 = mat2[0] * det2_35_34 - mat2[3] * det2_35_04 + mat2[4] * det2_35_03;
            var det3_235_035 = mat2[0] * det2_35_35 - mat2[3] * det2_35_05 + mat2[5] * det2_35_03;
            var det3_235_045 = mat2[0] * det2_35_45 - mat2[4] * det2_35_05 + mat2[5] * det2_35_04;
            var det3_235_123 = mat2[1] * det2_35_23 - mat2[2] * det2_35_13 + mat2[3] * det2_35_12;
            var det3_235_124 = mat2[1] * det2_35_24 - mat2[2] * det2_35_14 + mat2[4] * det2_35_12;
            var det3_235_125 = mat2[1] * det2_35_25 - mat2[2] * det2_35_15 + mat2[5] * det2_35_12;
            var det3_235_134 = mat2[1] * det2_35_34 - mat2[3] * det2_35_14 + mat2[4] * det2_35_13;
            var det3_235_135 = mat2[1] * det2_35_35 - mat2[3] * det2_35_15 + mat2[5] * det2_35_13;
            var det3_235_145 = mat2[1] * det2_35_45 - mat2[4] * det2_35_15 + mat2[5] * det2_35_14;
            var det3_235_234 = mat2[2] * det2_35_34 - mat2[3] * det2_35_24 + mat2[4] * det2_35_23;
            var det3_235_235 = mat2[2] * det2_35_35 - mat2[3] * det2_35_25 + mat2[5] * det2_35_23;
            var det3_235_245 = mat2[2] * det2_35_45 - mat2[4] * det2_35_25 + mat2[5] * det2_35_24;
            var det3_235_345 = mat2[3] * det2_35_45 - mat2[4] * det2_35_35 + mat2[5] * det2_35_34;
            var det3_245_012 = mat2[0] * det2_45_12 - mat2[1] * det2_45_02 + mat2[2] * det2_45_01;
            var det3_245_013 = mat2[0] * det2_45_13 - mat2[1] * det2_45_03 + mat2[3] * det2_45_01;
            var det3_245_014 = mat2[0] * det2_45_14 - mat2[1] * det2_45_04 + mat2[4] * det2_45_01;
            var det3_245_015 = mat2[0] * det2_45_15 - mat2[1] * det2_45_05 + mat2[5] * det2_45_01;
            var det3_245_023 = mat2[0] * det2_45_23 - mat2[2] * det2_45_03 + mat2[3] * det2_45_02;
            var det3_245_024 = mat2[0] * det2_45_24 - mat2[2] * det2_45_04 + mat2[4] * det2_45_02;
            var det3_245_025 = mat2[0] * det2_45_25 - mat2[2] * det2_45_05 + mat2[5] * det2_45_02;
            var det3_245_034 = mat2[0] * det2_45_34 - mat2[3] * det2_45_04 + mat2[4] * det2_45_03;
            var det3_245_035 = mat2[0] * det2_45_35 - mat2[3] * det2_45_05 + mat2[5] * det2_45_03;
            var det3_245_045 = mat2[0] * det2_45_45 - mat2[4] * det2_45_05 + mat2[5] * det2_45_04;
            var det3_245_123 = mat2[1] * det2_45_23 - mat2[2] * det2_45_13 + mat2[3] * det2_45_12;
            var det3_245_124 = mat2[1] * det2_45_24 - mat2[2] * det2_45_14 + mat2[4] * det2_45_12;
            var det3_245_125 = mat2[1] * det2_45_25 - mat2[2] * det2_45_15 + mat2[5] * det2_45_12;
            var det3_245_134 = mat2[1] * det2_45_34 - mat2[3] * det2_45_14 + mat2[4] * det2_45_13;
            var det3_245_135 = mat2[1] * det2_45_35 - mat2[3] * det2_45_15 + mat2[5] * det2_45_13;
            var det3_245_145 = mat2[1] * det2_45_45 - mat2[4] * det2_45_15 + mat2[5] * det2_45_14;
            var det3_245_234 = mat2[2] * det2_45_34 - mat2[3] * det2_45_24 + mat2[4] * det2_45_23;
            var det3_245_235 = mat2[2] * det2_45_35 - mat2[3] * det2_45_25 + mat2[5] * det2_45_23;
            var det3_245_245 = mat2[2] * det2_45_45 - mat2[4] * det2_45_25 + mat2[5] * det2_45_24;
            var det3_245_345 = mat2[3] * det2_45_45 - mat2[4] * det2_45_35 + mat2[5] * det2_45_34;

            // remaining 4x4 sub-determinants
            var det4_1234_0123 = mat1[0] * det3_234_123 - mat1[1] * det3_234_023 + mat1[2] * det3_234_013 - mat1[3] * det3_234_012;
            var det4_1234_0124 = mat1[0] * det3_234_124 - mat1[1] * det3_234_024 + mat1[2] * det3_234_014 - mat1[4] * det3_234_012;
            var det4_1234_0125 = mat1[0] * det3_234_125 - mat1[1] * det3_234_025 + mat1[2] * det3_234_015 - mat1[5] * det3_234_012;
            var det4_1234_0134 = mat1[0] * det3_234_134 - mat1[1] * det3_234_034 + mat1[3] * det3_234_014 - mat1[4] * det3_234_013;
            var det4_1234_0135 = mat1[0] * det3_234_135 - mat1[1] * det3_234_035 + mat1[3] * det3_234_015 - mat1[5] * det3_234_013;
            var det4_1234_0145 = mat1[0] * det3_234_145 - mat1[1] * det3_234_045 + mat1[4] * det3_234_015 - mat1[5] * det3_234_014;
            var det4_1234_0234 = mat1[0] * det3_234_234 - mat1[2] * det3_234_034 + mat1[3] * det3_234_024 - mat1[4] * det3_234_023;
            var det4_1234_0235 = mat1[0] * det3_234_235 - mat1[2] * det3_234_035 + mat1[3] * det3_234_025 - mat1[5] * det3_234_023;
            var det4_1234_0245 = mat1[0] * det3_234_245 - mat1[2] * det3_234_045 + mat1[4] * det3_234_025 - mat1[5] * det3_234_024;
            var det4_1234_0345 = mat1[0] * det3_234_345 - mat1[3] * det3_234_045 + mat1[4] * det3_234_035 - mat1[5] * det3_234_034;
            var det4_1234_1234 = mat1[1] * det3_234_234 - mat1[2] * det3_234_134 + mat1[3] * det3_234_124 - mat1[4] * det3_234_123;
            var det4_1234_1235 = mat1[1] * det3_234_235 - mat1[2] * det3_234_135 + mat1[3] * det3_234_125 - mat1[5] * det3_234_123;
            var det4_1234_1245 = mat1[1] * det3_234_245 - mat1[2] * det3_234_145 + mat1[4] * det3_234_125 - mat1[5] * det3_234_124;
            var det4_1234_1345 = mat1[1] * det3_234_345 - mat1[3] * det3_234_145 + mat1[4] * det3_234_135 - mat1[5] * det3_234_134;
            var det4_1234_2345 = mat1[2] * det3_234_345 - mat1[3] * det3_234_245 + mat1[4] * det3_234_235 - mat1[5] * det3_234_234;
            var det4_1235_0123 = mat1[0] * det3_235_123 - mat1[1] * det3_235_023 + mat1[2] * det3_235_013 - mat1[3] * det3_235_012;
            var det4_1235_0124 = mat1[0] * det3_235_124 - mat1[1] * det3_235_024 + mat1[2] * det3_235_014 - mat1[4] * det3_235_012;
            var det4_1235_0125 = mat1[0] * det3_235_125 - mat1[1] * det3_235_025 + mat1[2] * det3_235_015 - mat1[5] * det3_235_012;
            var det4_1235_0134 = mat1[0] * det3_235_134 - mat1[1] * det3_235_034 + mat1[3] * det3_235_014 - mat1[4] * det3_235_013;
            var det4_1235_0135 = mat1[0] * det3_235_135 - mat1[1] * det3_235_035 + mat1[3] * det3_235_015 - mat1[5] * det3_235_013;
            var det4_1235_0145 = mat1[0] * det3_235_145 - mat1[1] * det3_235_045 + mat1[4] * det3_235_015 - mat1[5] * det3_235_014;
            var det4_1235_0234 = mat1[0] * det3_235_234 - mat1[2] * det3_235_034 + mat1[3] * det3_235_024 - mat1[4] * det3_235_023;
            var det4_1235_0235 = mat1[0] * det3_235_235 - mat1[2] * det3_235_035 + mat1[3] * det3_235_025 - mat1[5] * det3_235_023;
            var det4_1235_0245 = mat1[0] * det3_235_245 - mat1[2] * det3_235_045 + mat1[4] * det3_235_025 - mat1[5] * det3_235_024;
            var det4_1235_0345 = mat1[0] * det3_235_345 - mat1[3] * det3_235_045 + mat1[4] * det3_235_035 - mat1[5] * det3_235_034;
            var det4_1235_1234 = mat1[1] * det3_235_234 - mat1[2] * det3_235_134 + mat1[3] * det3_235_124 - mat1[4] * det3_235_123;
            var det4_1235_1235 = mat1[1] * det3_235_235 - mat1[2] * det3_235_135 + mat1[3] * det3_235_125 - mat1[5] * det3_235_123;
            var det4_1235_1245 = mat1[1] * det3_235_245 - mat1[2] * det3_235_145 + mat1[4] * det3_235_125 - mat1[5] * det3_235_124;
            var det4_1235_1345 = mat1[1] * det3_235_345 - mat1[3] * det3_235_145 + mat1[4] * det3_235_135 - mat1[5] * det3_235_134;
            var det4_1235_2345 = mat1[2] * det3_235_345 - mat1[3] * det3_235_245 + mat1[4] * det3_235_235 - mat1[5] * det3_235_234;
            var det4_1245_0123 = mat1[0] * det3_245_123 - mat1[1] * det3_245_023 + mat1[2] * det3_245_013 - mat1[3] * det3_245_012;
            var det4_1245_0124 = mat1[0] * det3_245_124 - mat1[1] * det3_245_024 + mat1[2] * det3_245_014 - mat1[4] * det3_245_012;
            var det4_1245_0125 = mat1[0] * det3_245_125 - mat1[1] * det3_245_025 + mat1[2] * det3_245_015 - mat1[5] * det3_245_012;
            var det4_1245_0134 = mat1[0] * det3_245_134 - mat1[1] * det3_245_034 + mat1[3] * det3_245_014 - mat1[4] * det3_245_013;
            var det4_1245_0135 = mat1[0] * det3_245_135 - mat1[1] * det3_245_035 + mat1[3] * det3_245_015 - mat1[5] * det3_245_013;
            var det4_1245_0145 = mat1[0] * det3_245_145 - mat1[1] * det3_245_045 + mat1[4] * det3_245_015 - mat1[5] * det3_245_014;
            var det4_1245_0234 = mat1[0] * det3_245_234 - mat1[2] * det3_245_034 + mat1[3] * det3_245_024 - mat1[4] * det3_245_023;
            var det4_1245_0235 = mat1[0] * det3_245_235 - mat1[2] * det3_245_035 + mat1[3] * det3_245_025 - mat1[5] * det3_245_023;
            var det4_1245_0245 = mat1[0] * det3_245_245 - mat1[2] * det3_245_045 + mat1[4] * det3_245_025 - mat1[5] * det3_245_024;
            var det4_1245_0345 = mat1[0] * det3_245_345 - mat1[3] * det3_245_045 + mat1[4] * det3_245_035 - mat1[5] * det3_245_034;
            var det4_1245_1234 = mat1[1] * det3_245_234 - mat1[2] * det3_245_134 + mat1[3] * det3_245_124 - mat1[4] * det3_245_123;
            var det4_1245_1235 = mat1[1] * det3_245_235 - mat1[2] * det3_245_135 + mat1[3] * det3_245_125 - mat1[5] * det3_245_123;
            var det4_1245_1245 = mat1[1] * det3_245_245 - mat1[2] * det3_245_145 + mat1[4] * det3_245_125 - mat1[5] * det3_245_124;
            var det4_1245_1345 = mat1[1] * det3_245_345 - mat1[3] * det3_245_145 + mat1[4] * det3_245_135 - mat1[5] * det3_245_134;
            var det4_1245_2345 = mat1[2] * det3_245_345 - mat1[3] * det3_245_245 + mat1[4] * det3_245_235 - mat1[5] * det3_245_234;
            var det4_1345_0123 = mat1[0] * det3_345_123 - mat1[1] * det3_345_023 + mat1[2] * det3_345_013 - mat1[3] * det3_345_012;
            var det4_1345_0124 = mat1[0] * det3_345_124 - mat1[1] * det3_345_024 + mat1[2] * det3_345_014 - mat1[4] * det3_345_012;
            var det4_1345_0125 = mat1[0] * det3_345_125 - mat1[1] * det3_345_025 + mat1[2] * det3_345_015 - mat1[5] * det3_345_012;
            var det4_1345_0134 = mat1[0] * det3_345_134 - mat1[1] * det3_345_034 + mat1[3] * det3_345_014 - mat1[4] * det3_345_013;
            var det4_1345_0135 = mat1[0] * det3_345_135 - mat1[1] * det3_345_035 + mat1[3] * det3_345_015 - mat1[5] * det3_345_013;
            var det4_1345_0145 = mat1[0] * det3_345_145 - mat1[1] * det3_345_045 + mat1[4] * det3_345_015 - mat1[5] * det3_345_014;
            var det4_1345_0234 = mat1[0] * det3_345_234 - mat1[2] * det3_345_034 + mat1[3] * det3_345_024 - mat1[4] * det3_345_023;
            var det4_1345_0235 = mat1[0] * det3_345_235 - mat1[2] * det3_345_035 + mat1[3] * det3_345_025 - mat1[5] * det3_345_023;
            var det4_1345_0245 = mat1[0] * det3_345_245 - mat1[2] * det3_345_045 + mat1[4] * det3_345_025 - mat1[5] * det3_345_024;
            var det4_1345_0345 = mat1[0] * det3_345_345 - mat1[3] * det3_345_045 + mat1[4] * det3_345_035 - mat1[5] * det3_345_034;
            var det4_1345_1234 = mat1[1] * det3_345_234 - mat1[2] * det3_345_134 + mat1[3] * det3_345_124 - mat1[4] * det3_345_123;
            var det4_1345_1235 = mat1[1] * det3_345_235 - mat1[2] * det3_345_135 + mat1[3] * det3_345_125 - mat1[5] * det3_345_123;
            var det4_1345_1245 = mat1[1] * det3_345_245 - mat1[2] * det3_345_145 + mat1[4] * det3_345_125 - mat1[5] * det3_345_124;
            var det4_1345_1345 = mat1[1] * det3_345_345 - mat1[3] * det3_345_145 + mat1[4] * det3_345_135 - mat1[5] * det3_345_134;
            var det4_1345_2345 = mat1[2] * det3_345_345 - mat1[3] * det3_345_245 + mat1[4] * det3_345_235 - mat1[5] * det3_345_234;

            // remaining 5x5 sub-determinants
            var det5_01234_01234 = mat0[0] * det4_1234_1234 - mat0[1] * det4_1234_0234 + mat0[2] * det4_1234_0134 - mat0[3] * det4_1234_0124 + mat0[4] * det4_1234_0123;
            var det5_01234_01235 = mat0[0] * det4_1234_1235 - mat0[1] * det4_1234_0235 + mat0[2] * det4_1234_0135 - mat0[3] * det4_1234_0125 + mat0[5] * det4_1234_0123;
            var det5_01234_01245 = mat0[0] * det4_1234_1245 - mat0[1] * det4_1234_0245 + mat0[2] * det4_1234_0145 - mat0[4] * det4_1234_0125 + mat0[5] * det4_1234_0124;
            var det5_01234_01345 = mat0[0] * det4_1234_1345 - mat0[1] * det4_1234_0345 + mat0[3] * det4_1234_0145 - mat0[4] * det4_1234_0135 + mat0[5] * det4_1234_0134;
            var det5_01234_02345 = mat0[0] * det4_1234_2345 - mat0[2] * det4_1234_0345 + mat0[3] * det4_1234_0245 - mat0[4] * det4_1234_0235 + mat0[5] * det4_1234_0234;
            var det5_01234_12345 = mat0[1] * det4_1234_2345 - mat0[2] * det4_1234_1345 + mat0[3] * det4_1234_1245 - mat0[4] * det4_1234_1235 + mat0[5] * det4_1234_1234;
            var det5_01235_01234 = mat0[0] * det4_1235_1234 - mat0[1] * det4_1235_0234 + mat0[2] * det4_1235_0134 - mat0[3] * det4_1235_0124 + mat0[4] * det4_1235_0123;
            var det5_01235_01235 = mat0[0] * det4_1235_1235 - mat0[1] * det4_1235_0235 + mat0[2] * det4_1235_0135 - mat0[3] * det4_1235_0125 + mat0[5] * det4_1235_0123;
            var det5_01235_01245 = mat0[0] * det4_1235_1245 - mat0[1] * det4_1235_0245 + mat0[2] * det4_1235_0145 - mat0[4] * det4_1235_0125 + mat0[5] * det4_1235_0124;
            var det5_01235_01345 = mat0[0] * det4_1235_1345 - mat0[1] * det4_1235_0345 + mat0[3] * det4_1235_0145 - mat0[4] * det4_1235_0135 + mat0[5] * det4_1235_0134;
            var det5_01235_02345 = mat0[0] * det4_1235_2345 - mat0[2] * det4_1235_0345 + mat0[3] * det4_1235_0245 - mat0[4] * det4_1235_0235 + mat0[5] * det4_1235_0234;
            var det5_01235_12345 = mat0[1] * det4_1235_2345 - mat0[2] * det4_1235_1345 + mat0[3] * det4_1235_1245 - mat0[4] * det4_1235_1235 + mat0[5] * det4_1235_1234;
            var det5_01245_01234 = mat0[0] * det4_1245_1234 - mat0[1] * det4_1245_0234 + mat0[2] * det4_1245_0134 - mat0[3] * det4_1245_0124 + mat0[4] * det4_1245_0123;
            var det5_01245_01235 = mat0[0] * det4_1245_1235 - mat0[1] * det4_1245_0235 + mat0[2] * det4_1245_0135 - mat0[3] * det4_1245_0125 + mat0[5] * det4_1245_0123;
            var det5_01245_01245 = mat0[0] * det4_1245_1245 - mat0[1] * det4_1245_0245 + mat0[2] * det4_1245_0145 - mat0[4] * det4_1245_0125 + mat0[5] * det4_1245_0124;
            var det5_01245_01345 = mat0[0] * det4_1245_1345 - mat0[1] * det4_1245_0345 + mat0[3] * det4_1245_0145 - mat0[4] * det4_1245_0135 + mat0[5] * det4_1245_0134;
            var det5_01245_02345 = mat0[0] * det4_1245_2345 - mat0[2] * det4_1245_0345 + mat0[3] * det4_1245_0245 - mat0[4] * det4_1245_0235 + mat0[5] * det4_1245_0234;
            var det5_01245_12345 = mat0[1] * det4_1245_2345 - mat0[2] * det4_1245_1345 + mat0[3] * det4_1245_1245 - mat0[4] * det4_1245_1235 + mat0[5] * det4_1245_1234;
            var det5_01345_01234 = mat0[0] * det4_1345_1234 - mat0[1] * det4_1345_0234 + mat0[2] * det4_1345_0134 - mat0[3] * det4_1345_0124 + mat0[4] * det4_1345_0123;
            var det5_01345_01235 = mat0[0] * det4_1345_1235 - mat0[1] * det4_1345_0235 + mat0[2] * det4_1345_0135 - mat0[3] * det4_1345_0125 + mat0[5] * det4_1345_0123;
            var det5_01345_01245 = mat0[0] * det4_1345_1245 - mat0[1] * det4_1345_0245 + mat0[2] * det4_1345_0145 - mat0[4] * det4_1345_0125 + mat0[5] * det4_1345_0124;
            var det5_01345_01345 = mat0[0] * det4_1345_1345 - mat0[1] * det4_1345_0345 + mat0[3] * det4_1345_0145 - mat0[4] * det4_1345_0135 + mat0[5] * det4_1345_0134;
            var det5_01345_02345 = mat0[0] * det4_1345_2345 - mat0[2] * det4_1345_0345 + mat0[3] * det4_1345_0245 - mat0[4] * det4_1345_0235 + mat0[5] * det4_1345_0234;
            var det5_01345_12345 = mat0[1] * det4_1345_2345 - mat0[2] * det4_1345_1345 + mat0[3] * det4_1345_1245 - mat0[4] * det4_1345_1235 + mat0[5] * det4_1345_1234;
            var det5_02345_01234 = mat0[0] * det4_2345_1234 - mat0[1] * det4_2345_0234 + mat0[2] * det4_2345_0134 - mat0[3] * det4_2345_0124 + mat0[4] * det4_2345_0123;
            var det5_02345_01235 = mat0[0] * det4_2345_1235 - mat0[1] * det4_2345_0235 + mat0[2] * det4_2345_0135 - mat0[3] * det4_2345_0125 + mat0[5] * det4_2345_0123;
            var det5_02345_01245 = mat0[0] * det4_2345_1245 - mat0[1] * det4_2345_0245 + mat0[2] * det4_2345_0145 - mat0[4] * det4_2345_0125 + mat0[5] * det4_2345_0124;
            var det5_02345_01345 = mat0[0] * det4_2345_1345 - mat0[1] * det4_2345_0345 + mat0[3] * det4_2345_0145 - mat0[4] * det4_2345_0135 + mat0[5] * det4_2345_0134;
            var det5_02345_02345 = mat0[0] * det4_2345_2345 - mat0[2] * det4_2345_0345 + mat0[3] * det4_2345_0245 - mat0[4] * det4_2345_0235 + mat0[5] * det4_2345_0234;
            var det5_02345_12345 = mat0[1] * det4_2345_2345 - mat0[2] * det4_2345_1345 + mat0[3] * det4_2345_1245 - mat0[4] * det4_2345_1235 + mat0[5] * det4_2345_1234;

            mat0[0] = det5_12345_12345 * invDet; mat0[1] = -det5_02345_12345 * invDet; mat0[2] = det5_01345_12345 * invDet; mat0[3] = -det5_01245_12345 * invDet; mat0[4] = det5_01235_12345 * invDet; mat0[5] = -det5_01234_12345 * invDet;
            mat1[0] = -det5_12345_02345 * invDet; mat1[1] = det5_02345_02345 * invDet; mat1[2] = -det5_01345_02345 * invDet; mat1[3] = det5_01245_02345 * invDet; mat1[4] = -det5_01235_02345 * invDet; mat1[5] = det5_01234_02345 * invDet;
            mat2[0] = det5_12345_01345 * invDet; mat2[1] = -det5_02345_01345 * invDet; mat2[2] = det5_01345_01345 * invDet; mat2[3] = -det5_01245_01345 * invDet; mat2[4] = det5_01235_01345 * invDet; mat2[5] = -det5_01234_01345 * invDet;
            mat3[0] = -det5_12345_01245 * invDet; mat3[1] = det5_02345_01245 * invDet; mat3[2] = -det5_01345_01245 * invDet; mat3[3] = det5_01245_01245 * invDet; mat3[4] = -det5_01235_01245 * invDet; mat3[5] = det5_01234_01245 * invDet;
            mat4[0] = det5_12345_01235 * invDet; mat4[1] = -det5_02345_01235 * invDet; mat4[2] = det5_01345_01235 * invDet; mat4[3] = -det5_01245_01235 * invDet; mat4[4] = det5_01235_01235 * invDet; mat4[5] = -det5_01234_01235 * invDet;
            mat5[0] = -det5_12345_01234 * invDet; mat5[1] = det5_02345_01234 * invDet; mat5[2] = -det5_01345_01234 * invDet; mat5[3] = det5_01245_01234 * invDet; mat5[4] = -det5_01235_01234 * invDet; mat5[5] = det5_01234_01234 * invDet;

            return true;
        }
        public Matrix6x6 InverseFast() // returns the inverse ( m * m.Inverse() = identity )
        {
            var invMat = this;
            var r = invMat.InverseFastSelf();
            Debug.Assert(r);
            return invMat;
        }
        public unsafe bool InverseFastSelf()    // returns false if determinant is zero
        {
            // 6*27+2*30 = 222 multiplications
            //		2*1  =	 2 divisions

            fixed (float* matp = &mat0.p[0])
            {
                // r0 = m0.Inverse();
                var c0 = matp[1 * 6 + 1] * matp[2 * 6 + 2] - matp[1 * 6 + 2] * matp[2 * 6 + 1];
                var c1 = matp[1 * 6 + 2] * matp[2 * 6 + 0] - matp[1 * 6 + 0] * matp[2 * 6 + 2];
                var c2 = matp[1 * 6 + 0] * matp[2 * 6 + 1] - matp[1 * 6 + 1] * matp[2 * 6 + 0];

                var det = matp[0 * 6 + 0] * c0 + matp[0 * 6 + 1] * c1 + matp[0 * 6 + 2] * c2;
                if (MathX.Fabs(det) < Matrix_.MATRIX_INVERSE_EPSILON)
                    return false;

                var invDet = 1.0f / det;

                Matrix3x3 r0;
                r0.mat0.x = c0 * invDet;
                r0.mat0.y = (matp[0 * 6 + 2] * matp[2 * 6 + 1] - matp[0 * 6 + 1] * matp[2 * 6 + 2]) * invDet;
                r0.mat0.z = (matp[0 * 6 + 1] * matp[1 * 6 + 2] - matp[0 * 6 + 2] * matp[1 * 6 + 1]) * invDet;
                r0.mat1.x = c1 * invDet;
                r0.mat1.y = (matp[0 * 6 + 0] * matp[2 * 6 + 2] - matp[0 * 6 + 2] * matp[2 * 6 + 0]) * invDet;
                r0.mat1.z = (matp[0 * 6 + 2] * matp[1 * 6 + 0] - matp[0 * 6 + 0] * matp[1 * 6 + 2]) * invDet;
                r0.mat2.x = c2 * invDet;
                r0.mat2.y = (matp[0 * 6 + 1] * matp[2 * 6 + 0] - matp[0 * 6 + 0] * matp[2 * 6 + 1]) * invDet;
                r0.mat2.z = (matp[0 * 6 + 0] * matp[1 * 6 + 1] - matp[0 * 6 + 1] * matp[1 * 6 + 0]) * invDet;

                // r1 = r0 * m1;
                Matrix3x3 r1;
                r1.mat0.x = r0.mat0.x * matp[0 * 6 + 3] + r0.mat0.y * matp[1 * 6 + 3] + r0.mat0.z * matp[2 * 6 + 3];
                r1.mat0.y = r0.mat0.x * matp[0 * 6 + 4] + r0.mat0.y * matp[1 * 6 + 4] + r0.mat0.z * matp[2 * 6 + 4];
                r1.mat0.z = r0.mat0.x * matp[0 * 6 + 5] + r0.mat0.y * matp[1 * 6 + 5] + r0.mat0.z * matp[2 * 6 + 5];
                r1.mat1.x = r0.mat1.x * matp[0 * 6 + 3] + r0.mat1.y * matp[1 * 6 + 3] + r0.mat1.z * matp[2 * 6 + 3];
                r1.mat1.y = r0.mat1.x * matp[0 * 6 + 4] + r0.mat1.y * matp[1 * 6 + 4] + r0.mat1.z * matp[2 * 6 + 4];
                r1.mat1.z = r0.mat1.x * matp[0 * 6 + 5] + r0.mat1.y * matp[1 * 6 + 5] + r0.mat1.z * matp[2 * 6 + 5];
                r1.mat2.x = r0.mat2.x * matp[0 * 6 + 3] + r0.mat2.y * matp[1 * 6 + 3] + r0.mat2.z * matp[2 * 6 + 3];
                r1.mat2.y = r0.mat2.x * matp[0 * 6 + 4] + r0.mat2.y * matp[1 * 6 + 4] + r0.mat2.z * matp[2 * 6 + 4];
                r1.mat2.z = r0.mat2.x * matp[0 * 6 + 5] + r0.mat2.y * matp[1 * 6 + 5] + r0.mat2.z * matp[2 * 6 + 5];

                // r2 = m2 * r1;
                Matrix3x3 r2;
                r2.mat0.x = matp[3 * 6 + 0] * r1.mat0.x + matp[3 * 6 + 1] * r1.mat1.x + matp[3 * 6 + 2] * r1.mat2.x;
                r2.mat0.y = matp[3 * 6 + 0] * r1.mat0.y + matp[3 * 6 + 1] * r1.mat1.y + matp[3 * 6 + 2] * r1.mat2.y;
                r2.mat0.z = matp[3 * 6 + 0] * r1.mat0.z + matp[3 * 6 + 1] * r1.mat1.z + matp[3 * 6 + 2] * r1.mat2.z;
                r2.mat1.x = matp[4 * 6 + 0] * r1.mat0.x + matp[4 * 6 + 1] * r1.mat1.x + matp[4 * 6 + 2] * r1.mat2.x;
                r2.mat1.y = matp[4 * 6 + 0] * r1.mat0.y + matp[4 * 6 + 1] * r1.mat1.y + matp[4 * 6 + 2] * r1.mat2.y;
                r2.mat1.z = matp[4 * 6 + 0] * r1.mat0.z + matp[4 * 6 + 1] * r1.mat1.z + matp[4 * 6 + 2] * r1.mat2.z;
                r2.mat2.x = matp[5 * 6 + 0] * r1.mat0.x + matp[5 * 6 + 1] * r1.mat1.x + matp[5 * 6 + 2] * r1.mat2.x;
                r2.mat2.y = matp[5 * 6 + 0] * r1.mat0.y + matp[5 * 6 + 1] * r1.mat1.y + matp[5 * 6 + 2] * r1.mat2.y;
                r2.mat2.z = matp[5 * 6 + 0] * r1.mat0.z + matp[5 * 6 + 1] * r1.mat1.z + matp[5 * 6 + 2] * r1.mat2.z;

                // r3 = r2 - m3;
                Matrix3x3 r3;
                r3.mat0.x = r2.mat0.x - matp[3 * 6 + 3];
                r3.mat0.y = r2.mat0.y - matp[3 * 6 + 4];
                r3.mat0.z = r2.mat0.z - matp[3 * 6 + 5];
                r3.mat1.x = r2.mat1.x - matp[4 * 6 + 3];
                r3.mat1.y = r2.mat1.y - matp[4 * 6 + 4];
                r3.mat1.z = r2.mat1.z - matp[4 * 6 + 5];
                r3.mat2.x = r2.mat2.x - matp[5 * 6 + 3];
                r3.mat2.y = r2.mat2.y - matp[5 * 6 + 4];
                r3.mat2.z = r2.mat2.z - matp[5 * 6 + 5];

                // r3.InverseSelf();
                r2.mat0.x = r3.mat1.y * r3.mat2.z - r3.mat1.z * r3.mat2.y;
                r2.mat1.x = r3.mat1.z * r3.mat2.x - r3.mat1.x * r3.mat2.z;
                r2.mat2.x = r3.mat1.x * r3.mat2.y - r3.mat1.y * r3.mat2.x;

                det = r3.mat0.x * r2.mat0.x + r3.mat0.y * r2.mat1.x + r3.mat0.z * r2.mat2.x;
                if (MathX.Fabs(det) < Matrix_.MATRIX_INVERSE_EPSILON)
                    return false;

                invDet = 1.0f / det;

                r2.mat0.y = r3.mat0.z * r3.mat2.y - r3.mat0.y * r3.mat2.z;
                r2.mat0.z = r3.mat0.y * r3.mat1.z - r3.mat0.z * r3.mat1.y;
                r2.mat1.y = r3.mat0.x * r3.mat2.z - r3.mat0.z * r3.mat2.x;
                r2.mat1.z = r3.mat0.z * r3.mat1.x - r3.mat0.x * r3.mat1.z;
                r2.mat2.y = r3.mat0.y * r3.mat2.x - r3.mat0.x * r3.mat2.y;
                r2.mat2.z = r3.mat0.x * r3.mat1.y - r3.mat0.y * r3.mat1.x;

                r3.mat0.x = r2.mat0.x * invDet;
                r3.mat0.y = r2.mat0.y * invDet;
                r3.mat0.z = r2.mat0.z * invDet;
                r3.mat1.x = r2.mat1.x * invDet;
                r3.mat1.y = r2.mat1.y * invDet;
                r3.mat1.z = r2.mat1.z * invDet;
                r3.mat2.x = r2.mat2.x * invDet;
                r3.mat2.y = r2.mat2.y * invDet;
                r3.mat2.z = r2.mat2.z * invDet;

                // r2 = m2 * r0;
                r2.mat0.x = matp[3 * 6 + 0] * r0.mat0.x + matp[3 * 6 + 1] * r0.mat1.x + matp[3 * 6 + 2] * r0.mat2.x;
                r2.mat0.y = matp[3 * 6 + 0] * r0.mat0.y + matp[3 * 6 + 1] * r0.mat1.y + matp[3 * 6 + 2] * r0.mat2.y;
                r2.mat0.z = matp[3 * 6 + 0] * r0.mat0.z + matp[3 * 6 + 1] * r0.mat1.z + matp[3 * 6 + 2] * r0.mat2.z;
                r2.mat1.x = matp[4 * 6 + 0] * r0.mat0.x + matp[4 * 6 + 1] * r0.mat1.x + matp[4 * 6 + 2] * r0.mat2.x;
                r2.mat1.y = matp[4 * 6 + 0] * r0.mat0.y + matp[4 * 6 + 1] * r0.mat1.y + matp[4 * 6 + 2] * r0.mat2.y;
                r2.mat1.z = matp[4 * 6 + 0] * r0.mat0.z + matp[4 * 6 + 1] * r0.mat1.z + matp[4 * 6 + 2] * r0.mat2.z;
                r2.mat2.x = matp[5 * 6 + 0] * r0.mat0.x + matp[5 * 6 + 1] * r0.mat1.x + matp[5 * 6 + 2] * r0.mat2.x;
                r2.mat2.y = matp[5 * 6 + 0] * r0.mat0.y + matp[5 * 6 + 1] * r0.mat1.y + matp[5 * 6 + 2] * r0.mat2.y;
                r2.mat2.z = matp[5 * 6 + 0] * r0.mat0.z + matp[5 * 6 + 1] * r0.mat1.z + matp[5 * 6 + 2] * r0.mat2.z;

                // m2 = r3 * r2;
                matp[3 * 6 + 0] = r3.mat0.x * r2.mat0.x + r3.mat0.y * r2.mat1.x + r3.mat0.z * r2.mat2.x;
                matp[3 * 6 + 1] = r3.mat0.x * r2.mat0.y + r3.mat0.y * r2.mat1.y + r3.mat0.z * r2.mat2.y;
                matp[3 * 6 + 2] = r3.mat0.x * r2.mat0.z + r3.mat0.y * r2.mat1.z + r3.mat0.z * r2.mat2.z;
                matp[4 * 6 + 0] = r3.mat1.x * r2.mat0.x + r3.mat1.y * r2.mat1.x + r3.mat1.z * r2.mat2.x;
                matp[4 * 6 + 1] = r3.mat1.x * r2.mat0.y + r3.mat1.y * r2.mat1.y + r3.mat1.z * r2.mat2.y;
                matp[4 * 6 + 2] = r3.mat1.x * r2.mat0.z + r3.mat1.y * r2.mat1.z + r3.mat1.z * r2.mat2.z;
                matp[5 * 6 + 0] = r3.mat2.x * r2.mat0.x + r3.mat2.y * r2.mat1.x + r3.mat2.z * r2.mat2.x;
                matp[5 * 6 + 1] = r3.mat2.x * r2.mat0.y + r3.mat2.y * r2.mat1.y + r3.mat2.z * r2.mat2.y;
                matp[5 * 6 + 2] = r3.mat2.x * r2.mat0.z + r3.mat2.y * r2.mat1.z + r3.mat2.z * r2.mat2.z;

                // m0 = r0 - r1 * m2;
                matp[0 * 6 + 0] = r0.mat0.x - r1.mat0.x * matp[3 * 6 + 0] - r1.mat0.y * matp[4 * 6 + 0] - r1.mat0.z * matp[5 * 6 + 0];
                matp[0 * 6 + 1] = r0.mat0.y - r1.mat0.x * matp[3 * 6 + 1] - r1.mat0.y * matp[4 * 6 + 1] - r1.mat0.z * matp[5 * 6 + 1];
                matp[0 * 6 + 2] = r0.mat0.z - r1.mat0.x * matp[3 * 6 + 2] - r1.mat0.y * matp[4 * 6 + 2] - r1.mat0.z * matp[5 * 6 + 2];
                matp[1 * 6 + 0] = r0.mat1.x - r1.mat1.x * matp[3 * 6 + 0] - r1.mat1.y * matp[4 * 6 + 0] - r1.mat1.z * matp[5 * 6 + 0];
                matp[1 * 6 + 1] = r0.mat1.y - r1.mat1.x * matp[3 * 6 + 1] - r1.mat1.y * matp[4 * 6 + 1] - r1.mat1.z * matp[5 * 6 + 1];
                matp[1 * 6 + 2] = r0.mat1.z - r1.mat1.x * matp[3 * 6 + 2] - r1.mat1.y * matp[4 * 6 + 2] - r1.mat1.z * matp[5 * 6 + 2];
                matp[2 * 6 + 0] = r0.mat2.x - r1.mat2.x * matp[3 * 6 + 0] - r1.mat2.y * matp[4 * 6 + 0] - r1.mat2.z * matp[5 * 6 + 0];
                matp[2 * 6 + 1] = r0.mat2.y - r1.mat2.x * matp[3 * 6 + 1] - r1.mat2.y * matp[4 * 6 + 1] - r1.mat2.z * matp[5 * 6 + 1];
                matp[2 * 6 + 2] = r0.mat2.z - r1.mat2.x * matp[3 * 6 + 2] - r1.mat2.y * matp[4 * 6 + 2] - r1.mat2.z * matp[5 * 6 + 2];

                // m1 = r1 * r3;
                matp[0 * 6 + 3] = r1.mat0.x * r3.mat0.x + r1.mat0.y * r3.mat1.x + r1.mat0.z * r3.mat2.x;
                matp[0 * 6 + 4] = r1.mat0.x * r3.mat0.y + r1.mat0.y * r3.mat1.y + r1.mat0.z * r3.mat2.y;
                matp[0 * 6 + 5] = r1.mat0.x * r3.mat0.z + r1.mat0.y * r3.mat1.z + r1.mat0.z * r3.mat2.z;
                matp[1 * 6 + 3] = r1.mat1.x * r3.mat0.x + r1.mat1.y * r3.mat1.x + r1.mat1.z * r3.mat2.x;
                matp[1 * 6 + 4] = r1.mat1.x * r3.mat0.y + r1.mat1.y * r3.mat1.y + r1.mat1.z * r3.mat2.y;
                matp[1 * 6 + 5] = r1.mat1.x * r3.mat0.z + r1.mat1.y * r3.mat1.z + r1.mat1.z * r3.mat2.z;
                matp[2 * 6 + 3] = r1.mat2.x * r3.mat0.x + r1.mat2.y * r3.mat1.x + r1.mat2.z * r3.mat2.x;
                matp[2 * 6 + 4] = r1.mat2.x * r3.mat0.y + r1.mat2.y * r3.mat1.y + r1.mat2.z * r3.mat2.y;
                matp[2 * 6 + 5] = r1.mat2.x * r3.mat0.z + r1.mat2.y * r3.mat1.z + r1.mat2.z * r3.mat2.z;

                // m3 = -r3;
                matp[3 * 6 + 3] = -r3.mat0.x;
                matp[3 * 6 + 4] = -r3.mat0.y;
                matp[3 * 6 + 5] = -r3.mat0.z;
                matp[4 * 6 + 3] = -r3.mat1.x;
                matp[4 * 6 + 4] = -r3.mat1.y;
                matp[4 * 6 + 5] = -r3.mat1.z;
                matp[5 * 6 + 3] = -r3.mat2.x;
                matp[5 * 6 + 4] = -r3.mat2.y;
                matp[5 * 6 + 5] = -r3.mat2.z;
            }

            return true;
        }

        public static int Dimension
            => 36;

        public unsafe T ToFloatPtr<T>(FloatPtr<T> callback)
            => mat0.ToFloatPtr(callback);
        public unsafe string ToString(int precision = 2)
            => ToFloatPtr(array => StringX.FloatArrayToString(array, Dimension, precision));

        internal Vector6 mat0;
        internal Vector6 mat1;
        internal Vector6 mat2;
        internal Vector6 mat3;
        internal Vector6 mat4;
        internal Vector6 mat5;

        public static Matrix6x6 zero = new(new Vector6(0, 0, 0, 0, 0, 0), new Vector6(0, 0, 0, 0, 0, 0), new Vector6(0, 0, 0, 0, 0, 0), new Vector6(0, 0, 0, 0, 0, 0), new Vector6(0, 0, 0, 0, 0, 0), new Vector6(0, 0, 0, 0, 0, 0));
        public static Matrix6x6 identity = new(new Vector6(1, 0, 0, 0, 0, 0), new Vector6(0, 1, 0, 0, 0, 0), new Vector6(0, 0, 1, 0, 0, 0), new Vector6(0, 0, 0, 1, 0, 0), new Vector6(0, 0, 0, 0, 1, 0), new Vector6(0, 0, 0, 0, 0, 1));
        //#define default identity
    }

    public partial struct MatrixX
    {
        const int MATX_MAX_TEMP = 1024;
        static int MATX_QUAD(int x) => ((x) + 3) & ~3;
        void MATX_CLEAREND() { int s = numRows * numColumns; while (s < ((s + 3) & ~3)) { mat[s++] = 0.0f; } }
        static float[] MATX_ALLOCA(int n) => new float[MATX_QUAD(n)];

        //public MatrixX()
        //{
        //    numRows = numColumns = alloced = 0;
        //    mat = null;
        //}
        public MatrixX(int rows, int columns)
        {
            numRows = numColumns = alloced = 0;
            mat = null;
            SetSize(rows, columns);
        }
        public MatrixX(int rows, int columns, float[] src)
        {
            numRows = numColumns = alloced = 0;
            mat = null;
            SetData(rows, columns, src);
        }

        public void Set(int rows, int columns, float[] src)
        {
            SetSize(rows, columns);
            Array.Copy(src, mat, rows * columns);
        }
        public void Set(Matrix3x3 m1, Matrix3x3 m2)
        {
            SetSize(3, 6);
            for (var i = 0; i < 3; i++)
                for (var j = 0; j < 3; j++)
                {
                    mat[(i + 0) * numColumns + (j + 0)] = m1[i][j];
                    mat[(i + 0) * numColumns + (j + 3)] = m2[i][j];
                }
        }
        public void Set(Matrix3x3 m1, Matrix3x3 m2, Matrix3x3 m3, Matrix3x3 m4)
        {
            SetSize(6, 6);
            for (var i = 0; i < 3; i++)
                for (var j = 0; j < 3; j++)
                {
                    mat[(i + 0) * numColumns + (j + 0)] = m1[i][j];
                    mat[(i + 0) * numColumns + (j + 3)] = m2[i][j];
                    mat[(i + 3) * numColumns + (j + 0)] = m3[i][j];
                    mat[(i + 3) * numColumns + (j + 3)] = m4[i][j];
                }
        }

        public Span<float> this[int index]
        {
            get
            {
                Debug.Assert((index >= 0) && (index < numRows));
                return mat.AsSpan(index * numColumns);
            }
        }

//        public static implicit operator MatrixX(MatrixX a)
//        {
//            SetSize(a.numRows, a.numColumns);
//#if         MATX_SIMD
//            SIMDProcessor->Copy16(mat, a.mat, a.numRows * a.numColumns);
//#else
//            memcpy(mat, a.mat, a.numRows * a.numColumns * sizeof(float));
//#endif
//            tempIndex = 0;
//            return this;
//        }

        public static MatrixX operator *(MatrixX _, float a)
        {
            var m = new MatrixX();
            m.SetTempSize(_.numRows, _.numColumns);
#if MATX_SIMD
            SIMDProcessor.Mul16(m.mat, mat, a, numRows * numColumns);
#else
            var s = _.numRows * _.numColumns;
            for (var i = 0; i < s; i++)
                m.mat[i] = _.mat[i] * a;
#endif
            return m;
        }
        public static VectorX operator *(MatrixX _, VectorX vec)
        {
            Debug.Assert(_.numColumns == vec.Size);
            var dst = new VectorX();
            dst.SetTempSize(_.numRows);
#if MATX_SIMD
            SIMDProcessor.MatX_MultiplyVecX(dst, *this, vec);
#else
            _.Multiply(dst, vec);
#endif
            return dst;
        }
        public static MatrixX operator *(MatrixX _, MatrixX a)
        {
            Debug.Assert(_.numColumns == a.numRows);
            var dst = new MatrixX();
            dst.SetTempSize(_.numRows, a.numColumns);
#if MATX_SIMD
            SIMDProcessor.MatX_MultiplyMatX(dst, *this, a);
#else
            _.Multiply(dst, a);
#endif
            return dst;
        }
        public static MatrixX operator +(MatrixX _, MatrixX a)
        {
            Debug.Assert(_.numRows == a.numRows && _.numColumns == a.numColumns);
            var m = new MatrixX();
            m.SetTempSize(_.numRows, _.numColumns);
#if MATX_SIMD
            SIMDProcessor.Add16(m.mat, mat, a.mat, numRows * numColumns);
#else
            var s = _.numRows * _.numColumns;
            for (var i = 0; i < s; i++)
                m.mat[i] = _.mat[i] + a.mat[i];
#endif
            return m;
        }
        public static MatrixX operator -(MatrixX _, MatrixX a)
        {
            Debug.Assert(_.numRows == a.numRows && _.numColumns == a.numColumns);
            var m = new MatrixX();
            m.SetTempSize(_.numRows, _.numColumns);
#if MATX_SIMD
            SIMDProcessor.Sub16(m.mat, mat, a.mat, numRows * numColumns);
#else
            var s = _.numRows * _.numColumns;
            for (var i = 0; i < s; i++)
                m.mat[i] = _.mat[i] - a.mat[i];
#endif
            return m;
        }

        public static MatrixX operator *(float a, MatrixX m)
            => m * a;
        public static VectorX operator *(VectorX vec, MatrixX m)
            => m * vec;

        public bool Compare(MatrixX a)                             // exact compare, no epsilon
        {
            Debug.Assert(numRows == a.numRows && numColumns == a.numColumns);
            var s = numRows * numColumns;
            for (var i = 0; i < s; i++)
                if (mat[i] != a.mat[i])
                    return false;
            return true;
        }
        public bool Compare(MatrixX a, float epsilon)            // compare with epsilon
        {
            Debug.Assert(numRows == a.numRows && numColumns == a.numColumns);
            var s = numRows * numColumns;
            for (var i = 0; i < s; i++)
                if (MathX.Fabs(mat[i] - a.mat[i]) > epsilon)
                    return false;
            return true;
        }
        public static bool operator ==(MatrixX _, MatrixX a)                         // exact compare, no epsilon
            => _.Compare(a);
        public static bool operator !=(MatrixX _, MatrixX a)                         // exact compare, no epsilon
            => !_.Compare(a);

        public void SetSize(int rows, int columns)                                // set the number of rows/columns
        {
            //Debug.Assert(mat < tempPtr || mat > tempPtr + MATX_MAX_TEMP);
            var alloc = (rows * columns + 3) & ~3;
            if (alloc > alloced && alloced != -1)
            {
                mat = new float[alloc];
                alloced = alloc;
            }
            numRows = rows;
            numColumns = columns;
            MATX_CLEAREND();
        }
        public void ChangeSize(int rows, int columns, bool makeZero = false)      // change the size keeping data intact where possible
        {
            var alloc = (rows * columns + 3) & ~3;
            if (alloc > alloced && alloced != -1)
            {
                var oldMat = mat;
                mat = new float[alloc];
                alloced = alloc;
                if (oldMat != null)
                {
                    var minRow = Math.Min(numRows, rows);
                    var minColumn = Math.Min(numColumns, columns);
                    for (var i = 0; i < minRow; i++)
                        for (var j = 0; j < minColumn; j++)
                            mat[i * columns + j] = oldMat[i * numColumns + j];
                }
            }
            else
            {
                if (columns < numColumns)
                {
                    var minRow = Math.Min(numRows, rows);
                    for (var i = 0; i < minRow; i++)
                        for (var j = 0; j < columns; j++)
                            mat[i * columns + j] = mat[i * numColumns + j];
                }
                else if (columns > numColumns)
                {
                    for (var i = Math.Min(numRows, rows) - 1; i >= 0; i--)
                    {
                        if (makeZero)
                            for (var j = columns - 1; j >= numColumns; j--)
                                mat[i * columns + j] = 0.0f;
                        for (var j = numColumns - 1; j >= 0; j--)
                            mat[i * columns + j] = mat[i * numColumns + j];
                    }
                }
                if (makeZero && rows > numRows)
                    Array.Clear(mat, numRows * columns, (rows - numRows) * columns);
            }
            numRows = rows;
            numColumns = columns;
            MATX_CLEAREND();
        }
        public int NumRows => numRows;                    // get the number of rows
        public int NumColumns => numColumns;              // get the number of columns
        public void SetData(int rows, int columns, float[] data)                   // set float array pointer
        {
            //Debug.Assert(mat < tempPtr || mat > tempPtr + MATX_MAX_TEMP);
            //Debug.Assert((((uintptr_t)data) & 15) == 0); // data must be 16 byte aligned
            mat = data;
            alloced = -1;
            numRows = rows;
            numColumns = columns;
            MATX_CLEAREND();
        }
        public void Zero()                                                   // clear matrix
        {
#if MATX_SIMD
            SIMDProcessor.Zero16(mat, numRows * numColumns);
#else
            Array.Clear(mat, 0, numRows * numColumns);
#endif
        }
        public void Zero(int rows, int columns)                                   // set size and clear matrix
        {
            SetSize(rows, columns);
#if MATX_SIMD
            SIMDProcessor.Zero16(mat, numRows * numColumns);
#else
            Array.Clear(mat, 0, rows * columns);
#endif
        }
        public void Identity()                                               // clear to identity matrix
        {
            Debug.Assert(numRows == numColumns);
#if MATX_SIMD
            SIMDProcessor.Zero16(mat, numRows * numColumns);
#else
            Array.Clear(mat, 0, numRows * numColumns);
#endif
            for (var i = 0; i < numRows; i++)
                mat[i * numColumns + i] = 1.0f;
        }
        public void Identity(int rows, int columns)                               // set size and clear to identity matrix
        {
            Debug.Assert(rows == columns);
            SetSize(rows, columns);
            Identity();
        }
        public void Diag(VectorX v)                                      // create diagonal matrix from vector
        {
            Zero(v.Size, v.Size);
            for (var i = 0; i < v.Size; i++)
                mat[i * numColumns + i] = v[i];
        }
        public void Random(int seed, float l = 0.0f, float u = 1.0f)              // fill matrix with random values
        {
            var rnd = new Random(seed);
            var c = u - l;
            var s = numRows * numColumns;
            for (var i = 0; i < s; i++)
                mat[i] = l + rnd.RandomFloat() * c;
        }
        public void Random(int rows, int columns, int seed, float l = 0.0f, float u = 1.0f)
        {
            var rnd = new Random(seed);
            SetSize(rows, columns);
            var c = u - l;
            var s = numRows * numColumns;
            for (var i = 0; i < s; i++)
                mat[i] = l + rnd.RandomFloat() * c;
        }
        public void Negate()                                                 // this = - this
        {
#if MATX_SIMD
            SIMDProcessor.Negate16(mat, numRows * numColumns);
#else
            var s = numRows * numColumns;
            for (var i = 0; i < s; i++)
                mat[i] = -mat[i];
#endif
        }
        public void Clamp(float min, float max)                                   // clamp all values
        {
            var s = numRows * numColumns;
            for (var i = 0; i < s; i++)
            {
                if (mat[i] < min) mat[i] = min;
                else if (mat[i] > max) mat[i] = max;
            }
        }
        public MatrixX SwapRows(int r1, int r2)                                     // swap rows
        {
            var ptr = new float[numColumns];
            Array.Copy(mat, r1 * numColumns, ptr, 0, numColumns);
            Array.Copy(mat, r2 * numColumns, mat, r1 * numColumns, numColumns);
            Array.Copy(ptr, 0, mat, r2 * numColumns, numColumns);
            return this;
        }
        public unsafe MatrixX SwapColumns(int r1, int r2)                                  // swap columns
        {
            fixed (float* matp = &mat[0])
                for (var i = 0; i < numRows; i++)
                {
                    var ptr = matp + i * numColumns;
                    var tmp = ptr[r1];
                    ptr[r1] = ptr[r2];
                    ptr[r2] = tmp;
                }
            return this;
        }
        public MatrixX SwapRowsColumns(int r1, int r2)                              // swap rows and columns
        {
            SwapRows(r1, r2);
            SwapColumns(r1, r2);
            return this;
        }
        public MatrixX RemoveRow(int r)                                             // remove a row
        {
            Debug.Assert(r < numRows);
            numRows--;
            for (var i = r; i < numRows; i++)
                Array.Copy(mat, (i + 1) * numColumns, mat, i * numColumns, numColumns);
            return this;
        }
        public MatrixX RemoveColumn(int r)                                          // remove a column
        {
            Debug.Assert(r < numColumns);
            numColumns--;
            int i;
            for (i = 0; i < numRows - 1; i++)
                Array.ConstrainedCopy(mat, i * (numColumns + 1) + r + 1, mat, i * numColumns + r, numColumns);
            Array.ConstrainedCopy(mat, i * (numColumns + 1) + r + 1, mat, i * numColumns + r, numColumns - r);
            return this;
        }
        public MatrixX RemoveRowColumn(int r)                                       // remove a row and column
        {
            Debug.Assert(r < numRows && r < numColumns);
            numRows--;
            numColumns--;
            int i;
            if (r > 0)
            {
                for (i = 0; i < r - 1; i++)
                    Array.ConstrainedCopy(mat, i * (numColumns + 1) + r + 1, mat, i * numColumns + r, numColumns);
                Array.ConstrainedCopy(mat, i * (numColumns + 1) + r + 1, mat, i * numColumns + r, numColumns - r);
            }
            Array.Copy(mat, (r + 1) * (numColumns + 1), mat, r * numColumns, r);
            for (i = r; i < numRows - 1; i++)
                Array.Copy(mat, (i + 1) * (numColumns + 1) + r + 1, mat, i * numColumns + r, numColumns);
            Array.Copy(mat, (i + 1) * (numColumns + 1) + r + 1, mat, i * numColumns + r, numColumns - r);
            return this;
        }
        public void ClearUpperTriangle()                                     // clear the upper triangle
        {
            Debug.Assert(numRows == numColumns);
            for (var i = numRows - 2; i >= 0; i--)
                Array.Clear(mat, i * numColumns + i + 1, numColumns - 1 - i);
        }
        public void ClearLowerTriangle()                                     // clear the lower triangle
        {
            Debug.Assert(numRows == numColumns);
            for (var i = 1; i < numRows; i++)
                Array.Clear(mat, i * numColumns, i);
        }
        public void SquareSubMatrix(MatrixX m, int size)                  // get square sub-matrix from 0,0 to size,size
        {
            Debug.Assert(size <= m.numRows && size <= m.numColumns);
            SetSize(size, size);
            for (var i = 0; i < size; i++)
                Array.Copy(m.mat, i * m.numColumns, mat, i * numColumns, size);
        }
        public float MaxDifference(MatrixX m)                          // return maximum element difference between this and m
        {
            Debug.Assert(numRows == m.numRows && numColumns == m.numColumns);
            var maxDiff = -1.0f;
            for (var i = 0; i < numRows; i++)
                for (var j = 0; j < numColumns; j++)
                {
                    var diff = MathX.Fabs(mat[i * numColumns + j] - m[i][j]);
                    if (maxDiff < 0.0f || diff > maxDiff)
                        maxDiff = diff;
                }
            return maxDiff;
        }

        public bool IsSquare() => numRows == numColumns;
        public bool IsZero(float epsilon = Matrix_.MATRIX_EPSILON)
        {
            // returns true if this == Zero
            for (var i = 0; i < numRows; i++)
                for (var j = 0; j < numColumns; j++)
                    if (MathX.Fabs(mat[i * numColumns + j]) > epsilon)
                        return false;
            return true;
        }
        public bool IsIdentity(float epsilon = Matrix_.MATRIX_EPSILON)
        {
            // returns true if this == Identity
            Debug.Assert(numRows == numColumns);
            for (var i = 0; i < numRows; i++)
                for (var j = 0; j < numColumns; j++)
                    if (MathX.Fabs(mat[i * numColumns + j] - (i == j ? 1f : 0f)) > epsilon)
                        return false;
            return true;
        }
        public bool IsDiagonal(float epsilon = Matrix_.MATRIX_EPSILON)
        {
            // returns true if all elements are zero except for the elements on the diagonal
            Debug.Assert(numRows == numColumns);
            for (var i = 0; i < numRows; i++)
                for (var j = 0; j < numColumns; j++)
                    if (i != j && MathX.Fabs(mat[i * numColumns + j]) > epsilon)
                        return false;
            return true;
        }
        public bool IsTriDiagonal(float epsilon = Matrix_.MATRIX_EPSILON)
        {
            // returns true if all elements are zero except for the elements on the diagonal plus or minus one column
            if (numRows != numColumns)
                return false;
            for (var i = 0; i < numRows - 2; i++)
                for (var j = i + 2; j < numColumns; j++)
                {
                    if (MathX.Fabs(this[i][j]) > epsilon)
                        return false;
                    if (MathX.Fabs(this[j][i]) > epsilon)
                        return false;
                }
            return true;
        }
        public bool IsSymmetric(float epsilon = Matrix_.MATRIX_EPSILON)
        {
            // this[i][j] == this[j][i]
            if (numRows != numColumns)
                return false;
            for (var i = 0; i < numRows; i++)
                for (var j = 0; j < numColumns; j++)
                    if (MathX.Fabs(mat[i * numColumns + j] - mat[j * numColumns + i]) > epsilon)
                        return false;
            return true;
        }
        /// <summary>
        /// returns true if this * this.Transpose() == Identity
        /// </summary>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns>
        ///   <c>true</c> if the specified epsilon is orthogonal; otherwise, <c>false</c>.
        /// </returns>
        public unsafe bool IsOrthogonal(float epsilon = Matrix_.MATRIX_EPSILON)
        {
            if (!IsSquare())
                return false;
            fixed (float* mat = this.mat)
            {
                var ptr1 = mat;
                for (var i = 0; i < numRows; i++)
                {
                    for (var j = 0; j < numColumns; j++)
                    {
                        var ptr2 = mat + j;
                        var sum = ptr1[0] * ptr2[0] - (i == j ? 1f : 0f);
                        for (var n = 1; n < numColumns; n++)
                        {
                            ptr2 += numColumns;
                            sum += ptr1[n] * ptr2[0];
                        }
                        if (MathX.Fabs(sum) > epsilon)
                            return false;
                    }
                    ptr1 += numColumns;
                }
            }
            return true;
        }
        /// <summary>
        /// returns true if this * this.Transpose() == Identity and the length of each column vector is 1
        /// </summary>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns>
        ///   <c>true</c> if the specified epsilon is orthonormal; otherwise, <c>false</c>.
        /// </returns>
        public unsafe bool IsOrthonormal(float epsilon = Matrix_.MATRIX_EPSILON)
        {
            if (!IsSquare())
                return false;
            fixed (float* mat = this.mat)
            {
                var ptr1 = mat;
                for (var i = 0; i < numRows; i++)
                {
                    var colVecSum = 0f;
                    var colVecPtr = mat + i; // row 0 col i - don't worry, numRows == numColums because IsSquare()
                    for (var j = 0; j < numColumns; j++)
                    {
                        var ptr2 = mat + j;
                        var sum = ptr1[0] * ptr2[0] - (i == j ? 1f : 0f);
                        for (var n = 1; n < numColumns; n++)
                        {
                            ptr2 += numColumns;
                            sum += ptr1[n] * ptr2[0];
                        }
                        if (MathX.Fabs(sum) > epsilon)
                            return false;
                        // row j, col i - this works because numRows == numColumns
                        colVecSum += colVecPtr[0] * colVecPtr[0];
                        colVecPtr += numColumns; // next row, same column
                    }
                    ptr1 += numColumns;

                    // check that length of *column* vector i is 1 (no need for sqrt because sqrt(1)==1)
                    if (MathX.Fabs(colVecSum - 1.0f) > epsilon)
                        return false;
                }
            }
            return true;
        }
        /// <summary>
        /// returns true if the matrix is a P-matrix
        /// A square matrix is a P-matrix if all its principal minors are positive.
        /// </summary>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns>
        ///   <c>true</c> if [is p matrix] [the specified epsilon]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPMatrix(float epsilon = Matrix_.MATRIX_EPSILON)
        {
            if (!IsSquare())
                return false;
            if (numRows <= 0)
                return true;
            if (this[0][0] <= epsilon)
                return false;
            if (numRows <= 1)
                return true;

            var m = new MatrixX();
            m.SetData(numRows - 1, numColumns - 1, MATX_ALLOCA((numRows - 1) * (numColumns - 1)));

            int i, j;
            for (i = 1; i < numRows; i++)
                for (j = 1; j < numColumns; j++)
                    m[i - 1][j - 1] = this[i][j];

            if (!m.IsPMatrix(epsilon))
                return false;

            for (i = 1; i < numRows; i++)
            {
                var d = this[i][0] / this[0][0];
                for (j = 1; j < numColumns; j++)
                    m[i - 1][j - 1] = this[i][j] - d * this[0][j];
            }

            return m.IsPMatrix(epsilon);
        }
        /// <summary>
        /// returns true if the matrix is a Z-matrix
        /// A square matrix M is a Z-matrix if M[i][j] <= 0 for all i != j.
        /// </summary>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns>
        ///   <c>true</c> if the matrix is a Z-matrix; otherwise, <c>false</c>.
        /// </returns>
        public bool IsZMatrix(float epsilon = Matrix_.MATRIX_EPSILON)
        {
            if (!IsSquare())
                return false;
            for (var i = 0; i < numRows; i++)
                for (var j = 0; j < numColumns; j++)
                    if (this[i][j] > epsilon && i != j)
                        return false;
            return true;
        }

        /// <summary>
        /// returns true if the matrix is Positive Definite (PD)
        /// A square matrix M of order n is said to be PD if y'My > 0 for all vectors y of dimension n, y != 0.
        /// </summary>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns>
        ///   <c>true</c> if the matrix is Positive Definite (PD); otherwise, <c>false</c>.
        /// </returns>
        public bool IsPositiveDefinite(float epsilon = Matrix_.MATRIX_EPSILON)
        {
            // the matrix must be square
            if (!IsSquare())
                return false;

            // copy matrix
            var m = new MatrixX();
            m.SetData(numRows, numColumns, MATX_ALLOCA(numRows * numColumns));
            m = this;

            // add transpose
            int i, j, k;
            for (i = 0; i < numRows; i++)
                for (j = 0; j < numColumns; j++)
                    m[i][j] += this[j][i];

            // test Positive Definiteness with Gaussian pivot steps
            for (i = 0; i < numRows; i++)
            {
                for (j = i; j < numColumns; j++)
                    if (m[j][j] <= epsilon)
                        return false;
                var d = 1.0f / m[i][i];
                for (j = i + 1; j < numColumns; j++)
                {
                    var s = d * m[j][i];
                    m[j][i] = 0.0f;
                    for (k = i + 1; k < numRows; k++)
                        m[j][k] -= s * m[i][k];
                }
            }

            return true;
        }
        /// <summary>
        /// returns true if the matrix is Symmetric Positive Definite (PD)
        /// A square matrix M of order n is said to be PSD if y'My >= 0 for all vectors y of dimension n, y != 0.
        /// </summary>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns>
        ///   <c>true</c> if the matrix is Positive Semi Definite (PSD); otherwise, <c>false</c>.
        /// </returns>
        public bool IsSymmetricPositiveDefinite(float epsilon = Matrix_.MATRIX_EPSILON)
        {
            // the matrix must be symmetric
            if (!IsSymmetric(epsilon))
                return false;

            // copy matrix
            var m = new MatrixX();
            m.SetData(numRows, numColumns, MATX_ALLOCA(numRows * numColumns));
            m = this;

            // being able to obtain Cholesky factors is both a necessary and sufficient condition for positive definiteness
            return m.Cholesky_Factor();
        }
        /// <summary>
        /// returns true if the matrix is Positive Semi Definite (PSD)
        /// A square matrix M of order n is said to be PSD if y'My >= 0 for all vectors y of dimension n, y != 0.
        /// </summary>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns>
        ///   <c>true</c> if the matrix is Positive Semi Definite (PSD); otherwise, <c>false</c>.
        /// </returns>
        public bool IsPositiveSemiDefinite(float epsilon = Matrix_.MATRIX_EPSILON)
        {
            // the matrix must be square
            if (!IsSquare())
                return false;

            // copy original matrix
            var m = new MatrixX();
            m.SetData(numRows, numColumns, MATX_ALLOCA(numRows * numColumns));
            m = this;

            // add transpose
            int i, j, k;
            for (i = 0; i < numRows; i++)
                for (j = 0; j < numColumns; j++)
                    m[i][j] += this[j][i];

            // test Positive Semi Definiteness with Gaussian pivot steps
            for (i = 0; i < numRows; i++)
            {
                for (j = i; j < numColumns; j++)
                {
                    if (m[j][j] < -epsilon)
                        return false;
                    if (m[j][j] > epsilon)
                        continue;
                    for (k = 0; k < numRows; k++)
                        if (MathX.Fabs(m[k][j]) > epsilon || MathX.Fabs(m[j][k]) > epsilon)
                            return false;
                }

                if (m[i][i] <= epsilon)
                    continue;

                var d = 1.0f / m[i][i];
                for (j = i + 1; j < numColumns; j++)
                {
                    var s = d * m[j][i];
                    m[j][i] = 0.0f;
                    for (k = i + 1; k < numRows; k++)
                        m[j][k] -= s * m[i][k];
                }
            }

            return true;
        }

        /// <summary>
        /// returns true if the matrix is Symmetric Positive Semi Definite (PSD)
        /// </summary>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns>
        ///   <c>true</c> if the matrix is Symmetric Positive Semi Definite (PSD); otherwise, <c>false</c>.
        /// </returns>
        public bool IsSymmetricPositiveSemiDefinite(float epsilon = Matrix_.MATRIX_EPSILON)
            // the matrix must be symmetric
            => IsSymmetric(epsilon) && IsPositiveSemiDefinite(epsilon);

        public float Trace()                                           // returns product of diagonal elements
        {
            Debug.Assert(numRows == numColumns);
            // sum of elements on the diagonal
            var trace = 0.0f;
            for (int i = 0; i < numRows; i++)
                trace += mat[i * numRows + i];
            return trace;
        }
        public unsafe float Determinant()                                     // returns determinant of matrix
        {
            Debug.Assert(numRows == numColumns);
            fixed (float* mat = &this.mat[0])
                switch (numRows)
                {
                    case 1: return mat[0];
                    case 2: return reinterpret.cast_mat2(mat).Determinant();
                    case 3: return reinterpret.cast_mat3(mat).Determinant();
                    case 4: return reinterpret.cast_mat4(mat).Determinant();
                    case 5: return reinterpret.cast_mat5(mat).Determinant();
                    case 6: return reinterpret.cast_mat6(mat).Determinant();
                    default: return DeterminantGeneric();
                }
        }
        public MatrixX Transpose()                                     // returns transpose
        {
            var m = new MatrixX();
            m.SetTempSize(numColumns, numRows);
            for (var i = 0; i < numRows; i++)
                for (var j = 0; j < numColumns; j++)
                    m.mat[j * m.numColumns + i] = mat[i * numColumns + j];
            return m;
        }
        public MatrixX TransposeSelf()                                            // transposes the matrix itself
        {
            this = Transpose();
            return this;
        }
        public MatrixX Inverse()                                           // returns the inverse ( m * m.Inverse() = identity )
        {
            var m = new MatrixX();
            m.SetTempSize(numRows, numColumns);
            Array.Copy(mat, m.mat, numRows * numColumns);
            var r = m.InverseSelf();
            Debug.Assert(r);
            return m;
        }
        public unsafe bool InverseSelf()                                            // returns false if determinant is zero
        {
            Debug.Assert(numRows == numColumns);
            fixed (float* mat = &this.mat[0])
                switch (numRows)
                {
                    case 1:
                        if (MathX.Fabs(mat[0]) < Matrix_.MATRIX_INVERSE_EPSILON)
                            return false;
                        mat[0] = 1.0f / mat[0];
                        return true;
                    case 2: return reinterpret.cast_mat2(mat).InverseSelf();
                    case 3: return reinterpret.cast_mat3(mat).InverseSelf();
                    case 4: return reinterpret.cast_mat4(mat).InverseSelf();
                    case 5: return reinterpret.cast_mat5(mat).InverseSelf();
                    case 6: return reinterpret.cast_mat6(mat).InverseSelf();
                    default: return InverseSelfGeneric();
                }
        }
        public MatrixX InverseFast()                                       // returns the inverse ( m * m.Inverse() = identity )
        {
            var m = new MatrixX();
            m.SetTempSize(numRows, numColumns);
            Array.Copy(mat, m.mat, numRows * numColumns);
            var r = m.InverseFastSelf();
            Debug.Assert(r);
            return m;
        }
        public unsafe bool InverseFastSelf()                                        // returns false if determinant is zero
        {
            Debug.Assert(numRows == numColumns);
            fixed (float* mat = &this.mat[0])
                switch (numRows)
                {
                    case 1:
                        if (MathX.Fabs(mat[0]) < Matrix_.MATRIX_INVERSE_EPSILON)
                            return false;
                        mat[0] = 1.0f / mat[0];
                        return true;
                    case 2: return reinterpret.cast_mat2(mat).InverseFastSelf();
                    case 3: return reinterpret.cast_mat3(mat).InverseFastSelf();
                    case 4: return reinterpret.cast_mat3(mat).InverseFastSelf();
                    case 5: return reinterpret.cast_mat5(mat).InverseFastSelf();
                    case 6: return reinterpret.cast_mat6(mat).InverseFastSelf();
                    default: break;
                }
            return InverseSelfGeneric();
        }

        /// <summary>
        /// in-place inversion of the lower triangular matrix
        /// </summary>
        /// <returns>false if determinant is zero</returns>
        public bool LowerTriangularInverse()
        {
            for (var i = 0; i < numRows; i++)
            {
                var d = this[i][i];
                if (d == 0.0f)
                    return false;
                this[i][i] = d = 1.0f / d;
                for (var j = 0; j < i; j++)
                {
                    var sum = 0.0f;
                    for (var k = j; k < i; k++)
                        sum -= this[i][k] * this[k][j];
                    this[i][j] = sum * d;
                }
            }
            return true;
        }
        /// <summary>
        /// in-place inversion of the upper triangular matrix
        /// </summary>
        /// <returns>false if determinant is zero</returns>
        public bool UpperTriangularInverse()
        {
            for (var i = numRows - 1; i >= 0; i--)
            {
                var d = this[i][i];
                if (d == 0.0f)
                    return false;
                this[i][i] = d = 1.0f / d;
                for (var j = numRows - 1; j > i; j--)
                {
                    var sum = 0.0f;
                    for (var k = j; k > i; k--)
                        sum -= this[i][k] * this[k][j];
                    this[i][j] = sum * d;
                }
            }
            return true;
        }

        public VectorX Multiply(VectorX vec)                           // this * vec
        {
            Debug.Assert(numColumns == vec.Size);
            var dst = new VectorX();
            dst.SetTempSize(numRows);
#if MATX_SIMD
            SIMDProcessor.MatX_MultiplyVecX(dst, *this, vec);
#else
            Multiply(dst, vec);
#endif
            return dst;
        }
        public VectorX TransposeMultiply(VectorX vec)                  // this.Transpose() * vec
        {
            Debug.Assert(numRows == vec.Size);
            var dst = new VectorX();
            dst.SetTempSize(numColumns);
#if MATX_SIMD
    SIMDProcessor.MatX_TransposeMultiplyVecX(dst, *this, vec);
#else
            TransposeMultiply(dst, vec);
#endif
            return dst;
        }

        public MatrixX Multiply(MatrixX a)                             // this * a
        {
            Debug.Assert(numColumns == a.numRows);
            var dst = new MatrixX();
            dst.SetTempSize(numRows, a.numColumns);
#if MATX_SIMD
            SIMDProcessor.MatX_MultiplyMatX(dst, *this, a);
#else
            Multiply(dst, a);
#endif
            return dst;
        }
        public MatrixX TransposeMultiply(MatrixX a)                        // this.Transpose() * a
        {
            Debug.Assert(numRows == a.numRows);
            var dst = new MatrixX();
            dst.SetTempSize(numColumns, a.numColumns);
#if MATX_SIMD
            SIMDProcessor.MatX_TransposeMultiplyMatX(dst, *this, a);
#else
            TransposeMultiply(dst, a);
#endif
            return dst;
        }

        public unsafe void Multiply(VectorX dst, VectorX vec)             // dst = this * vec
        {
#if MATX_SIMD
            SIMDProcessor.MatX_MultiplyVecX(dst, *this, vec);
#else
            fixed (float* mat = this.mat, dstp = dst.p, vecp = vec.p)
            {
                var mPtr = mat;
                var dstPtr = dstp + dst.pi;
                var vPtr = vecp + vec.pi;
                for (var i = 0; i < numRows; i++)
                {
                    var sum = mPtr[0] * vPtr[0];
                    for (var j = 1; j < numColumns; j++)
                        sum += mPtr[j] * vPtr[j];
                    dstPtr[i] = sum;
                    mPtr += numColumns;
                }
            }
#endif
        }
        public unsafe void MultiplyAdd(VectorX dst, VectorX vec)          // dst += this * vec
        {
#if MATX_SIMD
            SIMDProcessor.MatX_MultiplyAddVecX(dst, *this, vec);
#else
            fixed (float* mat = this.mat, dstp = dst.p, vecp = vec.p)
            {
                var mPtr = mat;
                var dstPtr = dstp + dst.pi;
                var vPtr = vecp + vec.pi;
                for (var i = 0; i < numRows; i++)
                {
                    var sum = mPtr[0] * vPtr[0];
                    for (var j = 1; j < numColumns; j++)
                        sum += mPtr[j] * vPtr[j];
                    dstPtr[i] += sum;
                    mPtr += numColumns;
                }
            }
#endif
        }
        public unsafe void MultiplySub(VectorX dst, VectorX vec)          // dst -= this * vec
        {
#if MATX_SIMD
            SIMDProcessor.MatX_MultiplySubVecX(dst, *this, vec);
#else
            fixed (float* mat = this.mat, dstp = dst.p, vecp = vec.p)
            {
                var mPtr = mat;
                var dstPtr = dstp + dst.pi;
                var vPtr = vecp + vec.pi;
                for (var i = 0; i < numRows; i++)
                {
                    var sum = mPtr[0] * vPtr[0];
                    for (var j = 1; j < numColumns; j++)
                        sum += mPtr[j] * vPtr[j];
                    dstPtr[i] -= sum;
                    mPtr += numColumns;
                }
            }
#endif
        }
        public unsafe void TransposeMultiply(VectorX dst, VectorX vec)        // dst = this.Transpose() * vec
        {
#if MATX_SIMD
            SIMDProcessor.MatX_TransposeMultiplyVecX(dst, *this, vec);
#else
            fixed (float* mat = this.mat, dstp = dst.p, vecp = vec.p)
            {
                var dstPtr = dstp + dst.pi;
                var vPtr = vecp + vec.pi;
                for (var i = 0; i < numColumns; i++)
                {
                    var mPtr = mat + i;
                    var sum = mPtr[0] * vPtr[0];
                    for (var j = 1; j < numRows; j++)
                    {
                        mPtr += numColumns;
                        sum += mPtr[0] * vPtr[j];
                    }
                    dstPtr[i] = sum;
                }
            }
#endif
        }
        public unsafe void TransposeMultiplyAdd(VectorX dst, VectorX vec) // dst += this.Transpose() * vec
        {
#if MATX_SIMD
            SIMDProcessor.MatX_TransposeMultiplyAddVecX(dst, *this, vec);
#else
            fixed (float* mat = this.mat, dstp = dst.p, vecp = vec.p)
            {
                var dstPtr = dstp + dst.pi;
                var vPtr = vecp + vec.pi;
                for (var i = 0; i < numColumns; i++)
                {
                    var mPtr = mat + i;
                    var sum = mPtr[0] * vPtr[0];
                    for (var j = 1; j < numRows; j++)
                    {
                        mPtr += numColumns;
                        sum += mPtr[0] * vPtr[j];
                    }
                    dstPtr[i] += sum;
                }
            }
#endif
        }
        public unsafe void TransposeMultiplySub(VectorX dst, VectorX vec) // dst -= this.Transpose() * vec
        {
#if MATX_SIMD
            SIMDProcessor.MatX_TransposeMultiplySubVecX(dst, *this, vec);
#else
            fixed (float* mat = this.mat, dstp = dst.p, vecp = vec.p)
            {
                var dstPtr = dstp + dst.pi;
                var vPtr = vecp + vec.pi;
                for (var i = 0; i < numColumns; i++)
                {
                    var mPtr = mat + i;
                    var sum = mPtr[0] * vPtr[0];
                    for (var j = 1; j < numRows; j++)
                    {
                        mPtr += numColumns;
                        sum += mPtr[0] * vPtr[j];
                    }
                    dstPtr[i] -= sum;
                }
            }
#endif
        }

        public unsafe void Multiply(MatrixX dst, MatrixX a)                   // dst = this * a
        {
#if MATX_SIMD
            SIMDProcessor.MatX_MultiplyMatX(dst, *this, a);
#else
            Debug.Assert(numColumns == a.numRows);
            fixed (float* dstp = dst.mat, mat = this.mat, amat = a.mat)
            {
                var dstPtr = dstp;
                var m1Ptr = mat;
                var m2Ptr = amat;
                var k = numRows;
                var l = a.NumColumns;

                for (var i = 0; i < k; i++)
                {
                    for (var j = 0; j < l; j++)
                    {
                        m2Ptr = amat + j;
                        var sum = m1Ptr[0] * m2Ptr[0];
                        for (var n = 1; n < numColumns; n++)
                        {
                            m2Ptr += l;
                            sum += m1Ptr[n] * m2Ptr[0];
                        }
                        *dstPtr++ = sum;
                    }
                    m1Ptr += numColumns;
                }
            }
#endif
        }
        public unsafe void TransposeMultiply(MatrixX dst, MatrixX a)      // dst = this.Transpose() * a
        {
#if MATX_SIMD
            SIMDProcessor.MatX_TransposeMultiplyMatX(dst, *this, a);
#else
            Debug.Assert(numRows == a.numRows);
            fixed (float* dstp = dst.mat, mat = this.mat, amat = a.mat)
            {
                var dstPtr = dstp;
                var m1Ptr = mat;
                var k = numColumns;
                var l = a.numColumns;

                for (var i = 0; i < k; i++)
                {
                    for (var j = 0; j < l; j++)
                    {
                        m1Ptr = mat + i;
                        var m2Ptr = amat + j;
                        var sum = m1Ptr[0] * m2Ptr[0];
                        for (var n = 1; n < numRows; n++)
                        {
                            m1Ptr += numColumns;
                            m2Ptr += a.numColumns;
                            sum += m1Ptr[0] * m2Ptr[0];
                        }
                        *dstPtr++ = sum;
                    }
                }
            }
#endif
        }

        public int Dimension                                      // returns total number of values in matrix
            => numRows * numColumns;

        public unsafe Vector6 SubVec6(int row)                                       // interpret beginning of row as a Vector6
        {
            Debug.Assert(numColumns >= 6 && row >= 0 && row < numRows);
            fixed (float* mat = &this.mat[0])
                return reinterpret.cast_vec6(mat, row * numColumns);
        }
        public VectorX SubVecX(int row)                                     // interpret complete row as a VectorX
        {
            Debug.Assert(row >= 0 && row < numRows);
            var v = new VectorX();
            v.SetData(numColumns, mat, row * numColumns);
            return v;
        }
        public unsafe T ToFloatPtr<T>(FloatPtr<T> callback)
        {
            fixed (float* array = mat)
                return callback(array);
        }
        public unsafe string ToString(int precision = 2)
        {
            var dimension = Dimension;
            return ToFloatPtr(array => StringX.FloatArrayToString(array, dimension, precision));
        }

        // UPDATES


        int numRows;                // number of rows
        int numColumns;             // number of columns
        int alloced;                // floats allocated, if -1 then mat points to data set with SetData
        float[] mat;                 // memory the matrix is stored

        static float[] temp;// = new float[MATX_MAX_TEMP + 4];   // used to store intermediate results
                            //static float[] tempPtr = temp; //(float*)(((intptr_t)idMatX::temp + 15) & ~15);              // pointer to 16 byte aligned temporary memory
        static int tempIndex = 0;               // index into memory pool, wraps around

        void SetTempSize(int rows, int columns)
        {
            var newSize = (rows * columns + 3) & ~3;
            Debug.Assert(newSize < MATX_MAX_TEMP);
            if (tempIndex + newSize > MATX_MAX_TEMP)
                tempIndex = 0;
            mat = new float[newSize]; // tempPtr + tempIndex;
            tempIndex += newSize;
            alloced = newSize;
            numRows = rows;
            numColumns = columns;
            MATX_CLEAREND();
        }
        float DeterminantGeneric()
        {
            var index = new int[numRows];
            var tmp = new MatrixX();
            tmp.SetData(numRows, numColumns, MATX_ALLOCA(numRows * numColumns));
            tmp = this;

            if (!tmp.LU_Factor(index, out var det))
                return 0.0f;

            return det;
        }
        bool InverseSelfGeneric()
        {
            var index = new int[numRows];
            var tmp = new MatrixX();
            tmp.SetData(numRows, numColumns, MATX_ALLOCA(numRows * numColumns));
            tmp = this;

            if (!tmp.LU_Factor(index, out var _))
                return false;
            VectorX x = new(), b = new();
            x.SetData(numRows, VectorX.VECX_ALLOCA(numRows));
            b.SetData(numRows, VectorX.VECX_ALLOCA(numRows));
            b.Zero();

            for (var i = 0; i < numRows; i++)
            {
                b[i] = 1.0f;
                tmp.LU_Solve(x, b, index);
                for (var j = 0; j < numRows; j++)
                    this[j][i] = x[j];
                b[i] = 0.0f;
            }
            return true;
        }
    }
}
