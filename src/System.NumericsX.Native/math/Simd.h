#ifndef __MATH_SIMD_H__
#define __MATH_SIMD_H__

#ifdef _WIN32
#define VPCALL __fastcall
#else
#define VPCALL
#endif

class Vector2;
class Vector3;
class Vector4;
//class Vector5;
//class Vector6;
class VectorX;
//class Matrix2x2;
class Matrix3x3;
//class Matrix4x4;
//class Matrix5x5;
//class Matrix6x6;
class MatrixX;
class Plane;
class DrawVert;
class JointQuat;
class JointMat;
struct DominantTri;

const int MIXBUFFER_SAMPLES = 4096;

#endif