using Prototypes.Core.ECS.Transform;
using Scellecs.Morpeh.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Prototypes.SamplesBRG.PrimitivesSample.ECS.Graph
{
    [BurstCompile]
    public struct UpdateGraphParamSurfJob<T> : IJobFor where T : struct, IGraphFunction
    {
        [ReadOnly] public NativeFilter filter;
        [ReadOnly] public NativeStash<TransformComponent> transformStash;

        [ReadOnly] public float step;
        [ReadOnly] public float time;
        [ReadOnly] public int res;

        private T func;

        public void Execute(int x)
        {
            float u = (x + 0.5f) * step - 1f;

            for (int z = 0; z < res; z++)
            {
                float v = (z + 0.5f) * step - 1f;
                var entityId = filter[z + x * res];

                ref var transform = ref transformStash.Get(entityId);
                transform.translation = func.Perform(u, v, time);
                transform.scale = step;
            }
        }

        public static JobHandle ScheduleParallel(NativeFilter filter, NativeStash<TransformComponent> stash, JobHandle dependency = default, int innerloopBatchCount = 32)
        {
            int length = filter.length;
            int res = (int)math.sqrt(length);
            float time = Time.time;
            float step = 2f / res;

            return new UpdateGraphParamSurfJob<T>()
            {
                filter = filter,
                transformStash = stash,
                step = step,
                time = time,
                res = res
            }
            .ScheduleParallel(res, innerloopBatchCount, dependency);
        }
    }
}
