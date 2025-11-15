using System.Collections.Generic;
using Config;
using Core;
using Cysharp.Threading.Tasks;
using Infrastructure;
using Services.Interfaces;
using UnityEngine;
using VContainer;

namespace View
{
    public class OffsetVisualizer : MonoBehaviour, IOffsetVisualizer
    {
        [SerializeField] private GameObject _visualizationObjectPrefab;
        [SerializeField] private Transform _visualizationRoot;
        
        [Header("Color Settings")]
        [SerializeField] private Gradient _colorGradient;
        
        [Header("Pool Settings")]
        [SerializeField] private int _poolInitialSize = 50;
        [SerializeField] private bool _poolAutoExpand = true;

        [Inject] private IMatrixLoader _loader;
        [Inject] private OffsetFinderConfig _config;

        private List<Matrix4x4> _modelMatrices;
        private ObjectPool<VisualizationObject> _objectPool;

        private void Awake()
        {
            if (_colorGradient == null)
            {
                _colorGradient = CreateDefaultGradient();
            }
        }

        public async UniTask VisualizeAsync(List<FoundOffset> offsets)
        {
            if (_objectPool == null)
            {
                InitializePool();
            }

            _objectPool?.ReturnAll();

            if (_modelMatrices == null || _modelMatrices.Count == 0)
            {
                _modelMatrices = await _loader.LoadAsync(_config.ModelJsonPath);
            }

            if (_visualizationObjectPrefab == null)
            {
                Debug.LogError("Model prefab не назначен!");
                return;
            }

            var count = Mathf.Min(offsets.Count, _config.MaxGroupsToVisualize);

            for (int i = 0; i < count; i++)
            {
                var offsetColor = _colorGradient.Evaluate((float)i / Mathf.Max(count - 1, 1));

                foreach (var modelMatrix in _modelMatrices)
                {
                    var transformedMatrix = offsets[i].Matrix * modelMatrix;

                    var position = transformedMatrix.GetPosition();
                    var rotation = transformedMatrix.rotation;
                    var scale = transformedMatrix.lossyScale;

                    var visualizationObj = _objectPool?.Get(position, rotation);
                    if (visualizationObj != null)
                    {
                        visualizationObj.transform.localScale = scale;
                        visualizationObj.SetColor(offsetColor);
                    }
                }

                await UniTask.Yield();
            }
        }

        public void Clear()
        {
            _objectPool?.ReturnAll();
        }
        
        private void InitializePool()
        {
            if (_visualizationObjectPrefab.GetComponent<VisualizationObject>() == null)
            {
                _visualizationObjectPrefab.AddComponent<VisualizationObject>();
            }

            _objectPool = new ObjectPool<VisualizationObject>(
                _visualizationObjectPrefab,
                _visualizationRoot,
                _poolInitialSize,
                _poolAutoExpand
            );
        }
        
        private static Gradient CreateDefaultGradient()
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.red, 0f),
                    new GradientColorKey(Color.yellow, 0.33f),
                    new GradientColorKey(Color.green, 0.66f),
                    new GradientColorKey(Color.blue, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                }
            );
            return gradient;
        }
    }
}