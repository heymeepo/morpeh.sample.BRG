using Scellecs.Morpeh;
#if UNITY_EDITOR && ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Prototypes.Core.ECS.Configs
{
    [Serializable]
    public abstract class EcsComponentGroupConfig
    {
        public abstract void SetToEntity(Entity entity);
    }

    public abstract class EcsSimpleComponentGroupConfig<T> : EcsComponentGroupConfig where T : struct, IComponent
    {
#if UNITY_EDITOR && ODIN_INSPECTOR
        private string Name => typeof(T).Name;
        [LabelText("$Name")]
#endif
        [SerializeField] 
        protected T serializedComponent;

        public override void SetToEntity(Entity entity) => entity.SetComponent(serializedComponent);
    }

    public abstract class EcsSimpleComponentGroupConfig<T1, T2> : EcsComponentGroupConfig
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
#if UNITY_EDITOR && ODIN_INSPECTOR
        private string Name1 => typeof(T1).Name;
        [LabelText("$Name1")]
#endif
        [SerializeField] 
        protected T1 serializedComponent1;

#if UNITY_EDITOR && ODIN_INSPECTOR
        private string Name2 => typeof(T2).Name;
        [LabelText("$Name2")]
#endif
        [SerializeField] 
        protected T2 serializedComponent2;

        public override void SetToEntity(Entity entity)
        {
            entity.SetComponent(serializedComponent1);
            entity.SetComponent(serializedComponent2);
        }
    }

    public abstract class EcsSimpleComponentGroupConfig<T1, T2, T3> : EcsComponentGroupConfig
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
#if UNITY_EDITOR && ODIN_INSPECTOR
        private string Name1 => typeof(T1).Name;
        [LabelText("$Name1")]
#endif
        [SerializeField]
        protected T1 serializedComponent1;

#if UNITY_EDITOR && ODIN_INSPECTOR
        private string Name2 => typeof(T2).Name;
        [LabelText("$Name2")]
#endif
        [SerializeField]
        protected T2 serializedComponent2;

#if UNITY_EDITOR && ODIN_INSPECTOR
        private string Name3 => typeof(T3).Name;
        [LabelText("$Name3")]
#endif
        [SerializeField]
        protected T3 serializedComponent3;

        public override void SetToEntity(Entity entity)
        {
            entity.SetComponent(serializedComponent1);
            entity.SetComponent(serializedComponent2);
            entity.SetComponent(serializedComponent3);
        }
    }

    public class ComponentGroupEqualityComparer : IEqualityComparer<EcsComponentGroupConfig>
    {
        public static ComponentGroupEqualityComparer Default { get; private set; } = new ComponentGroupEqualityComparer();

        public bool Equals(EcsComponentGroupConfig x, EcsComponentGroupConfig y) => x != null && y != null && x.GetType() == y.GetType();

        public int GetHashCode(EcsComponentGroupConfig obj) => obj.GetType().GetHashCode();
    }
}
