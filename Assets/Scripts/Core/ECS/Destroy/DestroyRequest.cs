using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace Prototypes.Core.ECS.Destroy
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct DestroyRequest : IComponent 
    {
        public bool canBeDestroyed;
    }
}