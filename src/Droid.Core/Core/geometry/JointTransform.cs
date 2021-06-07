namespace Droid.Core
{
    public struct JointQuat
    {
        public Quat q;
        public Vector3 t;
    }

    public class JointMat
    {
        readonly float[] mat = new float[3 * 4];

        public void SetRotation(Matrix3x3 m)
        {
            // NOTE: Matrix3 is transposed because it is column-major
            mat[0 * 4 + 0] = m[0].x;
            mat[0 * 4 + 1] = m[1].x;
            mat[0 * 4 + 2] = m[2].x;
            mat[1 * 4 + 0] = m[0].y;
            mat[1 * 4 + 1] = m[1].y;
            mat[1 * 4 + 2] = m[2].y;
            mat[2 * 4 + 0] = m[0].z;
            mat[2 * 4 + 1] = m[1].z;
            mat[2 * 4 + 2] = m[2].z;
        }

        public void SetTranslation(Vector3 t)
        {
            mat[0 * 4 + 3] = t.x;
            mat[1 * 4 + 3] = t.y;
            mat[2 * 4 + 3] = t.z;
        }

        public static Vector3 operator *(JointMat _, Vector3 v)                           // only rotate
            => new(
            _.mat[0 * 4 + 0] * v.x + _.mat[0 * 4 + 1] * v.y + _.mat[0 * 4 + 2] * v.z,
            _.mat[1 * 4 + 0] * v.x + _.mat[1 * 4 + 1] * v.y + _.mat[1 * 4 + 2] * v.z,
            _.mat[2 * 4 + 0] * v.x + _.mat[2 * 4 + 1] * v.y + _.mat[2 * 4 + 2] * v.z);
        public static Vector3 operator *(JointMat _, Vector4 v)                          // rotate and translate
            => new(
            _.mat[0 * 4 + 0] * v.x + _.mat[0 * 4 + 1] * v.y + _.mat[0 * 4 + 2] * v.z + _.mat[0 * 4 + 3] * v.w,
            _.mat[1 * 4 + 0] * v.x + _.mat[1 * 4 + 1] * v.y + _.mat[1 * 4 + 2] * v.z + _.mat[1 * 4 + 3] * v.w,
            _.mat[2 * 4 + 0] * v.x + _.mat[2 * 4 + 1] * v.y + _.mat[2 * 4 + 2] * v.z + _.mat[2 * 4 + 3] * v.w);
        public static JointMat operator *(JointMat _, JointMat a)                          // transform
        {
            float dst_0, dst_1, dst_2;

            dst_0 = _.mat[0 * 4 + 0] * a.mat[0 * 4 + 0] + _.mat[1 * 4 + 0] * a.mat[0 * 4 + 1] + _.mat[2 * 4 + 0] * a.mat[0 * 4 + 2];
            dst_1 = _.mat[0 * 4 + 0] * a.mat[1 * 4 + 0] + _.mat[1 * 4 + 0] * a.mat[1 * 4 + 1] + _.mat[2 * 4 + 0] * a.mat[1 * 4 + 2];
            dst_2 = _.mat[0 * 4 + 0] * a.mat[2 * 4 + 0] + _.mat[1 * 4 + 0] * a.mat[2 * 4 + 1] + _.mat[2 * 4 + 0] * a.mat[2 * 4 + 2];
            _.mat[0 * 4 + 0] = dst_0;
            _.mat[1 * 4 + 0] = dst_1;
            _.mat[2 * 4 + 0] = dst_2;

            dst_0 = _.mat[0 * 4 + 1] * a.mat[0 * 4 + 0] + _.mat[1 * 4 + 1] * a.mat[0 * 4 + 1] + _.mat[2 * 4 + 1] * a.mat[0 * 4 + 2];
            dst_1 = _.mat[0 * 4 + 1] * a.mat[1 * 4 + 0] + _.mat[1 * 4 + 1] * a.mat[1 * 4 + 1] + _.mat[2 * 4 + 1] * a.mat[1 * 4 + 2];
            dst_2 = _.mat[0 * 4 + 1] * a.mat[2 * 4 + 0] + _.mat[1 * 4 + 1] * a.mat[2 * 4 + 1] + _.mat[2 * 4 + 1] * a.mat[2 * 4 + 2];
            _.mat[0 * 4 + 1] = dst_0;
            _.mat[1 * 4 + 1] = dst_1;
            _.mat[2 * 4 + 1] = dst_2;

            dst_0 = _.mat[0 * 4 + 2] * a.mat[0 * 4 + 0] + _.mat[1 * 4 + 2] * a.mat[0 * 4 + 1] + _.mat[2 * 4 + 2] * a.mat[0 * 4 + 2];
            dst_1 = _.mat[0 * 4 + 2] * a.mat[1 * 4 + 0] + _.mat[1 * 4 + 2] * a.mat[1 * 4 + 1] + _.mat[2 * 4 + 2] * a.mat[1 * 4 + 2];
            dst_2 = _.mat[0 * 4 + 2] * a.mat[2 * 4 + 0] + _.mat[1 * 4 + 2] * a.mat[2 * 4 + 1] + _.mat[2 * 4 + 2] * a.mat[2 * 4 + 2];
            _.mat[0 * 4 + 2] = dst_0;
            _.mat[1 * 4 + 2] = dst_1;
            _.mat[2 * 4 + 2] = dst_2;

            dst_0 = _.mat[0 * 4 + 3] * a.mat[0 * 4 + 0] + _.mat[1 * 4 + 3] * a.mat[0 * 4 + 1] + _.mat[2 * 4 + 3] * a.mat[0 * 4 + 2];
            dst_1 = _.mat[0 * 4 + 3] * a.mat[1 * 4 + 0] + _.mat[1 * 4 + 3] * a.mat[1 * 4 + 1] + _.mat[2 * 4 + 3] * a.mat[1 * 4 + 2];
            dst_2 = _.mat[0 * 4 + 3] * a.mat[2 * 4 + 0] + _.mat[1 * 4 + 3] * a.mat[2 * 4 + 1] + _.mat[2 * 4 + 3] * a.mat[2 * 4 + 2];
            _.mat[0 * 4 + 3] = dst_0;
            _.mat[1 * 4 + 3] = dst_1;
            _.mat[2 * 4 + 3] = dst_2;

            _.mat[0 * 4 + 3] += a.mat[0 * 4 + 3];
            _.mat[1 * 4 + 3] += a.mat[1 * 4 + 3];
            _.mat[2 * 4 + 3] += a.mat[2 * 4 + 3];

            return _;
        }
        public static JointMat operator /(JointMat _, JointMat a)                          // untransform
        {
            float dst_0, dst_1, dst_2;

            _.mat[0 * 4 + 3] -= a.mat[0 * 4 + 3];
            _.mat[1 * 4 + 3] -= a.mat[1 * 4 + 3];
            _.mat[2 * 4 + 3] -= a.mat[2 * 4 + 3];

            dst_0 = _.mat[0 * 4 + 0] * a.mat[0 * 4 + 0] + _.mat[1 * 4 + 0] * a.mat[1 * 4 + 0] + _.mat[2 * 4 + 0] * a.mat[2 * 4 + 0];
            dst_1 = _.mat[0 * 4 + 0] * a.mat[0 * 4 + 1] + _.mat[1 * 4 + 0] * a.mat[1 * 4 + 1] + _.mat[2 * 4 + 0] * a.mat[2 * 4 + 1];
            dst_2 = _.mat[0 * 4 + 0] * a.mat[0 * 4 + 2] + _.mat[1 * 4 + 0] * a.mat[1 * 4 + 2] + _.mat[2 * 4 + 0] * a.mat[2 * 4 + 2];
            _.mat[0 * 4 + 0] = dst_0;
            _.mat[1 * 4 + 0] = dst_1;
            _.mat[2 * 4 + 0] = dst_2;

            dst_0 = _.mat[0 * 4 + 1] * a.mat[0 * 4 + 0] + _.mat[1 * 4 + 1] * a.mat[1 * 4 + 0] + _.mat[2 * 4 + 1] * a.mat[2 * 4 + 0];
            dst_1 = _.mat[0 * 4 + 1] * a.mat[0 * 4 + 1] + _.mat[1 * 4 + 1] * a.mat[1 * 4 + 1] + _.mat[2 * 4 + 1] * a.mat[2 * 4 + 1];
            dst_2 = _.mat[0 * 4 + 1] * a.mat[0 * 4 + 2] + _.mat[1 * 4 + 1] * a.mat[1 * 4 + 2] + _.mat[2 * 4 + 1] * a.mat[2 * 4 + 2];
            _.mat[0 * 4 + 1] = dst_0;
            _.mat[1 * 4 + 1] = dst_1;
            _.mat[2 * 4 + 1] = dst_2;

            dst_0 = _.mat[0 * 4 + 2] * a.mat[0 * 4 + 0] + _.mat[1 * 4 + 2] * a.mat[1 * 4 + 0] + _.mat[2 * 4 + 2] * a.mat[2 * 4 + 0];
            dst_1 = _.mat[0 * 4 + 2] * a.mat[0 * 4 + 1] + _.mat[1 * 4 + 2] * a.mat[1 * 4 + 1] + _.mat[2 * 4 + 2] * a.mat[2 * 4 + 1];
            dst_2 = _.mat[0 * 4 + 2] * a.mat[0 * 4 + 2] + _.mat[1 * 4 + 2] * a.mat[1 * 4 + 2] + _.mat[2 * 4 + 2] * a.mat[2 * 4 + 2];
            _.mat[0 * 4 + 2] = dst_0;
            _.mat[1 * 4 + 2] = dst_1;
            _.mat[2 * 4 + 2] = dst_2;

            dst_0 = _.mat[0 * 4 + 3] * a.mat[0 * 4 + 0] + _.mat[1 * 4 + 3] * a.mat[1 * 4 + 0] + _.mat[2 * 4 + 3] * a.mat[2 * 4 + 0];
            dst_1 = _.mat[0 * 4 + 3] * a.mat[0 * 4 + 1] + _.mat[1 * 4 + 3] * a.mat[1 * 4 + 1] + _.mat[2 * 4 + 3] * a.mat[2 * 4 + 1];
            dst_2 = _.mat[0 * 4 + 3] * a.mat[0 * 4 + 2] + _.mat[1 * 4 + 3] * a.mat[1 * 4 + 2] + _.mat[2 * 4 + 3] * a.mat[2 * 4 + 2];
            _.mat[0 * 4 + 3] = dst_0;
            _.mat[1 * 4 + 3] = dst_1;
            _.mat[2 * 4 + 3] = dst_2;

            return _;
        }

        public bool Compare(JointMat a)                       // exact compare, no epsilon
        {
            for (var i = 0; i < 12; i++)
                if (mat[i] != a.mat[i])
                    return false;
            return true;
        }
        public bool Compare(JointMat a, float epsilon)  // compare with epsilon
        {
            for (var i = 0; i < 12; i++)
                if (MathX.Fabs(mat[i] - a.mat[i]) > epsilon)
                    return false;
            return true;
        }
        public static bool operator ==(JointMat _, JointMat a)                   // exact compare, no epsilon
            => _.Compare(a);
        public static bool operator !=(JointMat _, JointMat a)                   // exact compare, no epsilon
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is JointMat q && Compare(q);
        public override int GetHashCode()
            => mat.GetHashCode();

        public void Invert()
        {
            float tmp_0, tmp_1, tmp_2;

            // negate inverse rotated translation part
            tmp_0 = mat[0 * 4 + 0] * mat[0 * 4 + 3] + mat[1 * 4 + 0] * mat[1 * 4 + 3] + mat[2 * 4 + 0] * mat[2 * 4 + 3];
            tmp_1 = mat[0 * 4 + 1] * mat[0 * 4 + 3] + mat[1 * 4 + 1] * mat[1 * 4 + 3] + mat[2 * 4 + 1] * mat[2 * 4 + 3];
            tmp_2 = mat[0 * 4 + 2] * mat[0 * 4 + 3] + mat[1 * 4 + 2] * mat[1 * 4 + 3] + mat[2 * 4 + 2] * mat[2 * 4 + 3];
            mat[0 * 4 + 3] = -tmp_0;
            mat[1 * 4 + 3] = -tmp_1;
            mat[2 * 4 + 3] = -tmp_2;

            // transpose rotation part
            tmp_0 = mat[0 * 4 + 1];
            mat[0 * 4 + 1] = mat[1 * 4 + 0];
            mat[1 * 4 + 0] = tmp_0;
            tmp_1 = mat[0 * 4 + 2];
            mat[0 * 4 + 2] = mat[2 * 4 + 0];
            mat[2 * 4 + 0] = tmp_1;
            tmp_2 = mat[1 * 4 + 2];
            mat[1 * 4 + 2] = mat[2 * 4 + 1];
            mat[2 * 4 + 1] = tmp_2;
        }

        public Matrix3x3 ToMat3()
            => new(
            mat[0 * 4 + 0], mat[1 * 4 + 0], mat[2 * 4 + 0],
            mat[0 * 4 + 1], mat[1 * 4 + 1], mat[2 * 4 + 1],
            mat[0 * 4 + 2], mat[1 * 4 + 2], mat[2 * 4 + 2]);

        public Vector3 ToVec3()
            => new(mat[0 * 4 + 3], mat[1 * 4 + 3], mat[2 * 4 + 3]);

        static readonly int[] ToJointQuat_next = { 1, 2, 0 };
        public JointQuat ToJointQuat()
        {
            int i, j, k; float trace, s, t; JointQuat jq = new();

            trace = mat[0 * 4 + 0] + mat[1 * 4 + 1] + mat[2 * 4 + 2];

            if (trace > 0f)
            {
                t = trace + 1f;
                s = MathX.InvSqrt(t) * 0.5f;

                jq.q[3] = s * t;
                jq.q[0] = (mat[1 * 4 + 2] - mat[2 * 4 + 1]) * s;
                jq.q[1] = (mat[2 * 4 + 0] - mat[0 * 4 + 2]) * s;
                jq.q[2] = (mat[0 * 4 + 1] - mat[1 * 4 + 0]) * s;

            }
            else
            {
                i = 0;
                if (mat[1 * 4 + 1] > mat[0 * 4 + 0])
                    i = 1;
                if (mat[2 * 4 + 2] > mat[i * 4 + i])
                    i = 2;
                j = ToJointQuat_next[i];
                k = ToJointQuat_next[j];

                t = (mat[i * 4 + i] - (mat[j * 4 + j] + mat[k * 4 + k])) + 1f;
                s = MathX.InvSqrt(t) * 0.5f;

                jq.q[i] = s * t;
                jq.q[3] = (mat[j * 4 + k] - mat[k * 4 + j]) * s;
                jq.q[j] = (mat[i * 4 + j] + mat[j * 4 + i]) * s;
                jq.q[k] = (mat[i * 4 + k] + mat[k * 4 + i]) * s;
            }

            jq.t[0] = mat[0 * 4 + 3];
            jq.t[1] = mat[1 * 4 + 3];
            jq.t[2] = mat[2 * 4 + 3];

            return jq;
        }

        public unsafe T ToFloatPtr<T>(FloatPtr<T> callback)
        {
            fixed (float* _ = mat)
                return callback(_);
        }
    }
}