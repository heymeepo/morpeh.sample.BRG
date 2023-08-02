using Scellecs.Morpeh;
using System;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace Prototypes.SamplesBRG.AnimationSample.ECS.Movement
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public struct MoveDestinationComponent : IComponent
    {
        public float3 destination;
        public bool moveRequired;
    }
}
