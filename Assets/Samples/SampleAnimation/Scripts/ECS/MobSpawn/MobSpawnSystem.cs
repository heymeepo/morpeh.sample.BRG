using Prototypes.Core.ECS;
using Prototypes.Core.ECS.Transform;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Prototypes.SamplesBRG.AnimationSample.ECS.MobSpawn
{
    public sealed class MobSpawnSystem : UpdateBaseSystem
    {
        private Filter filter;

        private Stash<MobSpawnerTimerComponent> timerStash;
        private Stash<MobSpawnerComponent> spawnerStash;
        private Stash<TransformComponent> transformStash;

        public override void OnAwake()
        {
            filter = World.Filter
                .With<MobSpawnerComponent>()
                .With<MobSpawnerTimerComponent>()
                .With<TransformComponent>();

            timerStash = World.GetStash<MobSpawnerTimerComponent>();
            spawnerStash = World.GetStash<MobSpawnerComponent>();
            transformStash = World.GetStash<TransformComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            using var nativeFilter = filter.AsNative();
            using var output = new NativeQueue<EntityId>(Allocator.TempJob);

            new MobSpawnTimerJob()
            {
                filter = nativeFilter,
                timerStash = timerStash.AsNative(),
                output = output.AsParallelWriter(),
                deltaTime = deltaTime
            }
            .ScheduleParallel(nativeFilter.length, 64, default)
            .Complete();

            while (output.TryDequeue(out EntityId entityId))
            {
                if (World.TryGetEntity(entityId, out var entity))
                {
                    ref var spawnerTransform = ref transformStash.Get(entity);
                    ref var spawner = ref spawnerStash.Get(entity);

                    var mob = World.CreateEntity();

                    spawner.mobToSpawn.SetToEntity(mob);

                    mob.SetComponent(new MobLifespanComponent()
                    {
                        lifespanTime = spawner.mobLifespan
                    });
                    mob.SetComponent(new TransformComponent()
                    {
                        translation = spawnerTransform.translation,
                        rotation = quaternion.identity,
                        scale = 1f
                    });
                }
            }
        }
    }

    [BurstCompile]
    public struct MobSpawnTimerJob : IJobFor
    {
        [ReadOnly] public NativeFilter filter;
        [ReadOnly] public NativeStash<MobSpawnerTimerComponent> timerStash;
        [ReadOnly] public float deltaTime;

        [WriteOnly] public NativeQueue<EntityId>.ParallelWriter output;

        public void Execute(int index)
        {
            var entityId = filter[index];
            ref var timer = ref timerStash.Get(entityId);

            timer.delayTimer -= deltaTime;

            if (timer.delayTimer <= 0)
            {
                output.Enqueue(entityId);
                timer.delayTimer = timer.spawnDelay;
            }
        }
    }
}
