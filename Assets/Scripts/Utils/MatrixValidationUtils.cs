using System.Collections.Generic;
using System.Linq;
using Core;
using Extensions;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Утилиты для валидации и диагностики матричных данных
    /// </summary>
    public static class MatrixValidationUtils
    {
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
        /// Выводит статистику по списку матриц с учетом tol/digits и хеш-логики
        /// </summary>
        public static void LogMatrixSetStatistics(string name, Matrices matrices, float tolerance)
        {
            Debug.Log($"=== Matrix Statistics: {name} ===");
            Debug.Log($"Total count: {matrices.Count}");

            var digits = ToleranceUtils.DigitsFromTolerance(tolerance);

            // Детали по определителям
            float minDet = float.MaxValue;
            float maxDet = float.MinValue;
            float sumDet = 0f;

            foreach (var m in matrices)
            {
                float det = m.Original.determinant;
                minDet = Mathf.Min(minDet, det);
                maxDet = Mathf.Max(maxDet, det);
                sumDet += det;
            }

            float avgDet = sumDet / Mathf.Max(1, matrices.Count);
            Debug.Log($"Determinants: min={minDet:F6}, max={maxDet:F6}, avg={avgDet:F6}");

            // Поиск сингулярных (по оригиналу)
            var singularIdx = new List<int>();
            for (int i = 0; i < matrices.Count; i++)
            {
                if (IsSingular(matrices[i].Original, Mathf.Pow(10, -digits))) singularIdx.Add(i);
            }
            if (singularIdx.Count > 0)
            {
                Debug.LogWarning($"Singular matrices: {singularIdx.Count} (indices: {string.Join(", ", singularIdx)})");
            }
            else
            {
                Debug.Log("✅ All matrices are non-singular");
            }

            // Дубликаты по hash округленных значений
            var seen = new Dictionary<ulong, int>();
            var duplicates = new List<(int, int)>();
            for (int i = 0; i < matrices.Count; i++)
            {
                var h = matrices[i].Hash;
                if (seen.TryGetValue(h, out var j))
                {
                    duplicates.Add((j, i));
                }
                else
                {
                    seen[h] = i;
                }
            }

            if (duplicates.Count > 0)
            {
                Debug.LogWarning($"Duplicate matrices found (by rounded hash): {duplicates.Count} pairs");
                foreach (var (i,j) in duplicates.Take(10))
                {
                    Debug.Log($"  Duplicates: [{i}] ≈ [{j}] (digits={digits})");
                }
            }
            else
            {
                Debug.Log("✅ No duplicates found (by rounded hash)");
            }

            Debug.Log("==================");
        }

        /// <summary>
        /// Быстрая эвристическая проверка без полного поиска, согласованная с текущей логикой
        /// </summary>
        public static bool QuickFeasibilityCheck(Matrices model, Matrices space, float tolerance)
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
                Debug.LogWarning($"Space ({space.Count}) has fewer matrices than Model ({model.Count}). Offsets may be rare.");
            }

            var digits = ToleranceUtils.DigitsFromTolerance(tolerance);

            if (IsSingular(model[0].Original, Mathf.Pow(10, -digits)))
            {
                Debug.LogError("First matrix in Model is singular (det ≈ 0). Cannot compute offset!");
                return false;
            }

            // Доп. эвристика: распределение хешей не пустое, и нет тотальных коллизий
            var uniqueSpace = new HashSet<ulong>();
            foreach (var m in space)
            {
                uniqueSpace.Add(m.Hash);
            }
            Debug.Log($"Space unique rounded hashes: {uniqueSpace.Count}/{space.Count} (digits={digits})");
            Debug.Log($"✅ Quick feasibility check passed: Model={model.Count}, Space={space.Count}");
            return true;
        }

        /// <summary>
        /// Диагностика соответствия условия ∀m∈Model: offset*m ∈ Space по двум стратегиям (NearlyEqual и RoundedHash)
        /// Возвращает число совпадений и первые несовпадения.
        /// </summary>
        public static void DiagnoseOffset(string label, Matrix4x4 offset, Matrices model, Matrices space, float tolerance, int maxMismatches = 5)
        {
            var digits = ToleranceUtils.DigitsFromTolerance(tolerance);
            int okHash = 0, okNearly = 0;
            var mismatches = new List<string>();

            // Список space в original для NearlyEqual
            var spaceOriginal = new List<Matrix4x4>(space.Count);
            foreach (var s in space) spaceOriginal.Add(s.Original);

            for (int i = 0; i < model.Count; i++)
            {
                var transformed = offset * model[i].Original;

                // Проверка hash-логикой (основной путь)
                bool inSpaceByHash = space.Contains(transformed.Round(digits).GetHash());
                if (inSpaceByHash) okHash++;

                // Проверка NearlyEqual (диагностика): есть ли матрица в Space, почти равная transformed
                bool inSpaceByNearly = ToleranceUtils.ContainsMatrix(spaceOriginal, transformed, tolerance, tolerance);
                if (inSpaceByNearly) okNearly++;

                if (!inSpaceByHash || !inSpaceByNearly)
                {
                    if (mismatches.Count < maxMismatches)
                    {
                        mismatches.Add($"m[{i}]: hash={(inSpaceByHash?"Y":"N")}, nearly={(inSpaceByNearly?"Y":"N")}");
                    }
                }
            }

            Debug.Log($"[Diag:{label}] matches: hash={okHash}/{model.Count}, nearly={okNearly}/{model.Count}, digits={digits}, tol={tolerance}");
            if (mismatches.Count > 0)
            {
                Debug.Log($"[Diag:{label}] first mismatches: {string.Join("; ", mismatches)}");
            }
        }

        /// <summary>
        /// Проверяет, является ли матрица вырожденной (det = 0)
        /// </summary>
        private static bool IsSingular(Matrix4x4 matrix, float tolerance = 1e-6f)
        {
            return Mathf.Abs(matrix.determinant) < tolerance;
        }
    }
}