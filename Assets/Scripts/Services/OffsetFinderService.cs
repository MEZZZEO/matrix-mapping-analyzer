using System;
using System.Collections.Generic;
using System.Threading;
using Config;
using Core;
using Cysharp.Threading.Tasks;
using Model;
using Services.Interfaces;
using UnityEngine;
using Utils;
using VContainer;

namespace Services
{
    public class OffsetFinderService : IDisposable
    {
        [Inject] private readonly IOffsetFinderModel _model;
        [Inject] private readonly IMatrixLoader _loader;
        [Inject] private readonly IOffsetCalculator _calculator;
        [Inject] private readonly IResultExporter _exporter;
        [Inject] private readonly IOffsetVisualizer _visualizer;
        [Inject] private readonly OffsetFinderConfig _config;

        private List<FoundOffset> _lastResults;
        private CancellationTokenSource _cts;

        public async UniTask FindOffsetsAsync(Matrices modelMatrices, Matrices spaceMatrices)
        {
            _model.Reset();
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            _model.SetProcessing(true);
            var startTime = Time.realtimeSinceStartup;

            try
            {
                _model.SendStatus("Валидация данных...");
                MatrixValidationUtils.LogMatrixSetStatistics("Model", modelMatrices, _config.Tolerance);
                MatrixValidationUtils.LogMatrixSetStatistics("Space", spaceMatrices, _config.Tolerance);
                
                if (!MatrixValidationUtils.QuickFeasibilityCheck(modelMatrices, spaceMatrices, _config.Tolerance))
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

                if (_config.EnableDiagnostics)
                {
                    Debug.Log($"=== Диагностика кандидатов (не найденных смещений) ===");
                    var maxDiag = Mathf.Min(_config.MaxCandidatesToDiagnose, spaceMatrices.Count);
                    for (int i = 0; i < maxDiag; i++)
                    {
                        var candidateOffset = spaceMatrices[i].Original * modelMatrices[0].Original.inverse;
                        MatrixValidationUtils.DiagnoseOffset($"cand[{i}]", candidateOffset, modelMatrices, spaceMatrices, _config.Tolerance, 5);
                    }
                }

                if (_config.DiagnoseFoundOffsets && _lastResults.Count > 0)
                {
                    Debug.Log($"=== Диагностика найденных смещений ===");
                    var maxToCheck = Mathf.Min(3, _lastResults.Count);
                    for (int i = 0; i < maxToCheck; i++)
                    {
                        var foundOffset = _lastResults[i];
                        MatrixValidationUtils.DiagnoseOffset($"found[{i}] (spaceIdx={foundOffset.SpaceIndex})", 
                            foundOffset.Matrix, modelMatrices, spaceMatrices, _config.Tolerance, 3);
                    }
                }

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

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}