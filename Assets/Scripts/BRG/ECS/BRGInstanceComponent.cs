using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace Prototypes.BRG.ECS
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct BRGInstanceComponent : IComponent
    {
        public int batchIndex;
        public int instanceId;
    }
}
