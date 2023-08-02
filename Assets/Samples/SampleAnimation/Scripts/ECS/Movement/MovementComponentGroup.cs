using Prototypes.Core.ECS.Configs;
using Scellecs.Morpeh;
using Unity.Mathematics;
using UnityEngine;

namespace Prototypes.SamplesBRG.AnimationSample.ECS.Movement
{
    public sealed class MovementComponentGroup : EcsComponentGroupConfig
    {
        [SerializeField] private float moveSpeed;
        [SerializeField] private float turnSpeed;
        [SerializeField] private float facingMinAngle;

        public override void SetToEntity(Entity entity)
        {
            entity.SetComponent(new MoveSpeedComponent()
            { 
                speed = moveSpeed
            });
            entity.SetComponent(new TurnSpeedComponent()
            {
                speed = turnSpeed,
                minAngle = math.cos(math.radians(facingMinAngle))
            });
            entity.SetComponent(new MoveDestinationComponent()
            {
                destination = 0f,
                moveRequired = false
            });
        }
    }
}
