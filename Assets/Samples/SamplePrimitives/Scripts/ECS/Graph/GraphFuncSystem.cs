using Prototypes.Core.ECS;
using Prototypes.Core.ECS.Transform;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;

namespace Prototypes.SamplesBRG.PrimitivesSample.ECS.Graph
{
    public sealed class GraphFuncSystem : UpdateBaseSystem
    {
        private Filter filter;

        private Stash<TransformComponent> transformStash;

        public override void OnAwake()
        {
            filter = World.Filter
                .With<TransformComponent>()
                .With<GraphPointMarker>();

            transformStash = World.GetStash<TransformComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            using var nativeFilter = filter.AsNative();

            UpdateGraphParamSurfJob<TorusFunc>
                .ScheduleParallel(nativeFilter, transformStash.AsNative())
                .Complete();
        }
    }
}
