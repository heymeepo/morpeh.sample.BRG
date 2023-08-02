using Prototypes.BRG.Animation;
using Scellecs.Morpeh;
using System;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace Prototypes.SamplesBRG.AnimationSample.ECS.Animation
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [AnimationState("Move")]
    [Serializable]
    public struct MoveAnimationComponent : IComponent
    {
        public int index;
    }
}
