using Prototypes.Core.ECS.Configs;
using Scellecs.Morpeh;
using System;
using Unity.IL2CPP.CompilerServices;

namespace Prototypes.SamplesBRG.AnimationSample.ECS.MobSpawn
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public struct MobSpawnerComponent : IComponent
    {
        public IEntityConfig mobToSpawn;
        public float mobLifespan;
    }
}
