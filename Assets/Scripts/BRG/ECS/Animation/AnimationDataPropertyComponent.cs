using Scellecs.Morpeh;
using System;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace Prototypes.BRG
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [BatchMaterialProperty("_AnimationData", BatchMaterialPropertyFormat.Float4)]
    [Serializable]
    public struct AnimationDataPropertyComponent : IComponent
    {
        public float4 value;
    }
}
