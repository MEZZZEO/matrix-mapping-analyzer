using System;
using System.Collections.Generic;
using System.Threading;
using Config;
using Core;
using Cysharp.Threading.Tasks;
using Model;
using Services.Interfaces;
using UnityEngine;
using VContainer;

namespace Presenters
{
    public class OffsetFinderService
    {
        [Inject] private readonly OffsetFinderModel _model;
        [Inject] private readonly IMatrixLoader _loader;
        [Inject] private readonly IOffsetCalculator _calculator;
        [Inject] private readonly IResultExporter _exporter;
        [Inject] private readonly IOffsetVisualizer _visualizer;
        [Inject] private readonly OffsetFinderConfig _config;

        private List<FoundOffset> _lastResults;
        private CancellationTokenSource _cts;

        public async UniTask FindOffsetsAsync()
        {
            _model.Reset();
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            _model.SetProcessing(true);
            var startTime = Time.realtimeSinceStartup;

            try
            {
                _model.SendStatus("Загрузка model.json...");
                var modelMatrices = await _loader.LoadAsync(_config.ModelJsonPath);

                _model.SendStatus("Загрузка space.json...");
                var spaceMatrices = await _loader.LoadAsync(_config.SpaceJsonPath);

                _model.SendStatus("Валидация данных...");
                Services.MatrixValidationUtils.LogMatrixStatistics("Model", modelMatrices, _config.Tolerance);
                Services.MatrixValidationUtils.LogMatrixStatistics("Space", spaceMatrices, _config.Tolerance);
                
                if (!Services.MatrixValidationUtils.QuickFeasibilityCheck(modelMatrices, spaceMatrices))
                {
                    _model.SendStatus("Данные могут быть некорректными. Проверьте логи.");
                }

                _model.SetTotalCandidates(spaceMatrices.Count);
                _model.SendStatus("Поиск смещений матриц...");

                var progress = new Progress<int>(count => _model.SetProcessedCandidates(count));

                _lastResults = await _calculator.FindOffsetsAsync(
                    modelMatrices,
                    spaceMatrices,
                    _config.Tolerance,
                    progress,
                    _cts.Token,
                    _model.NotifyOffsetFound
                );

                _model.SendStatus($"Поиск завершён. Найдено: {_lastResults.Count}");
            }
            catch (OperationCanceledException)
            {
                _model.SendStatus("Операция отменена");
            }
            catch (Exception ex)
            {
                _model.SendStatus($"Ошибка: {ex.Message}");
                Debug.LogError(ex);
            }
            finally
            {
                var elapsed = Time.realtimeSinceStartup - startTime;
                _model.SetElapsedTime(elapsed);
                _model.SetProcessing(false);
            }
        }

        public void CancelOperation()
        {
            _cts?.Cancel();
        }

        public async UniTask VisualizeResultsAsync()
        {
            if (_lastResults == null || _lastResults.Count == 0)
            {
                _model.SendStatus("Нет результатов для визуализации");
                return;
            }

            _model.SendStatus("Визуализация...");
            await _visualizer.VisualizeAsync(_lastResults);
            _model.SendStatus($"Визуализировано {_lastResults.Count} групп объектов");
        }

        public async UniTask ExportResultsAsync()
        {
            if (_lastResults == null || _lastResults.Count == 0)
            {
                _model.SendStatus("Нет результатов для экспорта");
                return;
            }

            await _exporter.ExportAsync(_config.OutputJsonPath, _lastResults);
        }
    }
}