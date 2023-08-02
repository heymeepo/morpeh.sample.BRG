using Prototypes.BRG.Animation.TAO.VertexAnimation;
using Scellecs.Morpeh;
using System;
using Unity.IL2CPP.CompilerServices;

namespace Prototypes.BRG.ECS.Animation
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public unsafe struct AnimatorComponent : IComponent
    {
        [NonSerialized]
        public AnimationData* animations;
        public int currentIndex;
        public float speed;
        public float time;
        public bool isDone;

        public int newIndex;
    }
}
