using Scellecs.Morpeh;

namespace Prototypes.Core.ECS.Configs
{
    public interface IEntityConfig
    {
        public string Name { get; }
        public void SetToEntity(Entity entity);
    }
}