using System.Collections.Generic;
using Core;
using Cysharp.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IResultExporter
    {
        UniTask ExportAsync(string path, List<FoundOffset> offsets);
    }
}