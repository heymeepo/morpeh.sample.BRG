using Prototypes.Core.ECS;
using Prototypes.SamplesBRG.PrimitivesSample.UI.Graph;
using Scellecs.Morpeh;
using UnityEngine;

namespace Prototypes.SamplesBRG.PrimitivesSample.ECS.Graph
{
    public sealed class GraphSystemUI : LateUpdateBaseSystem
    {
        private readonly IGraphUI graph;
        private float sliderValue;

        private Entity graphEntity;

        public GraphSystemUI(IGraphUI graph) => this.graph = graph;

        public override void OnAwake()
        {
            graphEntity = World.Filter.With<GraphComponent>().FirstOrDefault();
            SetGraphInitialValue();
        }

        public override void OnUpdate(float deltaTime) => ProcessSlider();

        private void ProcessSlider()
        {
            if (Mathf.Approximately(sliderValue, graph.GetResolutionSliderValue()) == false)
            {
                var resolution = Mathf.CeilToInt(graph.GetResolutionSliderValue());
                ref var graphResolution = ref graphEntity.GetComponent<GraphComponent>().resolution;
                graphResolution = resolution;
                graph.UpdateResolution(resolution);
                sliderValue = graph.GetResolutionSliderValue();
            }
        }

        private void SetGraphInitialValue()
        {
            var graphEntity = World.Filter.With<GraphComponent>().FirstOrDefault();

            if (graphEntity != null)
            {
                var resolution = graphEntity.GetComponent<GraphComponent>().resolution;
                graph.UpdateResolution(resolution);
                sliderValue = graph.GetResolutionSliderValue();
            }
        }
    }
}
