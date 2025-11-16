using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Утилита для сравнения чисел, векторов и матриц с учетом абсолютной и относительной толерантности.
    /// Использует комбинированный подход: |a - b| &lt;= max(absTol, max(|a|, |b|) * relTol)
    /// </summary>
    public static class ToleranceUtils
    {
        /// <summary>
        /// Абсолютная толерантность по умолчанию
        /// </summary>
        public const float DefaultAbsTolerance = 1e-5f;

        /// <summary>
        /// Относительная толерантность по умолчанию
        /// </summary>
        public const float DefaultRelTolerance = 1e-5f;

        /// <summary>
        /// Проверяет, равны ли два числа с учетом толерантности
        /// </summary>
        public static bool NearlyEqual(float a, float b, float absTolerance = DefaultAbsTolerance, float relTolerance = DefaultRelTolerance)
        {
            float diff = Mathf.Abs(a - b);
            float maxAbs = Mathf.Max(Mathf.Abs(a), Mathf.Abs(b));
            float threshold = Mathf.Max(absTolerance, maxAbs * relTolerance);
            
            return diff <= threshold;
        }

        /// <summary>
        /// Проверяет, равны ли два вектора с учетом толерантности (покомпонентно)
        /// </summary>
        public static bool NearlyEqual(Vector3 a, Vector3 b, float absTolerance = DefaultAbsTolerance, float relTolerance = DefaultRelTolerance)
        {
            return NearlyEqual(a.x, b.x, absTolerance, relTolerance)
                && NearlyEqual(a.y, b.y, absTolerance, relTolerance)
                && NearlyEqual(a.z, b.z, absTolerance, relTolerance);
        }

        /// <summary>
        /// Проверяет, равны ли две матрицы с учетом толерантности (покомпонентно)
        /// </summary>
        public static bool NearlyEqual(Matrix4x4 a, Matrix4x4 b, float absTolerance = DefaultAbsTolerance, float relTolerance = DefaultRelTolerance)
        {
            for (int i = 0; i < 16; i++)
            {
                if (!NearlyEqual(a[i], b[i], absTolerance, relTolerance))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Проверяет, содержится ли матрица в списке с учетом толерантности
        /// </summary>
        public static bool ContainsMatrix(System.Collections.Generic.List<Matrix4x4> list, Matrix4x4 target, float absTolerance = DefaultAbsTolerance, float relTolerance = DefaultRelTolerance)
        {
            foreach (var matrix in list)
            {
                if (NearlyEqual(matrix, target, absTolerance, relTolerance))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Находит индекс матрицы в списке с учетом толерантности, или -1 если не найдена
        /// </summary>
        public static int IndexOfMatrix(System.Collections.Generic.List<Matrix4x4> list, Matrix4x4 target, float absTolerance = DefaultAbsTolerance, float relTolerance = DefaultRelTolerance)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (NearlyEqual(list[i], target, absTolerance, relTolerance))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Преобразует значение толерантности в количество знаков после запятой
        /// </summary>
        public static int DigitsFromTolerance(float tolerance)
        {
            if (tolerance <= 0f) return 6;
            var d = Mathf.Clamp(Mathf.RoundToInt(-Mathf.Log10(tolerance)), 0, 6);
            return d;
        }

        /// <summary>
        /// Округляет значение до заданного количества знаков после запятой
        /// </summary>
        public static float RoundToDigits(float value, int digits)
        {
            float scale = Mathf.Pow(10f, digits);
            return Mathf.Round(value * scale) / scale;
        }
    }
}