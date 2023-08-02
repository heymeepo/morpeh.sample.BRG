using Prototypes.Core.ECS.Configs;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace Prototypes.SamplesBRG.PrimitivesSample.ECS.Graph
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct GraphComponent : IComponent 
    {
        public int resolution;
        public IEntityConfig pointConfig;
    }
}
