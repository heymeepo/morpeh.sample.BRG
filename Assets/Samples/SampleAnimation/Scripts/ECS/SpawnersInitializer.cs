using Prototypes.Core.ECS;
using Prototypes.Core.ECS.Transform;
using Prototypes.Core.ECS.Configs;
using Prototypes.SamplesBRG.AnimationSample.ECS.MobSpawn;
using Scellecs.Morpeh;
using Unity.Mathematics;

namespace Prototypes.SamplesBRG.AnimationSample.ECS
{
    public sealed class SpawnersInitializer : InitializerBase
    {
        private readonly AreaSettings settings;

        public SpawnersInitializer(AreaSettings settings) => this.settings = settings;

        public override void OnAwake()
        {
            CreateSpawners();
        }

        private void CreateSpawners()
        {
            int halfAreaResolution = settings.AreaResolution / 2;
            int spawnersCount = settings.AreaResolution / 10;
            int spawnersPerConfig = (int)math.ceil(spawnersCount / settings.Configs.Length);

            var random = new Random(0x7FA9B3E8);

            for (int i = 0; i < settings.Configs.Length; i++)
            {
                for (int j = 0; j < spawnersPerConfig; j++)
                {
                    CreateSpawner(ref random, settings.Configs[i], halfAreaResolution);
                }
            }
        }

        private void CreateSpawner(ref Random random, IEntityConfig mobConfig, int halfAreaResolution)
        {
            float spawnDelay = 0.05f;
            float lifespan = 45f;

            var spawner = World.CreateEntity();

            spawner.SetComponent(new MobSpawnerComponent()
            {
                mobToSpawn = mobConfig,
                mobLifespan = lifespan,
            });
            spawner.SetComponent(new MobSpawnerTimerComponent()
            {
                spawnDelay = spawnDelay,
                delayTimer = 0f
            });
            spawner.SetComponent(new TransformComponent()
            {
                translation = new float3(
                     random.NextFloat(-halfAreaResolution, halfAreaResolution),
                     0f,
                     random.NextFloat(-halfAreaResolution, halfAreaResolution)),
                rotation = quaternion.identity,
                scale = 1f
            });
        }
    }
}
