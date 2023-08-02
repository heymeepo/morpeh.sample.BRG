using Scellecs.Morpeh;
using System;
using Unity.IL2CPP.CompilerServices;

namespace Prototypes.SamplesBRG.AnimationSample.ECS.Movement
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public struct MoveSpeedComponent : IComponent 
    {
        public float speed;
    }
}
