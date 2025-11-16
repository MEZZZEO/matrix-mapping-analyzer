using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Core;
using Services.Interfaces;
using UnityEngine;

namespace Services
{
    public class JsonMatrixLoader : IMatrixLoader
    {
        public async UniTask<List<Matrix4x4>> LoadAsync(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"Файл не найден: {path}");
                return new List<Matrix4x4>();
            }

            var json = await File.ReadAllTextAsync(path);
            var dataArray = JsonConvert.DeserializeObject<MatrixData[]>(json);

            var matrices = new List<Matrix4x4>(dataArray.Length);
            matrices.AddRange(dataArray.Select(data => data.ToMatrix4x4()));

            Debug.Log($"Загружено {matrices.Count} матриц из {path}");
            return matrices;
        }
    }
}