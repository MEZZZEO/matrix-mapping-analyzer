using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Core;
using Services.Interfaces;
using UnityEngine;

namespace Services
{
    public class ParallelOffsetCalculator : IOffsetCalculator
    {
        private const int BatchSize = 50;

        public async UniTask<List<FoundOffset>> FindOffsetsAsync(List<Matrix4x4> model, List<Matrix4x4> space,
            float tolerance, IProgress<int> progress, CancellationToken ct, Action<FoundOffset> onOffsetFound = null)
        {
            var results = new List<FoundOffset>();
            var firstModelInverse = model[0].inverse;
            var processed = 0;

            var absTolerance = tolerance;
            var relTolerance = tolerance;

            for (int batchStart = 0; batchStart < space.Count; batchStart += BatchSize)
            {
                if (ct.IsCancellationRequested)
                    break;

                var batchEnd = Mathf.Min(batchStart + BatchSize, space.Count);

                var batchResults = await UniTask.RunOnThreadPool(() =>
                {
                    var localResults = new List<FoundOffset>();

                    Parallel.For(batchStart, batchEnd, new ParallelOptions { CancellationToken = ct }, sIdx =>
                    {
                        var candidateOffset = space[sIdx] * firstModelInverse;

                        if (ValidateOffset(candidateOffset, model, space, absTolerance, relTolerance))
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
        
        private static bool ValidateOffset(Matrix4x4 offset, List<Matrix4x4> model, List<Matrix4x4> space, 
            float absTolerance, float relTolerance)
        {
            foreach (var m in model)
            {
                var transformed = offset * m;
                var foundInSpace = space.Any(s => ToleranceUtils.NearlyEqual(transformed, s, absTolerance, relTolerance));

                if (!foundInSpace)
                {
                    return false;
                }
            }
            
            return true;
        }
    }
}