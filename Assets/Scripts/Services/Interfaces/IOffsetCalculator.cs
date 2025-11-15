using System;
using System.Collections.Generic;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Services.Interfaces
{
    public interface IOffsetCalculator
    {
        UniTask<List<FoundOffset>> FindOffsetsAsync(
            List<Matrix4x4> model,
            List<Matrix4x4> space,
            float tolerance,
            IProgress<int> progress,
            CancellationToken ct,
            Action<FoundOffset> onOffsetFound = null);
    }
}