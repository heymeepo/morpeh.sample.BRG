using Prototypes.BRG.ECS.Animation;
using Prototypes.Core.ECS;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Prototypes.SamplesBRG.AnimationSample.ECS.Animation
{
    public sealed class IdleAnimationSystem : UpdateBaseSystem
    {
        private Filter filter;

        private Stash<AnimatorComponent> animatorStash;
        private Stash<IdleAnimationComponent> idleAnimationStash;

        public override void OnAwake()
        {
            filter = World.Filter
                .With<AnimatorComponent>()
                .With<IdleAnimationComponent>();

            animatorStash = World.GetStash<AnimatorComponent>();
            idleAnimationStash = World.GetStash<IdleAnimationComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            using var nativeFilter = filter.AsNative();

            new IdleAnimationJob()
            {
                filter = nativeFilter,
                animatorStash = animatorStash.AsNative(),
                idleAnimationStash = idleAnimationStash.AsNative()
            }
            .ScheduleParallel(nativeFilter.length, 32, default)
            .Complete();
        }
    }

    [BurstCompile]
    public struct IdleAnimationJob : IJobFor
    {
        [ReadOnly] public NativeFilter filter;

        [ReadOnly] public NativeStash<AnimatorComponent> animatorStash;
        [ReadOnly] public NativeStash<IdleAnimationComponent> idleAnimationStash;

        public void Execute(int index)
        {
            var entityId = filter[index];

            ref var animator = ref animatorStash.Get(entityId);
            ref var idleState = ref idleAnimationStash.Get(entityId);

            animator.SetAnimation(idleState.index, 1f);
        }
    }
}
