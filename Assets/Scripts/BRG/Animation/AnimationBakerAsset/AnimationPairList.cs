using Prototypes.Core.Utils;
using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Prototypes.BRG.Animation
{
    [Serializable]
    public class AnimationPairList
    {
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ValueDropdown("AddAnimationButton", DrawDropdownForListElements = false, NumberOfItemsBeforeEnablingSearch = 0)]
        [ListDrawerSettings(DraggableItems = true)]
#endif
        [SerializeField]
        private List<AnimationPair> pairs = new List<AnimationPair>();

        public AnimationPair this[int index] => pairs[index];

        public int Count => pairs.Count;

#if UNITY_EDITOR && ODIN_INSPECTOR
        private IEnumerable AddAnimationButton()
        {
            return ReflectionHelpers.UserDefinedAssemblies()
                .GetTypesWithAttribute<AnimationStateAttribute>()
                .Where(x => typeof(IComponent).IsAssignableFrom(x) && x.IsValueType)
                .Except(pairs.Select(x => x.animationStateComponent.GetType()))
                .Select(x => new AnimationPair() { animationStateComponent = (IComponent)Activator.CreateInstance(x) })
                .Select(x => new ValueDropdownItem(x.animationStateComponent.GetType().Name, x));
        }
#endif
    }
}
