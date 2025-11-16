using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Config;
using Cysharp.Threading.Tasks;
using Core;
using Extensions;
using Services.Interfaces;
using UnityEngine;
using Utils;

namespace Services
{
    /// <summary>
    /// Параллельный калькулятор для поиска смещений матриц
    /// Реализует условие: ∀m∈Model, offset⋅m∈Space
    /// </summary>
    public class ParallelOffsetCalculator : IOffsetCalculator
    {
        private readonly int _batchSize;
        private readonly int _maxDegreeOfParallelism;

        public ParallelOffsetCalculator(OffsetFinderConfig config)
        {
            _batchSize = config.BatchSize;
            _maxDegreeOfParallelism = Mathf.Clamp(config.MaxThreads, 1, SystemInfo.processorCount);
        }

        public async UniTask<List<FoundOffset>> FindOffsetsAsync(Matrices model, Matrices space,
            float tolerance, IProgress<int> progress, CancellationToken ct, Action<FoundOffset> onOffsetFound = null)
        {
            var results = new List<FoundOffset>();
            var firstModelInverse = model[0].Original.inverse;
            var processed = 0;

            var digits = ToleranceUtils.DigitsFromTolerance(tolerance);

            for (int batchStart = 0; batchStart < space.Count; batchStart += _batchSize)
            {
                if (ct.IsCancellationRequested)
                    break;

                var batchEnd = Mathf.Min(batchStart + _batchSize, space.Count);

                var batchResults = await UniTask.RunOnThreadPool(() =>
                {
                    var localResults = new List<FoundOffset>();

                    Parallel.For(batchStart, batchEnd, new ParallelOptions { CancellationToken = ct, MaxDegreeOfParallelism = _maxDegreeOfParallelism }, sIdx =>
                    {
                        var candidateOffset = space[sIdx].Original * firstModelInverse;

                        if (ValidateOffset(candidateOffset, model, space, digits))
                        {
                            lock (localResults)
                            {
                                localResults.Add(new FoundOffset(candidateOffset, sIdx));
                            }
                        }
                    });

                    return localResults;
                }, cancellationToken: ct);

                results.AddRange(batchResults);

                foreach (var offset in batchResults)
                {
                    onOffsetFound?.Invoke(offset);
                }

                processed += (batchEnd - batchStart);
                progress?.Report(processed);

                await UniTask.Yield();
            }
            
            return results;
        }
        
        private static bool ValidateOffset(Matrix4x4 offset, Matrices model, Matrices space, int digits)
        {
            foreach (var m in model)
            {
                var transformed = offset * m.Original;
                var foundInSpace = space.Contains(transformed.Round(digits).GetHash());

                if (!foundInSpace)
                {
                    return false;
                }
            }
            
            return true;
        }
    }
}