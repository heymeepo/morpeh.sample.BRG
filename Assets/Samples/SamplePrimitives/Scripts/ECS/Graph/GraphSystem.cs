using Prototypes.Core.ECS;
using Prototypes.Core.ECS.Configs;
using Prototypes.Core.ECS.Destroy;
using Prototypes.Core.ECS.Transform;
using Scellecs.Morpeh;
using Unity.Mathematics;
using Unity.VisualScripting;

namespace Prototypes.SamplesBRG.PrimitivesSample.ECS.Graph
{
    public sealed class GraphSystem : UpdateBaseSystem
    {
        private Filter graphFilter;
        private Entity graphEntity;

        private int graphResolution;

        public override void OnAwake()
        {
            graphFilter = World.Filter
                .With<TransformComponent>()
                .With<GraphPointMarker>();

            graphEntity = World.Filter.With<GraphComponent>().FirstOrDefault();
            graphResolution = 0;

            UpdateGraph();
        }

        public override void OnUpdate(float deltaTime) => UpdateGraph();

        private void UpdateGraph()
        {
            if (graphEntity != null) 
            { 
                ref var graph = ref graphEntity.GetComponent<GraphComponent>();

                if (graphResolution != graph.resolution)
                {
                    int diff = graph.resolution * graph.resolution - graphResolution * graphResolution;

                    if (diff > 0)
                    {
                        for (int i = 0; i < diff; i++)
                        {
                            CreateGraphPoint(graph.pointConfig);
                        }
                    }
                    else
                    {
                        foreach (var entity in graphFilter)
                        {
                            diff++;
                            entity.AddComponent<DestroyMarker>();

                            if (diff == 0)
                            {
                                break;
                            }
                        }
                    }

                    graphResolution = graph.resolution;
                }
            }
        }

        private void CreateGraphPoint(IEntityConfig config)
        {
            var entity = World.CreateEntity();

            entity.AddComponent<GraphPointMarker>();

            entity.SetComponent(new TransformComponent()
            {
                translation = 0f,
                rotation = quaternion.identity,
                scale = 1f
            });

            config.SetToEntity(entity);
        }
    }
}
