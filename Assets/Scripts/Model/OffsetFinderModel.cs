using Core;
using R3;

namespace Model
{
    public class OffsetFinderModel : IOffsetFinderModel
    {
        private readonly ReactiveProperty<int> _totalCandidates = new(0);
        private readonly ReactiveProperty<int> _processedCandidates = new(0);
        private readonly ReactiveProperty<int> _foundOffsets = new(0);
        private readonly ReactiveProperty<bool> _isProcessing = new(false);
        private readonly ReactiveProperty<float> _elapsedTime = new(0f);

        private readonly Subject<FoundOffset> _offsetFound = new();
        private readonly Subject<string> _statusMessage = new();

        private bool _disposed;

        public ReadOnlyReactiveProperty<int> TotalCandidates => _totalCandidates;
        public ReadOnlyReactiveProperty<int> ProcessedCandidates => _processedCandidates;
        public ReadOnlyReactiveProperty<int> FoundOffsets => _foundOffsets;
        public ReadOnlyReactiveProperty<bool> IsProcessing => _isProcessing;
        public ReadOnlyReactiveProperty<float> ElapsedTime => _elapsedTime;

        public Observable<FoundOffset> OnOffsetFound => _offsetFound;
        public Observable<string> OnStatusMessage => _statusMessage;

        public void SetTotalCandidates(int count) => _totalCandidates.Value = count;
        public void SetProcessedCandidates(int count) => _processedCandidates.Value = count;

        public void NotifyOffsetFound(FoundOffset offset)
        {
            _foundOffsets.Value++;
            _offsetFound.OnNext(offset);
        }

        public void SetProcessing(bool state) => _isProcessing.Value = state;
        public void SetElapsedTime(float time) => _elapsedTime.Value = time;
        public void SendStatus(string message) => _statusMessage.OnNext(message);

        public void Reset()
        {
            _totalCandidates.Value = 0;
            _processedCandidates.Value = 0;
            _foundOffsets.Value = 0;
            _isProcessing.Value = false;
            _elapsedTime.Value = 0f;
        }

        public void Dispose()
        {
            if (_disposed) return;

            _totalCandidates?.Dispose();
            _processedCandidates?.Dispose();
            _foundOffsets?.Dispose();
            _isProcessing?.Dispose();
            _elapsedTime?.Dispose();

            _offsetFound?.Dispose();
            _statusMessage?.Dispose();

            _disposed = true;
        }
    }
}