using System.Collections.Generic;
using UnityEngine;

namespace Services
{
    /// <summary>
    /// Утилиты для валидации и диагностики матричных данных
    /// </summary>
    public static class MatrixValidationUtils
    {
        /// <summary>
        /// Проверяет, является ли матрица вырожденной (det = 0)
        /// </summary>
        public static bool IsSingular(Matrix4x4 matrix, float tolerance = 1e-6f)
        {
            return Mathf.Abs(matrix.determinant) < tolerance;
        }

        /// <summary>
        /// Проверяет список матриц на вырожденность
        /// </summary>
        public static List<int> FindSingularMatrices(List<Matrix4x4> matrices, float tolerance = 1e-6f)
        {
            var singularIndices = new List<int>();
            
            for (int i = 0; i < matrices.Count; i++)
            {
                if (IsSingular(matrices[i], tolerance))
                {
                    singularIndices.Add(i);
                }
            }

            return singularIndices;
        }

        /// <summary>
        /// Проверяет, есть ли дубликаты в списке матриц
        /// </summary>
        public static List<(int, int)> FindDuplicates(List<Matrix4x4> matrices, float absTol = 1e-5f, float relTol = 1e-5f)
        {
            var duplicates = new List<(int, int)>();

            for (int i = 0; i < matrices.Count; i++)
            {
                for (int j = i + 1; j < matrices.Count; j++)
                {
                    if (ToleranceUtils.NearlyEqual(matrices[i], matrices[j], absTol, relTol))
                    {
                        duplicates.Add((i, j));
                    }
                }
            }

            return duplicates;
        }

        /// <summary>
        /// Выводит статистику по списку матриц
        /// </summary>
        public static void LogMatrixStatistics(string name, List<Matrix4x4> matrices, float tolerance = 1e-5f)
        {
            Debug.Log($"=== Matrix Statistics: {name} ===");
            Debug.Log($"Total count: {matrices.Count}");

            var singularIndices = FindSingularMatrices(matrices, tolerance);
            if (singularIndices.Count > 0)
            {
                Debug.LogWarning($"Singular matrices found: {singularIndices.Count} (indices: {string.Join(", ", singularIndices)})");
            }
            else
            {
                Debug.Log("✅ All matrices are non-singular");
            }

            var duplicates = FindDuplicates(matrices, tolerance, tolerance);
            if (duplicates.Count > 0)
            {
                Debug.LogWarning($"Duplicate matrices found: {duplicates.Count} pairs");
                if (duplicates.Count <= 10)
                {
                    foreach (var (i, j) in duplicates)
                    {
                        Debug.Log($"  Duplicates: [{i}] ≈ [{j}]");
                    }
                }
            }
            else
            {
                Debug.Log("✅ No duplicates found");
            }

            // Статистика определителей
            float minDet = float.MaxValue;
            float maxDet = float.MinValue;
            float sumDet = 0f;

            foreach (var m in matrices)
            {
                float det = m.determinant;
                minDet = Mathf.Min(minDet, det);
                maxDet = Mathf.Max(maxDet, det);
                sumDet += det;
            }

            float avgDet = sumDet / matrices.Count;
            Debug.Log($"Determinants: min={minDet:F6}, max={maxDet:F6}, avg={avgDet:F6}");
            Debug.Log("==================");
        }

        /// <summary>
        /// Проверяет, может ли существовать offset для данных model и space
        /// Быстрая эвристическая проверка без полного поиска
        /// </summary>
        public static bool QuickFeasibilityCheck(List<Matrix4x4> model, List<Matrix4x4> space)
        {
            if (model == null || model.Count == 0)
            {
                Debug.LogError("Model is empty!");
                return false;
            }

            if (space == null || space.Count == 0)
            {
                Debug.LogError("Space is empty!");
                return false;
            }

            if (space.Count < model.Count)
            {
                Debug.LogWarning($"Space ({space.Count}) has fewer matrices than Model ({model.Count}). " +
                                 "Offsets may not exist or be very rare.");
            }

            // Проверка на вырожденность первой матрицы модели (нужна для вычисления offset)
            if (IsSingular(model[0]))
            {
                Debug.LogError("First matrix in Model is singular (det ≈ 0). Cannot compute offset!");
                return false;
            }

            Debug.Log($"✅ Quick feasibility check passed: Model={model.Count}, Space={space.Count}");
            return true;
        }
    }
}