using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace Prototypes.BRG.ECS
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [System.Serializable]
    public struct RenderComponent : IComponent
    {
        public RenderSharedData sharedData;
    }
}
