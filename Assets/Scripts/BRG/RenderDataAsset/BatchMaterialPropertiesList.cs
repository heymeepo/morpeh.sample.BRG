#if UNITY_EDITOR && ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using Prototypes.Core.Utils;
using Scellecs.Morpeh;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Prototypes.BRG
{
    [Serializable]
    public class BatchMaterialPropertiesList
    {
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ValueDropdown("AddPropertyDefineButton", NumberOfItemsBeforeEnablingSearch = 0)]
        [ListDrawerSettings(DraggableItems = true)]
#endif
        [SerializeReference]
        private List<IComponent> instancedProperties = new List<IComponent>();

        public IComponent this[int index] => instancedProperties[index];

        public int Count => instancedProperties.Count;

#if UNITY_EDITOR && ODIN_INSPECTOR
        private IEnumerable AddPropertyDefineButton()
        {
            return ReflectionHelpers.UserDefinedAssemblies()
                .GetTypesWithAttribute<BatchMaterialPropertyAttribute>()
                .Where(x => typeof(IComponent).IsAssignableFrom(x) && x.IsValueType)
                .Except(instancedProperties.Select(x => x.GetType()))
                .Select(x => (IComponent)Activator.CreateInstance(x))
                .Select(x => new ValueDropdownItem(x.GetType().Name, x));
        }
#endif
    }
}
