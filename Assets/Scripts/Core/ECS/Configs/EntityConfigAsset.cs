using Scellecs.Morpeh;
using System.Collections;
using System;
using System.Linq;
using UnityEngine;
using Prototypes.Core.Utils;
using Prototypes.Core.ECS.MorpehWorkaround;
#if UNITY_EDITOR && ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Prototypes.Core.ECS.Configs
{
    [CreateAssetMenu(fileName = "EntityConfig", menuName = "EcsConfig/EntityConfig")]
    public sealed class EntityConfigAsset : ScriptableObject, IEntityConfig
    {
        public string Name => name;

        [SerializeField] private bool test = false;

#if UNITY_EDITOR && ODIN_INSPECTOR
        [ValueDropdown("AddComponentGroupButton", NumberOfItemsBeforeEnablingSearch = 0, DrawDropdownForListElements = false)]
        [ListDrawerSettings(DefaultExpandedState = true)]
#endif
        [SerializeReference] 
        private EcsComponentGroupConfig[] components = new EcsComponentGroupConfig[0];

#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowIf(nameof(test))]
#endif
        [SerializeReference] 
        private IComponent[] testComponents = new IComponent[0];

        public void SetToEntity(Entity entity)
        {
            for (int i = 0; i < components.Length; i++)
            {
                components[i].SetToEntity(entity);
            }

            if (test)
            {
                for (int i = 0; i < testComponents.Length; i++)
                {
                    entity.SetComponentBoxed(testComponents[i]);
                }
            }
        }

        private void OnValidate()
        {
            testComponents = testComponents.Distinct(TypeComponentEqualityComparer.Default).ToArray();
            components = components.Distinct(ComponentGroupEqualityComparer.Default).ToArray();
        }

#if UNITY_EDITOR && ODIN_INSPECTOR
        private IEnumerable AddComponentGroupButton()
        {
            return ReflectionHelpers.UserDefinedAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof(EcsComponentGroupConfig).IsAssignableFrom(x) && x.IsAbstract == false)
                .Except(components.Select(x => x.GetType()))
                .Select(x => Activator.CreateInstance(x))
                .Select(x => new ValueDropdownItem(x.GetType().Name, x));
        }
#endif
    }
}
