using Core;
using Cysharp.Threading.Tasks;
using Model;
using Presenters;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace View
{
    public class OffsetFinderUIController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button _buttonFind;
        [SerializeField] private Button _buttonCancel;
        [SerializeField] private Button _buttonVisualize;
        [SerializeField] private Button _buttonExport;
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private TextMeshProUGUI _txtStatus;
        [SerializeField] private TextMeshProUGUI _txtProgress;
        [SerializeField] private TextMeshProUGUI _txtFoundCount;
        [SerializeField] private TextMeshProUGUI _txtElapsedTime;
        [SerializeField] private Transform _resultsContainer;
        [SerializeField] private GameObject _resultItemPrefab;

        [Inject] private OffsetFinderModel _model;
        [Inject] private OffsetFinderService _service;

        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            BindButtons();
            BindModelToUI();
            UpdateButtonStates(false);
        }

        private void BindButtons()
        {
            _buttonFind.onClick.AddListener(() => OnFindClickedAsync().Forget());
            _buttonCancel.onClick.AddListener(OnCancelClicked);
            _buttonVisualize.onClick.AddListener(() => OnVisualizeClickedAsync().Forget());
            _buttonExport.onClick.AddListener(() => OnExportClickedAsync().Forget());
        }

        private void BindModelToUI()
        {
            _model.ProcessedCandidates
                .CombineLatest(_model.TotalCandidates, (p, t) => (processed: p, total: t))
                .Subscribe(x =>
                {
                    var progress = x.total > 0 ? (float)x.processed / x.total : 0f;
                    _progressSlider.value = progress;
                    _txtProgress.text = $"{x.processed} / {x.total} ({progress * 100f:F1}%)";
                })
                .AddTo(_disposables);

            _model.OnStatusMessage
                .Subscribe(msg => _txtStatus.text = msg)
                .AddTo(_disposables);

            _model.FoundOffsets
                .Subscribe(count => _txtFoundCount.text = $"Найдено: {count}")
                .AddTo(_disposables);

            _model.ElapsedTime
                .Subscribe(time => _txtElapsedTime.text = $"Время: {time:F2} с")
                .AddTo(_disposables);

            _model.IsProcessing
                .Subscribe(UpdateButtonStates)
                .AddTo(_disposables);

            _model.OnOffsetFound
                .Subscribe(AddOffsetToList)
                .AddTo(_disposables);
        }

        private async UniTaskVoid OnFindClickedAsync()
        {
            ClearResults();
            
            await _service.FindOffsetsAsync();
        }

        private void OnCancelClicked()
        {
            _service.CancelOperation();
        }

        private async UniTaskVoid OnVisualizeClickedAsync()
        {
            await _service.VisualizeResultsAsync();
        }

        private async UniTaskVoid OnExportClickedAsync()
        {
            await _service.ExportResultsAsync();
            
            _model.SendStatus("Результаты экспортированы");
        }

        private void UpdateButtonStates(bool isProcessing)
        {
            _buttonFind.interactable = !isProcessing;
            _buttonCancel.interactable = isProcessing;
            _buttonVisualize.interactable = !isProcessing && _model.FoundOffsets.CurrentValue > 0;
            _buttonExport.interactable = !isProcessing && _model.FoundOffsets.CurrentValue > 0;
        }

        private void AddOffsetToList(FoundOffset offset)
        {
            if (_resultItemPrefab == null) return;

            var item = Instantiate(_resultItemPrefab, _resultsContainer);
            var text = item.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = $"Offset #{_model.FoundOffsets.CurrentValue} → space[{offset.SpaceIndex}]";
            }
        }

        private void ClearResults()
        {
            foreach (Transform child in _resultsContainer)
            {
                Destroy(child.gameObject);
            }
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}