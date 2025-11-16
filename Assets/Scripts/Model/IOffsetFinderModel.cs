using System;
using Core;
using R3;

namespace Model
{
    public interface IOffsetFinderModel : IDisposable
    {
        ReadOnlyReactiveProperty<int> TotalCandidates { get; }
        ReadOnlyReactiveProperty<int> ProcessedCandidates { get; }
        ReadOnlyReactiveProperty<int> FoundOffsets { get; }
        ReadOnlyReactiveProperty<bool> IsProcessing { get; }
        ReadOnlyReactiveProperty<float> ElapsedTime { get; }

        Observable<FoundOffset> OnOffsetFound { get; }
        Observable<string> OnStatusMessage { get; }

        void SetTotalCandidates(int count);
        void SetProcessedCandidates(int count);
        void NotifyOffsetFound(FoundOffset offset);
        void SetProcessing(bool state);
        void SetElapsedTime(float time);
        void SendStatus(string message);
        void Reset();
    }
}