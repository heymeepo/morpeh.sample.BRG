using Prototypes.BRG.ECS.Animation;
using Prototypes.Core.ECS;
using Prototypes.SamplesBRG.AnimationSample.ECS.Movement;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Prototypes.SamplesBRG.AnimationSample.ECS.Animation
{
    public sealed class MovementAnimationSystem : UpdateBaseSystem
    {
        private Filter filter;

        private Stash<AnimatorComponent> animatorStash;
        private Stash<MoveAnimationComponent> moveAnimationStash;
        private Stash<MoveSpeedComponent> moveSpeedStash;
        private Stash<MoveDestinationComponent> moveDestinationStash;

        public override void OnAwake()
        {
            filter = World.Filter
                .With<AnimatorComponent>()
                .With<MoveAnimationComponent>()
                .With<MoveSpeedComponent>()
                .With<MoveDestinationComponent>();

            animatorStash = World.GetStash<AnimatorComponent>();
            moveAnimationStash = World.GetStash<MoveAnimationComponent>();
            moveSpeedStash = World.GetStash<MoveSpeedComponent>();
            moveDestinationStash = World.GetStash<MoveDestinationComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            using var nativeFilter = filter.AsNative();

            new MovementAnimationJob()
            {
                filter = nativeFilter,
                animatorStash = animatorStash.AsNative(),
                moveAnimationStash = moveAnimationStash.AsNative(),
                moveSpeedStash = moveSpeedStash.AsNative(),
                moveDestinationStash = moveDestinationStash.AsNative()
            }
            .ScheduleParallel(nativeFilter.length, 32, default)
            .Complete();
        }
    }

    [BurstCompile]
    public struct MovementAnimationJob : IJobFor
    {
        [ReadOnly] public NativeFilter filter;

        [ReadOnly] public NativeStash<AnimatorComponent> animatorStash;
        [ReadOnly] public NativeStash<MoveAnimationComponent> moveAnimationStash;
        [ReadOnly] public NativeStash<MoveSpeedComponent> moveSpeedStash;
        [ReadOnly] public NativeStash<MoveDestinationComponent> moveDestinationStash;

        public void Execute(int index)
        {
            var entityId = filter[index];

            ref var moveDestination = ref moveDestinationStash.Get(entityId);

            if (moveDestination.moveRequired)
            {
                ref var animator = ref animatorStash.Get(entityId);
                ref var moveState = ref moveAnimationStash.Get(entityId);
                ref var moveSpeed = ref moveSpeedStash.Get(entityId);

                float scaleFactor = 0.15f;
                float maxAnimationSpeed = 10f;

                float scaledMoveSpeed = math.log(moveSpeed.speed + 1f) * scaleFactor;
                float animationSpeed = math.clamp(scaledMoveSpeed, 0f, 1f) * maxAnimationSpeed;

                animator.SetAnimation(moveState.index, animationSpeed);
            }
        }
    }
}
