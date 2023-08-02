using Prototypes.BRG.Animation.TAO.VertexAnimation;
using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Prototypes.BRG.Animation
{
    [CreateAssetMenu(fileName = "AnimationData", menuName = "BRG/AnimationData")]
    public sealed class AnimationSharedData : ScriptableObject
    {
        [ReadOnly] public string id = default;
        [ReadOnly] public AnimationData[] animations;
        [ReadOnly, SerializeReference] public IComponent[] components;

        private void OnValidate()
        {
            if (id == null || id == string.Empty)
            { 
                id = Guid.NewGuid().ToString();
            }
        }
    }
}
