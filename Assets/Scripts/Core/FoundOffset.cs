using System;
using UnityEngine;

namespace Core
{
    [Serializable]
    public struct FoundOffset
    {
        public Matrix4x4 Matrix;
        public int SpaceIndex;

        public FoundOffset(Matrix4x4 m, int sIdx)
        {
            Matrix = m;
            SpaceIndex = sIdx;
        }
    }
}