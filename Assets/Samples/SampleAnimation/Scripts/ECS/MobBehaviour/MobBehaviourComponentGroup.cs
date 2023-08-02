using Prototypes.Core.ECS.Configs;
using Scellecs.Morpeh;
using UnityEngine;

namespace Prototypes.SamplesBRG.AnimationSample.ECS.MobBehaviour
{
    public class MobBehaviourComponentGroup : EcsComponentGroupConfig
    {
        [SerializeField, Range(1, 15)] private float waitingTime = 1f;

        public override void SetToEntity(Entity entity)
        {
            entity.SetComponent(new MobBehaviourComponent()
            {
                state = MobState.Waiting,
                waitingTime = waitingTime,
                waitingTimer = 0
            });
        }
    }
}
