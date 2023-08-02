using Prototypes.BRG.ECS.Animation;
using Prototypes.Core.ECS;
using Prototypes.Core.ECS.Destroy;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Prototypes.SamplesBRG.AnimationSample.ECS.Animation
{
    public sealed class DieAnimationSystem : UpdateBaseSystem
    {
        private Filter filter;

        private Stash<AnimatorComponent> animatorStash;
        private Stash<DieAnimationComponent> dieAnimationStash;
        private Stash<DestroyRequest> destroyStash;

        public override void OnAwake()
        {
            filter = World.Filter
                .With<AnimatorComponent>()
                .With<DieAnimationComponent>()
                .With<DestroyRequest>();

            animatorStash = World.GetStash<AnimatorComponent>();
            dieAnimationStash = World.GetStash<DieAnimationComponent>();
            destroyStash = World.GetStash<DestroyRequest>();
        }

        public override void OnUpdate(float deltaTime)
        {
            using var nativeFilter = filter.AsNative();

            new DieAnimationJob()
            {
                filter = nativeFilter,
                animatorStash = animatorStash.AsNative(),
                dieAnimationStash = dieAnimationStash.AsNative(),
                destroyStash = destroyStash.AsNative()
            }
            .ScheduleParallel(nativeFilter.length, 64, default)
            .Complete();
        }
    }

    [BurstCompile]
    public struct DieAnimationJob : IJobFor
    {
        [ReadOnly] public NativeFilter filter;

        [ReadOnly] public NativeStash<AnimatorComponent> animatorStash;
        [ReadOnly] public NativeStash<DieAnimationComponent> dieAnimationStash;
        [ReadOnly] public NativeStash<DestroyRequest> destroyStash;

        public void Execute(int index)
        {
            var entityId = filter[index];

            ref var animator = ref animatorStash.Get(entityId);
            ref var dieState = ref dieAnimationStash.Get(entityId);
            ref var request = ref destroyStash.Get(entityId);

            request.canBeDestroyed = animator.isDone;
            animator.SetAnimation(dieState.index, 1f);
        }
    }
}
