using Prototypes.BRG.ECS.Properties;
using Prototypes.Core.ECS;
using Prototypes.Core.ECS.Transform;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Prototypes.SamplesBRG.PrimitivesSample.ECS.Graph
{
    public sealed class GraphColorSystem : UpdateBaseSystem
    {
        private Filter filter;

        private Stash<TransformComponent> transformStash;
        private Stash<BaseColorPropertyComponent> colorStash;

        public override void OnAwake()
        {
            filter = World.Filter
                .With<GraphPointMarker>()
                .With<TransformComponent>()
                .With<BaseColorPropertyComponent>();

            transformStash = World.GetStash<TransformComponent>();
            colorStash = World.GetStash<BaseColorPropertyComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            using var nativeFilter = filter.AsNative();

            new UpdateGraphColorJob()
            {
                filter = nativeFilter,
                transformStash = transformStash.AsNative(),
                colorStash = colorStash.AsNative(),
            }
            .ScheduleParallel(nativeFilter.length, 64, default)
            .Complete();
        }
    }

    [BurstCompile]
    public struct UpdateGraphColorJob : IJobFor
    {
        [ReadOnly] public NativeFilter filter;
        [ReadOnly] public NativeStash<TransformComponent> transformStash;
        [ReadOnly] public NativeStash<BaseColorPropertyComponent> colorStash;

        public void Execute(int index)
        {
            var entityId = filter[index];

            ref var transform = ref transformStash.Get(entityId);
            ref var color = ref colorStash.Get(entityId);

            color.value.xyz = math.clamp(transform.translation.xyz * 0.5f + 0.5f, 0f, 1f);
        }
    }
}
