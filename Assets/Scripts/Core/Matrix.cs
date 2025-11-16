using Extensions;
using UnityEngine;

namespace Core
{
    public class Matrix
    {
        public readonly Matrix4x4 Original;
        public readonly Matrix4x4 Rounded;
        public readonly ulong Hash;

        public Matrix(Matrix4x4 matrix, int digits)
        {
            Original = matrix;
            Rounded = Original.Round(digits);
            Hash = Rounded.GetHash();
        }
    }
}