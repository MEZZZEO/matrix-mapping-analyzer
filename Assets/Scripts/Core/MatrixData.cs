using System;
using UnityEngine;

namespace Core
{
    [Serializable]
    public class MatrixData
    {
        public float m00, m01, m02, m03;
        public float m10, m11, m12, m13;
        public float m20, m21, m22, m23;
        public float m30, m31, m32, m33;

        public Matrix4x4 ToMatrix4x4()
        {
            var matrix = new Matrix4x4
            {
                m00 = m00, m01 = m01, m02 = m02, m03 = m03,
                m10 = m10, m11 = m11, m12 = m12, m13 = m13,
                m20 = m20, m21 = m21, m22 = m22, m23 = m23,
                m30 = m30, m31 = m31, m32 = m32, m33 = m33
            };
            return matrix;
        }

        public static MatrixData FromMatrix4x4(Matrix4x4 m)
        {
            return new MatrixData
            {
                m00 = m.m00, m01 = m.m01, m02 = m.m02, m03 = m.m03,
                m10 = m.m10, m11 = m.m11, m12 = m.m12, m13 = m.m13,
                m20 = m.m20, m21 = m.m21, m22 = m.m22, m23 = m.m23,
                m30 = m.m30, m31 = m.m31, m32 = m.m32, m33 = m.m33
            };
        }
    }
}