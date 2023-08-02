using Prototypes.Core.ECS;
using Prototypes.Core.ECS.Destroy;
using Prototypes.Core.ECS.Transform;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Prototypes.SamplesBRG.AnimationSample.ECS.Movement
{

    public sealed class MovementSystem : UpdateBaseSystem
    {
        private Filter filter;

        private Stash<TransformComponent> transformStash;
        private Stash<MoveSpeedComponent> moveSpeedStash;
        private Stash<TurnSpeedComponent> turnSpeedStash;
        private Stash<MoveDestinationComponent> moveDestinationStash;

        public override void OnAwake()
        {
            filter = World.Filter
                .With<TransformComponent>()
                .With<MoveSpeedComponent>()
                .With<TurnSpeedComponent>()
                .With<MoveDestinationComponent>()
                .Without<DestroyRequest>();

            transformStash = World.GetStash<TransformComponent>();
            moveSpeedStash = World.GetStash<MoveSpeedComponent>();
            turnSpeedStash = World.GetStash<TurnSpeedComponent>();
            moveDestinationStash = World.GetStash<MoveDestinationComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            using var nativeFilter = filter.AsNative();

            new MovementJob()
            {
                filter = nativeFilter,
                transformStash = transformStash.AsNative(),
                moveSpeedStash = moveSpeedStash.AsNative(),
                turnSpeedStash = turnSpeedStash.AsNative(),
                moveDestinationStash = moveDestinationStash.AsNative(),
                deltaTime = deltaTime
            }
            .ScheduleParallel(nativeFilter.length, 32, default)
            .Complete();
        }
    }

    [BurstCompile(FloatPrecision = FloatPrecision.Standard, FloatMode = FloatMode.Fast)]
    public struct MovementJob : IJobFor
    {
        [ReadOnly] public NativeFilter filter;

        [ReadOnly] public NativeStash<TransformComponent> transformStash;
        [ReadOnly] public NativeStash<MoveSpeedComponent> moveSpeedStash;
        [ReadOnly] public NativeStash<TurnSpeedComponent> turnSpeedStash;
        [ReadOnly] public NativeStash<MoveDestinationComponent> moveDestinationStash;

        [ReadOnly] public float deltaTime;

        public void Execute(int index)
        {
            var entityId = filter[index];

            ref var moveDestination = ref moveDestinationStash.Get(entityId);

            if (moveDestination.moveRequired)
            {
                ref var transform = ref transformStash.Get(entityId);
                ref var moveSpeed = ref moveSpeedStash.Get(entityId);
                ref var turnSpeed = ref turnSpeedStash.Get(entityId);

                var direction = math.normalizesafe(moveDestination.destination - transform.translation);
                var desiredRotation = quaternion.LookRotationSafe(direction, math.up());
                var maxDegreesDelta = turnSpeed.speed * deltaTime;

                transform.rotation = math.slerp(transform.rotation, desiredRotation, maxDegreesDelta);

                if (math.dot(transform.Forward(), direction) >= turnSpeed.minAngle)
                {
                    transform.translation += direction * moveSpeed.speed * deltaTime;
                }
            }
        }
    }
}
