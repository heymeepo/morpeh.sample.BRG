using Prototypes.Core.ECS;
using Prototypes.Core.ECS.Destroy;
using Prototypes.Core.ECS.Transform;
using Prototypes.SamplesBRG.AnimationSample.ECS.Movement;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Prototypes.SamplesBRG.AnimationSample.ECS.MobBehaviour
{
    public sealed class MobBehaviourSystem : UpdateBaseSystem
    {
        private readonly int areaHalfResolution;

        private Filter filter;

        private Stash<TransformComponent> transformStash;
        private Stash<MobBehaviourComponent> mobBehaviourStash;
        private Stash<MoveDestinationComponent> moveDestinationStash;

        private Random random;

        public MobBehaviourSystem(AreaSettings settings) => areaHalfResolution = settings.AreaResolution / 2;

        public override void OnAwake()
        {
            filter = World.Filter
                .With<TransformComponent>()
                .With<MobBehaviourComponent>()
                .With<MoveDestinationComponent>()
                .Without<DestroyRequest>();

            transformStash = World.GetStash<TransformComponent>();
            mobBehaviourStash = World.GetStash<MobBehaviourComponent>();
            moveDestinationStash = World.GetStash<MoveDestinationComponent>();

            random = new Random(0xDD3C6DC5);
        }

        public override void OnUpdate(float deltaTime)
        {
            using var nativeFilter = filter.AsNative();

            new MobStateJob()
            {
                filter = nativeFilter,
                transformStash = transformStash.AsNative(),
                mobBehaviourStash = mobBehaviourStash.AsNative(),
                moveDestinationStash = moveDestinationStash.AsNative(),
                seed = random.NextUInt(1u, 666666u),
                areaHalfResolution = areaHalfResolution,
                deltaTime = deltaTime
            }
            .ScheduleParallel(nativeFilter.length, 32, default)
            .Complete();
        }
    }

    [BurstCompile]
    public struct MobStateJob : IJobFor
    {
        [ReadOnly] public NativeFilter filter;

        [ReadOnly] public NativeStash<TransformComponent> transformStash;
        [ReadOnly] public NativeStash<MobBehaviourComponent> mobBehaviourStash;
        [ReadOnly] public NativeStash<MoveDestinationComponent> moveDestinationStash;

        [ReadOnly] public uint seed;
        [ReadOnly] public float areaHalfResolution;
        [ReadOnly] public float deltaTime;

        public void Execute(int index)
        {
            var entityId = filter[index];

            ref var transform = ref transformStash.Get(entityId);
            ref var state = ref mobBehaviourStash.Get(entityId);
            ref var destination = ref moveDestinationStash.Get(entityId);

            var random = new Random(seed + (uint)index);

            if (state.state == MobState.Runnig)
            {
                if (math.distancesq(transform.translation.xz, destination.destination.xz) <= 0.1f)
                {
                    state.state = MobState.Waiting;
                    state.waitingTimer = random.NextFloat(state.waitingTime, state.waitingTime + 5f);
                    destination.moveRequired = false;
                }
            }
            else
            {
                state.waitingTimer -= deltaTime;

                if (state.waitingTimer <= 0f)
                {
                    state.state = MobState.Runnig;
                    destination.destination =
                        new float3(
                            random.NextFloat(-areaHalfResolution, areaHalfResolution),
                            0f,
                            random.NextFloat(-areaHalfResolution, areaHalfResolution));

                    destination.moveRequired = true;
                }
            }
        }
    }
}
