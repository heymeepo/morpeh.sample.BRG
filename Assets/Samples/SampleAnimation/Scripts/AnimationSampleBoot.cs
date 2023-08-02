using Prototypes.BRG.ECS;
using Prototypes.BRG.ECS.Animation;
using Prototypes.Core.ECS;
using Prototypes.Core.ECS.Destroy;
using Prototypes.SamplesBRG.AnimationSample.ECS;
using Prototypes.SamplesBRG.AnimationSample.ECS.MobSpawn;
using Prototypes.SamplesBRG.AnimationSample.ECS.MobBehaviour;
using Prototypes.SamplesBRG.AnimationSample.ECS.Movement;
using Prototypes.SamplesBRG.AnimationSample.ECS.Animation;
using Prototypes.SamplesBRG.ECS.Metrics;
using Prototypes.SamplesBRG.UI.Metrics;
using System;
using UnityEngine;

namespace Prototypes.SamplesBRG.AnimationSample
{
    public sealed class AnimationSampleBoot : MonoBehaviour
    {
        [SerializeField] private AreaSettings areaSettings;
        [SerializeField] private MetricsUI metrics;

        private EcsStartup startup;
        private SimulationArea area;

        public void Awake()
        {
            CreateArea();
            InitializeUI();
            InitializeECS();
        }

        private void OnDestroy()
        {
            area.Dispose();
            startup?.Dispose();
        }

        private void CreateArea() => area = new SimulationArea(areaSettings.AreaResolution, areaSettings.AreaMaterial);

        private void InitializeUI() => metrics.Initialize();

        private void InitializeECS()
        {
            startup = new EcsStartup();

            startup
                .SystemsGroupOrder(0)
                .AddInitializer(new SpawnersInitializer(areaSettings))
                .AddUpdateSystem(new MobSpawnSystem())
                .AddUpdateSystem(new MobLifespanSystem())
                .AddUpdateSystem(new MobBehaviourSystem(areaSettings))
                .AddUpdateSystem(new MovementSystem());

            startup
                .SystemsGroupOrder(1)
                .AddUpdateSystem(new AnimatorInitializeSystem())
                .AddUpdateSystem(new IdleAnimationSystem())
                .AddUpdateSystem(new MovementAnimationSystem())
                .AddUpdateSystem(new DieAnimationSystem())
                .AddUpdateSystem(new AnimatorSystem())
                .AddUpdateSystem(new BatchRendererSystem());

            startup
                .SystemsGroupOrder(2)
                .AddLateSystem(new DestroySystem());

            startup
                .SystemsGroupOrder(3)
                .AddLateSystem(new MetricsSystemUI(metrics));

            startup.Initialize(true);
        }
    }
}
