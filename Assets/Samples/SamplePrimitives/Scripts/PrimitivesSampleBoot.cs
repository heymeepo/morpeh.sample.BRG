using Prototypes.BRG.ECS;
using Prototypes.Core.ECS;
using Prototypes.Core.ECS.Destroy;
using Prototypes.SamplesBRG.ECS.Metrics;
using Prototypes.SamplesBRG.PrimitivesSample.ECS.Graph;
using Prototypes.SamplesBRG.PrimitivesSample.UI.Graph;
using Prototypes.SamplesBRG.UI.Metrics;
using UnityEngine;

namespace Prototypes.SamplesBRG.PrimitivesSample
{
    public sealed class PrimitivesSampleBoot : MonoBehaviour
    {
        [SerializeField] private MetricsUI metricsUI;
        [SerializeField] private GraphUI graphUI;

        [SerializeField] private GraphSettings graphSettings;

        private EcsStartup startup;

        public void Awake()
        {
            InitializeUI();
            InitializeECS();
        }

        private void OnDestroy()
        {
            startup?.Dispose();
        }

        private void InitializeUI()
        { 
            metricsUI.Initialize();
            graphUI.Initialize(graphSettings.ResolutionMin, graphSettings.ResolutionMax);
        }

        private void InitializeECS()
        {
            startup = new EcsStartup();

            startup
                .SystemsGroupOrder(0)
                .AddInitializer(new GraphInitializer(graphSettings))
                .AddUpdateSystem(new GraphSystem())
                .AddUpdateSystem(new GraphFuncSystem());

            startup
                .SystemsGroupOrder(1)
                .AddUpdateSystem(new GraphColorSystem())
                .AddUpdateSystem(new BatchRendererSystem());

            startup
                .SystemsGroupOrder(2)
                .AddLateSystem(new DestroySystem());

            startup
                .SystemsGroupOrder(3)
                .AddLateSystem(new GraphSystemUI(graphUI))
                .AddLateSystem(new MetricsSystemUI(metricsUI));

            startup.Initialize(true);
        }
    }
}
