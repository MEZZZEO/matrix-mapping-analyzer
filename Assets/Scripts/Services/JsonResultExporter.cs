using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Services.Interfaces;
using UnityEngine;

namespace Services
{
    public class JsonResultExporter : IResultExporter
    {
        public async UniTask ExportAsync(string path, List<FoundOffset> offsets)
        {
            var matrixDataList = offsets
                .Select(o => MatrixData.FromMatrix4x4(o.Matrix))
                .ToList();

            var json = JsonConvert.SerializeObject(matrixDataList, Formatting.Indented);
            await File.WriteAllTextAsync(path, json);

            Debug.Log($"Экспортировано {offsets.Count} offset-матриц в {path}");
        }
    }
}