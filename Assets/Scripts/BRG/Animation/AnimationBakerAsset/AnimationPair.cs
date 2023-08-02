using Prototypes.Core.Utils;
using Scellecs.Morpeh;
using UnityEngine;
#if UNITY_EDITOR && ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Prototypes.BRG.Animation
{
    [System.Serializable]
    public class AnimationPair
    {
#if UNITY_EDITOR && ODIN_INSPECTOR
        [LabelText("$LabelText")]
#endif
        public AnimationClip clip;

        [HideInInspector, SerializeReference]
        public IComponent animationStateComponent;

#if UNITY_EDITOR && ODIN_INSPECTOR
        private string LabelText => animationStateComponent != null ? animationStateComponent.GetType().GetAttribute<AnimationStateAttribute>().EditorName : "Null";
#endif
    }
}
