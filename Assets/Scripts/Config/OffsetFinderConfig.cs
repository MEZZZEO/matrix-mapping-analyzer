using UnityEngine;

namespace Config
{
    [CreateAssetMenu(fileName = "OffsetFinderConfig", menuName = "OffsetFinder/Config")]
    public class OffsetFinderConfig : ScriptableObject
    {
        [Header("Пути к файлам")]
        public string ModelJsonPath = "Assets/Data/model.json";
        public string SpaceJsonPath = "Assets/Data/space.json";
        public string OutputJsonPath = "Assets/Data/offsets.json";

        [Header("Параметры поиска")]
        [Range(1e-6f, 1e-2f)]
        public float Tolerance = 1e-4f;

        [Header("Производительность")]
        [Range(10, 500)]
        public int BatchSize = 50;

        [Header("Визуализация")]
        [Range(1, 100)]
        public int MaxGroupsToVisualize = 10;

        [Header("Диагностика")]
        public bool EnableDiagnostics = false;
        [Range(1, 20)]
        public int MaxCandidatesToDiagnose = 3;
        public bool DiagnoseFoundOffsets = true;
    }
}