using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace Prototypes.Core.ECS
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class UpdateBaseSystem : IUpdateSystem
    {
        public World World { get; set; }

        public virtual void OnAwake() { }

        public abstract void OnUpdate(float deltaTime);

        public virtual void Dispose() { }
    }
}
