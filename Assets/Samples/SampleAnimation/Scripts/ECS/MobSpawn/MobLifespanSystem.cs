using Prototypes.Core.ECS;
using Prototypes.Core.ECS.Destroy;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Prototypes.SamplesBRG.AnimationSample.ECS.MobSpawn
{
    public sealed class MobLifespanSystem : UpdateBaseSystem
    {
        private Filter filter;

        private Stash<MobLifespanComponent> lifespanStash;
        private Stash<DestroyRequest> destroyStash;

        public override void OnAwake()
        {
            filter = World.Filter
                .With<MobLifespanComponent>()
                .Without<DestroyRequest>();

            lifespanStash = World.GetStash<MobLifespanComponent>();
            destroyStash = World.GetStash<DestroyRequest>();
        }

        public override void OnUpdate(float deltaTime)
        {
            using var nativeFilter = filter.AsNative();
            using var output = new NativeQueue<EntityId>(Allocator.TempJob);

            new MobLifespanJob()
            {
                filter = nativeFilter,
                lifespanStash = lifespanStash.AsNative(),
                output = output.AsParallelWriter(),
                deltaTime = deltaTime
            }
            .ScheduleParallel(nativeFilter.length, 64, default)
            .Complete();

            while (output.TryDequeue(out var entityId))
            {
                if (World.TryGetEntity(entityId, out var entity))
                {
                    destroyStash.Set(entity, new DestroyRequest()
                    {
                        canBeDestroyed = true
                    });
                }
            }
        }
    }

    [BurstCompile]
    public struct MobLifespanJob : IJobFor
    {
        [ReadOnly] public NativeFilter filter;
        [ReadOnly] public NativeStash<MobLifespanComponent> lifespanStash;
        [ReadOnly] public float deltaTime;

        [WriteOnly] public NativeQueue<EntityId>.ParallelWriter output;

        public void Execute(int index)
        {
            var entityId = filter[index];
            ref var lifespan = ref lifespanStash.Get(entityId);

            lifespan.lifespanTime -= deltaTime;

            if (lifespan.lifespanTime <= 0f)
            {
                output.Enqueue(entityId);
            }
        }
    }
}
