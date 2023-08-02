using Prototypes.BRG.Animation.TAO.VertexAnimation;
using Prototypes.Core.ECS;
using Prototypes.Core.ECS.MorpehWorkaround;
using Prototypes.Core.Utils;
using Scellecs.Morpeh;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Prototypes.BRG.ECS.Animation
{
    public sealed class AnimatorInitializeSystem : UpdateBaseSystem
    {
        private Dictionary<string, IntPtr> animationDataMap;

        private Filter filter;

        private Stash<AnimationComponent> animationStash;
        private Stash<AnimatorComponent> animatorStash;

        public override void OnAwake()
        {
            animationDataMap = new Dictionary<string, IntPtr>();

            filter = World.Filter
                .With<AnimationComponent>()
                .With<AnimationDataPropertyComponent>()
                .Without<AnimatorComponent>();

            animationStash = World.GetStash<AnimationComponent>();
            animatorStash = World.GetStash<AnimatorComponent>();
        }

        public unsafe override void OnUpdate(float deltaTime)
        {
            foreach (var entity in filter)
            {
                ref var animationComponent = ref animationStash.Get(entity);
                var key = animationComponent.sharedData.id;

                if (animationDataMap.TryGetValue(key, out var animationData) == false)
                {
                    var animationDataPtr = UnsafeHelpers.Malloc<AnimationData>(animationComponent.sharedData.animations.Length, Allocator.Persistent);

                    for (int i = 0; i < animationComponent.sharedData.animations.Length; i++)
                    {
                        animationDataPtr[i] = animationComponent.sharedData.animations[i];
                    }

                    animationDataMap[key] = animationData = (IntPtr)animationDataPtr;
                }

                animatorStash.Set(entity, new AnimatorComponent()
                {
                    currentIndex = 0,
                    speed = 1f,
                    animations = (AnimationData*)animationData,
                    isDone = false,
                    newIndex = 0,
                });

                for (int i = 0; i < animationComponent.sharedData.components.Length; i++)
                {
                    entity.SetComponentBoxed(animationComponent.sharedData.components[i]);
                }
            }
        }

        public override void Dispose()
        {
            foreach (var ptr in animationDataMap.Values)
            {
                unsafe
                {
                    UnsafeUtility.Free((void*)ptr, Allocator.Persistent);
                }
            }

            animationDataMap.Clear();
            animationDataMap = null;
        }
    }
}
