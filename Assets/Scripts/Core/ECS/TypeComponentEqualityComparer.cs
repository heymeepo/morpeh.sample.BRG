using Prototypes.Core.ECS.Configs;
using Scellecs.Morpeh;
using System.Collections.Generic;

namespace Prototypes.Core.ECS
{
    public class TypeComponentEqualityComparer : IEqualityComparer<IComponent>
    {
        public static TypeComponentEqualityComparer Default { get; private set; } = new TypeComponentEqualityComparer();

        public bool Equals(IComponent x, IComponent y) => x != null && y != null && x.GetType() == y.GetType();

        public int GetHashCode(IComponent obj) => obj.GetType().GetHashCode();
    }
}
