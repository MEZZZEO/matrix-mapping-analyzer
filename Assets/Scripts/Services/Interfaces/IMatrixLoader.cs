using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Services.Interfaces
{
    public interface IMatrixLoader
    {
        UniTask<List<Matrix4x4>> LoadAsync(string path);
    }
}