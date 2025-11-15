using Config;
using Model;
using Presenters;
using Services;
using Services.Interfaces;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using View;

namespace Infrastructure
{
    public class OffsetFinderScope : LifetimeScope
    {
        [SerializeField] private OffsetFinderConfig _config;
        [SerializeField] private OffsetVisualizer _visualizerPrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_config);

            builder.Register<OffsetFinderModel>(Lifetime.Singleton);

            builder.Register<IMatrixLoader, JsonMatrixLoader>(Lifetime.Singleton);
            builder.Register<IOffsetCalculator, ParallelOffsetCalculator>(Lifetime.Singleton);
            builder.Register<IResultExporter, JsonResultExporter>(Lifetime.Singleton);

            if (_visualizerPrefab != null)
            {
                builder.RegisterComponentInNewPrefab(_visualizerPrefab, Lifetime.Singleton)
                    .AsImplementedInterfaces();
            }

            builder.Register<OffsetFinderService>(Lifetime.Singleton);

            builder.RegisterComponentInHierarchy<OffsetFinderUIController>();
        }
    }
}