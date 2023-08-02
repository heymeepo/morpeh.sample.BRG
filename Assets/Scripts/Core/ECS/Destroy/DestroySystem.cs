using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Prototypes.Core.ECS.Destroy
{
    public sealed class DestroySystem : LateUpdateBaseSystem
    {
        private Filter requestFilter;
        private Filter destroyFilter;

        private Stash<DestroyRequest> requestStash;
        private Stash<DestroyMarker> destroyStash;

        public override void OnAwake()
        {
            requestFilter = World.Filter
                .With<DestroyRequest>()
                .Without<DestroyMarker>();

            destroyFilter = World.Filter
                .With<DestroyMarker>();

            requestStash = World.GetStash<DestroyRequest>();
            destroyStash = World.GetStash<DestroyMarker>();
        }

        public override void OnUpdate(float deltaTime)
        {
            ProcessDestroy();
            ProcessRequest();
        }

        private void ProcessRequest()
        {
            using var nativeFilter = requestFilter.AsNative();
            using var output = new NativeQueue<EntityId>(Allocator.TempJob);

            new DestroyRequestJob()
            {
                filter = nativeFilter,
                stash = requestStash.AsNative(),
                output = output.AsParallelWriter()
            }
            .ScheduleParallel(nativeFilter.length, 128, default)
            .Complete();

            while (output.TryDequeue(out EntityId id))
            {
                if (World.TryGetEntity(id, out Entity entity))
                {
                    destroyStash.Add(entity);
                }
            }
        }

        private void ProcessDestroy()
        {
            foreach (var entity in destroyFilter)
            {
                World.RemoveEntity(entity);
            }
        }
    }

    [BurstCompile]
    public struct DestroyRequestJob : IJobFor
    {
        [ReadOnly] public NativeFilter filter;
        [ReadOnly] public NativeStash<DestroyRequest> stash;

        [WriteOnly] public NativeQueue<EntityId>.ParallelWriter output;

        public void Execute(int index)
        {
            var entityId = filter[index];
            ref var request = ref stash.Get(entityId);

            if (request.canBeDestroyed)
            {
                output.Enqueue(entityId);
            }
        }
    }
}