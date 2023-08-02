using Scellecs.Morpeh;
using System;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace Prototypes.BRG.ECS.Properties
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [BatchMaterialProperty("_BaseColor", BatchMaterialPropertyFormat.Float4)]
    [Serializable]
    public struct BaseColorPropertyComponent : IComponent
    {
        public float4 value;
    }
}
