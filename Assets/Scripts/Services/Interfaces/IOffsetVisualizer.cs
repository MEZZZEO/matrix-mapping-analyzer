using System.Collections.Generic;
using Core;
using Cysharp.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IOffsetVisualizer
    {
        UniTask VisualizeAsync(List<FoundOffset> offsets);
        void Clear();
    }
}