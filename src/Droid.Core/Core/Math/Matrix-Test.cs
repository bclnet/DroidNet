//#define MATX_SIMD
using System;
using System.Diagnostics;

namespace Droid.Core
{
    partial struct MatrixX
    {
        public static void Test()
        {
            MatrixX original = new(), m1 = new(), m2 = new(), m3 = new(), q1 = new(), q2 = new(), r1 = new(), r2 = new();
            VectorX v = new(), w = new(), u = new(), c = new(), d = new();
            int offset, size;

            size = 6;
            original.Random(size, size, 0);
            original *= original.Transpose();

            var index1 = new int[size + 1];
            var index2 = new int[size + 1];

            //  MatrixX::LowerTriangularInverse

            m1 = original;
            m1.ClearUpperTriangle();
            m2 = m1;

            m2.InverseSelf();
            m1.LowerTriangularInverse();

            if (!m1.Compare(m2, 1e-4f))
                throw new InvalidOperationException("MatrixX::LowerTriangularInverse failed");

            //  MatrixX::UpperTriangularInverse

            m1 = original;
            m1.ClearLowerTriangle();
            m2 = m1;

            m2.InverseSelf();
            m1.UpperTriangularInverse();

            if (!m1.Compare(m2, 1e-4f))
                throw new InvalidOperationException("MatrixX::UpperTriangularInverse failed");

            //    MatrixX::Inverse_GaussJordan

            m1 = original;

            m1.Inverse_GaussJordan();
            m1 *= original;

            if (!m1.IsIdentity(1e-4f))
                throw new InvalidOperationException("MatrixX::Inverse_GaussJordan failed");

            //    MatrixX::Inverse_UpdateRankOne

            m1 = original;
            m2 = original;

            w.Random(size, 1);
            v.Random(size, 2);

            // invert m1
            m1.Inverse_GaussJordan();

            // modify and invert m2
            m2.Update_RankOne(v, w, 1.0f);
            if (!m2.Inverse_GaussJordan())
                Debug.Assert(false);

            // update inverse of m1
            m1.Inverse_UpdateRankOne(v, w, 1.0f);

            if (!m1.Compare(m2, 1e-4f))
                throw new InvalidOperationException("MatrixX::Inverse_UpdateRankOne failed");

            // MatrixX::Inverse_UpdateRowColumn

            for (offset = 0; offset < size; offset++)
            {
                m1 = original;
                m2 = original;

                v.Random(size, 1);
                w.Random(size, 2);
                w[offset] = 0.0f;

                // invert m1
                m1.Inverse_GaussJordan();

                // modify and invert m2
                m2.Update_RowColumn(v, w, offset);
                if (!m2.Inverse_GaussJordan())
                    Debug.Assert(false);

                // update inverse of m1
                m1.Inverse_UpdateRowColumn(v, w, offset);

                if (!m1.Compare(m2, 1e-3f))
                    throw new InvalidOperationException("MatrixX::Inverse_UpdateRowColumn failed");
            }

            //    MatrixX::Inverse_UpdateIncrement

            m1 = original;
            m2 = original;

            v.Random(size + 1, 1);
            w.Random(size + 1, 2);
            w[size] = 0.0f;

            // invert m1
            m1.Inverse_GaussJordan();

            // modify and invert m2
            m2.Update_Increment(v, w);
            if (!m2.Inverse_GaussJordan())
                Debug.Assert(false);

            // update inverse of m1
            m1.Inverse_UpdateIncrement(v, w);

            if (!m1.Compare(m2, 1e-4f))
                throw new InvalidOperationException("MatrixX::Inverse_UpdateIncrement failed");

            //    MatrixX::Inverse_UpdateDecrement

            for (offset = 0; offset < size; offset++)
            {
                m1 = original;
                m2 = original;

                v.SetSize(6);
                w.SetSize(6);
                for (var i = 0; i < size; i++)
                {
                    v[i] = original[i][offset];
                    w[i] = original[offset][i];
                }

                // invert m1
                m1.Inverse_GaussJordan();

                // modify and invert m2
                m2.Update_Decrement(offset);
                if (!m2.Inverse_GaussJordan())
                    Debug.Assert(false);

                // update inverse of m1
                m1.Inverse_UpdateDecrement(v, w, offset);

                if (!m1.Compare(m2, 1e-3f))
                    throw new InvalidOperationException("MatrixX::Inverse_UpdateDecrement failed");
            }

            //    MatrixX::LU_Factor

            m1 = original;

            m1.LU_Factor(null, out var _); // no pivoting
            m1.LU_UnpackFactors(m2, m3);
            m1 = m2 * m3;

            if (!original.Compare(m1, 1e-4f))
                throw new InvalidOperationException("MatrixX::LU_Factor failed");

            //    MatrixX::LU_UpdateRankOne

            m1 = original;
            m2 = original;

            w.Random(size, 1);
            v.Random(size, 2);

            // factor m1
            m1.LU_Factor(index1, out var _);

            // modify and factor m2
            m2.Update_RankOne(v, w, 1.0f);
            if (!m2.LU_Factor(index2, out var _))
                Debug.Assert(false);
            m2.LU_MultiplyFactors(m3, index2);
            m2 = m3;

            // update factored m1
            m1.LU_UpdateRankOne(v, w, 1.0f, index1);
            m1.LU_MultiplyFactors(m3, index1);
            m1 = m3;

            if (!m1.Compare(m2, 1e-4f))
                throw new InvalidOperationException("MatrixX::LU_UpdateRankOne failed");

            //    MatrixX::LU_UpdateRowColumn

            for (offset = 0; offset < size; offset++)
            {
                m1 = original;
                m2 = original;

                v.Random(size, 1);
                w.Random(size, 2);
                w[offset] = 0.0f;

                // factor m1
                m1.LU_Factor(index1, out var _);

                // modify and factor m2
                m2.Update_RowColumn(v, w, offset);
                if (!m2.LU_Factor(index2, out var _))
                    Debug.Assert(false);
                m2.LU_MultiplyFactors(m3, index2);
                m2 = m3;

                // update m1
                m1.LU_UpdateRowColumn(v, w, offset, index1);
                m1.LU_MultiplyFactors(m3, index1);
                m1 = m3;

                if (!m1.Compare(m2, 1e-3f))
                    throw new InvalidOperationException("MatrixX::LU_UpdateRowColumn failed");
            }

            //    MatrixX::LU_UpdateIncrement

            m1 = original;
            m2 = original;

            v.Random(size + 1, 1);
            w.Random(size + 1, 2);
            w[size] = 0.0f;

            // factor m1
            m1.LU_Factor(index1, out var _);

            // modify and factor m2
            m2.Update_Increment(v, w);
            if (!m2.LU_Factor(index2, out var _))
                Debug.Assert(false);
            m2.LU_MultiplyFactors(m3, index2);
            m2 = m3;

            // update factored m1
            m1.LU_UpdateIncrement(v, w, index1);
            m1.LU_MultiplyFactors(m3, index1);
            m1 = m3;

            if (!m1.Compare(m2, 1e-4f))
                throw new InvalidOperationException("MatrixX::LU_UpdateIncrement failed");

            //    MatrixX::LU_UpdateDecrement

            for (offset = 0; offset < size; offset++)
            {
                m1 = original;
                m2 = original;

                v.SetSize(6);
                w.SetSize(6);
                for (int i = 0; i < size; i++)
                {
                    v[i] = original[i][offset];
                    w[i] = original[offset][i];
                }

                // factor m1
                m1.LU_Factor(index1, out var _);

                // modify and factor m2
                m2.Update_Decrement(offset);
                if (!m2.LU_Factor(index2, out var _))
                    Debug.Assert(false);
                m2.LU_MultiplyFactors(m3, index2);
                m2 = m3;

                u.SetSize(6);
                for (int i = 0; i < size; i++)
                    u[i] = original[index1[offset]][i];

                // update factors of m1
                m1.LU_UpdateDecrement(v, w, u, offset, index1);
                m1.LU_MultiplyFactors(m3, index1);
                m1 = m3;

                if (!m1.Compare(m2, 1e-3f))
                    throw new InvalidOperationException("MatrixX::LU_UpdateDecrement failed");
            }

            //    MatrixX::LU_Inverse

            m2 = original;

            m2.LU_Factor(null, out var _);
            m2.LU_Inverse(m1, null);
            m1 *= original;

            if (!m1.IsIdentity(1e-4f))
                throw new InvalidOperationException("MatrixX::LU_Inverse failed");

            //    MatrixX::QR_Factor

            c.SetSize(size);
            d.SetSize(size);

            m1 = original;

            m1.QR_Factor(c, d);
            m1.QR_UnpackFactors(q1, r1, c, d);
            m1 = q1 * r1;

            if (!original.Compare(m1, 1e-4f))
                throw new InvalidOperationException("MatrixX::QR_Factor failed");

            //    MatrixX::QR_UpdateRankOne

            c.SetSize(size);
            d.SetSize(size);

            m1 = original;
            m2 = original;

            w.Random(size, 0);
            v = w;

            // factor m1
            m1.QR_Factor(c, d);
            m1.QR_UnpackFactors(q1, r1, c, d);

            // modify and factor m2
            m2.Update_RankOne(v, w, 1.0f);
            if (!m2.QR_Factor(c, d))
                Debug.Assert(false);
            m2.QR_UnpackFactors(q2, r2, c, d);
            m2 = q2 * r2;

            // update factored m1
            q1.QR_UpdateRankOne(r1, v, w, 1.0f);
            m1 = q1 * r1;

            if (!m1.Compare(m2, 1e-4f))
                throw new InvalidOperationException("MatrixX::QR_UpdateRankOne failed");

            //    MatrixX::QR_UpdateRowColumn

            for (offset = 0; offset < size; offset++)
            {
                c.SetSize(size);
                d.SetSize(size);

                m1 = original;
                m2 = original;

                v.Random(size, 1);
                w.Random(size, 2);
                w[offset] = 0.0f;

                // factor m1
                m1.QR_Factor(c, d);
                m1.QR_UnpackFactors(q1, r1, c, d);

                // modify and factor m2
                m2.Update_RowColumn(v, w, offset);
                if (!m2.QR_Factor(c, d))
                    Debug.Assert(false);
                m2.QR_UnpackFactors(q2, r2, c, d);
                m2 = q2 * r2;

                // update m1
                q1.QR_UpdateRowColumn(r1, v, w, offset);
                m1 = q1 * r1;

                if (!m1.Compare(m2, 1e-3f))
                    throw new InvalidOperationException("MatrixX::QR_UpdateRowColumn failed");
            }

            //    MatrixX::QR_UpdateIncrement

            c.SetSize(size + 1);
            d.SetSize(size + 1);

            m1 = original;
            m2 = original;

            v.Random(size + 1, 1);
            w.Random(size + 1, 2);
            w[size] = 0.0f;

            // factor m1
            m1.QR_Factor(c, d);
            m1.QR_UnpackFactors(q1, r1, c, d);

            // modify and factor m2
            m2.Update_Increment(v, w);
            if (!m2.QR_Factor(c, d))
                Debug.Assert(false);
            m2.QR_UnpackFactors(q2, r2, c, d);
            m2 = q2 * r2;

            // update factored m1
            q1.QR_UpdateIncrement(r1, v, w);
            m1 = q1 * r1;

            if (!m1.Compare(m2, 1e-4f))
                throw new InvalidOperationException("MatrixX::QR_UpdateIncrement failed");

            //    MatrixX::QR_UpdateDecrement

            for (offset = 0; offset < size; offset++)
            {
                c.SetSize(size + 1);
                d.SetSize(size + 1);

                m1 = original;
                m2 = original;

                v.SetSize(6);
                w.SetSize(6);
                for (int i = 0; i < size; i++)
                {
                    v[i] = original[i][offset];
                    w[i] = original[offset][i];
                }

                // factor m1
                m1.QR_Factor(c, d);
                m1.QR_UnpackFactors(q1, r1, c, d);

                // modify and factor m2
                m2.Update_Decrement(offset);
                if (!m2.QR_Factor(c, d))
                    Debug.Assert(false);
                m2.QR_UnpackFactors(q2, r2, c, d);
                m2 = q2 * r2;

                // update factors of m1
                q1.QR_UpdateDecrement(r1, v, w, offset);
                m1 = q1 * r1;

                if (!m1.Compare(m2, 1e-3f))
                    throw new InvalidOperationException("MatrixX::QR_UpdateDecrement failed");
            }

            //    MatrixX::QR_Inverse

            m2 = original;

            m2.QR_Factor(c, d);
            m2.QR_Inverse(m1, c, d);
            m1 *= original;

            if (!m1.IsIdentity(1e-4f))
                throw new InvalidOperationException("MatrixX::QR_Inverse failed");

            //    MatrixX::SVD_Factor

            m1 = original;
            m3.Zero(size, size);
            w.Zero(size);

            m1.SVD_Factor(w, m3);
            m2.Diag(w);
            m3.TransposeSelf();
            m1 = m1 * m2 * m3;

            if (!original.Compare(m1, 1e-4f))
                throw new InvalidOperationException("MatrixX::SVD_Factor failed");

            //    MatrixX::SVD_Inverse

            m2 = original;

            m2.SVD_Factor(w, m3);
            m2.SVD_Inverse(m1, w, m3);
            m1 *= original;

            if (!m1.IsIdentity(1e-4f))
                throw new InvalidOperationException("MatrixX::SVD_Inverse failed");

            //    MatrixX::Cholesky_Factor

            m1 = original;

            m1.Cholesky_Factor();
            m1.Cholesky_MultiplyFactors(m2);

            if (!original.Compare(m2, 1e-4f))
                throw new InvalidOperationException("MatrixX::Cholesky_Factor failed");

            //    MatrixX::Cholesky_UpdateRankOne

            m1 = original;
            m2 = original;

            w.Random(size, 0);

            // factor m1
            m1.Cholesky_Factor();
            m1.ClearUpperTriangle();

            // modify and factor m2
            m2.Update_RankOneSymmetric(w, 1.0f);
            if (!m2.Cholesky_Factor())
                Debug.Assert(false);
            m2.ClearUpperTriangle();

            // update factored m1
            m1.Cholesky_UpdateRankOne(w, 1.0f, 0);

            if (!m1.Compare(m2, 1e-4f))
                throw new InvalidOperationException("MatrixX::Cholesky_UpdateRankOne failed");

            //    MatrixX::Cholesky_UpdateRowColumn

            for (offset = 0; offset < size; offset++)
            {
                m1 = original;
                m2 = original;

                // factor m1
                m1.Cholesky_Factor();
                m1.ClearUpperTriangle();

                int[] pdtable = { 1, 0, 1, 0, 0, 0 };
                w.Random(size, pdtable[offset]);
                w *= 0.1f;

                // modify and factor m2
                m2.Update_RowColumnSymmetric(w, offset);
                if (!m2.Cholesky_Factor())
                    Debug.Assert(false);
                m2.ClearUpperTriangle();

                // update m1
                m1.Cholesky_UpdateRowColumn(w, offset);

                if (!m1.Compare(m2, 1e-3f))
                    throw new InvalidOperationException("MatrixX::Cholesky_UpdateRowColumn failed");
            }

            //    MatrixX::Cholesky_UpdateIncrement

            m1.Random(size + 1, size + 1, 0);
            m3 = m1 * m1.Transpose();

            m1.SquareSubMatrix(m3, size);
            m2 = m1;

            w.SetSize(size + 1);
            for (var i = 0; i < size + 1; i++)
                w[i] = m3[size][i];

            // factor m1
            m1.Cholesky_Factor();

            // modify and factor m2
            m2.Update_IncrementSymmetric(w);
            if (!m2.Cholesky_Factor())
                Debug.Assert(false);

            // update factored m1
            m1.Cholesky_UpdateIncrement(w);

            m1.ClearUpperTriangle();
            m2.ClearUpperTriangle();

            if (!m1.Compare(m2, 1e-4f))
                throw new InvalidOperationException("MatrixX::Cholesky_UpdateIncrement failed");

            //    MatrixX::Cholesky_UpdateDecrement

            for (offset = 0; offset < size; offset += size - 1)
            {
                m1 = original;
                m2 = original;

                v.SetSize(6);
                for (var i = 0; i < size; i++)
                    v[i] = original[i][offset];

                // factor m1
                m1.Cholesky_Factor();

                // modify and factor m2
                m2.Update_Decrement(offset);
                if (!m2.Cholesky_Factor())
                    Debug.Assert(false);

                // update factors of m1
                m1.Cholesky_UpdateDecrement(v, offset);

                if (!m1.Compare(m2, 1e-3f))
                    throw new InvalidOperationException("MatrixX::Cholesky_UpdateDecrement failed");
            }

            //    MatrixX::Cholesky_Inverse

            m2 = original;

            m2.Cholesky_Factor();
            m2.Cholesky_Inverse(m1);
            m1 *= original;

            if (!m1.IsIdentity(1e-4f))
                throw new InvalidOperationException("MatrixX::Cholesky_Inverse failed");

            //    MatrixX::LDLT_Factor

            m1 = original;

            m1.LDLT_Factor();
            m1.LDLT_MultiplyFactors(m2);

            if (!original.Compare(m2, 1e-4f))
                throw new InvalidOperationException("MatrixX::LDLT_Factor failed");

            m1.LDLT_UnpackFactors(m2, m3);
            m2 = m2 * m3 * m2.Transpose();

            if (!original.Compare(m2, 1e-4f))
                throw new InvalidOperationException("MatrixX::LDLT_Factor failed");

            //    MatrixX::LDLT_UpdateRankOne

            m1 = original;
            m2 = original;

            w.Random(size, 0);

            // factor m1
            m1.LDLT_Factor();
            m1.ClearUpperTriangle();

            // modify and factor m2
            m2.Update_RankOneSymmetric(w, 1.0f);
            if (!m2.LDLT_Factor())
            {
                Debug.Assert(false);
            }
            m2.ClearUpperTriangle();

            // update factored m1
            m1.LDLT_UpdateRankOne(w, 1.0f, 0);

            if (!m1.Compare(m2, 1e-4f))
                throw new InvalidOperationException("MatrixX::LDLT_UpdateRankOne failed");

            //    MatrixX::LDLT_UpdateRowColumn

            for (offset = 0; offset < size; offset++)
            {
                m1 = original;
                m2 = original;

                w.Random(size, 0);

                // factor m1
                m1.LDLT_Factor();
                m1.ClearUpperTriangle();

                // modify and factor m2
                m2.Update_RowColumnSymmetric(w, offset);
                if (!m2.LDLT_Factor())
                {
                    Debug.Assert(false);
                }
                m2.ClearUpperTriangle();

                // update m1
                m1.LDLT_UpdateRowColumn(w, offset);

                if (!m1.Compare(m2, 1e-3f))
                    throw new InvalidOperationException("MatrixX::LDLT_UpdateRowColumn failed");
            }

            //    MatrixX::LDLT_UpdateIncrement

            m1.Random(size + 1, size + 1, 0);
            m3 = m1 * m1.Transpose();

            m1.SquareSubMatrix(m3, size);
            m2 = m1;

            w.SetSize(size + 1);
            for (var i = 0; i < size + 1; i++)
                w[i] = m3[size][i];

            // factor m1
            m1.LDLT_Factor();

            // modify and factor m2
            m2.Update_IncrementSymmetric(w);
            if (!m2.LDLT_Factor())
                Debug.Assert(false);

            // update factored m1
            m1.LDLT_UpdateIncrement(w);

            m1.ClearUpperTriangle();
            m2.ClearUpperTriangle();

            if (!m1.Compare(m2, 1e-4f))
                throw new InvalidOperationException("MatrixX::LDLT_UpdateIncrement failed");

            //    MatrixX::LDLT_UpdateDecrement

            for (offset = 0; offset < size; offset++)
            {
                m1 = original;
                m2 = original;

                v.SetSize(6);
                for (var i = 0; i < size; i++)
                    v[i] = original[i][offset];

                // factor m1
                m1.LDLT_Factor();

                // modify and factor m2
                m2.Update_Decrement(offset);
                if (!m2.LDLT_Factor())
                    Debug.Assert(false);

                // update factors of m1
                m1.LDLT_UpdateDecrement(v, offset);

                if (!m1.Compare(m2, 1e-3f))
                    throw new InvalidOperationException("MatrixX::LDLT_UpdateDecrement failed");
            }

            //    MatrixX::LDLT_Inverse

            m2 = original;

            m2.LDLT_Factor();
            m2.LDLT_Inverse(m1);
            m1 *= original;

            if (!m1.IsIdentity(1e-4f))
                throw new InvalidOperationException("MatrixX::LDLT_Inverse failed");

            //    MatrixX::Eigen_SolveSymmetricTriDiagonal

            m3 = original;
            m3.TriDiagonal_ClearTriangles();
            m1 = m3;

            v.SetSize(size);

            m1.Eigen_SolveSymmetricTriDiagonal(v);

            m3.TransposeMultiply(m2, m1);

            for (var i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    m1[i][j] *= v[j];

            if (!m1.Compare(m2, 1e-4f))
                throw new InvalidOperationException("MatrixX::Eigen_SolveSymmetricTriDiagonal failed");

            //    MatrixX::Eigen_SolveSymmetric

            m3 = original;
            m1 = m3;

            v.SetSize(size);

            m1.Eigen_SolveSymmetric(v);

            m3.TransposeMultiply(m2, m1);

            for (var i = 0; i < size; i++)
                for (var j = 0; j < size; j++)
                    m1[i][j] *= v[j];

            if (!m1.Compare(m2, 1e-4f))
                throw new InvalidOperationException("MatrixX::Eigen_SolveSymmetric failed");

            //    MatrixX::Eigen_Solve

            m3 = original;
            m1 = m3;

            v.SetSize(size);
            w.SetSize(size);

            m1.Eigen_Solve(v, w);

            m3.TransposeMultiply(m2, m1);

            for (var i = 0; i < size; i++)
                for (var j = 0; j < size; j++)
                    m1[i][j] *= v[j];

            if (!m1.Compare(m2, 1e-4f))
                throw new InvalidOperationException("MatrixX::Eigen_Solve failed");
        }
    }
}