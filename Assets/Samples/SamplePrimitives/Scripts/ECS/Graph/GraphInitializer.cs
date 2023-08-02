using Prototypes.Core.ECS;
using Scellecs.Morpeh;

namespace Prototypes.SamplesBRG.PrimitivesSample.ECS.Graph
{
    public sealed class GraphInitializer : InitializerBase
    {
        private readonly GraphSettings settings;

        public GraphInitializer(GraphSettings settings) => this.settings = settings;

        public override void OnAwake()
        {
            var graphEntity = World.CreateEntity();

            graphEntity.SetComponent(new GraphComponent()
            {
                resolution = settings.ResolutionMin,
                pointConfig = settings.Config
            });
        }
    }
}
