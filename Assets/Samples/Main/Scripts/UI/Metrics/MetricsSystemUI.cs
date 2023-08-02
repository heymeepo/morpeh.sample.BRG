using Prototypes.BRG.ECS;
using Prototypes.Core.ECS;
using Prototypes.SamplesBRG.UI.Metrics;
using Scellecs.Morpeh;
using Unity.Profiling;
using UnityEngine;

namespace Prototypes.SamplesBRG.ECS.Metrics
{
    public class MetricsSystemUI : LateUpdateBaseSystem
    {
        private readonly IMetricsUI metrics;

        private Filter filter;

        private ProfilerRecorder batchesRecorder;
        private float time = 0f;
        private int frameCount = 0;

        private const float TIME_INTERVAL = 0.25f;

        public MetricsSystemUI(IMetricsUI metrics)
        {
            this.metrics = metrics;
            metrics.Initialize();
        }

        public override void OnAwake()
        {
            filter = World.Filter.With<BRGInstanceComponent>();
            batchesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
        }

        public override void OnUpdate(float deltaTime)
        {
            time += Time.unscaledDeltaTime;
            frameCount++;

            if (time > TIME_INTERVAL)
            {
                var fps = Mathf.RoundToInt(frameCount / time);
                var batches = (int)batchesRecorder.LastValue;
                var entities = filter.GetLengthSlow();

                metrics.UpdateFpsCount(fps);
                metrics.UpdateBatchesCount(batches);
                metrics.UpdateEntitiesCount(entities);

                frameCount = 0;
                time = 0f;
            }
        }

        public override void Dispose() => batchesRecorder.Dispose();
    }
}
