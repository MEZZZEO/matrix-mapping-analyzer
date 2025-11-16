using System.Runtime.InteropServices;
using UnityEngine;
using Utils;

namespace Extensions
{
    /// <summary>
    /// Методы расширения для работы с матрицами
    /// </summary>
    public static class MatrixExtensions
    {
        /// <summary>
        /// Вычисляет хеш матрицы на основе её бинарного представления
        /// </summary>
        public static uint GetHash(this Matrix4x4 matrix)
        {
            int byteCount = Marshal.SizeOf(typeof(float)) * 16;
            byte[] buffer = new byte[byteCount];

            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            
            try
            {
                Marshal.StructureToPtr(matrix, handle.AddrOfPinnedObject(), false);
            }
            finally
            {
                handle.Free();
            }
            
            return SuperFastHash.Hash(buffer);
        }

        /// <summary>
        /// Округляет все элементы матрицы до указанного количества знаков после запятой
        /// </summary>
        public static Matrix4x4 Round(this Matrix4x4 matrix, int digits)
        {
            var rounded = new Matrix4x4();

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    rounded[i, j] = ToleranceUtils.RoundToDigits(matrix[i, j], digits);
                }
            }

            return rounded;
        }
    }
}