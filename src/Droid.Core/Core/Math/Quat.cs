using System;

namespace Droid.Core
{
    public struct Quat
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Quat(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public void Set(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public unsafe float this[int index]
        {
            get
            {
                fixed (float* p = &x)
                    return p[index];
            }
            set
            {
                fixed (float* p = &x)
                    p[index] = value;
            }
        }

        public static Quat operator -(Quat _)
            => new(-_.x, -_.y, -_.z, -_.w);
        public static Quat operator +(Quat _, Quat a)
            => new(_.x + a.x, _.y + a.y, _.z + a.z, _.w + a.w);
        public static Quat operator -(Quat _, Quat a)
            => new(_.x - a.x, _.y - a.y, _.z - a.z, _.w - a.w);
        public static Quat operator *(Quat _, Quat a)
            => new(
                _.w * a.x + _.x * a.w + _.y * a.z - _.z * a.y,
                _.w * a.y + _.y * a.w + _.z * a.x - _.x * a.z,
                _.w * a.z + _.z * a.w + _.x * a.y - _.y * a.x,
                _.w * a.w - _.x * a.x - _.y * a.y - _.z * a.z);

        public static Vector3 operator *(Quat _, Vector3 a)
        {
#if false
            // it's faster to do the conversion to a 3x3 matrix and multiply the vector by this 3x3 matrix
            return ToMat3() * a;
#else
            var xxzz = _.x * _.x - _.z * _.z;
            var wwyy = _.w * _.w - _.y * _.y;

            var xw2 = _.x * _.w * 2.0f;
            var xy2 = _.x * _.y * 2.0f;
            var xz2 = _.x * _.z * 2.0f;
            var yw2 = _.y * _.w * 2.0f;
            var yz2 = _.y * _.z * 2.0f;
            var zw2 = _.z * _.w * 2.0f;

            return new(
                (xxzz + wwyy) * a.x + (xy2 + zw2) * a.y + (xz2 - yw2) * a.z,
                (xy2 - zw2) * a.x + (_.y * _.y + _.w * _.w - _.x * _.x - _.z * _.z) * a.y + (yz2 + xw2) * a.z,
                (xz2 + yw2) * a.x + (yz2 - xw2) * a.y + (wwyy - xxzz) * a.z);
#endif
        }
        public static Quat operator *(Quat _, float a)
            => new(_.x * a, _.y * a, _.z * a, _.w * a);

        public static Quat operator *(float a, Quat b)
            => b * a;
        public static Vector3 operator *(Vector3 a, Quat b)
            => b * a;

        public bool Compare(Quat a)                       // exact compare, no epsilon
            => (x == a.x) && (y == a.y) && (z == a.z) && (w == a.w);
        public bool Compare(Quat a, float epsilon)  // compare with epsilon
        {
            if (MathX.Fabs(x - a.x) > epsilon) return false;
            if (MathX.Fabs(y - a.y) > epsilon) return false;
            if (MathX.Fabs(z - a.z) > epsilon) return false;
            if (MathX.Fabs(w - a.w) > epsilon) return false;
            return true;
        }
        public static bool operator ==(Quat _, Quat a)                   // exact compare, no epsilon
            => _.Compare(a);
        public static bool operator !=(Quat _, Quat a)                   // exact compare, no epsilon
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Quat q && Compare(q);
        public override int GetHashCode()
            => x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ w.GetHashCode();

        public Quat Inverse()
            => new(-x, -y, -z, w);
        public float Length
        {
            get
            {
                var len = x * x + y * y + z * z + w * w;
                return MathX.Sqrt(len);
            }
        }
        public Quat Normalize()
        {
            var len = Length;
            if (len != 0)
            {
                var ilength = 1 / len;
                x *= ilength;
                y *= ilength;
                z *= ilength;
                w *= ilength;
            }
            return this;
        }

        public float CalcW()
            // take the absolute value because floating point rounding may cause the dot of x,y,z to be larger than 1
            => (float)Math.Sqrt((float)Math.Abs(1.0f - (x * x + y * y + z * z)));
        public static int Dimension
            => 4;

        public Angles ToAngles()
            => ToMat3().ToAngles();
        public Rotation ToRotation()
        {
            var vec = new Vector3 { x = x, y = y, z = z };
            var angle = MathX.ACos(w);
            if (angle == 0.0f)
                vec.Set(0.0f, 0.0f, 1.0f);
            else
            {
                vec.Normalize();
                vec.FixDegenerateNormal();
                angle *= 2.0f * MathX.M_RAD2DEG;
            }
            return new(Vector3.origin, vec, angle);
        }
        public Matrix3x3 ToMat3()
        {
            Matrix3x3 mat;

            float x2 = x + x, y2 = y + y, z2 = z + z;
            float xx = x * x2, xy = x * y2, xz = x * z2;
            float yy = y * y2, yz = y * z2, zz = z * z2;
            float wx = w * x2, wy = w * y2, wz = w * z2;

            mat.mat0.x = 1.0f - (yy + zz);
            mat.mat0.y = xy - wz;
            mat.mat0.z = xz + wy;

            mat.mat1.x = xy + wz;
            mat.mat1.y = 1.0f - (xx + zz);
            mat.mat1.z = yz - wx;

            mat.mat2.x = xz - wy;
            mat.mat2.y = yz + wx;
            mat.mat2.z = 1.0f - (xx + yy);

            return mat;
        }
        public Matrix4x4 ToMat4()
            => ToMat3().ToMat4();
        public CQuat ToCQuat()
            => w < 0.0f ? new CQuat(-x, -y, -z) : new CQuat(x, y, z);
        public Vector3 ToAngularVelocity()
        {
            var vec = new Vector3
            {
                x = x,
                y = y,
                z = z
            };
            vec.Normalize();
            return vec * MathX.ACos(w);
        }
        public unsafe T ToFloatPtr<T>(FloatPtr<T> callback)
        {
            fixed (float* p = &x)
                return callback(p);
        }
        public unsafe string ToString(int precision = 2)
            => ToFloatPtr(array => StringX.FloatArrayToString(array, Dimension, precision));

        /// <summary>
        /// Spherical linear interpolation between two quaternions.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public Quat Slerp(Quat from, Quat to, float t)
        {
            Quat temp;
            float omega, cosom, sinom, scale0, scale1;
            if (t <= 0.0f) { this = from; return this; }
            if (t >= 1.0f) { this = to; return this; }
            if (from == to) { this = to; return this; }
            cosom = from.x * to.x + from.y * to.y + from.z * to.z + from.w * to.w;
            if (cosom < 0.0f) { temp = -to; cosom = -cosom; }
            else temp = to;

            if ((1.0f - cosom) > 1e-6f)
            {
#if false
                omega = acos(cosom);
                sinom = 1.0f / sin(omega);
                scale0 = sin((1.0f - t) * omega) * sinom;
                scale1 = sin(t * omega) * sinom;
#else
                scale0 = 1.0f - cosom * cosom;
                sinom = MathX.InvSqrt(scale0);
                omega = MathX.ATan16(scale0 * sinom, cosom);
                scale0 = MathX.Sin16((1.0f - t) * omega) * sinom;
                scale1 = MathX.Sin16(t * omega) * sinom;
#endif
            }
            else { scale0 = 1.0f - t; scale1 = t; }

            this = (scale0 * from) + (scale1 * temp);
            return this;
        }
    }

    public struct CQuat
    {
        public float x;
        public float y;
        public float z;

        public CQuat(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public void Set(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public unsafe float this[int index]
        {
            get
            {
                fixed (float* p = &x)
                    return p[index];
            }
            set
            {
                fixed (float* p = &x)
                    p[index] = value;
            }
        }

        public bool Compare(CQuat a)                     // exact compare, no epsilon
            => (x == a.x) && (y == a.y) && (z == a.z);
        public bool Compare(CQuat a, float epsilon)    // compare with epsilon
        {
            if (MathX.Fabs(x - a.x) > epsilon) return false;
            if (MathX.Fabs(y - a.y) > epsilon) return false;
            if (MathX.Fabs(z - a.z) > epsilon) return false;
            return true;
        }
        public static bool operator ==(CQuat _, CQuat a)                 // exact compare, no epsilon
            => _.Compare(a);
        public static bool operator !=(CQuat _, CQuat a)                 // exact compare, no epsilon
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is CQuat q && Compare(q);
        public override int GetHashCode()
            => x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();

        public static int Dimension
            => 3;

        public Angles ToAngles()
            => ToQuat().ToAngles();
        public Rotation ToRotation()
            => ToQuat().ToRotation();
        public Matrix3x3 ToMat3()
            => ToQuat().ToMat3();
        public Matrix4x4 ToMat4()
            => ToQuat().ToMat4();
        public Quat ToQuat()
            // take the absolute value because floating point rounding may cause the dot of x,y,z to be larger than 1
            => new(x, y, z, (float)Math.Sqrt((float)Math.Abs(1.0f - (x * x + y * y + z * z))));
        public unsafe T ToFloatPtr<T>(FloatPtr<T> callback)
        {
            fixed (float* p = &x)
                return callback(p);
        }
        public unsafe string ToString(int precision = 2)
            => ToFloatPtr(array => StringX.FloatArrayToString(array, Dimension, precision));
    }
}